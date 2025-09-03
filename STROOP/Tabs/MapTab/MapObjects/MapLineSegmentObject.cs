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
    [ObjectDescription("Line Segment", "Misc", nameof(Create))]
    public class MapLineSegmentObject : MapLineObject
    {
        private PositionAngle _posAngle1;
        private PositionAngle _posAngle2;
        private bool _useFixedSize;
        private float _backwardsSize;

        public MapLineSegmentObject(PositionAngle posAngle1, PositionAngle posAngle2)
            : base()
        {
            _posAngle1 = posAngle1;
            _posAngle2 = posAngle2;
            _useFixedSize = false;
            _backwardsSize = 0;

            Size = 0;
            OutlineWidth = 3;
            OutlineColor = Color.Red;
        }

        public static MapObject Create(ObjectCreateParams creationParameters)
        {
            string text1 = DialogUtilities.GetStringFromDialog(labelText: "Enter the first PositionAngle.");
            if (text1 == null) return null;
            string text2 = DialogUtilities.GetStringFromDialog(labelText: "Enter the second PositionAngle.");
            if (text2 == null) return null;
            PositionAngle posAngle1 = PositionAngle.FromString(text1);
            PositionAngle posAngle2 = PositionAngle.FromString(text2);
            if (posAngle1 == null || posAngle2 == null) return null;
            return new MapLineSegmentObject(posAngle1, posAngle2);
        }

        protected override List<Vector3> GetVertices(MapGraphics graphics)
        {
            (double x1, double y1, double z1, double angle1) = _posAngle1.GetValues();
            (double x2, double y2, double z2, double angle2) = _posAngle2.GetValues();
            double dist = PositionAngle.GetHDistance(_posAngle1, _posAngle2);
            (double startX, double startZ) = MoreMath.ExtrapolateLine2D(x2, z2, x1, z1, dist + _backwardsSize);
            (double endX, double endZ) = MoreMath.ExtrapolateLine2D(x1, z1, x2, z2, (_useFixedSize ? 0 : dist) + Size);

            var vertices = new List<Vector3>();
            vertices.Add(new Vector3((float)startX, 0, (float)startZ));
            vertices.Add(new Vector3((float)endX, 0, (float)endZ));
            return vertices;
        }

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            ToolStripMenuItem itemUseFixedSize = new ToolStripMenuItem("Use Fixed Size");
            itemUseFixedSize.Click += (sender, e) =>
            {
                _useFixedSize = !_useFixedSize;
                itemUseFixedSize.Checked = _useFixedSize;
            };

            ToolStripMenuItem itemSetBackwardsSize = new ToolStripMenuItem("Set Backwards Size...");
            itemSetBackwardsSize.Click += (sender, e) =>
            {
                string text = DialogUtilities.GetStringFromDialog(labelText: "Enter backwards size.");
                double? backwardsSizeNullable = ParsingUtilities.ParseDoubleNullable(text);
                if (!backwardsSizeNullable.HasValue) return;
                _backwardsSize = (float)backwardsSizeNullable.Value;
            };

            var _contextMenuStrip = new ContextMenuStrip();
            _contextMenuStrip.Items.Add(itemUseFixedSize);
            _contextMenuStrip.Items.Add(itemSetBackwardsSize);
            return _contextMenuStrip;
        }

        public override string GetName()
        {
            return "Line Segment";
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.ArrowImage;
    }
}
