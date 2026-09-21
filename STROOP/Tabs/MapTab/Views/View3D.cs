using OpenTK.Mathematics;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab.Views;

public class View3D : ViewBase, PivotingView
{
    public enum Camera3DMode
    {
        InGame,
        FocusOnPositionAngle,
        Free,
    }

    public Camera3DMode camera3DMode = Camera3DMode.FocusOnPositionAngle;
    public float camera3DDistanceController = 50;
    public bool display3DLevelGeometry = true;

    public PositionAngle focusPositionAngle { get; set; } = PositionAngle.Mario;

    void PivotingView.Pivot(PositionAngle pivotPoint)
    {
        focusPositionAngle = pivotPoint;
        camera3DMode = Camera3DMode.FocusOnPositionAngle;
        var d = focusPositionAngle.position - position;
        yaw = (float)(System.Math.PI / 2 - System.Math.Atan2(d.Z, d.X));
        pitch = (float)-System.Math.Atan2(d.Y, System.Math.Sqrt(d.X * d.X + d.Z * d.Z));
        camera3DDistanceController = 10 * (float)(System.Math.Log(d.Length));
    }

    public override Matrix4 ComputeViewOrientation() => Matrix4.CreateRotationX(pitch) * Matrix4.CreateRotationY(yaw);
}
