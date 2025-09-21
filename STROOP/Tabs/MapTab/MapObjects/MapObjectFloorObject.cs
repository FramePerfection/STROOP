using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Models;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public class MapObjectFloorObject : MapFloorObject
    {
        public MapObjectFloorObject(PositionAngleProvider positionAngleProvider) : base(null)
        {
            this.positionAngleProvider = positionAngleProvider;
        }

        protected override List<TriangleDataModel> GetTrianglesOfAnyDist()
        {
            var lst = new List<TriangleDataModel>();
            foreach (var posAngle in positionAngleProvider())
            {
                var obj = PositionAngle.GetObjectAddress(posAngle);
                foreach (var tri in TriangleUtilities.GetObjectTrianglesForObject(obj))
                    if (tri.IsFloor())
                        lst.Add(tri);
            }

            return lst;
        }

        public override string GetName() => $"Floor Tris for {PositionAngle.NameOfMultiple(positionAngleProvider())}";

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.TriangleFloorImage;
    }
}
