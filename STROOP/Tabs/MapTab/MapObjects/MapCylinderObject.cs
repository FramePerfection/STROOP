using System.Collections.Generic;
using OpenTK;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public abstract class MapCylinderObject : MapCircleObject
    {
        protected MapCylinderObject(ObjectCreateParams creationParamters) : base(creationParamters) { }

        protected override List<(float centerX, float centerZ, float radius)> Get2DDimensions()
        {
            List<(float centerX, float centerZ, float radius, float minY, float maxY)> dimensions3D = Get3DDimensions();
            List<(float centerX, float centerZ, float radius)> dimensions2D = dimensions3D.ConvertAll(
                dimension => (dimension.centerX, dimension.centerZ, dimension.radius));
            return dimensions2D;
        }

        protected abstract List<(float centerX, float centerZ, float radius, float minY, float maxY)> Get3DDimensions();

        protected override void DrawOrthogonal(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                var color = new Vector4(Color.R / 255.0f, Color.G / 255.0f, Color.B / 255.0f, (float)Opacity);
                foreach (var dim in Get3DDimensions())
                {
                    var dist = (graphics.view.focusPositionAngle.position.Xz - new Vector2(dim.centerX, dim.centerZ)).Length;
                    dist /= dim.radius;
                    var scale = System.Math.Sqrt(1 - dist * dist);
                    if (!double.IsNaN(scale))
                    {
                        var transform =  Matrix4.CreateScale((float)scale * dim.radius, (dim.maxY - dim.minY) * 0.5f, 1)
                                        * graphics.BillboardMatrix
                                        * Matrix4.CreateTranslation(dim.centerX, (dim.minY + dim.maxY) * 0.5f, dim.centerZ);
                        graphics.circleRenderer.AddInstance(true, transform, OutlineWidth, color, Utilities.ColorUtilities.ColorToVec4(OutlineColor), Renderers.ShapeRenderer.Shapes.Quad);
                    }
                }
            });
        }

        protected override void Draw3D(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                var color = new Vector4(Color.R / 255.0f, Color.G / 255.0f, Color.B / 255.0f, (float)Opacity);
                foreach (var dim in Get3DDimensions())
                {
                    var transform = Matrix4.CreateScale(dim.radius, dim.maxY - dim.minY, dim.radius) * Matrix4.CreateTranslation(dim.centerX, dim.minY, dim.centerZ);
                    graphics.cylinderRenderer.Add(transform, color);
                }
            });
        }
    }
}
