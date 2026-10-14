using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Current Cell", "Current")]
    public class MapCurrentCellObject : MapQuadObject
    {
        const float COLUMN_HALF_HEIGHT_3D = 500;

        public MapCurrentCellObject()
            : base()
        {
            Opacity = 0.5;
            Color = Color.Yellow;
        }


        protected override List<(float xMin, float xMax, float zMin, float zMax, float y)> GetQuadList()
        {
            (int cellX, int cellZ) = VariableSpecialUtilities.GetMarioCell();
            int xMin = (cellX - 8) * 1024;
            int xMax = xMin + 1024;
            int zMin = (cellZ - 8) * 1024;
            int zMax = zMin + 1024;
            return new List<(float, float, float, float, float)>(
                new[]
                {
                    ((float)xMin, (float)xMax, (float)zMin, (float)zMax,
                        Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset))
                });
        }

        protected override List<(float xMin, float xMax, float zMin, float zMax, float yMin, float yMax)> GetBoxList3D(MapGraphics graphics)
        {
            var boxes = new List<(float, float, float, float, float, float)>();
            foreach (var quad in GetQuadList())
                boxes.Add((quad.xMin, quad.xMax, quad.zMin, quad.zMax, quad.y - COLUMN_HALF_HEIGHT_3D, quad.y + COLUMN_HALF_HEIGHT_3D));
            return boxes;
        }

        public override string GetName()
        {
            return "Current Cell";
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.CurrentCellImage;
    }
}
