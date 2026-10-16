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
using STROOP.Variables.SM64MemoryLayout;
using STROOP.Variables.Utilities;

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
                    DrawIcon(graphics, graphics.viewMode == MapGraphics.ViewMode.ThreeDimensional, dataPoint.x, dataPoint.y, dataPoint.z, dataPoint.angle, dataPoint.tex?.Value, new Vector4(1));
            });
        }

        protected override void DrawOrthogonal(MapGraphics graphics) => DrawTopDown(graphics);

        protected override void Draw3D(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                List<Vector3> path = Positions3D(graphics, out ushort marioAngle);

                Vector4 lineColor = Get3DAccentColor(Color.Yellow);
                for (int i = 0; i < path.Count - 1; i++)
                    graphics.lineRenderer.Add(path[i], path[i + 1], lineColor, 2);

                List<(float x, float y, float z, float angle, Lazy<Image> tex)> data = ToData(path.Skip(1).ToList(), marioAngle);
                if (graphics.marioModelAvailable)
                {
                    foreach (var dataPoint in data)
                        graphics.marioModelCopies.Add(new Vector3(dataPoint.x, dataPoint.y, dataPoint.z));
                    return;
                }

                data.Reverse();
                foreach (var dataPoint in data)
                {
                    DrawIcon(graphics, true, dataPoint.x, dataPoint.y, dataPoint.z, dataPoint.angle, dataPoint.tex?.Value, new Vector4(1));
                    DrawFacingLine3D(graphics, dataPoint.x, dataPoint.y, dataPoint.z, dataPoint.angle);
                }
            });
        }

        private List<Vector3> Positions3D(MapGraphics graphics, out ushort marioAngle)
        {
            float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
            float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
            float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
            float ySpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YSpeedOffset);
            float marioHSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
            marioAngle = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);

            float floorY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.FloorYOffset);
            bool onGround = marioY == floorY;
            float multiplier = 1;
            if (onGround)
            {
                uint floorTri = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset);
                multiplier = Config.Stream.GetSingle(floorTri + TriangleOffsetsConfig.NormY);
            }

            float effectiveSpeed = marioHSpeed * multiplier;
            var floors = graphics.floors.GetTriangles();

            List<Vector3> path = new List<Vector3> { new Vector3(marioX, marioY, marioZ) };
            float y = marioY;
            int numQuarterSteps = (int)(_numFrames * 4);
            for (int i = 0; i < numQuarterSteps; i++)
            {
                (double px, double pz) = MoreMath.AddVectorToPoint((0.25 + i / 4.0) * effectiveSpeed, marioAngle, marioX, marioZ);
                float x = (float)px, z = (float)pz;
                var (floor, newFloorY) = floors.FindFloorAndY(x, y, z);
                bool hasFloor = floor != null && !float.IsNaN(newFloorY);
                if (onGround)
                {
                    if (hasFloor && y > newFloorY + 100)
                    {
                        onGround = false;
                        ySpeed = 0;
                    }
                    else if (hasFloor)
                        y = newFloorY;
                }
                else
                {
                    y += ySpeed / 4;
                    if (hasFloor && y <= newFloorY)
                    {
                        y = newFloorY;
                        onGround = true;
                    }
                }

                if (i % 4 == 3 && !onGround)
                    ySpeed = Math.Max(ySpeed - 4, -75);

                path.Add(new Vector3(x, y, z));
            }

            return path;
        }

        private List<(float x, float y, float z, float angle, Lazy<Image> tex)> ToData(List<Vector3> quarterSteps, ushort marioAngle)
        {
            var fullStepTex = _useColoredMarios ? Config.ObjectAssociations.BlueMarioMapImage : Config.ObjectAssociations.MarioMapImage;
            var quarterStepTex = _useColoredMarios ? Config.ObjectAssociations.OrangeMarioMapImage : Config.ObjectAssociations.MarioMapImage;
            List<(float x, float y, float z, float angle, Lazy<Image> tex)> data =
                new List<(float x, float y, float z, float angle, Lazy<Image> tex)>();
            for (int i = 0; i < quarterSteps.Count; i++)
            {
                bool isFullStep = i % 4 == 3;
                if (!isFullStep && !_showQuarterSteps) continue;
                var tex = isFullStep ? fullStepTex : quarterStepTex;
                data.Add((quarterSteps[i].X, quarterSteps[i].Y, quarterSteps[i].Z, marioAngle, tex));
            }

            return data;
        }

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

            return ToData(points2D.ConvertAll(p => new Vector3(p.x, marioY, p.z)), marioAngle);
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
