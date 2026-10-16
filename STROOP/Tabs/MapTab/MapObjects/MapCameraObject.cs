using System;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public class MapCameraObject : MapIconPointObject
    {
        public MapCameraObject()
            : base(null)
        {
            positionAngleProvider = () => new[] { PositionAngle.Camera };
            InternalRotates = true;
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.CameraMapImage;
        public override string GetName() => "Camera";

        protected override void Draw3D(MapGraphics graphics)
        {
            base.Draw3D(graphics);
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                var camera = PositionAngle.Camera;
                var focus = PositionAngle.CameraFocus;
                Vector3 cameraPos = new Vector3((float)camera.X, (float)camera.Y, (float)camera.Z);
                Vector3 focusPos = new Vector3((float)focus.X, (float)focus.Y, (float)focus.Z);
                Vector3 viewDir = focusPos - cameraPos;
                if (viewDir.LengthSquared < 1e-4f)
                    return;
                Vector4 color = Get3DAccentColor(Color.White);

                graphics.lineRenderer.Add(cameraPos, focusPos, color, 1);

                viewDir.Normalize();
                Vector3 right = Vector3.Cross(viewDir, Vector3.UnitY);
                if (right.LengthSquared < 1e-6f)
                    right = Vector3.Cross(viewDir, Vector3.UnitZ);
                right.Normalize();
                Vector3 up = Vector3.Cross(right, viewDir);

                float depth = Size * Get3DIconScale(graphics, cameraPos.X, cameraPos.Y, cameraPos.Z) * 3;
                float halfHeight = depth * (float)Math.Tan(MathHelper.DegreesToRadians(22.5));
                float halfWidth = halfHeight * 4 / 3;
                Vector3 center = cameraPos + viewDir * depth;
                Vector3[] corners =
                {
                    center + right * halfWidth + up * halfHeight,
                    center - right * halfWidth + up * halfHeight,
                    center - right * halfWidth - up * halfHeight,
                    center + right * halfWidth - up * halfHeight,
                };
                for (int i = 0; i < corners.Length; i++)
                {
                    graphics.lineRenderer.Add(cameraPos, corners[i], color, 2);
                    graphics.lineRenderer.Add(corners[i], corners[(i + 1) % corners.Length], color, 2);
                }
            });
        }
    }
}
