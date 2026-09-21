using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.Views;

public class ViewTopDown : ViewBase
{
    public override Matrix4 ComputeViewOrientation() => Matrix4.CreateRotationY(yaw);
}
