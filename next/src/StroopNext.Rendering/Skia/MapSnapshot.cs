using System.Numerics;
using SkiaSharp;
using StroopNext.Map;
using StroopNext.Memory;
using StroopNext.Session;
using StroopNext.Settings;

namespace StroopNext.Rendering.Skia;

internal static class MapSnapshot
{
    public static int Run(string savestate, string output, double scale)
    {
        using var source = Savestate.Load(savestate);
        var bytes = new byte[source.Size];
        source.TryRead(bytes);
        var frame = GameFrame.Create(new RamImage(bytes));
        if (frame.Mario is not { } mario)
        {
            Console.Error.WriteLine("Not an SM64 savestate.");
            return 1;
        }

        var track = StandardLayers.CreateMarioPathTrack();
        var view = new MapView { Width = 1200, Height = 900, Scale = scale, Center = new Vector2(mario.Position.X, mario.Position.Z) };
        var scene = new MapScene();
        var store = new SettingsStore();
        var selection = new Selection();
        var only = Environment.GetEnvironmentVariable("STROOPNEXT_LAYERS")?.Split(',', StringSplitOptions.TrimEntries);
        foreach (var layer in StandardLayers.Create(track))
            if (only is null ? layer.VisibleByDefault : only.Contains("*") || only.Contains(layer.Id))
                layer.Build(new MapContext(frame, view, scene, store.Scope($"map.layer.{layer.Id}"), selection));

        using var surface = SKSurface.Create(new SKImageInfo((int)view.Width, (int)view.Height));
        using var renderer = new SkiaMapRenderer();
        renderer.Draw(surface.Canvas, scene, view, hovered: null);
        long started = System.Diagnostics.Stopwatch.GetTimestamp();
        const int repeats = 5;
        for (int i = 0; i < repeats; i++)
            renderer.Draw(surface.Canvas, scene, view, hovered: null);
        Console.WriteLine($"full redraw        {System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds / repeats:0.0} ms at {view.Width}×{view.Height}");
        if (scene.All.FirstOrDefault(i => i.Hit is MarioHit) is { } marioHit)
            renderer.DrawTooltip(surface.Canvas, view, view.WorldToScreen(new Vector2(mario.Position.X, mario.Position.Z)), marioHit.Hit!);
        using var image = surface.Snapshot();
        using var png = image.Encode(SKEncodedImageFormat.Png, 100);
        using (var file = File.Create(output))
            png.SaveTo(file);
        Console.WriteLine($"{scene.Count} primitives, {frame.Surfaces.All.Count} surfaces, {frame.Objects.All.Count} objects → {output}");
        return 0;
    }
}
