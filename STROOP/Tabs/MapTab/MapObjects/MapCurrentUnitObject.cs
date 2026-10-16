using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using System.Windows.Forms;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Current Unit", "Current")]
    public class MapCurrentUnitsObject : MapQuadObject
    {
        public MapCurrentUnitsObject() : this(() => new List<PositionAngle>(new[] { PositionAngle.Mario }))
        {
        }

        public MapCurrentUnitsObject(PositionAngleProvider positionAngleProvider)
            : base()
        {
            this.positionAngleProvider = positionAngleProvider;

            Opacity = 0.5;
            Color = Color.Purple;
        }

        static (float xMin, float xMax, float zMin, float zMax) GetUnit(PositionAngle obj)
        {
            var posAngleX = obj.X;
            var posAngleZ = obj.Z;
            int xMin = (short)posAngleX;
            int xMax = xMin + (posAngleX >= 0 ? 1 : -1);
            int zMin = (short)posAngleZ;
            int zMax = zMin + (posAngleZ >= 0 ? 1 : -1);
            return (Math.Min(xMin, xMax), Math.Max(xMin, xMax), Math.Min(zMin, zMax), Math.Max(zMin, zMax));
        }

        protected override List<(float xMin, float xMax, float zMin, float zMax, float y)> GetQuadList()
        {
            var quads = new List<(float, float, float, float, float)>();

            foreach (var obj in positionAngleProvider())
            {
                var unit = GetUnit(obj);
                quads.Add((unit.xMin, unit.xMax, unit.zMin, unit.zMax, (float)obj.Y));
            }

            return quads;
        }

        protected override List<(float xMin, float xMax, float zMin, float zMax, float yMin, float yMax)> GetBoxList3D(MapGraphics graphics)
        {
            var boxes = new List<(float, float, float, float, float, float)>();
            var floors = graphics.floors.GetTriangles();

            foreach (var obj in positionAngleProvider())
            {
                var unit = GetUnit(obj);
                float floorY = floors.FindFloorAndY((float)obj.X, (float)obj.Y, (float)obj.Z).floorY;
                float y = float.IsNaN(floorY) ? (float)obj.Y : floorY;
                boxes.Add((unit.xMin, unit.xMax, unit.zMin, unit.zMax, y, y));
            }

            return boxes;
        }

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker) => new ContextMenuStrip();

        public override string GetName() => $"Current Unit for {PositionAngle.NameOfMultiple(positionAngleProvider())}";

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.CurrentUnitImage;
    }
}
