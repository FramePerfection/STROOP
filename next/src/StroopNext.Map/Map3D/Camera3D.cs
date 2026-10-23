using System.Numerics;

namespace StroopNext.Map3D;

public readonly record struct Ray(Vector3 Origin, Vector3 Direction)
{
    public Vector3 At(float t) => Origin + Direction * t;
}

public enum CameraMode3D
{
    FollowMario,

    Free,

    Game,

    Side,

    FollowGhosts,
}

public enum SideAxis
{
    SideOfMario,
    BehindMario,
    AlongX,
    AlongZ,
}

public static class SideView
{
    public static ushort LookYaw(SideAxis axis, int marioFacing) => axis switch
    {
        SideAxis.SideOfMario => (ushort)(marioFacing + 0x4000),
        SideAxis.BehindMario => (ushort)marioFacing,
        SideAxis.AlongX => 0x4000,
        _ => 0x8000,
    };

    public static void Apply(Camera3D camera, Vector3 target, ushort lookYaw, float width, float? slabHalfDepth)
    {
        camera.Eye = null;
        camera.Target = target;
        camera.Yaw = StroopNext.Game.Angle.ToRadians(lookYaw);
        camera.Pitch = 0;
        camera.Distance = 30000;
        camera.Up = Vector3.UnitY;
        camera.OrthographicWidth = width;
        camera.SlabHalfDepth = slabHalfDepth is > 0 ? slabHalfDepth : null;
    }
}

public sealed class Camera3D
{
    public const double MaxPitch = 1.55;

    public Vector3 Target { get; set; }

    public double Yaw { get; set; } = Math.PI;

    public double Pitch { get; set; } = 0.9;

    public double Distance { get; set; } = 1800;

    public double FieldOfView { get; set; } = 60 * Math.PI / 180;

    public Vector3? Eye { get; set; }

    public float Width { get; set; } = 1;
    public float Height { get; set; } = 1;

    public Vector3 Position => Eye ?? Target - OrbitForward * (float)Distance;

    private Vector3 OrbitForward => new(
        (float)(Math.Sin(Yaw) * Math.Cos(Pitch)),
        (float)-Math.Sin(Pitch),
        (float)(Math.Cos(Yaw) * Math.Cos(Pitch)));

    public Vector3 Forward => Vector3.Normalize(Target - Position);

    public float? SlabHalfDepth { get; set; }

    private bool HasSlab => SlabHalfDepth is not null && OrthographicWidth is not null;

    public float Near => HasSlab
        ? MathF.Max(0.5f, Vector3.Distance(Position, Target) - SlabHalfDepth!.Value)
        : (float)Math.Clamp(Vector3.Distance(Position, Target) / 300, 1, 50);

    public const float DefaultFar = 60000;

    private float _far = DefaultFar;

    public float Far
    {
        get => HasSlab ? Vector3.Distance(Position, Target) + SlabHalfDepth!.Value : _far;
        set => _far = value;
    }

    public bool IsInsideSlab(Vector3 point)
        => !HasSlab || MathF.Abs(Vector3.Dot(point - Target, Forward)) <= SlabHalfDepth!.Value;

    public Vector3 Up { get; set; } = Vector3.UnitY;

    public float? OrthographicWidth { get; set; }

    public Matrix4x4 View => Matrix4x4.CreateLookAt(Position, Target, Up);

    public Matrix4x4 Projection => OrthographicWidth is float width
        ? Matrix4x4.CreateOrthographic(width, width * Math.Max(1, Height) / Math.Max(1, Width), Near, Far)
        : Matrix4x4.CreatePerspectiveFieldOfView((float)FieldOfView, Math.Max(1, Width) / Math.Max(1, Height), Near, Far);

    public Matrix4x4 ViewProjection => View * Projection;

    public Vector2? Project(Vector3 world)
    {
        var clip = Vector4.Transform(new Vector4(world, 1), ViewProjection);
        if (clip.W <= 1e-6f)
            return null;
        float x = clip.X / clip.W, y = clip.Y / clip.W;
        return new Vector2((x + 1) / 2 * Width, (1 - y) / 2 * Height);
    }

    public Ray ScreenRay(Vector2 screen)
    {
        Matrix4x4.Invert(ViewProjection, out var inverse);
        float x = screen.X / Math.Max(1, Width) * 2 - 1;
        float y = 1 - screen.Y / Math.Max(1, Height) * 2;
        var near = Vector4.Transform(new Vector4(x, y, 0, 1), inverse);
        var far = Vector4.Transform(new Vector4(x, y, 1, 1), inverse);
        var a = new Vector3(near.X, near.Y, near.Z) / near.W;
        var b = new Vector3(far.X, far.Y, far.Z) / far.W;
        return new Ray(a, Vector3.Normalize(b - a));
    }

    public void Orbit(float dxPixels, float dyPixels)
    {
        DetachFromEye();
        Yaw -= dxPixels * 0.008;
        Pitch = Math.Clamp(Pitch + dyPixels * 0.008, -MaxPitch, MaxPitch);
    }

    public void Zoom(double factor)
    {
        DetachFromEye();
        Distance = Math.Clamp(Distance * factor, 30, 40000);
    }

    public void DetachFromEye()
    {
        if (Eye is not { } eye)
            return;
        var toTarget = Target - eye;
        float length = toTarget.Length();
        if (length > 1e-3f)
        {
            Distance = length;
            Pitch = Math.Clamp(Math.Asin(-toTarget.Y / length), -MaxPitch, MaxPitch);
            Yaw = Math.Atan2(toTarget.X, toTarget.Z);
        }
        Eye = null;
    }

    public void Pan(float dxPixels, float dyPixels)
    {
        double unitsPerPixel = OrthographicWidth is float width
            ? width / Math.Max(1, Width)
            : 2 * Vector3.Distance(Position, Target) * Math.Tan(FieldOfView / 2) / Math.Max(1, Height);
        var forward = Forward;
        var right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
        var up = Vector3.Cross(right, forward);
        Target += (-right * dxPixels + up * dyPixels) * (float)unitsPerPixel;
    }
}
