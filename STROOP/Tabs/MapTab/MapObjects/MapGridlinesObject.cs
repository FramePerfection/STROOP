using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using System.Windows.Forms;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Unit Gridlines", "Grid", nameof(CreateCustom))]
    [ObjectDescription("Custom Gridlines", "Grid", nameof(CreateCustom))]
    public class MapGridlinesObject : MapLineObject
    {

        int _hExpanse = 16;
        int _vExpanse = 8;
        int _verticalLineDistance = 1;

        protected override Vector4 GetColor(MapGraphics graphics)
        {
            var c = base.GetColor(graphics);
            float maxSize = 4 * OutlineWidth;
            if (graphics.view.mode == MapView.ViewMode.TopDown && graphics.pixelsPerUnit.Y < maxSize / Size)
                c.W *= (graphics.pixelsPerUnit.Y * Size - 2) / (maxSize - 2);
            return c;
        }
        protected readonly Func<float, float> GetSize;
        protected readonly string name;
        protected MapGridlinesObject(string name, Func<float, float> GetSize)
            : base()
        {
            Size = 2;
            OutlineWidth = 3;
            OutlineColor = Color.Black;
            this.name = name;
            this.GetSize = GetSize;
        }

        public static MapGridlinesObject CreateCustom(ObjectCreateParams creationParameters) => new MapGridlinesObject("Custom Gridlines", _ => _) { Size = 0x2000 };

        public static MapGridlinesObject CreateUnits(ObjectCreateParams creationParameters) => new MapGridlinesObject("Unit Gridlines", _ => 1) { OutlineWidth = 1, Size = 1 };

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            var _contextMenuStrip = base.GetContextMenuStrip(targetTracker);

            ToolStripMenuItem itemSetHorizontalExpanse = new ToolStripMenuItem("[3D] Set horizontal expanse");
            itemSetHorizontalExpanse.Click += (sender, e) =>
            {
                string text = DialogUtilities.GetStringFromDialog(labelText: "Enter the number of horizontal cells away from the center:");
                int? hExpanse = ParsingUtilities.ParseIntNullable(text);
                if (hExpanse.HasValue && hExpanse.Value > 0)
                    _hExpanse = hExpanse.Value;
            };
            ToolStripMenuItem itemSetVerticalExpanse = new ToolStripMenuItem("[3D] Set vertical expanse");
            itemSetVerticalExpanse.Click += (sender, e) =>
            {
                string text = DialogUtilities.GetStringFromDialog(labelText: "Enter the number of vertical cells away from the center:");
                int? vExpanse = ParsingUtilities.ParseIntNullable(text);
                if (vExpanse.HasValue && vExpanse.Value > 0)
                    _vExpanse = vExpanse.Value;
            };
            ToolStripMenuItem itemSetHorizontalLineDivider = new ToolStripMenuItem("[3D] Set vertical line distance");
            itemSetHorizontalLineDivider.Click += (sender, e) =>
            {
                string text = DialogUtilities.GetStringFromDialog(labelText: "Enter the number of cells between each vertical line (0 for none):");
                int? horizontalLineDivider = ParsingUtilities.ParseIntNullable(text);
                if (horizontalLineDivider.HasValue && horizontalLineDivider.Value >= 0)
                    _verticalLineDistance = horizontalLineDivider.Value;
            };

            _contextMenuStrip.Items.Add(itemSetHorizontalExpanse);
            _contextMenuStrip.Items.Add(itemSetVerticalExpanse);
            _contextMenuStrip.Items.Add(itemSetHorizontalLineDivider);

            return _contextMenuStrip;
        }

        protected override List<Vector3> GetVertices(MapGraphics graphics)
        {
            List<Vector3> vertices = new List<Vector3>();
            AddVerticesToPositionAngle(graphics,
                vertices,
                graphics.mapCursorPosition,
                _hExpanse,
                _vExpanse,
                graphics.view.mode == MapView.ViewMode.ThreeDimensional ? 1 : float.NaN,
                _verticalLineDistance);
            return vertices;
        }

        protected void AddVerticesToPositionAngle(
            MapGraphics graphics,
            List<Vector3> vertices,
            Vector3 positionAngle,
            int horizontalExpanse = 0,
            int verticalExpanse = 0,
            float verticalMultiplier = float.NaN,
            float verticalConnectorSpacing = float.NaN)
        {
            float increment = Size;
            if (increment == 0)
                return;

            float viewXMin, viewXMax, viewZMin, viewZMax;

            float hExpanse = horizontalExpanse * Size;
            float vExpanse = verticalExpanse * Size;

            if (graphics.view.mode == MapView.ViewMode.TopDown)
            {
                if (graphics.pixelsPerUnit.X < 2 / Size || graphics.pixelsPerUnit.Y < 2 / Size)
                    return;

                viewXMin = (int)(graphics.MapViewXMin / increment - 1) * increment;
                viewXMax = (int)(graphics.MapViewXMax / increment + 1) * increment;
                viewZMin = (int)(graphics.MapViewZMin / increment - 1) * increment;
                viewZMax = (int)(graphics.MapViewZMax / increment + 1) * increment;
            }
            else
            {
                viewXMin = (int)((positionAngle.X - hExpanse) / increment - 1) * increment;
                viewXMax = (int)((positionAngle.X + hExpanse) / increment + 1) * increment;
                viewZMin = (int)((positionAngle.Z - hExpanse) / increment - 1) * increment;
                viewZMax = (int)((positionAngle.Z + hExpanse) / increment + 1) * increment;
            }

            float minY, maxY;
            bool is3DGrid = !float.IsNaN(verticalMultiplier);
            if (!is3DGrid)
                minY = maxY = positionAngle.Y;
            else
            {
                float verticalIncrement = increment * verticalMultiplier;
                minY = (int)((positionAngle.Y - vExpanse) / verticalIncrement - 1) * verticalIncrement;
                maxY = (int)((positionAngle.Y + vExpanse) / verticalIncrement + 1) * verticalIncrement;
            }
            for (float y = minY; y <= maxY; y += verticalMultiplier)
            {
                for (var x = viewXMin; x <= viewXMax; x += increment)
                {
                    vertices.Add(new Vector3(x, y, viewZMin));
                    vertices.Add(new Vector3(x, y, viewZMax));
                }
                for (var z = viewZMin; z <= viewZMax; z += increment)
                {
                    vertices.Add(new Vector3(viewXMin, y, z));
                    vertices.Add(new Vector3(viewXMax, y, z));
                }
            }
            if (is3DGrid && verticalConnectorSpacing != 0)
            {
                var scaledIncrement = increment * verticalConnectorSpacing;
                viewXMin = (int)((positionAngle.X - hExpanse) / scaledIncrement) * scaledIncrement;
                viewXMax = (int)((positionAngle.X + hExpanse) / scaledIncrement) * scaledIncrement;
                viewZMin = (int)((positionAngle.Z - hExpanse) / scaledIncrement) * scaledIncrement;
                viewZMax = (int)((positionAngle.Z + hExpanse) / scaledIncrement) * scaledIncrement;
                for (var z = viewZMin; z <= viewZMax; z += scaledIncrement)
                    for (var x = viewXMin; x <= viewXMax; x += scaledIncrement)
                    {
                        vertices.Add(new Vector3(x, minY, z));
                        vertices.Add(new Vector3(x, maxY, z));
                    }
            }
        }

        public override string GetName() => name;

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.CustomGridlinesImage;

        public override (SaveSettings save, LoadSettings load) SettingsSaveLoad => (
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.save(node);
                SaveValueNode(node, "HorizontalExpanse", _hExpanse.ToString());
                SaveValueNode(node, "VerticalExpanse", _vExpanse.ToString());
                SaveValueNode(node, "VerticalLineDistance", _verticalLineDistance.ToString());
            }
        ,
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.load(node);
                if (int.TryParse(LoadValueNode(node, "HorizontalExpanse"), out int hExpanse))
                    _hExpanse = hExpanse;
                if (int.TryParse(LoadValueNode(node, "VerticalExpanse"), out int vExpanse))
                    _vExpanse = vExpanse;
                if (int.TryParse(LoadValueNode(node, "VerticalLineDistance"), out int verticalLineDistance))
                    _verticalLineDistance = verticalLineDistance;
            }
        );
    }
}
