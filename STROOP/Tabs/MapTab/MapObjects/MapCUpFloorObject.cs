using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("C-Up Floor Triangles", "Triangles")]
    public class MapCUpFloorObject : MapCustomFloorObject
    {
        private static List<uint> GetCUpTriangleList()
        {
            return TriangleUtilities.GetLevelTriangles()
                .FindAll(tri => tri.IsFloor())
                .FindAll(tri =>
                {
                    double slopeAccel = tri.SlopeAccel;
                    double slopeDecel = 2.0 * tri.SlopeDecelValue;
                    double normalH = Math.Sqrt(tri.NormX * tri.NormX + tri.NormZ * tri.NormZ);
                    return slopeAccel * normalH > slopeDecel;
                })
                .ConvertAll(tri => tri.Address);
        }

        public MapCUpFloorObject() : base(GetCUpTriangleList())
        {
        }

        public override string GetName() => "C-Up Floor Tris";

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.TriangleFloorImage;
    }
}
