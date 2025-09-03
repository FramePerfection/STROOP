using System;
using System.Drawing;
using OpenTK;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public abstract class MapBackgroundObject : MapIconObject
    {
        Renderers.SpriteRenderer renderer;
        public MapBackgroundObject()
            : base(null)
        {
            InternalRotates = false;
            renderer = new Renderers.SpriteRenderer(MapGraphics.DrawLayers.Background, 1);
            renderer.texture = 0;
        }

        protected override void DrawTopDown(MapGraphics graphics)
        {
            renderer.ignoreView = true;
            renderer.SetDrawCalls(graphics);
            var img = GetInternalImage();
            if (img != null)
            {
                renderer.texture = GraphicsUtil.TextureFromImage(img.Value);
                graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() => renderer.AddInstance(Matrix4.CreateScale(2), 0, new Vector4(1)));
            }
        }

        protected override void DrawOrthogonal(MapGraphics graphics) => DrawTopDown(graphics);

        protected abstract BackgroundImage GetBackgroundImage();

        public override sealed Lazy<Image> GetInternalImage() => GetBackgroundImage().GetImage();
    }
}
