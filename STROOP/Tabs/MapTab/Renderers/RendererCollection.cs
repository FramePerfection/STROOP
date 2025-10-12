using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab.Renderers
{
    public class RendererCollection
    {
        const int OBJECTS_TEXTURE_SIZE = 256;
        const int OBJECTS_TEXTURE_LAYERS = 256;

        public readonly List<Renderer> renderers = new List<Renderer>();
        public readonly SpriteRenderer objectRenderer;
        public readonly TriangleRenderer triangleRenderer, triangleOverlayRenderer;
        public readonly LineRenderer lineRenderer;
        public readonly ShapeRenderer circleRenderer;
        public readonly GeometryRenderer cylinderRenderer, sphereRenderer;
        public readonly TextRenderer textRenderer;
        public readonly TransparencyRenderer transparencyRenderer;
        bool needsRecreateObjectMipmaps = false;

        public RendererCollection()
        {
            renderers.Add(objectRenderer = new SpriteRenderer(MapGraphics.DrawLayers.Objects, 256));
            AccessScope<MapTab>.content.graphics.DoGLInit(() =>
            {
                objectRenderer.texture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2DArray, objectRenderer.texture);

                GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.LinearMipmapNearest);
                GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
                GL.TexImage3D(TextureTarget.Texture2DArray,
                    0,
                    PixelInternalFormat.Rgba8,
                    OBJECTS_TEXTURE_SIZE,
                    OBJECTS_TEXTURE_SIZE,
                    OBJECTS_TEXTURE_LAYERS,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    IntPtr.Zero);
            });

            renderers.Add(triangleRenderer = new TriangleRenderer(0x10000));
            renderers.Add(triangleOverlayRenderer = new TriangleRenderer(0x10000) { drawlayer = MapGraphics.DrawLayers.GeometryOverlay });
            renderers.Add(lineRenderer = new LineRenderer());
            renderers.Add(circleRenderer = new ShapeRenderer(MapGraphics.DrawLayers.Overlay));
            renderers.Add(cylinderRenderer = new GeometryRenderer(GeometryRenderer.GeometryData.Cylinder()));
            renderers.Add(sphereRenderer = new GeometryRenderer(GeometryRenderer.GeometryData.Sphere(128, 64)));
            renderers.Add(textRenderer = new TextRenderer());
        }

        int objectLayer = 0;
        Dictionary<Image, int> knownIcons = new Dictionary<Image, int>();

        public int GetObjectTextureLayer(Image image)
        {
            if (knownIcons.TryGetValue(image, out var known))
                return known;
            var imageData = GraphicsUtil.GetPixelData(image, OBJECTS_TEXTURE_SIZE, OBJECTS_TEXTURE_SIZE);
            var result = objectLayer++;
            GL.BindTexture(TextureTarget.Texture2DArray, objectRenderer.texture);
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, result, OBJECTS_TEXTURE_SIZE, OBJECTS_TEXTURE_SIZE, 1, PixelFormat.Rgba, PixelType.UnsignedByte, imageData);
            GL.BindTexture(TextureTarget.Texture2DArray, 0);
            needsRecreateObjectMipmaps = true;
            knownIcons[image] = result;
            return result;
        }

        public void UpdateObjectMap()
        {
            if (needsRecreateObjectMipmaps)
            {
                GL.BindTexture(TextureTarget.Texture2DArray, objectRenderer.texture);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);
                needsRecreateObjectMipmaps = false;
            }
        }
    }
}
