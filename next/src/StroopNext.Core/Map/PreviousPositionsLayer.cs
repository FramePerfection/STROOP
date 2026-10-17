using System.Globalization;
using System.Numerics;
using StroopNext.Map3D;
using StroopNext.Session;
using StroopNext.Settings;

namespace StroopNext.Map;

public sealed record PreviousPosition(uint GlobalTimer, Vector3 Position, ushort Angle, int Stage, int? QuarterStep, int? EarlyQuarterSteps = null);

public static class PreviousPositions
{
    public const uint BufferAddress = 0x807F4800;
    public const int FrameStages = 7;
    public const int RecordsPerFrame = FrameStages + 4 * 4;
    public const uint RecordSize = 0x10;

    public static readonly string[] StageNames =
    [
        "Initial", "After warp_area", "After check_instant_warp", "After platform displacement",
        "After initial wall check A", "After initial wall check B", "After object interactions",
        "Quarter step: intended", "Quarter step: after wall check A", "Quarter step: after wall check B", "Quarter step: after floor check",
    ];

    public static readonly MapColor[] StageColors =
    [
        MapColor.Rgb(0xFF, 0x8F, 0xC8), MapColor.Rgb(0xF5, 0xD0, 0x20), MapColor.Rgb(0xA0, 0x60, 0xE0), MapColor.Rgb(0xA0, 0xA0, 0xA8),
        MapColor.Rgb(0x3D, 0xD6, 0xC5), MapColor.Rgb(0x44, 0xC0, 0x66), MapColor.Rgb(0xA0, 0x6A, 0x3A),
        MapColor.Rgb(0xF0, 0x8C, 0x2E), MapColor.Rgb(0x3D, 0xD6, 0xC5), MapColor.Rgb(0x44, 0xC0, 0x66), MapColor.Rgb(0x3A, 0x9B, 0xF0),
    ];

    public static List<PreviousPosition> Read(GameFrame frame, int frames, bool skipIdentical)
    {
        var result = new List<PreviousPosition>();
        uint timer = frame.GlobalTimer;
        if (!frame.IsKnownGame || !frame.Ram.IsValid(BufferAddress, (int)(128 * RecordsPerFrame * RecordSize)))
            return result;
        frames = Math.Clamp(frames, 1, 128);
        for (int back = frames; back > 0; back--)
        {
            uint expected = timer - (uint)back;
            uint frameBase = BufferAddress + (expected & 0x7F) * RecordsPerFrame * RecordSize;
            ushort GtLo(int i) => frame.Ram.U16(frameBase + RecordSize * (uint)i + 0xC);
            PreviousPosition Record(int i, int stage, int? qs) => new(expected,
                new Vector3(frame.Ram.F32(frameBase + RecordSize * (uint)i), frame.Ram.F32(frameBase + RecordSize * (uint)i + 4), frame.Ram.F32(frameBase + RecordSize * (uint)i + 8)),
                frame.Ram.U16(frameBase + RecordSize * (uint)i + 0xE), stage, qs);

            if (GtLo(0) != (ushort)expected)
                continue;
            for (int i = 0; i < FrameStages; i++)
                if (GtLo(i) == (ushort)expected)
                    Add(Record(i, i, null));
            for (int q = 0; q < 4; q++)
            {
                int first = FrameStages + q * 4;
                if (GtLo(first) != (ushort)expected)
                {
                    if (result.Count > 0)
                        result[^1] = result[^1] with { EarlyQuarterSteps = q };
                    break;
                }
                for (int k = 0; k < 4; k++)
                    Add(Record(first + k, FrameStages + k, q));
            }
        }
        return result;

        void Add(PreviousPosition p)
        {
            if (skipIdentical && result.Count > 0 && result[^1].Position == p.Position && result[^1].Angle == p.Angle)
                return;
            result.Add(p);
        }
    }
}

public sealed record PreviousPositionHit(PreviousPosition Point) : IMapHit
{
    public string Title => $"Timer {Point.GlobalTimer} · {PreviousPositions.StageNames[Point.Stage]}";

    public IEnumerable<string> Details =>
    [
        .. Point.QuarterStep is int q ? [$"Quarter step {q + 1}/4"] : Array.Empty<string>(),
        string.Create(CultureInfo.InvariantCulture, $"Position ({Point.Position.X:0.###}, {Point.Position.Y:0.###}, {Point.Position.Z:0.###})"),
        $"Angle {Point.Angle}",
        .. Point.EarlyQuarterSteps is int e ? [$"Movement stopped after {e}/4 quarter steps"] : Array.Empty<string>(),
    ];
}

public sealed class PreviousPositionsLayer : MapLayer, IMapLayer3D
{
    public override string Id => "previousPositions";
    public override string Name => "Previous positions (patched ROM)";
    public override bool VisibleByDefault => false;

    public readonly Setting<int> Frames;
    public readonly Setting<bool> SkipIdentical;
    public readonly Setting<bool> ShowTruncated;

    public PreviousPositionsLayer()
    {
        Frames = Declare(new Setting<int>("frames", "Frames", 16, "Needs a ROM patched to record Mario's sub-frame positions at 0x807F4800.") { Range = (1, 128, 1) });
        SkipIdentical = Declare(new Setting<bool>("skipIdentical", "Skip identical points", true));
        ShowTruncated = Declare(new Setting<bool>("truncated", "Highlight frames with < 4/4 quarter steps", true));
    }

    public override void Build(MapContext c)
    {
        var points = PreviousPositions.Read(c.Frame, c.Settings.Get(Frames), c.Settings.Get(SkipIdentical));
        if (points.Count == 0)
            return;
        bool truncated = c.Settings.Get(ShowTruncated);
        c.Scene.Add(new MapPolyline(points.Select(p => Xz.Of(p.Position)).ToArray(), MapPalette.White.WithAlpha(0.5), 1));
        foreach (var p in points)
        {
            var at = Xz.Of(p.Position);
            c.Scene.Add(new MapMarker(at, p.QuarterStep is null ? 8 : 6, MarkerShape.Triangle, PreviousPositions.StageColors[p.Stage], MapPalette.Black, p.Angle) { Hit = new PreviousPositionHit(p) });
            if (truncated && p.EarlyQuarterSteps is int e)
                c.Scene.Add(new MapLabel(at, $"{e}/4", MapColor.Rgb(0xFF, 0x50, 0x50)));
        }
    }

    public void Build3D(Map3DContext c)
    {
        var points = PreviousPositions.Read(c.Frame, c.Settings.Get(Frames), c.Settings.Get(SkipIdentical));
        if (points.Count == 0)
            return;
        var lift = new Vector3(0, 2, 0);
        c.Scene.Add(new Line3D(points.Select(p => p.Position + lift).ToArray(), MapPalette.White.WithAlpha(0.6)));
        foreach (var p in points)
            c.Scene.Add(new Point3D(p.Position + lift, p.QuarterStep is null ? 8 : 6, PreviousPositions.StageColors[p.Stage]) { Hit = new PreviousPositionHit(p) });
    }
}
