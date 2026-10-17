using System.Globalization;
using System.Numerics;
using StroopNext.Game;
using StroopNext.Map3D;
using StroopNext.Session;

namespace StroopNext.Map;

public readonly record struct MeasureEnd(Vector3 Point, bool FollowsMario = false)
{
    public static MeasureEnd Mario => new(default, true);

    public Vector3? Resolve(GameFrame frame) => FollowsMario ? frame.Mario?.Position : Point;
}

public readonly record struct Measurement(Vector3 A, Vector3 B)
{
    public float DX => B.X - A.X;
    public float DY => B.Y - A.Y;
    public float DZ => B.Z - A.Z;
    public float HorizontalDistance => MathF.Sqrt(DX * DX + DZ * DZ);
    public float Distance => Vector3.Distance(A, B);

    public ushort Yaw => Angle.Towards(A.X, A.Z, B.X, B.Z);

    public string Summary => string.Create(CultureInfo.InvariantCulture,
        $"XZ {HorizontalDistance:0.###} · 3D {Distance:0.###}");

    public IEnumerable<string> Details =>
    [
        string.Create(CultureInfo.InvariantCulture, $"ΔX {DX:0.###} · ΔY {DY:0.###} · ΔZ {DZ:0.###}"),
        string.Create(CultureInfo.InvariantCulture, $"Horizontal {HorizontalDistance:0.###} · 3D {Distance:0.###}"),
        string.Create(CultureInfo.InvariantCulture, $"Yaw A → B {Yaw} ({Angle.ToDegrees(Yaw):0.##}°)"),
        string.Create(CultureInfo.InvariantCulture, $"A ({A.X:0.###}, {A.Y:0.###}, {A.Z:0.###})"),
        string.Create(CultureInfo.InvariantCulture, $"B ({B.X:0.###}, {B.Y:0.###}, {B.Z:0.###})"),
    ];
}

public sealed record MeasureHit(Measurement Measurement) : IMapHit
{
    public string Title => $"Tape measure · {Measurement.Summary}";
    public IEnumerable<string> Details => Measurement.Details;
}

public sealed class MapToolsLayer : MapLayer, IMapLayer3D
{
    public static readonly MapColor MeasureColor = MapColor.Rgb(0xFF, 0xFF, 0xFF);
    public static readonly MapColor DrawingColor = MapColor.Rgb(0xFF, 0x7A, 0xC8);

    private readonly List<List<Vector2>> _strokes = new();

    public override string Id => "tools";
    public override string Name => "Measure & drawing";

    public MeasureEnd? Start { get; private set; }
    public MeasureEnd? End { get; private set; }
    public IReadOnlyList<IReadOnlyList<Vector2>> Strokes => _strokes;

    public event Action? Changed;

    public void SetStart(MeasureEnd start)
    {
        Start = start;
        Changed?.Invoke();
    }

    public void SetEnd(MeasureEnd end)
    {
        End = end;
        Changed?.Invoke();
    }

    public void ClearMeasure()
    {
        Start = End = null;
        Changed?.Invoke();
    }

    public Measurement? Measure(GameFrame frame)
        => Start?.Resolve(frame) is { } a && End?.Resolve(frame) is { } b ? new Measurement(a, b) : null;

    public void BeginStroke(Vector2 at)
    {
        _strokes.Add([at]);
        Changed?.Invoke();
    }

    public void ExtendStroke(Vector2 at, float minSpacing)
    {
        if (_strokes.Count == 0)
            return;
        var stroke = _strokes[^1];
        if (Vector2.Distance(stroke[^1], at) < minSpacing)
            return;
        stroke.Add(at);
        Changed?.Invoke();
    }

    public void ClearDrawings()
    {
        _strokes.Clear();
        Changed?.Invoke();
    }

    public override void Build(MapContext c)
    {
        foreach (var stroke in _strokes)
            if (stroke.Count >= 2)
                c.Scene.Add(new MapPolyline(stroke.ToArray(), DrawingColor, 2.5f));
            else if (stroke.Count == 1)
                c.Scene.Add(new MapMarker(stroke[0], 4, MarkerShape.Circle, DrawingColor, MapColor.Transparent));

        if (Start?.Resolve(c.Frame) is { } lone && End is null)
            c.Scene.Add(new MapMarker(Xz.Of(lone), 9, MarkerShape.Diamond, MeasureColor, MapPalette.Black) { Hit = new PointHit("Tape measure start", lone) });

        if (Measure(c.Frame) is not { } m)
            return;
        var hit = new MeasureHit(m);
        Vector2 a = Xz.Of(m.A), b = Xz.Of(m.B);
        c.Scene.Add(new MapPolyline([a, b], MapPalette.Black.WithAlpha(0.6), 4));
        c.Scene.Add(new MapPolyline([a, b], MeasureColor, 2) { Hit = hit });
        var corner = new Vector2(b.X, a.Y);
        c.Scene.Add(new MapPolyline([a, corner], MapColor.Rgb(0xE5, 0x53, 0x4B).WithAlpha(0.7), 1.5f));
        c.Scene.Add(new MapPolyline([corner, b], MapColor.Rgb(0x3A, 0x9B, 0xF0).WithAlpha(0.7), 1.5f));
        c.Scene.Add(new MapMarker(a, 8, MarkerShape.Diamond, MeasureColor, MapPalette.Black) { Hit = hit });
        c.Scene.Add(new MapMarker(b, 8, MarkerShape.Diamond, MeasureColor, MapPalette.Black) { Hit = hit });
        c.Scene.Add(new MapLabel((a + b) / 2, m.Summary, MeasureColor));
    }

    public void Build3D(Map3DContext c)
    {
        if (Measure(c.Frame) is not { } m)
            return;
        var lift = new Vector3(0, 3, 0);
        var hit = new MeasureHit(m);
        c.Scene.Add(new Line3D([m.A + lift, m.B + lift], MeasureColor) { Hit = hit });
        c.Scene.Add(new Point3D(m.A + lift, 8, MeasureColor) { Hit = hit });
        c.Scene.Add(new Point3D(m.B + lift, 8, MeasureColor) { Hit = hit });
    }
}
