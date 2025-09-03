using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using OpenTK;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using System;
using System.Windows.Forms;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Iwerlipses", "Movement")]
    public class MapIwerlipsesObject : MapObject
    {
        public MapIwerlipsesObject() : base(null)
        {
            Opacity = 0.5;
            Color = Color.Red;
        }

        uint _numQSteps = 4;
        MarioState _marioState;
        bool _lockPositions => itemLockPositions.Checked;
        ToolStripMenuItem itemLockPositions;
        List<Matrix4> _ellipseTransforms = new List<Matrix4>();

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            var _contextMenuStrip = base.GetContextMenuStrip(targetTracker);

            itemLockPositions = new ToolStripMenuItem("Lock positions");
            itemLockPositions.Click += (_, __) => itemLockPositions.Checked = !itemLockPositions.Checked;
            _contextMenuStrip.Items.Add(itemLockPositions);

            var itemSetNumQSteps = new ToolStripMenuItem("Set number of quarter-steps");
            itemSetNumQSteps.Click += (_, __) => DialogUtilities.UpdateNumberFromDialog(ref _numQSteps, "4", "Enter number of quarter-steps:");
            _contextMenuStrip.Items.Add(itemSetNumQSteps);


            return _contextMenuStrip;
        }

        public override (SaveSettings save, LoadSettings load) SettingsSaveLoad => (
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.save(node);
                SaveValueNode(node, "NumQSteps", _numQSteps.ToString());
            }
        ,
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.load(node);
                if (uint.TryParse(LoadValueNode(node, "NumQSteps"), out uint numQSteps))
                    _numQSteps = numQSteps;
            }
        );

        protected override void DrawTopDown(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                var color = ColorUtilities.ColorToVec4(Color, OpacityByte);
                var outlineColor = ColorUtilities.ColorToVec4(OutlineColor);
                foreach (var transform in _ellipseTransforms)
                    graphics.circleRenderer.AddInstance(
                        graphics.view.mode != MapView.ViewMode.TopDown,
                        transform,
                        OutlineWidth,
                        color,
                        outlineColor);
            });
        }

        protected override void Draw3D(MapGraphics graphics) => DrawTopDown(graphics);

        protected override void DrawOrthogonal(MapGraphics graphics) { } //Nope lol

        public override void Update()
        {
            base.Update();

            if (!_lockPositions)
            {
                _marioState = MarioState.CreateMarioState();
            }

            _ellipseTransforms.Clear();
            for (int it_numQSteps = 1; it_numQSteps <= _numQSteps; it_numQSteps++)
            {
                MarioState marioStateCenter = AirMovementCalculator.ApplyInputRepeatedly(_marioState, RelativeDirection.Center, it_numQSteps);
                MarioState marioStateForward = AirMovementCalculator.ApplyInputRepeatedly(_marioState, RelativeDirection.Forward, it_numQSteps);
                MarioState marioStateBackward = AirMovementCalculator.ApplyInputRepeatedly(_marioState, RelativeDirection.Backward, it_numQSteps);
                MarioState marioStateLeft = AirMovementCalculator.ApplyInputRepeatedly(_marioState, RelativeDirection.Left, it_numQSteps);

                ushort marioAngle = _marioState.MarioAngle;
                (float cx, float cz) = (marioStateCenter.X, marioStateCenter.Z);
                (float fx, float fz) = (marioStateForward.X, marioStateForward.Z);
                (float bx, float bz) = (marioStateBackward.X, marioStateBackward.Z);
                (float lx, float lz) = (marioStateLeft.X, marioStateLeft.Z);

                double sideDist = MoreMath.GetDistanceBetween(cx, cz, lx, lz);
                double forwardDist = MoreMath.GetDistanceBetween(cx, cz, fx, fz);
                double backwardDist = MoreMath.GetDistanceBetween(cx, cz, bx, bz);

                var newTransform = Matrix4.Identity
                                    * Matrix4.CreateRotationX((float)Math.PI / 2)
                                    * Matrix4.CreateScale((float)sideDist, 1, (float)Math.Abs(forwardDist + backwardDist) / 2)
                                    * Matrix4.CreateRotationY((float)MoreMath.AngleUnitsToRadians(marioStateCenter.MarioAngle))
                                    * Matrix4.CreateTranslation(marioStateCenter.X, marioStateCenter.Y, marioStateCenter.Z);
                _ellipseTransforms.Add(newTransform);
            }
        }

        public override IHoverData GetHoverData(MapGraphics graphics, ref Vector3 position) => null;

        public override string GetName() => "Iwerlipses";

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.IwerlipsesImage;
    }
}
