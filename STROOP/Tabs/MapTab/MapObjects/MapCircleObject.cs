using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using OpenTK;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public abstract class MapCircleObject : MapObject
    {
        protected MapCircleObject(ObjectCreateParams creationParameters)
            : base(creationParameters)
        {
            Opacity = 0.5;
            Color = Color.Red;
        }

        protected override void DrawTopDown(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                List<(float centerX, float centerZ, float radius)> dimensionList = Get2DDimensions();
                var color = ColorUtilities.ColorToVec4(Color, OpacityByte);
                var outlineColor = ColorUtilities.ColorToVec4(OutlineColor);
                foreach (var dim in dimensionList)
                {
                    var transform = graphics.BillboardMatrix * Matrix4.CreateScale(dim.radius) * Matrix4.CreateTranslation(dim.centerX, 0, dim.centerZ);
                    graphics.circleRenderer.AddInstance(
                        graphics.view.mode != MapView.ViewMode.TopDown,
                        transform,
                        OutlineWidth,
                        color,
                        outlineColor);
                }
            });
        }

        public override IHoverData GetHoverData(MapGraphics graphics, ref Vector3 position) => null;

        protected abstract List<(float centerX, float centerZ, float radius)> Get2DDimensions();
    }
}
