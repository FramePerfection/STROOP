using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Punch Floor Triangles", "Triangles")]
    public class MapPunchFloorObject : MapCustomFloorObject
    {
        private static List<uint> GetPunchTriangleList()
        {
            return TriangleUtilities.GetLevelTriangles()
                .FindAll(tri => tri.IsFloor())
                .FindAll(tri =>
                {
                    double slopeAccel = tri.SlopeAccel;
                    double slopeDecel = 0.5 * tri.SlopeDecelValue;
                    double normalH = Math.Sqrt(tri.NormX * tri.NormX + tri.NormZ * tri.NormZ);
                    return slopeAccel * normalH > slopeDecel;
                })
                .ConvertAll(tri => tri.Address);
        }

        public MapPunchFloorObject() : base(GetPunchTriangleList())
        {
        }

        public override string GetName() => "Punch Floor Tris";

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.TriangleFloorImage;
    }
}
