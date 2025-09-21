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

            ;
            return lst;
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.ArrowImage;

        public override string GetName() => $"Facing Divider for {PositionAngle.NameOfMultiple(positionAngleProvider())}";
    }
}
