using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Mathematics;
using STROOP.Structs.Configurations;
using STROOP.Tabs.MapTab;
using STROOP.Utilities;
using STROOP.Structs;

namespace STROOP.Tabs.BruteforceTab.Surfaces.GeneralPurpose.MethodControllers
{
    class RestrictHPositionMapElement : MapTab.MapObjects.MapObject, ITrackerMethodMapObject<RestrictHPosition>
    {
        class HoverData : IHoverData
        {
            public bool setBase = false;
            readonly RestrictHPositionMapElement parent;

            public HoverData(RestrictHPositionMapElement parent) { this.parent = parent; }

            public void AddContextMenuItems(MapTab.MapTab tab, ContextMenuStrip menu) { }

            public DragMask CanDrag() => parent.dragMask;

            public void DragTo(Vector3 position, bool setY)
            {
                bool snapEnabled = !GlobalKeyboard.IsAltDown();
                if (setBase)
                {
                    position.Y = parent.arrowTip.Y;
                    parent.direction = Vector3.Normalize(parent.arrowTip - position);
                    if (snapEnabled)
                    {
                        double minSnapDist = double.PositiveInfinity;
                        double snapAngle = 0;
                        var angle = Math.Atan2(parent.direction.Z, parent.direction.X);
                        const double snapMargin = 0.02f * Math.PI * 2;
                        const int NUM_DIVISIONS = 8;
                        for (int i = 0; i < NUM_DIVISIONS; i++)
                        {
                            var snapTestAngle = Math.PI * 2.0 / NUM_DIVISIONS * i;
                            var snapTestDist = Math.Abs((angle - snapTestAngle + Math.PI) % (Math.PI * 2));
                            if (snapTestDist < minSnapDist)
                            {
                                minSnapDist = snapTestDist;
                                snapAngle = snapTestAngle;
                            }
                        }
                        if (minSnapDist < snapMargin)
                            parent.direction = -new Vector3((float)Math.Cos(snapAngle), 0, (float)Math.Sin(snapAngle));
                    }
                }
                else
                {
                    parent.arrowTip = position;
                    if (snapEnabled)
                        parent.arrowTip = new Vector3((float)Math.Round(parent.arrowTip.X), parent.arrowTip.Y, (float)Math.Round(parent.arrowTip.Z));
                }
                parent.RecalculateParameters();
            }

            public void LeftClick(Vector3 position) { }

            public void RightClick(Vector3 position) { }

            public void SetLookAt(Vector3 lookAt) { }
        }

        RestrictHPosition parent;
        HoverData hoverData;
        Vector3 direction;
        Vector3 arrowTip;
        float len = 50;
        bool ignoreWatchVarUpdate = false;

        Vector3 arrowBase(MapGraphics graphics) => arrowTip - direction * len / graphics.MapViewScaleValue;

        public override IHoverData GetHoverData(MapGraphics graphics, ref Vector3 position)
        {
            if (graphics.view.mode == MapView.ViewMode.TopDown)
            {
                if (graphics.HoverTopDown(arrowBase(graphics), 15 / graphics.MapViewScaleValue))
                {
                    hoverData.setBase = true;
                    return hoverData;
                }
                var andereDist = Vector3.Dot(graphics.mapCursorPosition, direction) - Vector3.Dot(arrowTip, direction);
                if (Math.Abs(andereDist) < 15 / graphics.MapViewScaleValue)
                {
                    hoverData.setBase = false;
                    return hoverData;
                }
            }
            return null;
        }

        public RestrictHPositionMapElement()
        {
            OutlineColor = Color.Red;
            OutlineWidth = 2;
            enableDragging = true;
            hoverData = new HoverData(this);
            direction = new Vector3(0, 0, 1);
        }

        public void SetParent(RestrictHPosition parent)
        {
            if (this.parent != null)
                throw new InvalidOperationException("How did this happen?");
            this.parent = parent;
            var p = parent;
            Action updateFromControl = () =>
            {
                if (ignoreWatchVarUpdate)
                    return;

                var vars = p.parameterPanel.GetWatchVariablesByName("nx", "nz", "d");
                var dir = new Vector2((float)vars[0].GetNumberValues<double>().First(), (float)vars[1].GetNumberValues<double>().First());
                if (dir.Length == 0)
                    dir.Y = -1;
                var d = Convert.ToSingle((float)vars[2].GetNumberValues<double>().First());
                var delta = (Vector2.Dot(arrowTip.Xz, dir) - d) * dir;
                arrowTip -= new Vector3(delta.X, 0, delta.Y);
                direction = Vector3.Normalize(new Vector3(dir.X, 0, dir.Y));
            };
            foreach (var ctrl in parent.parameterPanel.GetCurrentVariableControls())
                ctrl.view.ValueSet += updateFromControl;
            updateFromControl();
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.ArrowImage;

        public override string GetName() => "RestrictHPosition";

        protected override void DrawTopDown(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                var side = new Vector3(-direction.Z, 0, direction.X);
                var pos1 = arrowTip + side * 50000;
                var pos2 = arrowTip - side * 50000;
                Vector4 color = ColorUtilities.ColorToVec4(OutlineColor, OpacityByte);
                graphics.lineRenderer.Add(pos1, pos2, color, OutlineWidth);

                var yaw = MoreMath.RadiansToAngleUnits(Math.Atan2(direction.X, direction.Z));
                var b = arrowBase(graphics);
                graphics.lineRenderer.AddArrow(b.X, b.Y, b.Z, len / graphics.MapViewScaleValue, (float)yaw, 20 / graphics.MapViewScaleValue, color, OutlineWidth);
                graphics.textRenderer.AddText(
                    $"RestrictHPosition ({parent.GetFuncIndex()?.ToString() ?? "-"})",
                    arrowTip,
                    OutlineColor,
                    StringAlignment.Center);
            });
        }
        protected override void DrawOrthogonal(MapGraphics graphics) => DrawTopDown(graphics);

        void RecalculateParameters()
        {
            ignoreWatchVarUpdate = true;
            var d = Vector2.Dot(arrowTip.Xz, direction.Xz);
            parent.parameterPanel.SetVariableValueByName("nx", (double)direction.X);
            parent.parameterPanel.SetVariableValueByName("nz", (double)direction.Z);
            parent.parameterPanel.SetVariableValueByName("d", (double)d);
            ignoreWatchVarUpdate = false;
        }
    }

    class RestrictHPosition : TrackerMethodControllerBase<RestrictHPositionMapElement, RestrictHPosition>
    {
        protected override RestrictHPositionMapElement CreateMapObject() => new RestrictHPositionMapElement();
    }
}
