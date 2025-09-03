using STROOP.Utilities;
using OpenTK;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab
{
    public class MapView
    {
        public enum ViewMode
        {
            TopDown,
            Orthogonal,
            ThreeDimensional
        }

        public enum Camera3DMode
        {
            InGame,
            FocusOnPositionAngle,
            Free,
        }

        public MapGraphics MapGraphics;
        public ViewMode mode = ViewMode.TopDown;
        public Camera3DMode camera3DMode = Camera3DMode.FocusOnPositionAngle;
        public PositionAngle focusPositionAngle = PositionAngle.Mario;
        public Vector2 orthoOffset = Vector2.Zero;
        public float orthoRelativeNearPlane = float.NaN, orthoRelativeFarPlane = float.NaN;
        public bool displayOrthoLevelGeometry = true;
        public bool display3DLevelGeometry = true;
        public bool drawCylinderOutlines = false;

        public Vector3 position;
        public float yaw, pitch, camera3DDistanceController = 50;
        public float movementSpeed = 2000.0f;

        public Matrix4 ComputeViewOrientation() => Matrix4.CreateRotationX(pitch) * Matrix4.CreateRotationY(yaw);
        public Vector3 ComputeViewDirection() => Vector3.TransformPosition(new Vector3(0, 0, 1), ComputeViewOrientation());

        public void Pivot(PositionAngle pivotPoint)
        {
            camera3DMode = Camera3DMode.FocusOnPositionAngle;
            focusPositionAngle = pivotPoint;
            var d = focusPositionAngle.position - position;
            yaw = (float)(System.Math.PI / 2 - System.Math.Atan2(d.Z, d.X));
            pitch = (float)-System.Math.Atan2(d.Y, System.Math.Sqrt(d.X * d.X + d.Z * d.Z));
            camera3DDistanceController = 10 * (float)(System.Math.Log(d.Length));
        }
    }
}
