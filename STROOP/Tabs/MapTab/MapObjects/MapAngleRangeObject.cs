using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using System.Windows.Forms;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public class MapAngleRangeObject : MapLineObject
    {
        private bool _useRelativeAngles;
        private int _angleDiff;

        private ToolStripMenuItem _itemUseRelativeAngles;

        public MapAngleRangeObject(PositionAngleProvider positionAngleProvider)
        {
            this.positionAngleProvider = positionAngleProvider;

            _useRelativeAngles = false;
            _angleDiff = 16;

            Size = 1000;
            OutlineWidth = 1;
            OutlineColor = Color.Black;
        }

        protected override List<Vector3> GetVertices(MapGraphics graphics)
        {
            List<Vector3> vertices = new List<Vector3>();
            foreach (var _posAngle in positionAngleProvider())
            {
                int startingAngle = _useRelativeAngles ? MoreMath.NormalizeAngleTruncated(_posAngle.Angle) : 0;
                for (int angle = startingAngle; angle < startingAngle + 65536; angle += _angleDiff)
                {
                    (double x1, double y1, double z1, double a) = _posAngle.GetValues();
                    (double x2, double z2) = MoreMath.AddVectorToPoint(Size, angle, x1, z1);
                    vertices.Add(new Vector3((float)x1, (float)y1, (float)z1));
                    vertices.Add(new Vector3((float)x2, (float)y1, (float)z2));
                }
            }
            return vertices;
        }

        public override string GetName() => $"Angle Range for {PositionAngle.NameOfMultiple(positionAngleProvider())}";

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.CustomGridlinesImage;

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            _itemUseRelativeAngles = new ToolStripMenuItem("Use Relative Angles");
            _itemUseRelativeAngles.Click += (sender, e) => _itemUseRelativeAngles.Checked = !_itemUseRelativeAngles.Checked;

            ToolStripMenuItem itemSetAngleDiff = new ToolStripMenuItem("Set Angle Diff");
            itemSetAngleDiff.Click += (sender, e) =>
            {
                string text = DialogUtilities.GetStringFromDialog(labelText: "Enter angle diff.");
                int? angleDiff = ParsingUtilities.ParseIntNullable(text);
                if (angleDiff.HasValue && angleDiff.Value > 0)
                    _angleDiff = angleDiff.Value;
            };

            var _contextMenuStrip = new ContextMenuStrip();
            _contextMenuStrip.Items.Add(_itemUseRelativeAngles);
            _contextMenuStrip.Items.Add(itemSetAngleDiff);

            return _contextMenuStrip;
        }

        public override (SaveSettings, LoadSettings) SettingsSaveLoad => (
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.save(node);
                SaveValueNode(node, "UseRelativeAngles", _itemUseRelativeAngles.ToString());
                SaveValueNode(node, "AngleDiff", _angleDiff.ToString());
            }
        ,
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.load(node);
                if (bool.TryParse(LoadValueNode(node, "UseRelativeAngles"), out bool useRelativeAngles))
                    _itemUseRelativeAngles.Checked = useRelativeAngles;
                if (int.TryParse(LoadValueNode(node, "AngleDiff"), out int angleDiff))
                    _angleDiff = angleDiff;
            }
        );
    }
}
