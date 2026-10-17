using System.Globalization;
using System.Numerics;
using System.Text.Json;
using StroopNext.Game;
using StroopNext.Map3D;
using StroopNext.Session;
using StroopNext.Settings;

namespace StroopNext.Map;

public enum TrackerTarget { Mario, Holp, Camera, SelectedObjects, ObjectsNamed, Object, Point }

public enum TrackerKind
{
    Marker, Circle, Cylinder, LineToMario, Path, CurrentUnit,
    FacingArrow, MovingArrow, IntendedArrow, SlidingArrow, TwirlArrow, AngleToMarioArrow,
    FacingDivider, Sector, AngleRange,
    Hitbox, EffectiveHitbox, Hurtbox, EffectiveHurtbox, HomeLine,
    NearbyFloorUnits, NearbyCeilingUnits,
}

public enum TrackerOrder { Normal, OnBottom, OnTop }

public sealed record TrackerSpec(
    Guid Id,
    TrackerKind Kind,
    TrackerTarget Target,
    uint Address = 0,
    string? Name = null,
    float X = 0, float Y = 0, float Z = 0, int Yaw = 0,
    uint Color = 0xFFFFD42A,
    double Opacity = 1,
    double Size = 0,
    double Size2 = 0,
    bool Visible = true,
    TrackerOrder Order = TrackerOrder.Normal)
{
    public static TrackerSpec Create(TrackerKind kind, TrackerTarget target, uint address = 0, string? name = null, Vector3 point = default, int yaw = 0)
        => new(Guid.NewGuid(), kind, target, address, name, point.X, point.Y, point.Z, yaw, DefaultColor(kind), 1, DefaultSize(kind), DefaultSize2(kind));

    public static string? SizeLabel(TrackerKind kind) => kind switch
    {
        TrackerKind.Marker => "Size (px)",
        TrackerKind.Circle or TrackerKind.Cylinder => "Radius",
        TrackerKind.Path => "Frames",
        TrackerKind.FacingDivider => "Half length",
        TrackerKind.Sector => "Half angle",
        TrackerKind.AngleRange => "Spoke step",
        TrackerKind.FacingArrow or TrackerKind.MovingArrow or TrackerKind.SlidingArrow or TrackerKind.AngleToMarioArrow => "Length (0 = speed)",
        TrackerKind.IntendedArrow or TrackerKind.TwirlArrow => "Length",
        TrackerKind.NearbyFloorUnits or TrackerKind.NearbyCeilingUnits => "Grid radius (units)",
        _ => null,
    };

    public static double DefaultSize(TrackerKind kind) => kind switch
    {
        TrackerKind.Marker => 10,
        TrackerKind.Circle or TrackerKind.Cylinder => 100,
        TrackerKind.Path => 150,
        TrackerKind.FacingDivider => 1000,
        TrackerKind.Sector => 4096,
        TrackerKind.AngleRange => 4096,
        TrackerKind.IntendedArrow or TrackerKind.TwirlArrow => 300,
        TrackerKind.NearbyFloorUnits or TrackerKind.NearbyCeilingUnits => 5,
        _ => 0,
    };

    public static double DefaultSize2(TrackerKind kind) => kind switch
    {
        TrackerKind.Cylinder => 160,
        TrackerKind.Sector => 1000,
        TrackerKind.AngleRange => 600,
        _ => 0,
    };

    public static uint DefaultColor(TrackerKind kind) => kind switch
    {
        TrackerKind.Hitbox or TrackerKind.EffectiveHitbox => 0xFFF08C2E,
        TrackerKind.Hurtbox or TrackerKind.EffectiveHurtbox => 0xFFC04CE0,
        TrackerKind.FacingArrow => 0xFFE53333,
        TrackerKind.MovingArrow => 0xFF44C066,
        TrackerKind.IntendedArrow => 0xFFF5D020,
        TrackerKind.SlidingArrow => 0xFF3A9BF0,
        TrackerKind.TwirlArrow => 0xFFC04CE0,
        TrackerKind.Path => 0xFFFF7AC8,
        TrackerKind.FacingDivider or TrackerKind.Sector => 0xFFE5534B,
        TrackerKind.CurrentUnit => 0xFFB070F0,
        TrackerKind.NearbyFloorUnits => 0xFF4F86E0,
        TrackerKind.NearbyCeilingUnits => 0xFFE04A4A,
        _ => 0xFFFFD42A,
    };

    public static IReadOnlyList<TrackerKind> KindsFor(TrackerTarget target) => target switch
    {
        TrackerTarget.Mario =>
        [
            TrackerKind.FacingArrow, TrackerKind.MovingArrow, TrackerKind.IntendedArrow, TrackerKind.SlidingArrow, TrackerKind.TwirlArrow,
            TrackerKind.Path, TrackerKind.FacingDivider, TrackerKind.Sector, TrackerKind.AngleRange, TrackerKind.CurrentUnit,
            TrackerKind.NearbyFloorUnits, TrackerKind.NearbyCeilingUnits,
            TrackerKind.Hitbox, TrackerKind.Circle, TrackerKind.Cylinder, TrackerKind.Marker,
        ],
        TrackerTarget.Holp or TrackerTarget.Point =>
            [TrackerKind.Marker, TrackerKind.Circle, TrackerKind.Cylinder, TrackerKind.LineToMario, TrackerKind.CurrentUnit, TrackerKind.NearbyFloorUnits, TrackerKind.Path],
        TrackerTarget.Camera =>
            [TrackerKind.Marker, TrackerKind.FacingArrow, TrackerKind.LineToMario, TrackerKind.Circle, TrackerKind.Path],
        _ =>
        [
            TrackerKind.Hitbox, TrackerKind.EffectiveHitbox, TrackerKind.Hurtbox, TrackerKind.EffectiveHurtbox, TrackerKind.HomeLine,
            TrackerKind.FacingArrow, TrackerKind.MovingArrow, TrackerKind.AngleToMarioArrow, TrackerKind.LineToMario, TrackerKind.Path,
            TrackerKind.FacingDivider, TrackerKind.Sector, TrackerKind.CurrentUnit, TrackerKind.NearbyFloorUnits,
            TrackerKind.Circle, TrackerKind.Cylinder, TrackerKind.Marker,
        ],
    };

    public static string KindName(TrackerKind kind) => kind switch
    {
        TrackerKind.LineToMario => "Line to Mario",
        TrackerKind.CurrentUnit => "Current unit",
        TrackerKind.FacingArrow => "Facing arrow",
        TrackerKind.MovingArrow => "Moving arrow",
        TrackerKind.IntendedArrow => "Intended arrow",
        TrackerKind.SlidingArrow => "Sliding arrow",
        TrackerKind.TwirlArrow => "Twirl arrow",
        TrackerKind.AngleToMarioArrow => "Angle to Mario",
        TrackerKind.FacingDivider => "Facing divider",
        TrackerKind.AngleRange => "Angle range",
        TrackerKind.EffectiveHitbox => "Effective hitbox",
        TrackerKind.EffectiveHurtbox => "Effective hurtbox",
        TrackerKind.HomeLine => "Home line",
        TrackerKind.NearbyFloorUnits => "Nearby floor units",
        TrackerKind.NearbyCeilingUnits => "Nearby ceiling units",
        _ => kind.ToString(),
    };

    public string TargetName => Target switch
    {
        TrackerTarget.SelectedObjects => "Selected objects",
        TrackerTarget.ObjectsNamed => $"All \"{Name}\"",
        TrackerTarget.Object => $"{Name ?? "Object"} (0x{Address:X8})",
        TrackerTarget.Point => Name ?? string.Create(CultureInfo.InvariantCulture, $"Point ({X:0}, {Y:0}, {Z:0})"),
        TrackerTarget.Holp => "HOLP",
        _ => Target.ToString(),
    };

    public string Title => $"{TargetName} · {KindName(Kind)}";

    public MapColor MapColor => MapColor.FromHex(Color).WithAlpha(Opacity * ((Color >> 24) / 255.0));
}

public readonly record struct TrackedPoint(Vector3 Position, ushort Yaw, ushort MovingYaw, float Speed, GameObject? Object, string Label);

public static class TrackerTargets
{
    public static IEnumerable<TrackedPoint> Resolve(TrackerSpec spec, GameFrame frame, Selection selection)
    {
        switch (spec.Target)
        {
            case TrackerTarget.Mario when frame.Mario is { } m:
                yield return new TrackedPoint(m.Position, m.FacingYaw, m.SlideYaw, Variables.MarioVariables.DeFactoSpeed(frame) ?? m.HSpeed, null, "Mario");
                break;
            case TrackerTarget.Holp when Variables.MarioVariables.HolpPosition(frame) is { } h:
                yield return new TrackedPoint(h, 0, 0, 0, null, "HOLP");
                break;
            case TrackerTarget.Camera when frame.Camera is { } cam:
                ushort yaw = Angle.Towards(cam.Position.X, cam.Position.Z, cam.Focus.X, cam.Focus.Z);
                yield return new TrackedPoint(cam.Position, yaw, yaw, 0, null, "Camera");
                break;
            case TrackerTarget.Point:
                yield return new TrackedPoint(new Vector3(spec.X, spec.Y, spec.Z), (ushort)spec.Yaw, (ushort)spec.Yaw, 0, null, spec.TargetName);
                break;
            case TrackerTarget.Object when frame.Objects.Find(spec.Address) is { } o:
                yield return Of(o);
                break;
            case TrackerTarget.SelectedObjects:
                foreach (var o in frame.Objects.All)
                    if (selection.IsSelected(o.Address))
                        yield return Of(o);
                break;
            case TrackerTarget.ObjectsNamed:
                foreach (var o in frame.Objects.All)
                    if (string.Equals(o.Name, spec.Name, StringComparison.OrdinalIgnoreCase))
                        yield return Of(o);
                break;
        }
    }

    private static TrackedPoint Of(GameObject o) => new(o.Position, o.FacingYaw, o.MovingYaw, o.HSpeed, o, $"{o.Name} (slot {o.SlotNumber})");
}

public sealed class TrackersLayer : MapLayer, IMapLayer3D
{
    public static readonly Setting<TrackerSpec[]> ListSetting = new("list", "Trackers", []);

    private TrackerSpec[]? _trackers;
    private readonly Dictionary<(Guid, uint), PathHistory> _paths = new();

    public override string Id => "trackers";
    public override string Name => "Trackers";

    public event Action? Changed;

    public IReadOnlyList<TrackerSpec> Trackers(SettingsScope scope) => _trackers ??= scope.Get(ListSetting) ?? [];

    public void Update(SettingsScope scope, Func<IReadOnlyList<TrackerSpec>, IEnumerable<TrackerSpec>> change)
    {
        _trackers = change(Trackers(scope)).ToArray();
        scope.Set(ListSetting, _trackers);
        Changed?.Invoke();
    }

    public void Add(SettingsScope scope, TrackerSpec spec) => Update(scope, list => list.Append(spec));
    public void Remove(SettingsScope scope, Guid id) => Update(scope, list => list.Where(t => t.Id != id));
    public void Replace(SettingsScope scope, TrackerSpec spec) => Update(scope, list => list.Select(t => t.Id == spec.Id ? spec : t));

    public void Move(SettingsScope scope, Guid id, int delta) => Update(scope, list =>
    {
        var items = list.ToList();
        int i = items.FindIndex(t => t.Id == id);
        int j = Math.Clamp(i + delta, 0, items.Count - 1);
        if (i < 0 || i == j)
            return items;
        var item = items[i];
        items.RemoveAt(i);
        items.Insert(j, item);
        return items;
    });

    public static readonly Setting<Dictionary<string, TrackerSpec[]>> PresetsSetting = new("presets", "Tracker presets", new());

    public IReadOnlyList<string> PresetNames(SettingsScope scope)
        => (scope.Get(PresetsSetting) ?? new()).Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList();

    public void SavePreset(SettingsScope scope, string name)
    {
        var presets = new Dictionary<string, TrackerSpec[]>(scope.Get(PresetsSetting) ?? new()) { [name] = Trackers(scope).ToArray() };
        scope.Set(PresetsSetting, presets);
        Changed?.Invoke();
    }

    public bool LoadPreset(SettingsScope scope, string name)
    {
        if ((scope.Get(PresetsSetting) ?? new()).GetValueOrDefault(name) is not { } preset)
            return false;
        _paths.Clear();
        Update(scope, _ => preset.Select(t => t with { Id = Guid.NewGuid() }));
        return true;
    }

    public void DeletePreset(SettingsScope scope, string name)
    {
        var presets = new Dictionary<string, TrackerSpec[]>(scope.Get(PresetsSetting) ?? new());
        if (presets.Remove(name))
        {
            scope.Set(PresetsSetting, presets);
            Changed?.Invoke();
        }
    }

    private static readonly JsonSerializerOptions FileOptions = new() { WriteIndented = true };

    public string Export(SettingsScope scope) => JsonSerializer.Serialize(Trackers(scope), FileOptions);

    public int Import(SettingsScope scope, string json)
    {
        var imported = JsonSerializer.Deserialize<TrackerSpec[]>(json) ?? [];
        Update(scope, list => list.Concat(imported.Select(t => t with { Id = Guid.NewGuid() })));
        return imported.Length;
    }

    private IEnumerable<TrackerSpec> DrawOrder(SettingsScope scope)
    {
        var list = Trackers(scope);
        foreach (var order in new[] { TrackerOrder.OnBottom, TrackerOrder.Normal, TrackerOrder.OnTop })
            foreach (var t in list)
                if (t.Visible && t.Order == order)
                    yield return t;
    }


    private sealed class PathHistory
    {
        public readonly List<Vector3> Points = new();
        public uint LastTimer = uint.MaxValue;
        public int Level = -1;
    }

    private List<Vector3> Sample(TrackerSpec spec, TrackedPoint p, GameFrame frame)
    {
        var key = (spec.Id, p.Object?.Address ?? 0);
        if (!_paths.TryGetValue(key, out var history))
            _paths[key] = history = new PathHistory();
        if (history.Level != (frame.LevelIndex ?? -1))
        {
            history.Points.Clear();
            history.Level = frame.LevelIndex ?? -1;
        }
        if (frame.GlobalTimer != history.LastTimer)
        {
            history.LastTimer = frame.GlobalTimer;
            history.Points.Add(p.Position);
            int max = Math.Max(2, (int)spec.Size);
            if (history.Points.Count > max)
                history.Points.RemoveRange(0, history.Points.Count - max);
        }
        return history.Points;
    }

    private void ForgetRemovedPaths(IReadOnlyList<TrackerSpec> trackers)
    {
        if (_paths.Count == 0)
            return;
        var ids = trackers.Where(t => t.Kind == TrackerKind.Path).Select(t => t.Id).ToHashSet();
        foreach (var key in _paths.Keys.Where(k => !ids.Contains(k.Item1)).ToList())
            _paths.Remove(key);
    }


    private static float ArrowLength(TrackerSpec t, TrackedPoint p) => t.Size > 0 ? (float)t.Size : MathF.Max(1, MathF.Abs(p.Speed));

    private static (Vector3 Center, float Radius, float Bottom, float Height)? Box(TrackerSpec t, TrackedPoint p, GameFrame frame)
    {
        if (p.Object is { } o)
        {
            bool hurt = t.Kind is TrackerKind.Hurtbox or TrackerKind.EffectiveHurtbox;
            bool effective = t.Kind is TrackerKind.EffectiveHitbox or TrackerKind.EffectiveHurtbox;
            float radius = hurt ? o.HurtboxRadius : o.HitboxRadius, height = hurt ? o.HurtboxHeight : o.HitboxHeight;
            if (radius <= 0)
                return null;
            float bottom = o.Position.Y - o.HitboxDownOffset;
            if (effective)
            {
                radius += MarioState.HitboxRadius;
                bottom -= MarioState.HitboxHeight;
                height += MarioState.HitboxHeight;
            }
            return (o.Position, radius, bottom, height);
        }
        if (t.Target == TrackerTarget.Mario && frame.Mario is { } m)
            return (m.Position, MarioState.HitboxRadius, m.Position.Y, MarioState.HitboxHeight);
        return null;
    }

    private static ushort? ArrowYaw(TrackerSpec t, TrackedPoint p, GameFrame frame) => t.Kind switch
    {
        TrackerKind.FacingArrow => p.Yaw,
        TrackerKind.MovingArrow => p.MovingYaw,
        TrackerKind.IntendedArrow when frame.Mario is { } m && m.IntendedMag > 0 => m.IntendedYaw,
        TrackerKind.SlidingArrow when frame.Mario is { } m && m.SlidingSpeed > 0 => m.SlidingYaw,
        TrackerKind.TwirlArrow when frame.Mario is { } m => m.TwirlYaw,
        TrackerKind.AngleToMarioArrow when frame.Mario is { } m => Angle.Towards(p.Position.X, p.Position.Z, m.Position.X, m.Position.Z),
        _ => null,
    };

    private static float ArrowWorldLength(TrackerSpec t, TrackedPoint p, GameFrame frame) => t.Kind switch
    {
        TrackerKind.SlidingArrow when frame.Mario is { } m && t.Size <= 0 => m.SlidingSpeed,
        TrackerKind.IntendedArrow when frame.Mario is { } m => (float)(t.Size > 0 ? t.Size * m.IntendedMag / 32 : m.IntendedMag),
        TrackerKind.AngleToMarioArrow when frame.Mario is { } m && t.Size <= 0 => Vector2.Distance(Xz.Of(p.Position), Xz.Of(m.Position)),
        _ => ArrowLength(t, p),
    };

    private static IEnumerable<(int X, int Z, float Height, float DeltaY)> NearbyUnits(TrackerSpec t, TrackedPoint p, GameFrame frame)
    {
        int radius = Math.Clamp((int)t.Size, 1, 60);
        int cx = GameCollision.Truncate(p.Position.X), cz = GameCollision.Truncate(p.Position.Z);
        bool ceiling = t.Kind == TrackerKind.NearbyCeilingUnits;
        for (int dz = -radius; dz <= radius; dz++)
            for (int dx = -radius; dx <= radius; dx++)
            {
                int x = cx + dx, z = cz + dz;
                if (!ceiling)
                {
                    if (GameCollision.FindFloor(frame.Surfaces, x + 0.5f, p.Position.Y + 78, z + 0.5f) is { } found)
                        yield return (x, z, found.Height, found.Height - p.Position.Y);
                }
                else if (GameCollision.FindFloor(frame.Surfaces, x + 0.5f, p.Position.Y + 78, z + 0.5f) is { } floor
                         && Ceiling(frame.Surfaces, x, floor.Height + 80, z) is { } ceilingY)
                    yield return (x, z, ceilingY, ceilingY - p.Position.Y);
            }
    }

    private static float? Ceiling(SurfaceTable surfaces, int x, float y, int z)
    {
        float? best = null;
        foreach (var list in new[] { surfaces.Static, surfaces.Dynamic })
            for (int i = 0; i < list.Count; i++)
            {
                var s = list[i];
                if (s.Class != SurfaceClass.Ceiling || !GameCollision.IsInside(s, x, z))
                    continue;
                float h = s.HeightAt(x, z);
                if (h >= y && (best is null || h < best))
                    best = h;
            }
        return best;
    }

    private static MapColor UnitShade(MapColor baseColor, float deltaY)
    {
        double t = Math.Clamp(deltaY / 200.0, -1, 1);
        return t >= 0 ? baseColor.Lerp(MapColor.Rgb(0xE0, 0x4A, 0x4A), t) : baseColor.Lerp(MapColor.Rgb(0x4F, 0x86, 0xE0), -t);
    }

    private static Vector2 Along(Vector2 from, double yaw, double length)
    {
        var (dx, dz) = Angle.Direction(yaw);
        return from + new Vector2((float)(dx * length), (float)(dz * length));
    }


    public override void Build(MapContext c)
    {
        var trackers = Trackers(c.Settings);
        ForgetRemovedPaths(trackers);
        var mario = c.Frame.Mario;
        foreach (var t in DrawOrder(c.Settings))
        {
            var color = t.MapColor;
            foreach (var p in TrackerTargets.Resolve(t, c.Frame, c.Selection))
            {
                var at = Xz.Of(p.Position);
                IMapHit hit = p.Object is { } obj ? new ObjectHit(obj) : new PointHit($"{t.Title}", p.Position);
                switch (t.Kind)
                {
                    case TrackerKind.Marker:
                        c.Scene.Add(new MapMarker(at, (float)Math.Max(3, t.Size), MarkerShape.Circle, color, MapPalette.Black) { Hit = hit });
                        break;
                    case TrackerKind.Circle:
                    case TrackerKind.Cylinder:
                        c.Scene.Add(new MapCircle(at, (float)t.Size, color.WithAlpha(color.A / 255.0 * 0.15), color, 1.5f) { Hit = hit });
                        break;
                    case TrackerKind.Hitbox or TrackerKind.EffectiveHitbox or TrackerKind.Hurtbox or TrackerKind.EffectiveHurtbox:
                        if (Box(t, p, c.Frame) is { } box)
                            c.Scene.Add(new MapCircle(at, box.Radius, color.WithAlpha(color.A / 255.0 * 0.15), color, 1.5f) { Hit = hit });
                        break;
                    case TrackerKind.LineToMario when mario is not null:
                        c.Scene.Add(new MapPolyline([at, Xz.Of(mario.Position)], color, 1.5f) { Hit = hit });
                        break;
                    case TrackerKind.HomeLine when p.Object is { } o:
                        c.Scene.Add(new MapPolyline([Xz.Of(o.Home), at], color, 1.5f));
                        c.Scene.Add(new MapMarker(Xz.Of(o.Home), 7, MarkerShape.Diamond, MapColor.Transparent, color) { Hit = new PointHit($"Home of {p.Label}", o.Home) });
                        break;
                    case TrackerKind.Path:
                        var points = Sample(t, p, c.Frame);
                        if (points.Count >= 2)
                            c.Scene.Add(new MapPolyline(points.Select(Xz.Of).ToArray(), color, 1.5f));
                        break;
                    case TrackerKind.CurrentUnit:
                        var (x0, x1) = GameCollision.UnitSpan(GameCollision.Truncate(p.Position.X));
                        var (z0, z1) = GameCollision.UnitSpan(GameCollision.Truncate(p.Position.Z));
                        c.Scene.Add(new MapPolygon([new(x0, z0), new(x1, z0), new(x1, z1), new(x0, z1)], color.WithAlpha(color.A / 255.0 * 0.5), color) { Hit = hit });
                        break;
                    case TrackerKind.NearbyFloorUnits or TrackerKind.NearbyCeilingUnits:
                        foreach (var (ux, uz, _, deltaY) in NearbyUnits(t, p, c.Frame))
                        {
                            var (ax0, ax1) = GameCollision.UnitSpan(ux);
                            var (az0, az1) = GameCollision.UnitSpan(uz);
                            var shade = UnitShade(color, deltaY);
                            c.Scene.Add(new MapPolygon([new(ax0, az0), new(ax1, az0), new(ax1, az1), new(ax0, az1)],
                                shade.WithAlpha(0.45), shade.WithAlpha(0.8)));
                        }
                        break;
                    case TrackerKind.FacingDivider:
                        c.Scene.Add(new MapPolyline([Along(at, p.Yaw + 0x4000, t.Size), Along(at, p.Yaw - 0x4000, t.Size)], color, 2));
                        break;
                    case TrackerKind.Sector:
                    {
                        var wedge = new List<Vector2> { at };
                        for (int i = 0; i <= 16; i++)
                            wedge.Add(Along(at, p.Yaw - t.Size + 2 * t.Size * i / 16, t.Size2));
                        c.Scene.Add(new MapPolygon(wedge.ToArray(), color.WithAlpha(color.A / 255.0 * 0.15), color, 1.5f));
                        break;
                    }
                    case TrackerKind.AngleRange:
                        int step = Math.Max(16, (int)t.Size);
                        for (int yaw = 0; yaw < 65536; yaw += step)
                            c.Scene.Add(new MapPolyline([at, Along(at, p.Yaw + yaw, t.Size2)], color.WithAlpha(color.A / 255.0 * (yaw == 0 ? 1 : 0.5)), yaw == 0 ? 2 : 1));
                        break;
                    default:
                        if (ArrowYaw(t, p, c.Frame) is ushort arrowYaw)
                        {
                            float px = MathF.Max(8, ArrowWorldLength(t, p, c.Frame) * (float)c.View.Scale);
                            c.Scene.Add(new MapArrow(at, arrowYaw, px, color, 2) { Hit = hit });
                        }
                        break;
                }
            }
        }
    }


    public void Build3D(Map3DContext c)
    {
        var trackers = Trackers(c.Settings);
        ForgetRemovedPaths(trackers);
        var mario = c.Frame.Mario;
        var lift = new Vector3(0, 2, 0);
        foreach (var t in DrawOrder(c.Settings))
        {
            var color = t.MapColor;
            foreach (var p in TrackerTargets.Resolve(t, c.Frame, c.Selection))
            {
                IMapHit hit = p.Object is { } obj ? new ObjectHit(obj) : new PointHit(t.Title, p.Position);
                switch (t.Kind)
                {
                    case TrackerKind.Marker:
                        c.Scene.Add(new Point3D(p.Position, (float)Math.Max(3, t.Size), color) { Hit = hit });
                        break;
                    case TrackerKind.Circle:
                        c.Scene.Add(new Cylinder3D(p.Position, (float)t.Size, 1, color.WithAlpha(color.A / 255.0 * 0.3)) { Hit = hit });
                        break;
                    case TrackerKind.Cylinder:
                        c.Scene.Add(new Cylinder3D(p.Position, (float)t.Size, (float)t.Size2, color.WithAlpha(color.A / 255.0 * 0.3)) { Hit = hit });
                        break;
                    case TrackerKind.Hitbox or TrackerKind.EffectiveHitbox or TrackerKind.Hurtbox or TrackerKind.EffectiveHurtbox:
                        if (Box(t, p, c.Frame) is { } box)
                            c.Scene.Add(new Cylinder3D(new Vector3(box.Center.X, box.Bottom, box.Center.Z), box.Radius, box.Height, color.WithAlpha(color.A / 255.0 * 0.3)) { Hit = hit });
                        break;
                    case TrackerKind.LineToMario when mario is not null:
                        c.Scene.Add(new Line3D([p.Position + lift, mario.Position + lift], color));
                        break;
                    case TrackerKind.HomeLine when p.Object is { } o:
                        c.Scene.Add(new Line3D([o.Home + lift, p.Position + lift], color));
                        c.Scene.Add(new Point3D(o.Home, 7, color) { Hit = new PointHit($"Home of {p.Label}", o.Home) });
                        break;
                    case TrackerKind.Path:
                        var points = Sample(t, p, c.Frame);
                        if (points.Count >= 2)
                            c.Scene.Add(new Line3D(points.Select(v => v + lift).ToArray(), color));
                        break;
                    case TrackerKind.CurrentUnit:
                        var (x0, x1) = GameCollision.UnitSpan(GameCollision.Truncate(p.Position.X));
                        var (z0, z1) = GameCollision.UnitSpan(GameCollision.Truncate(p.Position.Z));
                        float y = p.Position.Y + 1;
                        c.Scene.Add(new Triangle3D(new(x0, y, z0), new(x1, y, z0), new(x1, y, z1), color.WithAlpha(0.5)));
                        c.Scene.Add(new Triangle3D(new(x0, y, z0), new(x1, y, z1), new(x0, y, z1), color.WithAlpha(0.5)));
                        break;
                    case TrackerKind.NearbyFloorUnits or TrackerKind.NearbyCeilingUnits:
                        foreach (var (ux, uz, height, deltaY) in NearbyUnits(t, p, c.Frame))
                        {
                            var (ax0, ax1) = GameCollision.UnitSpan(ux);
                            var (az0, az1) = GameCollision.UnitSpan(uz);
                            var shade = UnitShade(color, deltaY).WithAlpha(0.5);
                            float uy = height + 1;
                            c.Scene.Add(new Triangle3D(new(ax0, uy, az0), new(ax1, uy, az0), new(ax1, uy, az1), shade));
                            c.Scene.Add(new Triangle3D(new(ax0, uy, az0), new(ax1, uy, az1), new(ax0, uy, az1), shade));
                        }
                        break;
                    case TrackerKind.FacingDivider:
                    {
                        var a = Along(Xz.Of(p.Position), p.Yaw + 0x4000, t.Size);
                        var b = Along(Xz.Of(p.Position), p.Yaw - 0x4000, t.Size);
                        c.Scene.Add(new Line3D([new(a.X, p.Position.Y + 2, a.Y), new(b.X, p.Position.Y + 2, b.Y)], color));
                        break;
                    }
                    case TrackerKind.Sector or TrackerKind.AngleRange:
                    {
                        int count = t.Kind == TrackerKind.Sector ? 2 : (int)Math.Ceiling(65536 / Math.Max(16, t.Size));
                        double length = t.Size2;
                        for (int i = 0; i < count; i++)
                        {
                            double yaw = t.Kind == TrackerKind.Sector ? p.Yaw + (i == 0 ? -t.Size : t.Size) : p.Yaw + i * Math.Max(16, t.Size);
                            var e = Along(Xz.Of(p.Position), yaw, length);
                            c.Scene.Add(new Line3D([p.Position + lift, new(e.X, p.Position.Y + 2, e.Y)], color));
                        }
                        break;
                    }
                    default:
                        if (ArrowYaw(t, p, c.Frame) is ushort arrowYaw)
                            c.Scene.Add(new Arrow3D(p.Position + lift, arrowYaw, ArrowWorldLength(t, p, c.Frame), 4, color) { Hit = hit });
                        break;
                }
            }
        }
    }
}
