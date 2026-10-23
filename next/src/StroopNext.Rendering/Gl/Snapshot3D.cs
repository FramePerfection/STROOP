using StroopNext.Rendering.Skia;
using StroopNext.Rendering.Gl;
using StroopNext.Rendering.Diagnostics;
using System.Diagnostics;
using System.Numerics;
using OpenTK.Graphics.OpenGL4;
using SkiaSharp;
using StroopNext.Game;
using StroopNext.Ghosts;
using StroopNext.Map;
using StroopNext.Map3D;
using StroopNext.Memory;
using StroopNext.Session;
using StroopNext.Settings;

namespace StroopNext.Rendering.Gl;

internal static class Snapshot3D
{
    private const int Width = 1200, Height = 900;

    private static GameFrame Load(string savestate, out byte[] ram)
    {
        using var source = Savestate.Load(savestate);
        ram = new byte[source.Size];
        source.TryRead(ram);
        var frame = GameFrame.Create(new RamImage(ram));
        if (Environment.GetEnvironmentVariable("STROOPNEXT_CAPTURE") is { } other && File.Exists(other))
        {
            using var otherSource = Savestate.Load(other);
            var otherRam = new byte[otherSource.Size];
            otherSource.TryRead(otherRam);
            var otherFrame = GameFrame.Create(new RamImage(otherRam));
            if (otherFrame.Mario is { } otherMario)
            {
                StroopNext.Graphics.MarioAnimations.Capture(otherFrame.Ram, otherMario.MarioObject);
                Console.WriteLine($"captured Mario's animation {otherFrame.Ram.S16(otherMario.MarioObject + 0x38)} from {Path.GetFileName(other)}");
            }
        }
        if (Environment.GetEnvironmentVariable("STROOPNEXT_GHOSTHACK") == "1" && frame.Layout?.MarioObjectPointer is uint pointer)
        {
            var hack = new InGameGhosts();
            hack.Install(frame, out string? error)?.ApplyTo(ram);
            uint mario = frame.Ram.U32(pointer);
            new RamPatch().WriteU16(mario + 2, (ushort)(frame.Ram.U16(mario + 2) | 0x40)).ApplyTo(ram);
            var caps = hack.ColoredCaps(GameFrame.Create(new RamImage(ram)));
            caps?.ApplyTo(ram);
            Console.WriteLine($"cap writes: {caps?.Writes.Count ?? 0}");
            Console.WriteLine(error ?? "ghost hack applied in memory");
            frame = GameFrame.Create(new RamImage(ram));
        }
        return frame;
    }

    private static void Build(Scene3D scene, IReadOnlyList<IMapLayer> layers, GameFrame frame, Camera3D camera, SettingsStore store, Selection selection)
    {
        scene.Clear();
        camera.Far = Camera3D.DefaultFar;
        var only = Environment.GetEnvironmentVariable("STROOPNEXT_LAYERS")?.Split(',', StringSplitOptions.TrimEntries);
        foreach (var layer in layers)
            if ((only is null ? layer.VisibleByDefault : only.Contains("*") || only.Contains(layer.Id)) && layer is IMapLayer3D layer3D)
                layer3D.Build3D(new Map3DContext(frame, camera, scene, store.Scope($"map.layer.{layer.Id}"), selection));
    }

    public static int Run(string savestate, string output, string[] options)
    {
        var frame = Load(savestate, out _);
        if (frame.Mario is not { } mario)
        {
            Console.Error.WriteLine("Not an SM64 savestate.");
            return 1;
        }

        var camera = new Camera3D { Width = Width, Height = Height, Target = mario.Position + new Vector3(0, 80, 0) };
        string mode = options.Length > 0 ? options[0] : "follow";
        double Option(int i, double fallback) => options.Length > i ? double.Parse(options[i], System.Globalization.CultureInfo.InvariantCulture) : fallback;
        if (mode == "side")
        {
            var axis = (SideAxis)(int)Option(1, 0);
            SideView.Apply(camera, camera.Target, SideView.LookYaw(axis, mario.FacingYaw), (float)Option(2, 3000), (float)Option(3, 0));
        }
        else if (mode == "game" && frame.Camera is { } game)
        {
            camera.Eye = game.Position;
            camera.Target = game.Focus;
            camera.FieldOfView = 45 * Math.PI / 180;
        }
        else
        {
            double D(int i, double fallback) => options.Length > i ? double.Parse(options[i], System.Globalization.CultureInfo.InvariantCulture) : fallback;
            camera.Yaw = D(1, 200) * Math.PI / 180;
            camera.Pitch = D(2, 40) * Math.PI / 180;
            camera.Distance = D(3, 2200);
        }

        var scene = new Scene3D();
        var layers = StandardLayers.Create(StandardLayers.CreateMarioPathTrack());
        if (Environment.GetEnvironmentVariable("STROOPNEXT_GHOST") is { } ghostFile && File.Exists(ghostFile))
            using (var stream = File.OpenRead(ghostFile))
                layers.OfType<GhostsLayer>().First().Add(Ghost.Read(stream, Path.GetFileNameWithoutExtension(ghostFile)));
        Build(scene, layers, frame, camera, new SettingsStore(), new Selection());

        using var gl = new OffscreenGl(Width, Height);
        using (var renderer = new GlMapRenderer())
        {
            renderer.Render(scene, camera, Width, Height, hovered: null);
            GL.Finish();
            var pixels = GlMapRenderer.ReadPixels(Width, Height);
            if (GL.GetError() is var error && error != ErrorCode.NoError)
                Console.Error.WriteLine($"GL error: {error}");
            using var bitmap = new SKBitmap(new SKImageInfo(Width, Height, SKColorType.Bgra8888, SKAlphaType.Premul));
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bitmap.GetPixels(), pixels.Length);
            using var image = SKImage.FromBitmap(bitmap);
            using var png = image.Encode(SKEncodedImageFormat.Png, 100);
            using var file = File.Create(output);
            png.SaveTo(file);
        }
        Console.WriteLine($"{gl.RendererName} · {scene.Background.Count} background + {scene.Items.Count} live primitives → {output}");
        return 0;
    }

    public static int Bench(string savestate, int frames)
    {
        var services = new GameServices();
        Load(savestate, out var ram);
        var first = GameFrame.Create(new RamImage(ram), services: services);
        if (first.Mario is not { } mario0)
        {
            Console.Error.WriteLine("Not an SM64 savestate.");
            return 1;
        }

        var track = StandardLayers.CreateMarioPathTrack();
        var layers = StandardLayers.Create(track);
        var store = new SettingsStore();
        var selection = new Selection();
        var scene = new Scene3D();
        var camera = new Camera3D { Width = Width, Height = Height, Yaw = 3.5, Pitch = 0.8, Distance = 1800 };

        using var gl = new OffscreenGl(Width, Height);
        using var renderer = new GlMapRenderer();
        var process = Process.GetCurrentProcess();
        long privateAfterWarmup = 0;
        double build = 0, render = 0, gpu = 0;
        const int warmup = 20;
        long allocated = 0;
        int[] gcStart = [GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2)];
        var stage = new Stopwatch();
        for (int i = 0; i < frames; i++)
        {
            double a = i * 0.05;
            var patch = new RamPatch()
                .WriteU32(first.Layout!.GlobalTimer!.Value, first.GlobalTimer + (uint)i + 1)
                .WriteF32(mario0.Address + MarioOffsets.X, mario0.Position.X + (float)(Math.Cos(a) * 400))
                .WriteF32(mario0.Address + MarioOffsets.Z, mario0.Position.Z + (float)(Math.Sin(a) * 400))
                .WriteU16(mario0.Address + MarioOffsets.FacingYaw, (ushort)(i * 300));
            patch.ApplyTo(ram);
            var frame = GameFrame.Create(new RamImage(ram), services: services);
            track.Record(frame);
            camera.Target = frame.Mario!.Position + new Vector3(0, 80, 0);

            bool measured = i >= warmup;
            long a0 = GC.GetAllocatedBytesForCurrentThread();
            stage.Restart();
            Build(scene, layers, frame, camera, store, selection);
            if (measured)
            {
                build += stage.Elapsed.TotalMilliseconds;
                allocated += GC.GetAllocatedBytesForCurrentThread() - a0;
            }

            stage.Restart();
            renderer.Render(scene, camera, Width, Height, hovered: null);
            double cpu = stage.Elapsed.TotalMilliseconds;
            stage.Restart();
            GL.Finish();
            if (measured)
            {
                render += cpu;
                gpu += stage.Elapsed.TotalMilliseconds;
            }
            if (i == warmup)
            {
                GC.Collect();
                process.Refresh();
                privateAfterWarmup = process.PrivateMemorySize64;
            }
            if (i % 1000 == 999)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                process.Refresh();
                Console.WriteLine($"  frame {i + 1,5}: managed {GC.GetTotalMemory(false) / 1048576.0:0.00} MB · private {process.PrivateMemorySize64 / 1048576.0:0.0} MB");
            }
        }
        int n = Math.Max(1, frames - warmup);
        Console.WriteLine($"{gl.RendererName}, {frames} frames at {Width}×{Height}");
        Console.WriteLine($"scene              {scene.Background.Count} background + {scene.Items.Count} live primitives");
        Console.WriteLine($"per frame          build {build / n:0.000} ms · render CPU {render / n:0.000} ms · wait for GPU {gpu / n:0.000} ms (after {warmup} warm-up frames)");
        Console.WriteLine($"background uploads {renderer.BackgroundUploads} for {frames} frames");
        if (scene.GameVisuals is { } game && layers.OfType<GameVisualsLayer>().FirstOrDefault() is { } visuals)
            Console.WriteLine($"game scene         level {game.Level.TriangleCount} tris · {game.Instances.Count} object instances · {game.Textures.All.Count} textures · " +
                              $"{visuals.Builder.CachedMeshCount} cached meshes · {renderer.GameDrawCalls} draw calls per frame");
        GC.Collect();
        process.Refresh();
        Console.WriteLine($"private bytes      {privateAfterWarmup / 1048576.0:0.0} MB after warm-up → {process.PrivateMemorySize64 / 1048576.0:0.0} MB at the end (GPU buffers are reused, so this must stay flat)");
        Console.WriteLine($"build allocations  {allocated / n / 1024.0:0.0} KB per frame · GCs gen0 {GC.CollectionCount(0) - gcStart[0]} gen1 {GC.CollectionCount(1) - gcStart[1]} gen2 {GC.CollectionCount(2) - gcStart[2]}");
        return 0;
    }
}
