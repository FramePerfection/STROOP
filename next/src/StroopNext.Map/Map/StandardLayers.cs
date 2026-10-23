using System.Numerics;
using StroopNext.Game;
using StroopNext.Session;
using StroopNext.Settings;

namespace StroopNext.Map;

internal static class Xz
{
    public static Vector2 Of(Vector3 v) => new(v.X, v.Z);
}

public static class MapPalette
{
    public static readonly Argb8Color Floor = Argb8Color.Rgb(0x4F, 0x86, 0xE0);
    public static readonly Argb8Color ObjectFloor = Argb8Color.Rgb(0x2E, 0xC4, 0xB6);
    public static readonly Argb8Color Wall = Argb8Color.Rgb(0x3C, 0xB0, 0x4B);
    public static readonly Argb8Color ObjectWall = Argb8Color.Rgb(0xA6, 0xD8, 0x4A);
    public static readonly Argb8Color Ceiling = Argb8Color.Rgb(0xE0, 0x4A, 0x4A);
    public static readonly Argb8Color Highlight = Argb8Color.Rgb(0xFF, 0xD4, 0x2A);
    public static readonly Argb8Color Mario = Argb8Color.Rgb(0xE5, 0x33, 0x33);
    public static readonly Argb8Color Object = Argb8Color.Rgb(0xF0, 0x8C, 0x2E);
    public static readonly Argb8Color Hurtbox = Argb8Color.Rgb(0xC0, 0x4C, 0xE0);
    public static readonly Argb8Color Intended = Argb8Color.Rgb(0xF5, 0xD0, 0x20);
    public static readonly Argb8Color Sliding = Argb8Color.Rgb(0x3A, 0x9B, 0xF0);
    public static readonly Argb8Color Moving = Argb8Color.Rgb(0x44, 0xC0, 0x66);
    public static readonly Argb8Color Path = Argb8Color.Rgb(0xFF, 0x7A, 0xC8);
    public static readonly Argb8Color Camera = Argb8Color.Rgb(0x9A, 0x9A, 0xA8);
    public static readonly Argb8Color Grid = Argb8Color.Rgb(0x80, 0x80, 0x90);
    public static readonly Argb8Color White = Argb8Color.Rgb(255, 255, 255);
    public static readonly Argb8Color Black = Argb8Color.Rgb(0, 0, 0);
}

public static class StandardLayers
{
    public static IReadOnlyList<IMapLayer> Create(SampleTrack<Vector3> marioPath) =>
    [
        new GameVisualsLayer(),
        new GridLayer(),
        new FloorsLayer(),
        new CeilingsLayer(),
        new WallsLayer(),
        new CellsLayer(),
        new ParallelUniversesLayer(),
        new CurrentUnitLayer(),
        new SectorLayer(),
        new FacingDividerLayer(),
        new PathLayer(marioPath),
        new PreviousPositionsLayer(),
        new NextPositionsLayer(),
        new IwerlipsesLayer(),
        new ObjectsLayer(),
        new HolpLayer(),
        new TrackersLayer(),
        new MapToolsLayer(),
        new GhostsLayer(),
        new CameraLayer(),
        new MarioLayer(),
    ];

    public static SampleTrack<Vector3> CreateMarioPathTrack(int capacity = 4096)
        => new("mario.path", f => f.Mario?.Position, capacity);
}

public sealed partial class GridLayer : MapLayer
{
    public override string Id => "grid";
    public override string Name => "Grid";

    public readonly Setting<int> Spacing;
    public readonly Setting<bool> ShowLevelBounds;
    public readonly Setting<bool> UnitGrid;

    public GridLayer()
    {
        Spacing = Declare(new Setting<int>("spacing", "Spacing (0 = auto)", 0) { Range = (0, 8192, 1) });
        ShowLevelBounds = Declare(new Setting<bool>("bounds", "Show level bounds (±8192)", true));
        UnitGrid = Declare(new Setting<bool>("units", "Unit gridlines when zoomed in", true,
            "One line per game unit from 4 px per unit, fading in: the grid collision code rounds positions to."));
    }

    private static double UnitGridAlpha(double scale) => scale < 4 ? 0 : Math.Min(1, (scale - 4) / 8);

    public override void Build(MapContext c)
    {
        var (min, max) = c.View.VisibleBounds();
        double spacing = c.Settings.Get(Spacing);
        if (spacing <= 0)
        {
            spacing = 1;
            while (spacing * c.View.Scale < 40)
                spacing *= 2;
        }
        bool bounds = c.Settings.Get(ShowLevelBounds);
        c.Scene.KeyBackground(Id);
        c.Scene.KeyBackground(spacing);
        c.Scene.KeyBackground(bounds);
        if ((max.X - min.X) / spacing > 600 || (max.Y - min.Y) / spacing > 600)
            return;

        var minor = MapPalette.Grid.WithAlpha(0.18);
        var axis = MapPalette.Grid.WithAlpha(0.55);
        for (double x = Math.Floor(min.X / spacing) * spacing; x <= max.X; x += spacing)
            c.Scene.AddBackground(new MapPolyline([new((float)x, min.Y), new((float)x, max.Y)], x == 0 ? axis : minor));
        for (double z = Math.Floor(min.Y / spacing) * spacing; z <= max.Y; z += spacing)
            c.Scene.AddBackground(new MapPolyline([new(min.X, (float)z), new(max.X, (float)z)], z == 0 ? axis : minor));

        if (bounds)
            c.Scene.AddBackground(new MapPolygon([new(-8192, -8192), new(8192, -8192), new(8192, 8192), new(-8192, 8192)], Argb8Color.Transparent, MapPalette.Grid.WithAlpha(0.7), 1.5f));

        double unitAlpha = c.Settings.Get(UnitGrid) ? UnitGridAlpha(c.View.Scale) : 0;
        if (unitAlpha > 0 && spacing > 1 && max.X - min.X < 1200 && max.Y - min.Y < 1200)
        {
            var unit = MapPalette.White.WithAlpha(0.45 * unitAlpha);
            for (double x = Math.Floor(min.X); x <= max.X; x++)
                c.Scene.Add(new MapPolyline([new((float)x, min.Y), new((float)x, max.Y)], unit));
            for (double z = Math.Floor(min.Y); z <= max.Y; z++)
                c.Scene.Add(new MapPolyline([new(min.X, (float)z), new(max.X, (float)z)], unit));
        }
    }
}

public abstract partial class HorizontalSurfaceLayer : MapLayer
{
    public readonly Setting<double> Opacity;
    public readonly Setting<int> HeightRange;
    public readonly Setting<bool> IncludeObjects;
    public readonly Setting<bool> Outline;
    public readonly Setting<bool> ShowUnits;

    public const double UnitsMinScale = 1.5;

    protected HorizontalSurfaceLayer(double defaultOpacity)
    {
        ShowUnits = Declare(new Setting<bool>("units", "Show triangle units when zoomed in", true,
            "The exact units the game's integer test puts in each triangle (find_floor), from 1.5 px per unit."));
        Opacity = Declare(new Setting<double>("opacity", "Opacity", defaultOpacity) { Range = (0, 1, 0.05) });
        HeightRange = Declare(new Setting<int>("heightRange", "Only within ±Y of Mario (0 = all)", 0) { Range = (0, 20000, 50) });
        IncludeObjects = Declare(new Setting<bool>("objects", "Include object triangles", true));
        Outline = Declare(new Setting<bool>("outline", "Outline", true));
    }

    protected abstract SurfaceClass Class { get; }
    protected abstract uint CurrentTriangle(MarioState mario);
    protected abstract Argb8Color ColorFor(Surface s, MarioState? mario);

    protected virtual object? ColorKey(MarioState? mario) => null;

    private object? _cachedVersion;
    private Surface[] _sortedStatic = [];

    private float DrawKey(Surface s) => Class == SurfaceClass.Floor ? s.MaxY : -s.MinY;

    public override void Build(MapContext c)
    {
        var mario = c.Frame.Mario;
        double opacity = c.Settings.Get(Opacity);
        int range = c.Settings.Get(HeightRange);
        bool objects = c.Settings.Get(IncludeObjects);
        bool outline = c.Settings.Get(Outline) && c.View.Scale > 0.02;
        bool filtered = range > 0 && mario is not null;
        var surfaces = c.Frame.Surfaces;

        c.Scene.KeyBackground(Id);
        c.Scene.KeyBackground(surfaces.StaticVersion);
        c.Scene.KeyBackground((opacity, range, outline));
        c.Scene.KeyBackground(filtered ? mario!.Position.Y : null);
        c.Scene.KeyBackground(ColorKey(mario));
        bool units = c.Settings.Get(ShowUnits) && c.View.Scale >= UnitsMinScale;
        c.Scene.KeyBackground(units);

        if (!ReferenceEquals(_cachedVersion, surfaces.StaticVersion))
        {
            _sortedStatic = surfaces.Static.Where(s => s.Class == Class).OrderBy(DrawKey).ToArray();
            _cachedVersion = surfaces.StaticVersion;
        }

        var (viewMin, viewMax) = c.View.VisibleBounds();
        bool Visible(Surface s)
        {
            if (filtered && (s.MaxY < mario!.Position.Y - range || s.MinY > mario.Position.Y + range))
                return false;
            var p = s.PointsXZ;
            var min = Vector2.Min(p[0], Vector2.Min(p[1], p[2]));
            var max = Vector2.Max(p[0], Vector2.Max(p[1], p[2]));
            return !(max.X < viewMin.X || min.X > viewMax.X || max.Y < viewMin.Y || min.Y > viewMax.Y);
        }
        MapPolygon Polygon(Surface s)
        {
            var color = ColorFor(s, mario);
            return new MapPolygon(s.PointsXZ, color.WithAlpha(opacity), outline ? color.Lerp(MapPalette.Black, 0.4).WithAlpha(Math.Min(1, opacity + 0.2)) : Argb8Color.Transparent) { Hit = SurfaceHit.Of(s) };
        }

        foreach (var s in _sortedStatic)
            if (Visible(s))
            {
                c.Scene.AddBackground(Polygon(s));
                if (units && UnitsShape(s, viewMin, viewMax) is { } shape)
                    c.Scene.AddBackground(new MapPolygon(shape, Argb8Color.Transparent, MapPalette.White.WithAlpha(0.55), 1));
            }

        if (objects)
        {
            List<Surface>? dynamic = null;
            foreach (var s in surfaces.Dynamic)
                if (s.Class == Class && Visible(s))
                    (dynamic ??= new List<Surface>()).Add(s);
            if (dynamic is not null)
            {
                dynamic.Sort((x, y) => DrawKey(x).CompareTo(DrawKey(y)));
                foreach (var s in dynamic)
                    c.Scene.Add(Polygon(s));
            }
        }

        if (mario is not null && surfaces.Find(CurrentTriangle(mario)) is { } h && h.Class == Class && (objects || h.IsStatic))
        {
            var color = ColorFor(h, mario).Lerp(MapPalette.Highlight, 0.35);
            c.Scene.Add(new MapPolygon(h.PointsXZ, color.WithAlpha(Math.Min(1, opacity + 0.25)), MapPalette.Highlight, 2.5f) { Hit = SurfaceHit.Of(h) });
            if (units && UnitsShape(h, viewMin, viewMax) is { } shape)
                c.Scene.Add(new MapPolygon(shape, MapPalette.Highlight.WithAlpha(0.18), MapPalette.Highlight, 1.5f));
        }
    }

    public static Vector2[]? UnitsShape(Surface s, Vector2 viewMin, Vector2 viewMax)
    {
        var rows = GameCollision.UnitRows(s, (int)Math.Floor(viewMin.Y) - 1, (int)Math.Ceiling(viewMax.Y) + 1).ToList();
        if (rows.Count == 0)
            return null;
        var left = new List<Vector2>(rows.Count * 2);
        var right = new List<Vector2>(rows.Count * 2);
        foreach (var (z, xMin, xMax) in rows)
        {
            var (z0, z1) = GameCollision.UnitSpan(z);
            float x0 = GameCollision.UnitSpan(xMin).Min, x1 = GameCollision.UnitSpan(xMax).Max;
            left.Add(new Vector2(x0, z0));
            left.Add(new Vector2(x0, z1));
            right.Add(new Vector2(x1, z0));
            right.Add(new Vector2(x1, z1));
        }
        right.Reverse();
        left.AddRange(right);
        return left.ToArray();
    }
}

public sealed partial class FloorsLayer : HorizontalSurfaceLayer
{
    public readonly Setting<bool> HeightShading;

    public FloorsLayer() : base(0.55)
    {
        HeightShading = Declare(new Setting<bool>("heightShading", "Shade by height relative to Mario", true));
    }

    public override string Id => "floors";
    public override string Name => "Floors";
    protected override SurfaceClass Class => SurfaceClass.Floor;
    protected override uint CurrentTriangle(MarioState mario) => mario.FloorTri;

    private bool _shading;

    public override void Build(MapContext c)
    {
        _shading = c.Settings.Get(HeightShading);
        base.Build(c);
    }

    protected override object? ColorKey(MarioState? mario) => _shading ? mario?.Position.Y : null;

    protected override Argb8Color ColorFor(Surface s, MarioState? mario)
    {
        var color = s.IsStatic ? MapPalette.Floor : MapPalette.ObjectFloor;
        if (!_shading || mario is null)
            return color;
        double t = Math.Clamp((s.MaxY - mario.Position.Y) / 1500.0, -1, 1);
        return t >= 0 ? color.Lerp(MapPalette.White, 0.55 * t) : color.Lerp(MapPalette.Black, 0.55 * -t);
    }
}

public sealed partial class CeilingsLayer : HorizontalSurfaceLayer
{
    public CeilingsLayer() : base(0.35) { }

    public override string Id => "ceilings";
    public override string Name => "Ceilings";
    public override bool VisibleByDefault => false;
    protected override SurfaceClass Class => SurfaceClass.Ceiling;
    protected override uint CurrentTriangle(MarioState mario) => mario.CeilingTri;
    protected override Argb8Color ColorFor(Surface s, MarioState? mario) => MapPalette.Ceiling;
}

public sealed partial class WallsLayer : MapLayer
{
    public override string Id => "walls";
    public override string Name => "Walls";

    public readonly Setting<double> Opacity;
    public readonly Setting<bool> ShowHitbox;
    public readonly Setting<double> HitboxRadius;
    public readonly Setting<bool> AtMarioHeightOnly;
    public readonly Setting<bool> IncludeObjects;
    public readonly Setting<WallSlice> Slice;
    public readonly Setting<int> SliceHeight;
    public readonly Setting<bool> PushArrows;

    public WallsLayer()
    {
        Slice = Declare(new Setting<WallSlice>("slice", "Draw walls", WallSlice.WholeWall,
            "Whole wall: its longest edge from above. At Mario's height / At a height: the wall cut at that Y, where it actually is."));
        SliceHeight = Declare(new Setting<int>("sliceHeight", "Cut height (above Mario, or absolute)", 30) { Range = (-20000, 20000, 10) });
        PushArrows = Declare(new Setting<bool>("arrows", "Push direction arrows", false, "Which way each wall pushes Mario (along its projection axis)."));
        Opacity = Declare(new Setting<double>("opacity", "Hitbox opacity", 0.3) { Range = (0, 1, 0.05) });
        ShowHitbox = Declare(new Setting<bool>("hitbox", "Show wall hitbox band", true));
        HitboxRadius = Declare(new Setting<double>("radius", "Hitbox radius", MarioState.WallHitboxRadius) { Range = (0, 500, 1) });
        AtMarioHeightOnly = Declare(new Setting<bool>("marioHeight", "Only walls at Mario's height", true,
            "Hides walls the game's wall checks cannot hit: 30 and 60 units above Mario's feet on the ground, 30 and 150 in the air (±5)."));
        IncludeObjects = Declare(new Setting<bool>("objects", "Include object walls", true));
    }

    public static (Vector2 A, Vector2 B) Segment(Surface s)
    {
        Vector2 a = Xz.Of(s.V1), b = Xz.Of(s.V2), c = Xz.Of(s.V3);
        float ab = Vector2.DistanceSquared(a, b), bc = Vector2.DistanceSquared(b, c), ca = Vector2.DistanceSquared(c, a);
        return ab >= bc && ab >= ca ? (a, b) : bc >= ca ? (b, c) : (c, a);
    }

    public static Vector2[]? HitboxBand(Surface s, float radius)
    {
        float axisNormal = s.ProjectsOnX ? s.Normal.X : s.Normal.Z;
        if (MathF.Abs(axisNormal) < 1e-4f)
            return null;
        var (a, b) = Segment(s);
        float d = radius / MathF.Abs(axisNormal);
        var offset = s.ProjectsOnX ? new Vector2(d, 0) : new Vector2(0, d);
        return [a + offset, b + offset, b - offset, a - offset];
    }

    private sealed record WallShape(float Radius, Vector2[] Segment, Vector2[]? Band);

    private readonly System.Runtime.CompilerServices.ConditionalWeakTable<Surface, WallShape> _shapes = new();

    private WallShape ShapeOf(Surface s, float radius)
    {
        if (_shapes.TryGetValue(s, out var shape) && shape.Radius == radius)
            return shape;
        var (a, b) = Segment(s);
        shape = new WallShape(radius, [a, b], HitboxBand(s, radius));
        _shapes.AddOrUpdate(s, shape);
        return shape;
    }

    public static float[] CheckOffsets(MarioState mario) => mario.Position.Y == mario.FloorY ? [30, 60] : [30, 150];

    public static bool AtCheckHeight(Surface s, MarioState mario)
    {
        foreach (float offset in CheckOffsets(mario))
        {
            float y = mario.Position.Y + offset;
            if (s.MinY - 5 <= y && y <= s.MaxY + 5)
                return true;
        }
        return false;
    }

    public static (Vector2 A, Vector2 B)? SliceAt(Surface s, float height)
    {
        Span<Vector2> points = stackalloc Vector2[3];
        int n = 0;
        Vector3[] v = [s.V1, s.V2, s.V3];
        for (int i = 0; i < 3; i++)
        {
            Vector3 p = v[i], q = v[(i + 1) % 3];
            if ((p.Y - height) * (q.Y - height) > 0 || p.Y == q.Y)
                continue;
            float t = (height - p.Y) / (q.Y - p.Y);
            if (n < 3)
                points[n++] = new Vector2(p.X + (q.X - p.X) * t, p.Z + (q.Z - p.Z) * t);
        }
        if (n < 2)
            return null;
        (Vector2, Vector2) best = (points[0], points[1]);
        float bestD = Vector2.DistanceSquared(points[0], points[1]);
        for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
                if (Vector2.DistanceSquared(points[i], points[j]) > bestD)
                {
                    bestD = Vector2.DistanceSquared(points[i], points[j]);
                    best = (points[i], points[j]);
                }
        return best;
    }

    public static Vector2[]? BandAround(Surface s, Vector2 a, Vector2 b, float radius)
    {
        float axisNormal = s.ProjectsOnX ? s.Normal.X : s.Normal.Z;
        if (MathF.Abs(axisNormal) < 1e-4f)
            return null;
        float d = radius / MathF.Abs(axisNormal);
        var offset = s.ProjectsOnX ? new Vector2(d, 0) : new Vector2(0, d);
        return [a + offset, b + offset, b - offset, a - offset];
    }

    public static ushort PushYaw(Surface s)
        => s.ProjectsOnX ? (ushort)(s.Normal.X >= 0 ? 0x4000 : 0xC000) : (ushort)(s.Normal.Z >= 0 ? 0x0000 : 0x8000);

    public override void Build(MapContext c)
    {
        var mario = c.Frame.Mario;
        var slice = c.Settings.Get(Slice);
        float? sliceY = slice switch
        {
            WallSlice.AtMarioHeight when mario is not null => mario.Position.Y + c.Settings.Get(SliceHeight),
            WallSlice.AtHeight => c.Settings.Get(SliceHeight),
            _ => null,
        };
        bool arrows = c.Settings.Get(PushArrows) && c.View.Scale > 0.05;
        bool band = c.Settings.Get(ShowHitbox);
        float radius = (float)c.Settings.Get(HitboxRadius);
        double opacity = c.Settings.Get(Opacity);
        bool heightOnly = c.Settings.Get(AtMarioHeightOnly) && mario is not null;
        bool objects = c.Settings.Get(IncludeObjects);
        var (viewMin, viewMax) = c.View.VisibleBounds();
        float reach = radius * 4;

        var surfaces = c.Frame.Surfaces;
        c.Scene.KeyBackground(Id);
        c.Scene.KeyBackground(surfaces.StaticVersion);
        c.Scene.KeyBackground((band, radius, opacity));
        c.Scene.KeyBackground(heightOnly ? (mario!.Position.Y, mario.Position.Y == mario.FloorY) : null);
        c.Scene.KeyBackground((sliceY, arrows));

        for (int pass = 0; pass < (objects ? 2 : 1); pass++)
        {
            bool level = pass == 0;
            var list = level ? surfaces.Static : surfaces.Dynamic;
            for (int i = 0; i < list.Count; i++)
            {
                var s = list[i];
                if (s.Class != SurfaceClass.Wall)
                    continue;
                if (heightOnly && !AtCheckHeight(s, mario!))
                    continue;
                var shape = ShapeOf(s, radius);
                if (sliceY is float y)
                {
                    if (SliceAt(s, y) is not var (sa, sb))
                        continue;
                    shape = new WallShape(radius, [sa, sb], BandAround(s, sa, sb, radius));
                }
                Vector2 a = shape.Segment[0], b = shape.Segment[1];
                if (MathF.Max(a.X, b.X) + reach < viewMin.X || MathF.Min(a.X, b.X) - reach > viewMax.X || MathF.Max(a.Y, b.Y) + reach < viewMin.Y || MathF.Min(a.Y, b.Y) - reach > viewMax.Y)
                    continue;

                bool current = mario is not null && s.Address == mario.WallTri;
                var color = s.IsStatic ? MapPalette.Wall : MapPalette.ObjectWall;
                var hit = SurfaceHit.Of(s);
                Action<MapPrimitive> add = level ? c.Scene.AddBackground : c.Scene.Add;
                if (band && shape.Band is { } quad)
                    add(new MapPolygon(quad, color.WithAlpha(opacity), color.WithAlpha(Math.Min(1, opacity + 0.3))) { Hit = hit });
                add(new MapPolyline(shape.Segment, color, 2.5f) { Hit = hit });
                if (arrows)
                    add(new MapArrow((a + b) / 2, PushYaw(s), 14, color.Lerp(MapPalette.White, 0.3), 2));
                if (current)
                    c.Scene.Add(new MapPolyline(shape.Segment, MapPalette.Highlight, 3.5f) { Hit = hit });
            }
        }
    }
}

public sealed partial class ObjectsLayer : MapLayer
{
    public override string Id => "objects";
    public override string Name => "Objects";

    public readonly Setting<bool> ShowHitboxes;
    public readonly Setting<bool> ShowHurtboxes;
    public readonly Setting<bool> ShowFacing;
    public readonly Setting<bool> ShowNames;
    public readonly Setting<bool> ShowEffectiveHitboxes;
    public readonly Setting<bool> ShowHome;
    public readonly Setting<bool> ShowMovingYaw;
    public readonly Setting<bool> ShowAngleToMario;
    public readonly Setting<bool> SelectedOnly;

    public ObjectsLayer()
    {
        ShowHitboxes = Declare(new Setting<bool>("hitbox", "Hitbox cylinders", true));
        ShowHurtboxes = Declare(new Setting<bool>("hurtbox", "Hurtbox cylinders", false));
        ShowEffectiveHitboxes = Declare(new Setting<bool>("effectiveHitbox", "Effective hitboxes (+ Mario's radius)", false,
            "Where Mario's centre must be to touch the object: hitbox radius + Mario's 37."));
        ShowFacing = Declare(new Setting<bool>("facing", "Facing direction", true));
        ShowMovingYaw = Declare(new Setting<bool>("moving", "Moving yaw (green)", false));
        ShowAngleToMario = Declare(new Setting<bool>("toMario", "Angle to Mario (yellow)", false));
        ShowHome = Declare(new Setting<bool>("home", "Home position and line", false));
        ShowNames = Declare(new Setting<bool>("names", "Names (selected objects always show theirs)", false));
        SelectedOnly = Declare(new Setting<bool>("selectedOnly", "Only selected objects", false, "Select objects by clicking them (Ctrl/⌘ adds)."));
    }

    public override void Build(MapContext c)
    {
        uint marioObject = c.Frame.Mario?.MarioObject ?? 0;
        bool hitboxes = c.Settings.Get(ShowHitboxes), hurtboxes = c.Settings.Get(ShowHurtboxes);
        bool facing = c.Settings.Get(ShowFacing), names = c.Settings.Get(ShowNames);
        bool effective = c.Settings.Get(ShowEffectiveHitboxes), home = c.Settings.Get(ShowHome);
        bool moving = c.Settings.Get(ShowMovingYaw), toMario = c.Settings.Get(ShowAngleToMario), selectedOnly = c.Settings.Get(SelectedOnly);
        var marioPosition = c.Frame.Mario?.Position;

        var (viewMin, viewMax) = c.View.VisibleBounds();
        float arrowReach = (float)(30 / c.View.Scale);
        foreach (var o in c.Frame.Objects.All)
        {
            if (o.Address == marioObject)
                continue;
            var p = Xz.Of(o.Position);
            float reach = MathF.Max(MathF.Max(o.HitboxRadius, o.HurtboxRadius), 0) + arrowReach;
            if (p.X + reach < viewMin.X || p.X - reach > viewMax.X || p.Y + reach < viewMin.Y || p.Y - reach > viewMax.Y)
                continue;
            bool selected = c.Selection.IsSelected(o.Address);
            if (selectedOnly && !selected)
                continue;
            var stroke = selected ? MapPalette.Highlight : MapPalette.Object;
            var hit = new ObjectHit(o);

            if (home && o.Home != o.Position)
            {
                var h = Xz.Of(o.Home);
                c.Scene.Add(new MapPolyline([h, p], MapPalette.Object.WithAlpha(0.6), 1));
                c.Scene.Add(new MapMarker(h, 6, MarkerShape.Diamond, Argb8Color.Transparent, MapPalette.Object));
            }
            if (effective && o.HitboxRadius > 0)
                c.Scene.Add(new MapCircle(p, o.HitboxRadius + MarioState.HitboxRadius, Argb8Color.Transparent, stroke.WithAlpha(0.7), 1));
            if (hurtboxes && o.HurtboxRadius > 0)
                c.Scene.Add(new MapCircle(p, o.HurtboxRadius, Argb8Color.Transparent, MapPalette.Hurtbox.WithAlpha(0.8)));
            if (hitboxes && o.HitboxRadius > 0)
                c.Scene.Add(new MapCircle(p, o.HitboxRadius, MapPalette.Object.WithAlpha(selected ? 0.35 : 0.18), stroke, selected ? 3 : 1.5f) { Hit = hit });
            if (moving)
                c.Scene.Add(new MapArrow(p, o.MovingYaw, 22, MapPalette.Moving, 2));
            if (toMario && marioPosition is { } mp)
                c.Scene.Add(new MapArrow(p, Angle.Towards(o.Position.X, o.Position.Z, mp.X, mp.Z), 22, MapPalette.Intended, 1.5f));
            if (facing)
                c.Scene.Add(new MapArrow(p, o.FacingYaw, 18, stroke, 2));
            c.Scene.Add(new MapMarker(p, selected ? 10 : 7, MarkerShape.Square, MapPalette.Object, selected ? MapPalette.Highlight : MapPalette.Black) { Hit = hit });
            if (names || selected)
                c.Scene.Add(new MapLabel(p, $"{o.SlotNumber}: {o.Name}", selected ? MapPalette.Highlight : MapPalette.White));
        }
    }
}

public sealed partial class MarioLayer : MapLayer
{
    public override string Id => "mario";
    public override string Name => "Mario";

    public readonly Setting<bool> ShowHitbox;
    public readonly Setting<bool> ShowWallRadius;
    public readonly Setting<bool> ShowFacing;
    public readonly Setting<bool> ShowIntended;
    public readonly Setting<bool> ShowSliding;
    public readonly Setting<bool> ShowMoving;
    public readonly Setting<bool> ShowTwirl;
    public readonly Setting<bool> SpeedLength;

    public MarioLayer()
    {
        ShowTwirl = Declare(new Setting<bool>("twirl", "Twirl yaw (purple)", false));
        SpeedLength = Declare(new Setting<bool>("speedLength", "Arrow length = distance per frame", false,
            "Facing, moving and sliding arrows show how far Mario goes this frame (H speed, sliding speed) instead of a fixed size."));
        ShowHitbox = Declare(new Setting<bool>("hitbox", "Hitbox (r = 37)", true));
        ShowWallRadius = Declare(new Setting<bool>("wallRadius", "Wall collision radius (r = 50)", false));
        ShowFacing = Declare(new Setting<bool>("facing", "Facing yaw (red)", true));
        ShowIntended = Declare(new Setting<bool>("intended", "Intended yaw, scaled by stick magnitude (yellow)", true));
        ShowSliding = Declare(new Setting<bool>("sliding", "Sliding velocity yaw (blue)", false));
        ShowMoving = Declare(new Setting<bool>("moving", "Moving (slide) yaw (green)", false));
        Declare(ShowVelocity3D);
    }

    public override void Build(MapContext c)
    {
        if (c.Frame.Mario is not { } m)
            return;
        var p = Xz.Of(m.Position);
        var hit = new MarioHit(m, c.Frame.Services.Actions.NameOf(m.Action));

        if (c.Settings.Get(ShowWallRadius))
            c.Scene.Add(new MapCircle(p, MarioState.WallHitboxRadius, Argb8Color.Transparent, MapPalette.Mario.WithAlpha(0.6)));
        if (c.Settings.Get(ShowHitbox))
            c.Scene.Add(new MapCircle(p, MarioState.HitboxRadius, MapPalette.Mario.WithAlpha(0.2), MapPalette.Mario, 1.5f) { Hit = hit });
        bool speedLength = c.Settings.Get(SpeedLength);
        float Length(float fixedPx, float worldUnits) => speedLength ? MathF.Max(8, MathF.Abs(worldUnits) * (float)c.View.Scale) : fixedPx;
        float deFacto = Variables.MarioVariables.DeFactoSpeed(c.Frame) ?? m.HSpeed;
        if (c.Settings.Get(ShowTwirl))
            c.Scene.Add(new MapArrow(p, m.TwirlYaw, 30, MapPalette.Hurtbox));
        if (c.Settings.Get(ShowMoving))
            c.Scene.Add(new MapArrow(p, m.SlideYaw, Length(36, deFacto), MapPalette.Moving));
        if (c.Settings.Get(ShowSliding) && m.SlidingSpeed > 0)
            c.Scene.Add(new MapArrow(p, m.SlidingYaw, Length(36, m.SlidingSpeed), MapPalette.Sliding));
        if (c.Settings.Get(ShowIntended) && m.IntendedMag > 0)
            c.Scene.Add(new MapArrow(p, m.IntendedYaw, Math.Max(10, 44 * m.IntendedMag / 32f), MapPalette.Intended));
        if (c.Settings.Get(ShowFacing))
            c.Scene.Add(new MapArrow(p, m.FacingYaw, Length(44, deFacto), MapPalette.Mario, 2.5f));
        c.Scene.Add(new MapMarker(p, 16, MarkerShape.Triangle, MapPalette.Mario, MapPalette.White, m.FacingYaw) { Hit = hit });
    }
}

public sealed partial class HolpLayer : MapLayer
{
    public override string Id => "holp";
    public override string Name => "HOLP";

    public static readonly Argb8Color HolpColor = Argb8Color.Rgb(0x3D, 0xD6, 0xF5);

    public override void Build(MapContext c)
    {
        if (c.Frame.Mario is not { } m || Variables.MarioVariables.HolpPosition(c.Frame) is not { } h)
            return;
        var p = Xz.Of(h);
        c.Scene.Add(new MapPolyline([Xz.Of(m.Position), p], HolpColor.WithAlpha(0.45), 1));
        c.Scene.Add(new MapMarker(p, 11, MarkerShape.Diamond, HolpColor, MapPalette.Black) { Hit = new PointHit("HOLP", h) });
    }
}

public sealed partial class PathLayer : MapLayer
{
    private readonly SampleTrack<Vector3> _track;

    public override string Id => "path";
    public override string Name => "Mario path";

    public readonly Setting<int> Frames;
    public readonly Setting<bool> ShowPoints;

    public PathLayer(SampleTrack<Vector3> track)
    {
        _track = track;
        Frames = Declare(new Setting<int>("frames", "Frames of history", 150) { Range = (2, 4096, 10) });
        ShowPoints = Declare(new Setting<bool>("points", "One dot per frame", true));
    }

    public override void Build(MapContext c)
    {
        var samples = _track.Snapshot();
        int count = Math.Min(samples.Length, Math.Max(2, c.Settings.Get(Frames)));
        if (count < 2)
            return;
        var recent = samples.AsSpan(samples.Length - count);
        var points = new Vector2[count];
        for (int i = 0; i < count; i++)
            points[i] = Xz.Of(recent[i].Value);
        c.Scene.Add(new MapPolyline(points, MapPalette.Path.WithAlpha(0.8), 1.5f));

        if (!c.Settings.Get(ShowPoints) || c.View.Scale < 0.03)
            return;
        var (viewMin, viewMax) = c.View.VisibleBounds();
        for (int i = 0; i < count; i++)
        {
            var at = points[i];
            if (at.X < viewMin.X || at.X > viewMax.X || at.Y < viewMin.Y || at.Y > viewMax.Y)
                continue;
            float? moved = i == 0 ? null : Vector2.Distance(points[i], points[i - 1]);
            double age = (double)(count - 1 - i) / count;
            c.Scene.Add(new MapMarker(points[i], 5, MarkerShape.Circle, MapPalette.Path.WithAlpha(1 - age * 0.7), Argb8Color.Transparent)
            {
                Hit = new PathSampleHit(recent[i].GlobalTimer, recent[i].Value, moved),
            });
        }
    }
}

public sealed partial class CameraLayer : MapLayer
{
    public override string Id => "camera";
    public override string Name => "Camera";

    public override void Build(MapContext c)
    {
        if (c.Frame.Camera is not { } cam)
            return;
        var p = Xz.Of(cam.Position);
        double yaw = Angle.Towards(cam.Position.X, cam.Position.Z, cam.Focus.X, cam.Focus.Z);
        c.Scene.Add(new MapArrow(p, yaw, 26, MapPalette.Camera, 2));
        c.Scene.Add(new MapMarker(p, 12, MarkerShape.Diamond, MapPalette.Camera, MapPalette.Black) { Hit = new CameraHit(cam) });
    }
}

public enum MapOrientation { North, Camera, MarioFacing, Custom }

public enum WallSlice { WholeWall, AtMarioHeight, AtHeight }

public static class MapViewSettings
{
    public static readonly Setting<bool> FollowMario = new("follow", "Follow Mario", true);
    public static readonly Setting<MapOrientation> Orientation = new("orientation", "Orientation", MapOrientation.North);

    public static readonly Setting<int> CustomYaw = new("customYaw", "Yaw pointing up", 32768) { Range = (0, 65535, 1024) };

    public static IReadOnlyList<Setting> All => [FollowMario, Orientation, CustomYaw];

    public static void Apply(MapView view, GameFrame frame, bool follow, MapOrientation orientation, int customYaw = 32768)
    {
        if (follow && frame.Mario is { } m)
            view.Center = Xz.Of(m.Position);
        switch (orientation)
        {
            case MapOrientation.North:
                view.Rotation = 0;
                break;
            case MapOrientation.MarioFacing when frame.Mario is { } mario:
                view.PointYawUp(mario.FacingYaw);
                break;
            case MapOrientation.Camera when frame.Camera is { } cam:
                view.PointYawUp(Angle.Towards(cam.Position.X, cam.Position.Z, cam.Focus.X, cam.Focus.Z));
                break;
            case MapOrientation.Custom:
                view.PointYawUp(customYaw);
                break;
        }
    }
}
