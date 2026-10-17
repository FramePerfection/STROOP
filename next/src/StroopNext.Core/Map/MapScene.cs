using System.Numerics;

namespace StroopNext.Map;

public readonly record struct MapColor(byte A, byte R, byte G, byte B)
{
    public static readonly MapColor Transparent = new(0, 0, 0, 0);

    public static MapColor Rgb(byte r, byte g, byte b) => new(255, r, g, b);
    public static MapColor FromHex(uint argb) => new((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);

    public MapColor WithAlpha(double alpha) => this with { A = (byte)Math.Clamp(Math.Round(alpha * 255), 0, 255) };

    public MapColor Lerp(MapColor to, double t)
    {
        t = Math.Clamp(t, 0, 1);
        byte L(byte a, byte b) => (byte)Math.Round(a + (b - a) * t);
        return new MapColor(L(A, to.A), L(R, to.R), L(G, to.G), L(B, to.B));
    }

    public bool IsVisible => A > 0;
}

public interface IMapHit
{
    string Title { get; }
    IEnumerable<string> Details { get; }
}

public enum MarkerShape { Circle, Square, Diamond, Triangle }

public abstract record MapPrimitive
{
    public IMapHit? Hit { get; init; }
}

public sealed record MapPolygon(Vector2[] Points, MapColor Fill, MapColor Stroke, float StrokePx = 1) : MapPrimitive;
public sealed record MapPolyline(Vector2[] Points, MapColor Stroke, float StrokePx = 1) : MapPrimitive;
public sealed record MapCircle(Vector2 Center, float Radius, MapColor Fill, MapColor Stroke, float StrokePx = 1) : MapPrimitive;

public sealed record MapArrow(Vector2 From, double Yaw, float LengthPx, MapColor Color, float StrokePx = 2) : MapPrimitive;

public sealed record MapMarker(Vector2 Center, float SizePx, MarkerShape Shape, MapColor Fill, MapColor Stroke, double? Yaw = null) : MapPrimitive;

public sealed record MapLabel(Vector2 At, string Text, MapColor Color, float OffsetXPx = 8, float OffsetYPx = -8) : MapPrimitive;

public sealed class SceneKey : IEquatable<SceneKey>
{
    private readonly object?[] _parts;

    internal SceneKey(object?[] parts) => _parts = parts;

    internal static SceneKey Of(List<object?> parts) => new(parts.ToArray());

    public bool Equals(SceneKey? other)
    {
        if (other is null || other._parts.Length != _parts.Length)
            return false;
        for (int i = 0; i < _parts.Length; i++)
            if (!(ReferenceEquals(_parts[i], other._parts[i]) || Equals(_parts[i], other._parts[i])))
                return false;
        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as SceneKey);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var part in _parts)
            hash.Add(part);
        return hash.ToHashCode();
    }
}

public sealed class MapScene
{
    private readonly List<MapPrimitive> _background = new();
    private readonly List<MapPrimitive> _items = new();
    private readonly List<object?> _keyParts = new();
    private SceneKey? _key;

    public IReadOnlyList<MapPrimitive> Background => _background;
    public IReadOnlyList<MapPrimitive> Items => _items;

    public IEnumerable<MapPrimitive> All => _background.Concat(_items);

    public int Count => _background.Count + _items.Count;

    public StroopNext.Graphics.GameScene? GameVisuals { get; set; }
    public StroopNext.Map3D.GameVisualsOptions? GameVisualsOptions { get; set; }
    public bool GameShadows { get; set; }

    public void Add(MapPrimitive primitive) => _items.Add(primitive);

    public void AddBackground(MapPrimitive primitive) => _background.Add(primitive);

    public void KeyBackground(object? part)
    {
        _keyParts.Add(part);
        _key = null;
    }

    public SceneKey BackgroundKey => _key ??= new SceneKey(_keyParts.ToArray());

    public void Clear()
    {
        _background.Clear();
        _items.Clear();
        _keyParts.Clear();
        _key = null;
        GameVisuals = null;
        GameVisualsOptions = null;
        GameShadows = false;
    }

    public MapPrimitive? HitTest(MapView view, Vector2 screen, float tolerancePx = 4)
    {
        var world = view.ScreenToWorld(screen);
        return HitTest(_items, view, screen, world, tolerancePx) ?? HitTest(_background, view, screen, world, tolerancePx);
    }

    private static MapPrimitive? HitTest(List<MapPrimitive> list, MapView view, Vector2 screen, Vector2 world, float tolerancePx)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var item = list[i];
            if (item.Hit is not null && Contains(view, item, screen, world, tolerancePx))
                return item;
        }
        return null;
    }

    private static bool Contains(MapView view, MapPrimitive item, Vector2 screen, Vector2 world, float tol) => item switch
    {
        MapPolygon p => PointInPolygon(world, p.Points) || NearPolyline(view, screen, p.Points, tol, closed: true),
        MapPolyline l => NearPolyline(view, screen, l.Points, tol + l.StrokePx / 2, closed: false),
        MapCircle c => Vector2.Distance(world, c.Center) <= c.Radius + tol / view.Scale,
        MapMarker m => Vector2.Distance(view.WorldToScreen(m.Center), screen) <= m.SizePx / 2 + tol,
        MapArrow a => Vector2.Distance(view.WorldToScreen(a.From), screen) <= tol * 2,
        _ => false,
    };

    internal static bool PointInPolygon(Vector2 p, Vector2[] poly)
    {
        bool inside = false;
        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
        {
            if ((poly[i].Y > p.Y) != (poly[j].Y > p.Y) &&
                p.X < (poly[j].X - poly[i].X) * (p.Y - poly[i].Y) / (poly[j].Y - poly[i].Y) + poly[i].X)
                inside = !inside;
        }
        return inside;
    }

    private static bool NearPolyline(MapView view, Vector2 screen, Vector2[] points, float tol, bool closed)
    {
        int count = closed ? points.Length : points.Length - 1;
        for (int i = 0; i < count; i++)
        {
            var a = view.WorldToScreen(points[i]);
            var b = view.WorldToScreen(points[(i + 1) % points.Length]);
            if (DistanceToSegment(screen, a, b) <= tol)
                return true;
        }
        return false;
    }

    private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        float lengthSq = ab.LengthSquared();
        float t = lengthSq == 0 ? 0 : Math.Clamp(Vector2.Dot(p - a, ab) / lengthSq, 0, 1);
        return Vector2.Distance(p, a + ab * t);
    }
}
