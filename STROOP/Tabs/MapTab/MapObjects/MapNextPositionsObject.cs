using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Next Positions", "Movement")]
    public class MapNextPositionsObject : MapIconObject
    {
        private bool _useColoredMarios = true;
        private bool _showQuarterSteps = true;
        private double _numFrames = 4;

        public MapNextPositionsObject()
            : base(null)
        {
            InternalRotates = true;
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.NextPositionsImage;

        public override string GetName() => "Next Positions";

        public override float GetY() => (float)PositionAngle.Mario.Y;

        protected override void DrawTopDown(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                List<(float x, float y, float z, float angle, Lazy<Image> tex)> data = GetData();
                data.Reverse();
                foreach (var dataPoint in data)
                    DrawIcon(graphics, graphics.view.mode == MapView.ViewMode.ThreeDimensional, dataPoint.x, dataPoint.y, dataPoint.z, dataPoint.angle, dataPoint.tex?.Value, new Vector4(1));
            });
        }

        protected override void DrawOrthogonal(MapGraphics graphics) => DrawTopDown(graphics);

        public List<(float x, float y, float z, float angle, Lazy<Image> tex)> GetData()
        {
            float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
            float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
            float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
            float marioYSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YSpeedOffset);
            float marioHSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
            ushort marioAngle = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);

            float floorY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.FloorYOffset);
            float multiplier = 1;
            if (marioY == floorY) // on the ground
            {
                uint floorTri = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset);
                float yNorm = Config.Stream.GetSingle(floorTri + TriangleOffsetsConfig.NormY);
                multiplier = yNorm;
            }
            float effectiveSpeed = marioHSpeed * multiplier;

            List<(float x, float z)> points2D = Enumerable.Range(0, (int)(_numFrames * 4)).ToList()
                .ConvertAll(index => 0.25 + index / 4.0)
                .ConvertAll(frameStep => ((float x, float z))MoreMath.AddVectorToPoint(
                    frameStep * effectiveSpeed, marioAngle, marioX, marioZ));

            var fullStepTex = _useColoredMarios ? Config.ObjectAssociations.BlueMarioMapImage : Config.ObjectAssociations.MarioMapImage;
            var quarterStepTex = _useColoredMarios ? Config.ObjectAssociations.OrangeMarioMapImage : Config.ObjectAssociations.MarioMapImage;
            List<(float x, float y, float z, float angle, Lazy<Image> tex)> data =
                new List<(float x, float y, float z, float angle, Lazy<Image> tex)>();
            for (int i = 0; i < points2D.Count; i++)
            {
                bool isFullStep = i % 4 == 3;
                if (!isFullStep && !_showQuarterSteps) continue;
                (float x, float z) = points2D[i];
                var tex = isFullStep ? fullStepTex : quarterStepTex;
                data.Add((x, marioY, z, marioAngle, tex));
            }
            return data;
        }

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            ToolStripMenuItem itemUseColoredMarios = new ToolStripMenuItem("Use Colored Marios");
            itemUseColoredMarios.Click += (sender, e) =>
            {
                _useColoredMarios = !_useColoredMarios;
                itemUseColoredMarios.Checked = _useColoredMarios;
            };
            itemUseColoredMarios.Checked = _useColoredMarios;

            ToolStripMenuItem itemShowQuarterSteps = new ToolStripMenuItem("Show Quarter Steps");
            itemShowQuarterSteps.Click += (sender, e) =>
            {
                _showQuarterSteps = !_showQuarterSteps;
                itemShowQuarterSteps.Checked = _showQuarterSteps;
            };
            itemShowQuarterSteps.Checked = _showQuarterSteps;

            ToolStripMenuItem itemSetNumFrames = new ToolStripMenuItem("Set Num Frames...");
            itemSetNumFrames.Click += (sender, e) =>
            {
                string text = DialogUtilities.GetStringFromDialog(labelText: "Enter num frames to the nearest 1/4th.");
                double? numFramesNullable = ParsingUtilities.ParseDoubleNullable(text);
                if (!numFramesNullable.HasValue) return;
                double numFrames = numFramesNullable.Value;
                _numFrames = numFrames;
            };

            var _contextMenuStrip = new ContextMenuStrip();
            _contextMenuStrip.Items.Add(itemUseColoredMarios);
            _contextMenuStrip.Items.Add(itemShowQuarterSteps);
            _contextMenuStrip.Items.Add(itemSetNumFrames);
            return _contextMenuStrip;
        }

        public override bool ParticipatesInGlobalIconSize() => true;
    }
}
