using System.Globalization;
using System.Numerics;
using StroopNext.Game;
using StroopNext.Ghosts;
using StroopNext.Graphics;
using StroopNext.Map3D;

namespace StroopNext.Map;

public sealed class GhostEntry(Ghost ghost, Argb8Color color)
{
    public Ghost Ghost { get; } = ghost;
    public Argb8Color Color { get; set; } = color;
    public bool Visible { get; set; } = true;
    public bool ShowPath { get; set; } = true;

    public bool Bold { get; set; }

    public string? FilePath { get; set; }

    public uint? RecordedStart { get; set; } = ghost.FirstTimer;

    public Func<int?>? PreviewFrame { get; set; }

    public bool IsMarker { get; set; }

    public bool IsLight { get; set; }

    internal Vector2[] Path2D => _path2D is { } p && _pathCount == Ghost.Frames.Count ? p : FillPaths().Item1;
    internal Vector3[] Path3D => _path3D is { } p && _pathCount == Ghost.Frames.Count ? p : FillPaths().Item2;

    private Vector2[]? _path2D;
    private Vector3[]? _path3D;
    private int _pathCount = -1;

    private (Vector2[], Vector3[]) FillPaths()
    {
        var lift = new Vector3(0, 2, 0);
        var frames = Ghost.InPlaybackOrder.ToArray();
        _path2D = frames.Select(f => Xz.Of(f.Position)).ToArray();
        _path3D = frames.Select(f => f.Position + lift).ToArray();
        _pathCount = Ghost.Frames.Count;
        return (_path2D, _path3D);
    }

    public GhostFrame? FrameNow(uint globalTimer)
    {
        if (PreviewFrame?.Invoke() is int index)
        {
            if (_ordered is null || _orderedCount != Ghost.Frames.Count)
            {
                _ordered = Ghost.InPlaybackOrder.ToArray();
                _orderedCount = Ghost.Frames.Count;
            }
            return _ordered.Length == 0 ? null : _ordered[Math.Clamp(index, 0, _ordered.Length - 1)];
        }
        return Ghost.FrameAt(globalTimer);
    }

    private GhostFrame[]? _ordered;
    private int _orderedCount;
}

public sealed record GhostHit(Ghost Ghost, uint GlobalTimer, GhostFrame Frame) : IMapHit
{
    public string Title => $"Ghost \"{Ghost.Name}\" at timer {GlobalTimer}";

    public IEnumerable<string> Details =>
    [
        string.Create(CultureInfo.InvariantCulture, $"Position ({Frame.Position.X:0.###}, {Frame.Position.Y:0.###}, {Frame.Position.Z:0.###})"),
        $"Yaw {Frame.FacingYaw} · animation {Frame.AnimationIndex} frame {Frame.AnimationFrame}",
        $"Frame {Ghost.Order(unchecked(GlobalTimer - 1 - Ghost.PlaybackBaseFrame))} · {Ghost.Frames.Count} frames from timer {Ghost.FirstTimer}",
    ];
}

public sealed class GhostsLayer : MapLayer, IMapLayer3D
{
    public static readonly Argb8Color[] Colors =
    [
        Argb8Color.Rgb(0x44, 0xC0, 0x66), Argb8Color.Rgb(0x3A, 0x9B, 0xF0), Argb8Color.Rgb(0xF5, 0xD0, 0x20),
        Argb8Color.Rgb(0xC0, 0x4C, 0xE0), Argb8Color.Rgb(0x3D, 0xD6, 0xF5), Argb8Color.Rgb(0xF0, 0x8C, 0x2E),
    ];

    private readonly List<GhostEntry> _ghosts = new();

    public override string Id => "ghosts";
    public override string Name => "Ghosts";

    public IReadOnlyList<GhostEntry> Ghosts => _ghosts;

    public event Action? Changed;

    public GhostEntry Add(Ghost ghost, bool visible = true, string? filePath = null)
    {
        var entry = new GhostEntry(ghost, Colors[_ghosts.Count % Colors.Length]) { Visible = visible, FilePath = filePath };
        _ghosts.Add(entry);
        NotifyChanged();
        return entry;
    }

    public void Remove(GhostEntry entry)
    {
        _ghosts.Remove(entry);
        NotifyChanged();
    }

    private int _quiet;
    private bool _pending;

    public void NotifyChanged()
    {
        if (_quiet > 0)
            _pending = true;
        else
            Changed?.Invoke();
    }

    public IDisposable Batch()
    {
        _quiet++;
        return new BatchEnd(this);
    }

    private sealed class BatchEnd(GhostsLayer layer) : IDisposable
    {
        private bool _done;

        public void Dispose()
        {
            if (_done)
                return;
            _done = true;
            if (--layer._quiet == 0 && layer._pending)
            {
                layer._pending = false;
                layer.Changed?.Invoke();
            }
        }
    }

    public override void Build(MapContext c)
    {
        foreach (var g in _ghosts)
        {
            if (!g.Visible)
                continue;
            if (g.ShowPath && g.Ghost.Frames.Count >= 2)
                c.Scene.Add(new MapPolyline(g.Path2D, g.Color.WithAlpha(g.Bold ? 0.95 : g.IsLight ? 0.3 : 0.45), g.Bold ? 3.5f : g.IsLight ? 1f : 1.5f));
            if (g.FrameNow(c.Frame.GlobalTimer) is { } now)
            {
                var hit = new GhostHit(g.Ghost, c.Frame.GlobalTimer, now);
                var at = Xz.Of(now.Position);
                if (!g.IsLight)
                    c.Scene.Add(new MapCircle(at, MarioState.HitboxRadius, g.Color.WithAlpha(0.2), g.Color, 1.5f) { Hit = hit });
                c.Scene.Add(new MapMarker(at, g.IsLight ? 10 : 14, MarkerShape.Triangle, g.Color, MapPalette.White, now.FacingYaw) { Hit = hit });
                if (!g.IsLight)
                    c.Scene.Add(new MapLabel(at, g.Ghost.Name, g.Color));
            }
        }
    }

    public void Build3D(Map3DContext c)
    {
        bool gameDrawsGhosts = InGameGhosts.GameGhostNodes(c.Frame.Ram).Any();
        foreach (var g in _ghosts)
        {
            if (!g.Visible)
                continue;
            if (g.ShowPath && g.Ghost.Frames.Count >= 2)
                c.Scene.Add(new Line3D(g.Path3D, g.Color.WithAlpha(g.Bold ? 1 : g.IsLight ? 0.35 : 0.6)));
            if (g.FrameNow(c.Frame.GlobalTimer) is { } now)
            {
                var hit = new GhostHit(g.Ghost, c.Frame.GlobalTimer, now);
                bool model = c.Scene.GameVisuals is not null && !g.IsMarker;
                if (model && !gameDrawsGhosts
                    && c.Scene.GameVisuals!.AddVirtualGhost?.Invoke(new Graphics.VirtualGhost(now.Position, (short)now.Pitch, (short)now.Yaw, (short)now.Roll, now.AnimationIndex, (short)(now.AnimationFrame + 1),
                        g.IsLight ? null : g.Ghost.Bodies.GetValueOrDefault(g.Ghost.IndexAt(c.Frame.GlobalTimer)))) != true)
                    c.Scene.MarioModelGhosts.Add((now.Position, now.FacingYaw));
                c.Scene.Add(new Cylinder3D(now.Position, MarioState.HitboxRadius, MarioState.HitboxHeight, g.Color.WithAlpha(model ? (g.IsLight ? 0.08 : 0.12) : 0.35)) { Hit = hit });
                if (!model)
                    c.Scene.Add(new Arrow3D(now.Position + new Vector3(0, 80, 0), now.FacingYaw, 80, 6, g.Color) { Hit = hit });
            }
        }
    }
}
