using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using System.Windows.Forms;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Previous Positions", "Movement")]
    public class MapPreviousPositionsObject : MapObject
    {
        public struct DataPoint((float x, float y, float z, ushort angle, ushort _) srcData, Lazy<Image> tex)
        {
            public float x = srcData.x, y = srcData.y, z = srcData.z, angle = srcData.angle;
            public Lazy<Image> tex = tex;

            public bool ExactMatch(DataPoint other)
                => x == other.x && y == other.y && z == other.z && angle == other.angle;
        }

        const uint BUFFER_DATA_BASE_OFFSET = 0x807F4800; // this goes to the very end of RDRAM for now

        static readonly Lazy<Image>[] MARIO_IMAGES =
        [
            Config.ObjectAssociations.PinkMarioMapImage, // Initial
            Config.ObjectAssociations.YellowMarioMapImage, // After warp_area
            Config.ObjectAssociations.PurpleMarioMapImage, // After check_instant_warp
            Config.ObjectAssociations.GreyMarioMapImage, // After platform displacement
            Config.ObjectAssociations.TurquoiseMarioMapImage, // After initial wall check A
            Config.ObjectAssociations.GreenMarioMapImage, // After initial wall check B
            Config.ObjectAssociations.BrownMarioMapImage, // After object interactions

            // These are used for each quarter-step
            Config.ObjectAssociations.OrangeMarioMapImage, // Intended position
            Config.ObjectAssociations.TurquoiseMarioMapImage, // After wall check A
            Config.ObjectAssociations.GreenMarioMapImage, // After wall check B
            Config.ObjectAssociations.BlueMarioMapImage, // After floor check
        ];

        private DateTime _showEachPointStartTime = DateTime.MinValue;
        uint numFramesToShow = 16;

        ToolStripMenuItem itemSkipIdenticalPoints = new ToolStripMenuItem("Skip identical points");
        bool skipIdenticalPoints {get => itemSkipIdenticalPoints.Checked; set => itemSkipIdenticalPoints.Checked = value; }

        public MapPreviousPositionsObject()
            : base()
        {
            InternalRotates = true;
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.NextPositionsImage;

        public override string GetName()
        {
            return "Previous Positions";
        }

        public override float GetY()
        {
            return (float)PositionAngle.Mario.Y;
        }

        protected override void DrawTopDown(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                var data = GetData();
                foreach (var dataPoint in data)
                    DrawIcon(
                        graphics,
                        graphics.view.mode == MapView.ViewMode.ThreeDimensional,
                        dataPoint.x, dataPoint.y, dataPoint.z, dataPoint.angle,
                        dataPoint.tex.Value,
                        new Vector4(1));

                if (OutlineWidth != 0)
                {
                    var color = ColorUtilities.ColorToVec4(OutlineColor, OpacityByte);
                    for (int i = 0; i < data.Count - 1; i++)
                        graphics.lineRenderer.Add(
                            new Vector3(data[i].x, data[i].y, data[i].z),
                            new Vector3(data[i + 1].x, data[i + 1].y, data[i + 1].z),
                            color,
                            OutlineWidth);
                }
            });
        }

        protected override void DrawOrthogonal(MapGraphics graphics) => DrawTopDown(graphics);

        public List<DataPoint> GetData()
        {
            double secondsPerPoint = 0.5;
            double elapsedSeconds = DateTime.Now.Subtract(_showEachPointStartTime).TotalSeconds;
            int pointToShow = (int)(elapsedSeconds / secondsPerPoint);
            bool showSinglePoint = _showEachPointStartTime != DateTime.MinValue;

            uint globalTimer = Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress);

            const int numBaseFrames = 7;

            List<DataPoint> allResults = new List<DataPoint>();

            for (int frame = (int)(numFramesToShow); frame > 0; frame--)
            {
                var expectedGt = globalTimer - frame;
                var qsData = new (float qsX, float qsY, float qsZ, ushort qsA, ushort gtLo)[numBaseFrames + 4 * 4];
                var baseOffset = BUFFER_DATA_BASE_OFFSET + (expectedGt & 0x7F) * qsData.Length * 0x10;
                for (int i = 0; i < qsData.Length; i++)
                    qsData[i] = (
                        Config.Stream.GetSingle((uint)(baseOffset + 0x10 * i)),
                        Config.Stream.GetSingle((uint)(baseOffset + 4 + 0x10 * i)),
                        Config.Stream.GetSingle((uint)(baseOffset + 8 + 0x10 * i)),
                        Config.Stream.GetUInt16((uint)(baseOffset + 0xE + 0x10 * i)),
                        Config.Stream.GetUInt16((uint)(baseOffset + 0xC + 0x10 * i)));

                if (qsData[0].gtLo != (ushort)expectedGt)
                    continue;

                for (int i = 0; i < numBaseFrames; i++)
                    if (qsData[i].gtLo == expectedGt)
                        if (AddOrYieldIfNew(new DataPoint(qsData[i], MARIO_IMAGES[i])))
                            return [allResults[^1]];

                for (int i = 0; i < 4; i++)
                {
                    int baseIndex = numBaseFrames + i * 4;
                    if (qsData[baseIndex].gtLo != (ushort)expectedGt)
                        break;

                    for (int k = 0; k < 4; k++)
                    {
                        if (AddOrYieldIfNew(new DataPoint(qsData[baseIndex + k], MARIO_IMAGES[k + 7])))
                            return [allResults[^1]];
                    }
                }
            }

            if (showSinglePoint)
            {
                int count = 0;
                foreach (var point in allResults)
                    if (count++ == pointToShow)
                        return [point];
            }

            _showEachPointStartTime = DateTime.MinValue;
            return allResults;

            bool AddOrYieldIfNew(DataPoint dataPoint)
            {
                if (allResults.Count == 0 || (showSinglePoint && !skipIdenticalPoints) || !allResults[^1].ExactMatch(dataPoint))
                {
                    allResults.Add(dataPoint);
                    return showSinglePoint && allResults.Count == pointToShow;
                }

                return false;
            }
        }

        public override bool ParticipatesInGlobalIconSize() => true;

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            var _contextMenuStrip = base.GetContextMenuStrip(targetTracker);
            ToolStripMenuItem itemShowEachPoint = new ToolStripMenuItem("Show Each Point");
            itemShowEachPoint.Click += (sender, e) => { _showEachPointStartTime = DateTime.Now; };

            ToolStripMenuItem itemSetNumFrames = new ToolStripMenuItem("Set Number of Frames");
            itemSetNumFrames.Click += (sender, e) =>
            {
                string text = DialogUtilities.GetStringFromDialog(labelText: "Enter num frames (1 - 128):");
                uint? numFramesNullable = ParsingUtilities.ParseUIntNullable(text);
                if (!numFramesNullable.HasValue) return;
                numFramesToShow = numFramesNullable.Value;
            };

            skipIdenticalPoints = true;
            itemSkipIdenticalPoints.Click += (sender, e) => skipIdenticalPoints = !skipIdenticalPoints;

            _contextMenuStrip = new ContextMenuStrip();
            _contextMenuStrip.Items.Add(itemShowEachPoint);
            _contextMenuStrip.Items.Add(itemSkipIdenticalPoints);
            _contextMenuStrip.Items.Add(itemSetNumFrames);

            return _contextMenuStrip;
        }
    }
}
