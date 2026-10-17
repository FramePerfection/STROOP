using System.Globalization;
using System.Numerics;
using StroopNext.Game;
using StroopNext.Map3D;
using StroopNext.Settings;

namespace StroopNext.Map;

public readonly record struct Iwerlipse(int QuarterSteps, Vector3 Center, float SideRadius, float ForwardRadius, ushort FacingYaw)
{
    public static List<Iwerlipse> From(AirState s, int maxQuarterSteps)
    {
        var result = new List<Iwerlipse>();
        for (int n = 1; n <= maxQuarterSteps; n++)
        {
            var center = AirMovement.Repeat(s, RelativeDirection.Center, n).Position;
            var forward = AirMovement.Repeat(s, RelativeDirection.Forward, n).Position;
            var backward = AirMovement.Repeat(s, RelativeDirection.Backward, n).Position;
            var left = AirMovement.Repeat(s, RelativeDirection.Left, n).Position;
            float Xz(Vector3 a, Vector3 b) => MathF.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Z - b.Z) * (a.Z - b.Z));
            result.Add(new Iwerlipse(n, center, Xz(center, left), (Xz(center, forward) + Xz(center, backward)) / 2, s.FacingYaw));
        }
        return result;
    }

    public Vector2[] Outline(int segments = 48)
    {
        var (fx, fz) = Angle.Direction(FacingYaw);
        var forward = new Vector2((float)fx, (float)fz);
        var side = new Vector2(forward.Y, -forward.X);
        var c = new Vector2(Center.X, Center.Z);
        var points = new Vector2[segments];
        for (int i = 0; i < segments; i++)
        {
            double t = 2 * Math.PI * i / segments;
            points[i] = c + side * (float)(SideRadius * Math.Cos(t)) + forward * (float)(ForwardRadius * Math.Sin(t));
        }
        return points;
    }
}

public sealed record IwerlipseHit(Iwerlipse Ellipse) : IMapHit
{
    public string Title => $"Iwerlipse · {Ellipse.QuarterSteps} quarter step{(Ellipse.QuarterSteps > 1 ? "s" : "")}";

    public IEnumerable<string> Details =>
    [
        string.Create(CultureInfo.InvariantCulture, $"Neutral stick ends at ({Ellipse.Center.X:0.###}, {Ellipse.Center.Y:0.###}, {Ellipse.Center.Z:0.###})"),
        string.Create(CultureInfo.InvariantCulture, $"Reach: ±{Ellipse.SideRadius:0.###} sideways, ±{Ellipse.ForwardRadius:0.###} forwards"),
    ];
}

public sealed class IwerlipsesLayer : MapLayer, IMapLayer3D
{
    public static readonly MapColor Color = MapColor.Rgb(0xE5, 0x53, 0x4B);

    public override string Id => "iwerlipses";
    public override string Name => "Iwerlipses";
    public override bool VisibleByDefault => false;

    public readonly Setting<int> QuarterSteps;
    public readonly Setting<bool> Lock;

    private AirState? _locked;

    public IwerlipsesLayer()
    {
        QuarterSteps = Declare(new Setting<int>("quarterSteps", "Quarter steps", 4, "One ellipse per quarter step, up to this many (4 = one frame).") { Range = (1, 64, 1) });
        Lock = Declare(new Setting<bool>("lock", "Lock to the current position", false, "Keep the ellipses where they are while the game advances."));
    }

    private List<Iwerlipse> Compute(Session.GameFrame frame, SettingsScope settings)
    {
        if (!settings.Get(Lock))
            _locked = null;
        else if (_locked is null && frame.Mario is { } m)
            _locked = AirState.Of(m);
        AirState? state = _locked ?? (frame.Mario is { } mario ? AirState.Of(mario) : null);
        return state is { } s ? Iwerlipse.From(s, settings.Get(QuarterSteps)) : [];
    }

    public override void Build(MapContext c)
    {
        foreach (var e in Compute(c.Frame, c.Settings))
            c.Scene.Add(new MapPolygon(e.Outline(), Color.WithAlpha(0.08), Color.WithAlpha(0.8), 1.5f) { Hit = new IwerlipseHit(e) });
    }

    public void Build3D(Map3DContext c)
    {
        foreach (var e in Compute(c.Frame, c.Settings))
        {
            var outline = e.Outline();
            var loop = new Vector3[outline.Length + 1];
            for (int i = 0; i <= outline.Length; i++)
            {
                var p = outline[i % outline.Length];
                loop[i] = new Vector3(p.X, e.Center.Y, p.Y);
            }
            c.Scene.Add(new Line3D(loop, Color.WithAlpha(0.9)) { Hit = new IwerlipseHit(e) });
        }
    }
}
