using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using System.Windows.Forms;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("PU Gridlines", "Grid")]
    public class MapPuGridlinesObject : MapLineObject
    {
        private enum PuGridlineSetting { SETTING1, SETTING2, SETTING3 };
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

                        int xMin = ((((int)graphics.MapViewXMin) / 65536) - 1) * 65536;
                        int xMax = ((((int)graphics.MapViewXMax) / 65536) + 1) * 65536;
                        int zMin = ((((int)graphics.MapViewZMin) / 65536) - 1) * 65536;
                        int zMax = ((((int)graphics.MapViewZMax) / 65536) + 1) * 65536;

                        var vertices = new List<Vector3>();
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

                        int xMin = ((((int)graphics.MapViewXMin) / 65536) - 1) * 65536 - 32768;
                        int xMax = ((((int)graphics.MapViewXMax) / 65536) + 1) * 65536 + 32768;
                        int zMin = ((((int)graphics.MapViewZMin) / 65536) - 1) * 65536 - 32768;
                        int zMax = ((((int)graphics.MapViewZMax) / 65536) + 1) * 65536 + 32768;

                        var vertices = new List<Vector3>();
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

                        int xMin = ((((int)graphics.MapViewXMin) / 65536) - 1) * 65536;
                        int xMax = ((((int)graphics.MapViewXMax) / 65536) + 1) * 65536;
                        int zMin = ((((int)graphics.MapViewZMin) / 65536) - 1) * 65536;
                        int zMax = ((((int)graphics.MapViewZMax) / 65536) + 1) * 65536;

                        List<Vector3> vertices = new List<Vector3>();
                        for (int x = xMin; x <= xMax; x += 65536)
                        {
                            for (int z = zMin; z <= zMax; z += 65536)
                            {
                                float x1 = x - 8192;
                                float x2 = x + 8192;
                                float z1 = z - 8192;
                                float z2 = z + 8192;

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
