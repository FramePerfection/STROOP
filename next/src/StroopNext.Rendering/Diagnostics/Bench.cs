using System.Diagnostics;
using System.Numerics;
using SkiaSharp;
using StroopNext.Rendering.Skia;
using StroopNext.Game;
using StroopNext.Map;
using StroopNext.Memory;
using StroopNext.Session;
using StroopNext.Settings;
using StroopNext.Variables;

namespace StroopNext.Rendering.Diagnostics;

internal static class Bench
{
    public static int Run(string savestate, int seconds, IReadOnlyCollection<string>? without = null, IDisposable? timerResolution = null)
    {
        using var resolution = timerResolution;
        var source = Savestate.Load(savestate);
        using var session = new GameSession();
        var track = session.AddTrack(StandardLayers.CreateMarioPathTrack());
        session.Attach(source);

        var first = WaitForFrame(session);
        if (first.Mario is not { } mario0 || first.Layout?.GlobalTimer is not uint timerAddress)
        {
            Console.Error.WriteLine("Not an SM64 savestate.");
            return 1;
        }
        uint marioAddress = mario0.Address;
        first.Release();

        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
        var game = new Thread(() => SimulateGame(source, timerAddress, marioAddress, mario0.Position, stop.Token)) { IsBackground = true };
        game.Start();

        var layers = StandardLayers.Create(track);
        var store = new SettingsStore();
        var selection = new Selection();
        var view = new MapView { Width = 1200, Height = 900, Scale = 0.3, CullMarginPx = 300 };
        var scene = new MapScene();
        using var surface = SKSurface.Create(new SKImageInfo(1200, 900));
        using var renderer = new SkiaMapRenderer();
        var display = DisplayOptions.Default;

        var process = Process.GetCurrentProcess();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        process.Refresh();
        long privateStart = process.PrivateMemorySize64;
        long allocatedStart = GC.GetTotalAllocatedBytes(precise: true);
        int[] gcStart = [GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2)];
        var cpuStart = process.TotalProcessorTime;
        var wall = Stopwatch.StartNew();

        double tVars = 0, tScene = 0, tRender = 0;
        long aVars = 0, aScene = 0, aRender = 0;
        int frames = 0, primitives = 0;
        long lastSequence = -1;
        var stage = new Stopwatch();
        while (!stop.IsCancellationRequested)
        {
            var frame = session.AcquireLatest();
            if (frame is not null && frame.Sequence != lastSequence)
            {
                lastSequence = frame.Sequence;
                frames++;

                long a0 = GC.GetAllocatedBytesForCurrentThread();
                stage.Restart();
                foreach (var variable in MarioVariables.All)
                {
                    variable.Format(frame, display);
                    variable.FormatDelta(frame, display);
                }
                tVars += stage.Elapsed.TotalMilliseconds;
                long a1 = GC.GetAllocatedBytesForCurrentThread();
                aVars += a1 - a0;

                stage.Restart();
                MapViewSettings.Apply(view, frame, follow: true, MapOrientation.North);
                scene.Clear();
                foreach (var layer in layers)
                    if (layer.VisibleByDefault && without?.Contains(layer.Id) != true)
                        layer.Build(new MapContext(frame, view, scene, store.Scope($"map.layer.{layer.Id}"), selection));
                primitives += scene.Count;
                tScene += stage.Elapsed.TotalMilliseconds;
                long a2 = GC.GetAllocatedBytesForCurrentThread();
                aScene += a2 - a1;

                stage.Restart();
                renderer.Draw(surface.Canvas, scene, view, hovered: null);
                surface.Canvas.Flush();
                tRender += stage.Elapsed.TotalMilliseconds;
                aRender += GC.GetAllocatedBytesForCurrentThread() - a2;
            }
            frame?.Release();
            Thread.Sleep(16);
        }
        game.Join();

        var cpu = process.TotalProcessorTime - cpuStart;
        double elapsed = wall.Elapsed.TotalSeconds;
        long allocated = GC.GetTotalAllocatedBytes(precise: true) - allocatedStart;
        int[] gcs = [GC.CollectionCount(0) - gcStart[0], GC.CollectionCount(1) - gcStart[1], GC.CollectionCount(2) - gcStart[2]];
        var diagnostics = session.Diagnostics;
        int pathSamples = track.Snapshot().Length;
        session.Detach();
        scene.Clear();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        process.Refresh();
        long privateEnd = process.PrivateMemorySize64;

        int n = Math.Max(1, frames);
        Console.WriteLine($"duration            {elapsed:0.0} s, {frames} frames consumed ({frames / elapsed:0.0}/s), path samples {pathSamples}");
        Console.WriteLine($"session             {diagnostics}");
        Console.WriteLine($"after detach        {session.Diagnostics}");
        Console.WriteLine($"process CPU         {cpu.TotalMilliseconds / (elapsed * 1000) * 100:0.0}% of one core (whole process, including the simulated game)");
        Console.WriteLine($"per frame (UI side) vars {tVars / n:0.000} ms · scene {tScene / n:0.000} ms ({primitives / n} primitives) · render {tRender / n:0.000} ms");
        Console.WriteLine($"UI-side allocations vars {aVars / n / 1024.0:0.0} KB · scene {aScene / n / 1024.0:0.0} KB · render {aRender / n / 1024.0:0.0} KB per frame");
        Console.WriteLine($"background cache    {renderer.BackgroundReuses} reuses, {renderer.BackgroundRenders} re-renders, {renderer.BackgroundTime.TotalMilliseconds / n:0.000} ms per frame of the render time");
        Console.WriteLine($"allocations         {allocated / 1024.0 / 1024.0:0.0} MB total, {allocated / n / 1024.0:0.0} KB per consumed frame (all threads)");
        Console.WriteLine($"GCs                 gen0 {gcs[0]} · gen1 {gcs[1]} · gen2 {gcs[2]}");
        Console.WriteLine($"private bytes       {privateStart / 1048576.0:0.0} MB → {privateEnd / 1048576.0:0.0} MB after full GC");
        return 0;
    }

    private static GameFrame WaitForFrame(GameSession session)
    {
        for (int i = 0; i < 500; i++)
        {
            if (session.AcquireLatest() is { } frame)
                return frame;
            Thread.Sleep(10);
        }
        throw new TimeoutException("No frame from the savestate.");
    }

    private static void SimulateGame(IRamSource source, uint timerAddress, uint mario, Vector3 origin, CancellationToken token)
    {
        var clock = Stopwatch.StartNew();
        uint timer = 0;
        while (!token.IsCancellationRequested)
        {
            timer++;
            double a = timer * 0.05;
            var patch = new RamPatch()
                .WriteU32(timerAddress, 100000 + timer)
                .WriteF32(mario + MarioOffsets.X, origin.X + (float)(Math.Cos(a) * 600))
                .WriteF32(mario + MarioOffsets.Z, origin.Z + (float)(Math.Sin(a) * 600))
                .WriteU16(mario + MarioOffsets.FacingYaw, (ushort)(timer * 300))
                .WriteF32(mario + MarioOffsets.HSpeed, (float)(30 + 10 * Math.Sin(a)));
            foreach (var (offset, bytes) in patch.ToSwappedRuns())
                source.TryWrite(offset, bytes);
            double next = timer * 1000.0 / 30;
            int sleep = (int)(next - clock.Elapsed.TotalMilliseconds);
            if (sleep > 0)
                Thread.Sleep(sleep);
        }
    }
}
