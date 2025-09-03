using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public abstract class MapQuadObject : MapObject
    {
        public MapQuadObject() : base() { }

        protected override void DrawTopDown(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {

                var color = Utilities.ColorUtilities.ColorToVec4(Color, OpacityByte);
                var outlineColor = Utilities.ColorUtilities.ColorToVec4(OutlineColor);

                foreach (var quad in GetQuadList())
                {
                    Matrix4 transform = Matrix4.CreateRotationX((float)Math.PI / 2)
                        * Matrix4.CreateScale((quad.xMax - quad.xMin) * 0.5f, 1, (quad.zMax - quad.zMin) * 0.5f)
                        * Matrix4.CreateTranslation((quad.xMin + quad.xMax) * 0.5f, quad.y, (quad.zMin + quad.zMax) * 0.5f);
                    graphics.circleRenderer.AddInstance(
                             graphics.view.mode != MapView.ViewMode.TopDown,
                             transform,
                             OutlineWidth,
                             color,
                             outlineColor,
                             Renderers.ShapeRenderer.Shapes.Quad);
                }
            });
        }

        protected override void DrawOrthogonal(MapGraphics graphics) => DrawTopDown(graphics);

        protected abstract List<(float xMin, float xMax, float zMin, float zMax, float y)> GetQuadList();
    }
}
