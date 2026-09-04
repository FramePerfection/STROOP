using OpenTK.Mathematics;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab.Views;

public interface PivotingView
{
    public PositionAngle focusPositionAngle { get; protected set; }
    public void Pivot(PositionAngle pivotPoint) => focusPositionAngle = pivotPoint;
}

public abstract class ViewBase
{
    public string name = "Custom";

    // TODO: consider what this is (ab)used for
    public Vector3 position;

    // TODO: split between keyboard and mouse inputs as well as radial vs linear?
    /// <summary> Displacement in units per t, where t is either seconds for keyboard keys or some number of pixels for mouse movement. </summary>
    public float movementSpeed = 2000.0f;

    // TODO: remove from here by using inheritance for mouse events properly
    public float yaw, pitch;

    public abstract Matrix4 ComputeViewOrientation();

    public Vector3 ComputeViewDirection() => ComputeViewOrientation().Row2.Xyz;
}
