using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Mathematics;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Cell Gridlines", "Grid")]
    public class MapCellGridlinesObject : MapLineObject
    {
        public MapCellGridlinesObject()
            : base()
        {
            OutlineWidth = 3;
            OutlineColor = Color.Black;
        }

        const int CELL_EXPANSE_3D = 3;
        const float COLUMN_HALF_HEIGHT_3D = 2000;

        protected override List<Vector3> GetVertices(MapGraphics graphics)
        {
            float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);

            List<Vector3> vertices = new List<Vector3>();
            if (graphics.viewMode == MapGraphics.ViewMode.ThreeDimensional)
            {
                float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
                float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
                if (!IsUsableCoordinate(marioX) || !IsUsableCoordinate(marioY) || !IsUsableCoordinate(marioZ))
                    return vertices;
                float xMin = ((float)Math.Floor(marioX / 1024) - CELL_EXPANSE_3D) * 1024;
                float zMin = ((float)Math.Floor(marioZ / 1024) - CELL_EXPANSE_3D) * 1024;
                float extent = (2 * CELL_EXPANSE_3D + 1) * 1024;
                AddColumnGrid3D(vertices, xMin, xMin + extent, zMin, zMin + extent, 1024, marioY, marioY - COLUMN_HALF_HEIGHT_3D, marioY + COLUMN_HALF_HEIGHT_3D);
                return vertices;
            }

            for (int x = -8192; x <= 8192; x += 1024)
            {
                vertices.Add(new Vector3(x, marioY, -8192));
                vertices.Add(new Vector3(x, marioY, 8192));
            }

            for (int z = -8192; z <= 8192; z += 1024)
            {
                vertices.Add(new Vector3(-8192, marioY, z));
                vertices.Add(new Vector3(8192, marioY, z));
            }

            return vertices;
        }

        const int MAX_COLUMN_LINES_3D = 64;

        static bool IsUsableCoordinate(float value) => float.IsFinite(value) && Math.Abs(value) < 1e7f;

        static int LineCount(float min, float max, float spacing)
        {
            if (!(spacing > 0) || !float.IsFinite(min) || !float.IsFinite(max) || max < min)
                return 0;
            return (int)Math.Min(MAX_COLUMN_LINES_3D, Math.Round((max - min) / spacing) + 1);
        }

        public static void AddColumnGrid3D(List<Vector3> vertices, float xMin, float xMax, float zMin, float zMax, float spacing, float yRef, float yMin, float yMax)
        {
            int xCount = LineCount(xMin, xMax, spacing), zCount = LineCount(zMin, zMax, spacing);
            foreach (float y in new[] { yRef, yMin, yMax })
            {
                for (int i = 0; i < xCount; i++)
                {
                    float x = xMin + i * spacing;
                    vertices.Add(new Vector3(x, y, zMin));
                    vertices.Add(new Vector3(x, y, zMax));
                }

                for (int i = 0; i < zCount; i++)
                {
                    float z = zMin + i * spacing;
                    vertices.Add(new Vector3(xMin, y, z));
                    vertices.Add(new Vector3(xMax, y, z));
                }
            }

            for (int i = 0; i < xCount; i++)
                for (int j = 0; j < zCount; j++)
                {
                    float x = xMin + i * spacing, z = zMin + j * spacing;
                    vertices.Add(new Vector3(x, yMin, z));
                    vertices.Add(new Vector3(x, yMax, z));
                }
        }

        public override string GetName()
        {
            return "Cell Gridlines";
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.CellGridlinesImage;
    }
}
