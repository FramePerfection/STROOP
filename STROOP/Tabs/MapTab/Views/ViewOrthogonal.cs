using OpenTK.Mathematics;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab.Views;

public class ViewOrthogonal : ViewBase, PivotingView
{
    public Vector2 orthoOffset = Vector2.Zero;
    public float orthoRelativeNearPlane = float.NaN, orthoRelativeFarPlane = float.NaN;
    public bool displayOrthoLevelGeometry = true;

    public override Matrix4 ComputeViewOrientation() => Matrix4.CreateRotationX(pitch) * Matrix4.CreateRotationY(yaw);

    public PositionAngle focusPositionAngle { get; set; } = PositionAngle.Mario;
}
