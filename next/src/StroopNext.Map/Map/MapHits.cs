using System.Globalization;
using System.Numerics;
using StroopNext.Game;

namespace StroopNext.Map;

internal static class Fmt
{
    public static string F(float v) => v.ToString("0.###", CultureInfo.InvariantCulture) is var s && s == "-0" ? "0" : s;
    public static string V(Vector3 v) => $"({F(v.X)}, {F(v.Y)}, {F(v.Z)})";
}

public sealed record SurfaceHit(Surface Surface) : IMapHit
{
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Surface, SurfaceHit> Cache = new();

    public static SurfaceHit Of(Surface surface) => Cache.GetValue(surface, static s => new SurfaceHit(s));

    public string Title => $"{Surface.Class} triangle 0x{Surface.Address:X8}";

    public IEnumerable<string> Details =>
    [
        $"Index {Surface.Index}{(Surface.IsStatic ? "" : " (object)")} · type 0x{(ushort)Surface.Type:X4}",
        $"V1 {Fmt.V(Surface.V1)}",
        $"V2 {Fmt.V(Surface.V2)}",
        $"V3 {Fmt.V(Surface.V3)}",
        $"Normal {Fmt.V(Surface.Normal)} · Y {Fmt.F(Surface.MinY)}..{Fmt.F(Surface.MaxY)}",
        .. Surface.Class == SurfaceClass.Wall ? [$"Pushes along {(Surface.ProjectsOnX ? "X" : "Z")}"] : Array.Empty<string>(),
    ];
}

public sealed record ObjectHit(GameObject Object) : IMapHit
{
    public string Title => $"{Object.Name} · slot {Object.SlotNumber}";

    public IEnumerable<string> Details =>
    [
        $"Address 0x{Object.Address:X8} · behavior 0x{Object.SegmentedBehavior:X8}",
        $"Position {Fmt.V(Object.Position)}",
        $"H speed {Fmt.F(Object.HSpeed)} · Y speed {Fmt.F(Object.YSpeed)} · yaw {Object.FacingYaw}",
        $"Hitbox r {Fmt.F(Object.HitboxRadius)} h {Fmt.F(Object.HitboxHeight)} · hurtbox r {Fmt.F(Object.HurtboxRadius)}",
        $"Action {Object.Action} · timer {Object.Timer}",
    ];
}

public sealed record MarioHit(MarioState Mario, string? ActionName) : IMapHit
{
    public string Title => "Mario";

    public IEnumerable<string> Details =>
    [
        $"Position {Fmt.V(Mario.Position)}",
        $"H speed {Fmt.F(Mario.HSpeed)} · yaw {Mario.FacingYaw}",
        $"Action {ActionName ?? $"0x{Mario.Action:X8}"}",
    ];
}

public sealed record PathSampleHit(uint GlobalTimer, Vector3 Position, float? DistanceFromPrevious) : IMapHit
{
    public string Title => $"Mario at timer {GlobalTimer}";

    public IEnumerable<string> Details =>
    [
        $"Position {Fmt.V(Position)}",
        .. DistanceFromPrevious is float d ? [$"Moved {Fmt.F(d)} (XZ) since previous frame"] : Array.Empty<string>(),
    ];
}

public sealed record CameraHit(CameraState Camera) : IMapHit
{
    public string Title => "Camera";
    public IEnumerable<string> Details => [$"Position {Fmt.V(Camera.Position)}", $"Focus {Fmt.V(Camera.Focus)}"];
}

public sealed record PointHit(string Name, Vector3 Position) : IMapHit
{
    public string Title => Name;
    public IEnumerable<string> Details => [$"Position {Fmt.V(Position)}"];
}
