using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using System.Windows.Forms;
using OpenTK.Mathematics;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("PU Gridlines", "Grid")]
    public class MapPuGridlinesObject : MapLineObject
    {
        private enum PuGridlineSetting
        {
            SETTING1,
            SETTING2,
            SETTING3
        };

        private PuGridlineSetting _setting;

        public MapPuGridlinesObject()
            : base()
        {
            OutlineWidth = 1;
            OutlineColor = Color.Black;

            _setting = PuGridlineSetting.SETTING1;
        }

        protected override List<Vector3> GetVertices(MapGraphics graphics)
        {
            switch (_setting)
            {
                case PuGridlineSetting.SETTING1:
                    {
                        float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);

                        (int xMin, int xMax, int zMin, int zMax) = GetPuBounds(graphics);

                        var vertices = new List<Vector3>();
                        if (graphics.viewMode == MapGraphics.ViewMode.ThreeDimensional)
                        {
                            MapCellGridlinesObject.AddColumnGrid3D(vertices, xMin, xMax, zMin, zMax, 65536, marioY, marioY - WALL_HALF_HEIGHT_3D, marioY + WALL_HALF_HEIGHT_3D);
                            return vertices;
                        }

                        for (int x = xMin; x <= xMax; x += 65536)
                        {
                            vertices.Add(new Vector3(x, marioY, zMin));
                            vertices.Add(new Vector3(x, marioY, zMax));
                        }

                        for (int z = zMin; z <= zMax; z += 65536)
                        {
                            vertices.Add(new Vector3(xMin, marioY, z));
                            vertices.Add(new Vector3(xMax, marioY, z));
                        }

                        return vertices;
                    }
                case PuGridlineSetting.SETTING2:
                    {
                        float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);

                        (int xMin, int xMax, int zMin, int zMax) = GetPuBounds(graphics);
                        xMin -= 32768;
                        xMax += 32768;
                        zMin -= 32768;
                        zMax += 32768;

                        var vertices = new List<Vector3>();
                        if (graphics.viewMode == MapGraphics.ViewMode.ThreeDimensional)
                        {
                            MapCellGridlinesObject.AddColumnGrid3D(vertices, xMin, xMax, zMin, zMax, 65536, marioY, marioY - WALL_HALF_HEIGHT_3D, marioY + WALL_HALF_HEIGHT_3D);
                            return vertices;
                        }

                        for (int x = xMin; x <= xMax; x += 65536)
                        {
                            vertices.Add(new Vector3(x, marioY, zMin));
                            vertices.Add(new Vector3(x, marioY, zMax));
                        }

                        for (int z = zMin; z <= zMax; z += 65536)
                        {
                            vertices.Add(new Vector3(xMin, marioY, z));
                            vertices.Add(new Vector3(xMax, marioY, z));
                        }

                        return vertices;
                    }
                case PuGridlineSetting.SETTING3:
                    {
                        float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);

                        (int xMin, int xMax, int zMin, int zMax) = GetPuBounds(graphics);

                        List<Vector3> vertices = new List<Vector3>();
                        for (int x = xMin; x <= xMax; x += 65536)
                        {
                            for (int z = zMin; z <= zMax; z += 65536)
                            {
                                float x1 = x - 8192;
                                float x2 = x + 8192;
                                float z1 = z - 8192;
                                float z2 = z + 8192;

                                if (graphics.viewMode == MapGraphics.ViewMode.ThreeDimensional)
                                {
                                    MapCellGridlinesObject.AddColumnGrid3D(vertices, x1, x2, z1, z2, x2 - x1, marioY, marioY - WALL_HALF_HEIGHT_3D, marioY + WALL_HALF_HEIGHT_3D);
                                    continue;
                                }

                                vertices.Add(new Vector3(x1, marioY, z1));
                                vertices.Add(new Vector3(x1, marioY, z2));

                                vertices.Add(new Vector3(x2, marioY, z1));
                                vertices.Add(new Vector3(x2, marioY, z2));

                                vertices.Add(new Vector3(x1, marioY, z1));
                                vertices.Add(new Vector3(x2, marioY, z1));

                                vertices.Add(new Vector3(x1, marioY, z2));
                                vertices.Add(new Vector3(x2, marioY, z2));
                            }
                        }

                        return vertices;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        const int PU_EXPANSE_3D = 3;
        const float WALL_HALF_HEIGHT_3D = 2000;

        static (int xMin, int xMax, int zMin, int zMax) GetPuBounds(MapGraphics graphics)
        {
            if (graphics.viewMode == MapGraphics.ViewMode.ThreeDimensional)
            {
                Vector3 center = graphics.currentView.position;
                int centerX = (int)Math.Round(center.X / 65536);
                int centerZ = (int)Math.Round(center.Z / 65536);
                return ((centerX - PU_EXPANSE_3D) * 65536, (centerX + PU_EXPANSE_3D) * 65536,
                    (centerZ - PU_EXPANSE_3D) * 65536, (centerZ + PU_EXPANSE_3D) * 65536);
            }

            return (((((int)graphics.MapViewXMin) / 65536) - 1) * 65536,
                ((((int)graphics.MapViewXMax) / 65536) + 1) * 65536,
                ((((int)graphics.MapViewZMin) / 65536) - 1) * 65536,
                ((((int)graphics.MapViewZMax) / 65536) + 1) * 65536);
        }

        public override string GetName()
        {
            return "PU Gridlines";
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.UnitGridlinesImage;

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            List<string> itemNames = new List<string>() { "Setting 1", "Setting 2", "Setting 3" };
            List<PuGridlineSetting> itemValues = EnumUtilities.GetEnumValues<PuGridlineSetting>(typeof(PuGridlineSetting));
            Action<PuGridlineSetting> setterAction = (PuGridlineSetting setting) => _setting = setting;
            PuGridlineSetting startingValue = _setting;
            (List<ToolStripMenuItem> itemList, Action<PuGridlineSetting> valueAction) =
                ControlUtilities.CreateCheckableItems(
                    itemNames, itemValues, setterAction, startingValue);

            var _contextMenuStrip = new ContextMenuStrip();
            itemList.ForEach(item => _contextMenuStrip.Items.Add(item));

            return _contextMenuStrip;
        }
    }
}
