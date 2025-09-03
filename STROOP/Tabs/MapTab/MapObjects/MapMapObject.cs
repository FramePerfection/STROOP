using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public abstract class MapMapObject : MapBackgroundObject
    {
        Renderers.SpriteRenderer renderer;
        public MapMapObject() : base()
        {
            InternalRotates = true;
            renderer = new Renderers.SpriteRenderer(MapGraphics.DrawLayers.Background);
            renderer.texture = 0;
        }

        public abstract MapLayout GetMapLayout();
        protected override BackgroundImage GetBackgroundImage() => GetMapLayout().MapImage.Value;

        protected override void DrawTopDown(MapGraphics graphics)
        {
            renderer.SetDrawCalls(graphics);
            var img = GetInternalImage();
            if (img == null)
                return;
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                renderer.texture = GraphicsUtil.TextureFromImage(img.Value);
                foreach (var dim in GetDimensions(graphics))
                    renderer.AddInstance(
                        graphics.BillboardMatrix
                        * Matrix4.CreateScale(dim.size.Width, 1, dim.size.Height)
                        * Matrix4.CreateTranslation(dim.loc.X, 0, dim.loc.Y),
                        graphics.rendererCollection.GetObjectTextureLayer(GetInternalImage().Value),
                        new Vector4(1)
                    );
            });
        }

        protected override void DrawOrthogonal(MapGraphics graphics) { }

        protected List<(PointF loc, SizeF size)> GetDimensions(MapGraphics graphics)
        {
            RectangleF rectangle = GetMapLayout().Coordinates;
            float rectangleCenterX = rectangle.X + rectangle.Width / 2;
            float rectangleCenterZ = rectangle.Y + rectangle.Height / 2;
            List<(float x, float z)> rectangleCenters = graphics.MapViewEnablePuView ?
                currentMapTab.GetPuCoordinates(graphics, rectangleCenterX, rectangleCenterZ) :
                new List<(float x, float z)>() { (rectangleCenterX, rectangleCenterZ) };

            List<(PointF loc, SizeF size)> dimensions = rectangleCenters.ConvertAll(
                rectangleCenter => (new PointF(rectangleCenter.x, rectangleCenter.z), new SizeF(rectangle.Width, rectangle.Height)));
            return dimensions;
        }
    }
}
