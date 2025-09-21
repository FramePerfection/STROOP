using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Mathematics;
using STROOP.Structs.Configurations;
using STROOP.Structs;

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

        protected override List<Vector3> GetVertices(MapGraphics graphics)
        {
            float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);

            List<Vector3> vertices = new List<Vector3>();
            for (int x = -8192; x <= 8192; x += 1024)
            {
                vertices.Add(new Vector3(x, marioY, - 8192));
                vertices.Add(new Vector3(x, marioY, 8192));
            }

            for (int z = -8192; z <= 8192; z += 1024)
            {
                vertices.Add(new Vector3(-8192, marioY, z));
                vertices.Add(new Vector3(8192, marioY, z));
            }

            return vertices;
        }

        public override string GetName()
        {
            return "Cell Gridlines";
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.CellGridlinesImage;
    }
}
