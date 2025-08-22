using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using OpenTK;
using System.Windows.Forms;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Previous Positions", "Movement")]
    public class MapPreviousPositionsObject : MapObject
    {
        public struct DataPoint
        {
            public float x, y, z, angle;
            public Lazy<Image> tex;
            public DataPoint(float x, float y, float z, float angle, Lazy<Image> tex)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.angle = angle;
                this.tex = tex;
            }
        }

        private DateTime _showEachPointStartTime = DateTime.MinValue;
        uint numFramesToShow = 1; uint firstRecord;
        Dictionary<uint, List<DataPoint>> dataByFrame = new Dictionary<uint, List<DataPoint>>();

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
            Lazy<Image>[] marioImages = new[] {
                Config.ObjectAssociations.PinkMarioMapImage,
                Config.ObjectAssociations.YellowMarioMapImage,
                Config.ObjectAssociations.PurpleMarioMapImage,
                Config.ObjectAssociations.GreyMarioMapImage,
                Config.ObjectAssociations.TurquoiseMarioMapImage,
                Config.ObjectAssociations.GreenMarioMapImage,
                Config.ObjectAssociations.BrownMarioMapImage,

                Config.ObjectAssociations.OrangeMarioMapImage,
                Config.ObjectAssociations.TurquoiseMarioMapImage,
                Config.ObjectAssociations.GreenMarioMapImage,
                Config.ObjectAssociations.BlueMarioMapImage,
            };

            uint READ_INITIAL_OFFSET = RomVersionConfig.Version == RomVersion.US ? 0x80372F00 : 0x80400010;

            var dsjaoisd = Config.Stream.GetUInt32(0x803733c0);

            uint globalTimer = Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress);

            var qsData = new(float qsX, float qsY, float qsZ, ushort qsA)[7 + 4 * 4];
            for (int i = 0; i < qsData.Length; i++)
                qsData[i] = (
                    Config.Stream.GetSingle((uint)(READ_INITIAL_OFFSET + 0x10 * i)),
                    Config.Stream.GetSingle((uint)(READ_INITIAL_OFFSET + 4 + 0x10 * i)),
                    Config.Stream.GetSingle((uint)(READ_INITIAL_OFFSET + 8 + 0x10 * i)),
                    Config.Stream.GetUInt16((uint)(READ_INITIAL_OFFSET + 0xE + 0x10 * i)));

            int numBaseFrames = 7;

            var val = Config.Stream.GetInt32(RomVersionConfig.Version == RomVersion.US ? 0x80372E3C : 0x80400000);
            int numQFrames = (val - 0x10 * numBaseFrames) / 0x40;

            List<DataPoint> allResults = new List<DataPoint>();
            for (int i = 0; i < numBaseFrames; i++)
                allResults.Add(new DataPoint(qsData[i].qsX, qsData[i].qsY, qsData[i].qsZ, qsData[i].qsA, marioImages[i]));
            numQFrames = Math.Min(numQFrames, 4);
            for (int i = 0; i < numQFrames; i++)
            {
                int baseIndex = numBaseFrames + i * 4;
                for (int k = 0; k < 4; k++)
                    allResults.Add(new DataPoint(qsData[baseIndex + k].qsX, qsData[baseIndex + k].qsY, qsData[baseIndex + k].qsZ, qsData[baseIndex + k].qsA, marioImages[k + 7]));
            }

            var funny = globalTimer - numFramesToShow;
            int maxDelettions = 0;
            for (uint record = firstRecord; record <= funny && maxDelettions++ < 100; record++)
                dataByFrame.Remove(record);

            dataByFrame[globalTimer] = allResults;
            firstRecord = funny;
            double secondsPerPoint = 0.5;
            double elapsedSeconds = DateTime.Now.Subtract(_showEachPointStartTime).TotalSeconds;
            int pointToShow = (int)(elapsedSeconds / secondsPerPoint);
            bool showSinglePoint = _showEachPointStartTime != DateTime.MinValue;

            List<DataPoint> combinedResults = new List<DataPoint>();
            int count = 0;
            for (long frame = globalTimer - numFramesToShow + 1; frame <= globalTimer; frame++)
            {
                if (dataByFrame.TryGetValue((uint)frame, out var datas))
                    foreach (var dataPoint in datas)
                    {
                        if (showSinglePoint && count == pointToShow)
                            return new List<DataPoint>(new[] { dataPoint });
                        count++;
                        combinedResults.Insert(0, dataPoint);
                    }
            }

            _showEachPointStartTime = DateTime.MinValue;
            return combinedResults;
        }

        public override bool ParticipatesInGlobalIconSize() => true;

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            var _contextMenuStrip = base.GetContextMenuStrip(targetTracker);
            ToolStripMenuItem itemShowEachPoint = new ToolStripMenuItem("Show Each Point");
            itemShowEachPoint.Click += (sender, e) =>
            {
                _showEachPointStartTime = DateTime.Now;
            };

            ToolStripMenuItem itemSetNumFrames = new ToolStripMenuItem("Set Number of Frames");
            itemSetNumFrames.Click += (sender, e) =>
            {
                string text = DialogUtilities.GetStringFromDialog(labelText: "Enter num frames.");
                uint? numFramesNullable = ParsingUtilities.ParseUIntNullable(text);
                if (!numFramesNullable.HasValue) return;
                numFramesToShow = numFramesNullable.Value;

            };

            _contextMenuStrip = new ContextMenuStrip();
            _contextMenuStrip.Items.Add(itemShowEachPoint);
            _contextMenuStrip.Items.Add(itemSetNumFrames);

            return _contextMenuStrip;
        }
    }
}
