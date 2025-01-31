using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using STROOP.Tabs.MapTab;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Core.Variables;

namespace STROOP.Tabs.BruteforceTab.Surfaces.GeneralPurpose.MethodControllers
{
    class XZRadialLimitMapObject : MapTab.MapObjects.MapCircleObject, ITrackerMethodMapObject<XZRadialLimit>
    {
        class HoverData : IHoverData
        {
            readonly XZRadialLimitMapObject parent;
            public HoverData(XZRadialLimitMapObject parent) { this.parent = parent; }

            public void AddContextMenuItems(MapTab.MapTab tab, ContextMenuStrip menu) { }

            public DragMask CanDrag() => parent.dragMask;

            public void DragTo(Vector3 position, bool setY)
            {
                parent.x = position.X;
                parent.z = position.Z;
                parent.UpdateVars();
            }

            public void LeftClick(Vector3 position) { }

            public void RightClick(Vector3 position) { }

            public void SetLookAt(Vector3 lookAt) { }
        }

        readonly HoverData hover;
        XZRadialLimit parent;
        float x, z;
        IBruteforceVariableView[] vars;
        BruteforceVariableView<float> varX => vars[0] as BruteforceVariableView<float>;
        BruteforceVariableView<float> varZ => vars[1] as BruteforceVariableView<float>;
        BruteforceVariableView<float> varSize => vars[2] as BruteforceVariableView<float>;
        BruteforceVariableView<bool> varApproach => vars[3] as BruteforceVariableView<bool>;

        IgnoreScope ignoreUpdates = new IgnoreScope();

        public XZRadialLimitMapObject() : base(null) { hover = new HoverData(this); }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.CylinderImage;

        public override string GetName() => "XZRadialLimit";


        public void SetParent(XZRadialLimit parent)
        {
            this.parent = parent;
            enableDragging = true;
            vars = parent.parameterPanel.GetWatchVariablesByName("x", "z", "dist", "approach").Select(x => x as IBruteforceVariableView).ToArray();
            Action updateFromControl = () =>
            {
                if (ignoreUpdates)
                    return;
                x = varX?._getterFunction().FirstOrDefault() ?? 0;
                z = varZ?._getterFunction().FirstOrDefault() ?? 0;
                Size = varSize?._getterFunction().FirstOrDefault() ?? 0;
            };
            foreach (var var in vars)
                var.ValueSet += updateFromControl;
            SizeChanged += () =>
            {
                using (ignoreUpdates.New())
                    (vars[2] as NamedVariableCollection.IView<float>)?._setterFunction(Size);
            };
            updateFromControl();
        }

        void UpdateVars()
        {
            using (ignoreUpdates.New())
            {
                varX?._setterFunction(x);
                varZ?._setterFunction(z);
                varSize?._setterFunction(Size);
            }
        }

        public override IHoverData GetHoverData(MapGraphics graphics, ref Vector3 position)
        {
            if (graphics.HoverTopDown(new Vector3(x, 0, z), Size))
                return hover;
            return base.GetHoverData(graphics, ref position);
        }

        protected override void DrawTopDown(MapGraphics graphics)
        {
            base.DrawTopDown(graphics);
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                if (varApproach?._getterFunction().FirstOrDefault() ?? false /* approach */)
                {
                    var szThing = 40.0f / graphics.MapViewScaleValue;
                    var szThing2 = 10.0f / graphics.MapViewScaleValue;
                    const int NUM_ARROWS = 6;
                    for (int i = 0; i < NUM_ARROWS; i++)
                    {
                        var alpha = (i / (float)NUM_ARROWS) * Math.PI * 2;
                        var cos = (float)Math.Cos(alpha);
                        var sin = (float)Math.Sin(alpha);
                        graphics.lineRenderer.AddArrow(
                                x + cos * (Size + szThing),
                                0,
                                z + sin * (Size + szThing),
                                szThing, -((float)Utilities.MoreMath.RadiansToAngleUnits(alpha) + 0x4000),
                                szThing2,
                                Utilities.ColorUtilities.ColorToVec4(Color),
                                2);
                    }
                }
                graphics.textRenderer.AddText(
                    $"XZRadialLimit ({parent.GetFuncIndex()?.ToString() ?? "-"})",
                    new Vector3(x, 0, z),
                    OutlineColor,
                    StringAlignment.Center);
            });

        }

        protected override void DrawOrthogonal(MapGraphics graphics)
        {
            // TODO: This is an infinitely high cylinder, what to do?
        }

        protected override List<(float centerX, float centerZ, float radius)> Get2DDimensions()
        {
            return new List<(float centerX, float centerZ, float radius)>(new[] { (x, z, Size) });
        }
    }

    class XZRadialLimit : TrackerMethodControllerBase<XZRadialLimitMapObject, XZRadialLimit>
    {
        protected override XZRadialLimitMapObject CreateMapObject() => new XZRadialLimitMapObject();
    }
}
