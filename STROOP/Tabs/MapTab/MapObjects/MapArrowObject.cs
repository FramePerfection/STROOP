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
    public class MapArrowObject : MapLineObject
    {
        public delegate double GetYaw(PositionAngle obj);
        public delegate double GetRecommendedSize(PositionAngle obj);

        public GetYaw getYaw;
        public GetRecommendedSize getRecommendedSize;

        private float _arrowHeadSideLength;
        double _yawOffset;

        private ToolStripMenuItem _itemRecommendedArrowLength;
        private bool useRecommendedArrowLength => _itemRecommendedArrowLength.Checked;

        string name;

        public MapArrowObject(PositionAngleProvider positionAngleProvider, GetYaw getYaw, GetRecommendedSize getRecommendedSize, string name)
            : base()
        {
            this.positionAngleProvider = positionAngleProvider;
            this.getYaw = getYaw;
            this.getRecommendedSize = getRecommendedSize;
            this.name = name;
            _arrowHeadSideLength = 100;

            Size = 300;
            OutlineWidth = 3;
            OutlineColor = Color.Yellow;
        }

        protected override void DrawTopDown(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {

                Vector4 color = GetColor(graphics);
                foreach (var posAngle in positionAngleProvider())
                    graphics.lineRenderer.AddArrow(
                        (float)posAngle.X, (float)posAngle.Y, (float)posAngle.Z,
                        useRecommendedArrowLength ? (float)getRecommendedSize(posAngle) : Size,
                        (float)(getYaw(posAngle) + _yawOffset),
                        _arrowHeadSideLength,
                        posAngle.GetArrowColor(color),
                        OutlineWidth);
            });
        }
        protected override List<Vector3> GetVertices(MapGraphics graphics) => new List<Vector3>();

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.ArrowImage;

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            var _contextMenuStrip = base.GetContextMenuStrip(targetTracker);

            _itemRecommendedArrowLength = new ToolStripMenuItem("Use Recommended Arrow Size");
            _itemRecommendedArrowLength.Click += (sender, e) => _itemRecommendedArrowLength.Checked = !_itemRecommendedArrowLength.Checked;

            ToolStripMenuItem itemSetArrowHeadSideLength = new ToolStripMenuItem("Set Arrow Head Side Length");
            itemSetArrowHeadSideLength.Click += (sender, e) =>
                DialogUtilities.UpdateNumberFromDialog(ref _arrowHeadSideLength, labelText: "Enter the side length of the arrow head:");

            var itemSetYawOffset = new ToolStripMenuItem("Set Yaw Offset");
            itemSetYawOffset.Click += (sender, e) =>
                DialogUtilities.UpdateNumberFromDialog(
                    ref _yawOffset,
                    textboxText: "0",
                    labelText: "Enter Yaw Offset:",
                    parser: (string str, out double result) =>
                    {
                        if (double.TryParse(str, out double doubleResult))
                        {
                            result = doubleResult;
                            return true;
                        }
                        else if (ParsingUtilities.TryParseHex(str, out uint hexResult))
                        {
                            result = hexResult;
                            return true;
                        }
                        result = double.NaN;
                        return false;
                    });

            var itemSetYawReverse = new ToolStripMenuItem("Reverse Angles");
            itemSetYawReverse.Click += (sender, e) => _yawOffset = itemSetYawReverse.Checked ? 0 : 0x8000;

            _contextMenuStrip = new ContextMenuStrip();
            _contextMenuStrip.Items.Add(_itemRecommendedArrowLength);
            _contextMenuStrip.Items.Add(itemSetArrowHeadSideLength);
            _contextMenuStrip.Items.Add(itemSetYawOffset);
            _contextMenuStrip.Items.Add(itemSetYawReverse);
            _contextMenuStrip.Opening += (_, __) => itemSetYawReverse.Checked = _yawOffset == 0x8000;

            return _contextMenuStrip;
        }

        public override (SaveSettings, LoadSettings) SettingsSaveLoad => (
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.save(node);
                SaveValueNode(node, "UseRecommendedArrowLength", useRecommendedArrowLength.ToString());
                SaveValueNode(node, "ArrowHeadSideLength", _arrowHeadSideLength.ToString());
                SaveValueNode(node, "YawOffset", _yawOffset.ToString());
            }
        ,
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.load(node);
                if (bool.TryParse(LoadValueNode(node, "UseRecommendedArrowLength"), out bool useRecommendedArrowLength))
                    _itemRecommendedArrowLength.Checked = useRecommendedArrowLength;
                if (float.TryParse(LoadValueNode(node, "ArrowHeadSideLength"), out float arrowHeadSideLength))
                    _arrowHeadSideLength = arrowHeadSideLength;
                if (float.TryParse(LoadValueNode(node, "YawOffset"), out float yawOffset))
                    _yawOffset = yawOffset;
            }
        );

        public override string GetName() => $"{name} for {PositionAngle.NameOfMultiple(positionAngleProvider())}";

        public class ArrowSource
        {
            public static GetYaw ObjectFacingYaw = posAngle => Config.Stream.GetUInt16(PositionAngle.GetObjectAddress(posAngle) + ObjectConfig.YawFacingOffset);
            public static GetYaw ObjectGraphicsYaw = posAngle => Config.Stream.GetUInt16(PositionAngle.GetObjectAddress(posAngle) + ObjectConfig.GraphicsYawOffset);
            public static GetYaw ObjectMovingYaw = posAngle => Config.Stream.GetUInt16(PositionAngle.GetObjectAddress(posAngle) + ObjectConfig.YawMovingOffset);
            public static GetYaw ObjectAngleToMario = posAngle => Config.Stream.GetUInt16(PositionAngle.GetObjectAddress(posAngle) + ObjectConfig.AngleToMarioOffset);

            public static GetYaw MarioFacingYaw = posAngle => Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
            public static GetYaw MarioIndendedYaw = posAngle => Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.IntendedYawOffset);
            public static GetYaw MarioMovingYaw = posAngle => Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.MovingYawOffset);
            public static GetYaw MarioSlidingYaw = posAngle => WatchVariableSpecialUtilities.GetMarioSlidingAngle();
            public static GetYaw MarioTwirlingYaw = posAngle => Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.TwirlYawOffset);


            public static GetRecommendedSize ObjectHSpeed = posAngle => Config.Stream.GetSingle(PositionAngle.GetObjectAddress(posAngle) + ObjectConfig.HSpeedOffset);
            public static GetRecommendedSize ObjectDistanceToMario = posAngle => Config.Stream.GetSingle(PositionAngle.GetObjectAddress(posAngle) + ObjectConfig.DistanceToMarioOffset);


            public static GetRecommendedSize MarioHSpeed = posAngle => Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
            public static GetRecommendedSize MarioSlidingSpeed = posAngle => WatchVariableSpecialUtilities.GetMarioSlidingSpeed();


            public static GetRecommendedSize Constant(double length) => _ => length;
        }
    }
}
