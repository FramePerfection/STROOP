using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Mathematics;
using STROOP.Utilities;
using STROOP.Structs.Configurations;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public class MapFacingDividerObject : MapLineObject
    {
        public MapFacingDividerObject(PositionAngleProvider positionAngleProvider)
            : base()
        {
            this.positionAngleProvider = positionAngleProvider;

            Size = 1000;
            OutlineWidth = 3;
            OutlineColor = Color.Red;
        }

        protected override List<Vector3> GetVertices(MapGraphics graphics)
        {
            var lst = new List<Vector3>();
            foreach (var _posAngle in positionAngleProvider())
            {
                (float x, float y, float z, float angle) = ((float, float, float, float))_posAngle.GetValues();

                (float x1, float z1) =
                    ((float, float))MoreMath.AddVectorToPoint(Size, angle - 16384, x, z);
                (float x2, float z2) =
                    ((float, float))MoreMath.AddVectorToPoint(Size, angle + 16384, x, z);

                lst.Add(new Vector3(x1, y, z1));
                lst.Add(new Vector3(x2, y, z2));
            }

            return lst;
        }

        protected override void Draw3D(MapGraphics graphics)
        {
            base.Draw3D(graphics);

            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                var outlineColor = OpenTKUtilities.ColorToVec4(OutlineColor, OpacityByte);
                var fillColor = new Vector4(outlineColor.Xyz, outlineColor.W * 0.25f);
                var vertices = GetVertices(graphics);
                for (int i = 0; i + 1 < vertices.Count; i += 2)
                {
                    Vector3 a = vertices[i], b = vertices[i + 1];
                    Vector3 up = new Vector3(0, Size, 0);
                    Vector3 p0 = a - up, p1 = b - up, p2 = b + up, p3 = a + up;
                    graphics.triangleRenderer.Add(p0, p1, p2, false, fillColor, outlineColor, new Vector3(1, 0, 1), true);
                    graphics.triangleRenderer.Add(p0, p2, p3, false, fillColor, outlineColor, new Vector3(1, 1, 0), true);
                }
            });
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.ArrowImage;

        public override string GetName() => $"Facing Divider for {PositionAngle.NameOfMultiple(positionAngleProvider())}";
    }
}
