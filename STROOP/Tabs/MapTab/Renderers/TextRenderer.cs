using OpenTK;
using OpenTK.Graphics.OpenGL;
using STROOP.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using Imaging = System.Drawing.Imaging;

namespace STROOP.Tabs.MapTab.Renderers
{
    using BrushCacheEntry = StrongBox<(Brush brush, int unusedCount)>;

    public class TextRenderer : Renderer
    {
        const int BRUSH_UNUSED_COUNT_LIMIT = 120; // 4 seconds at 30 FPS

        public static class Fonts
        {
            public static Font small = new Font("Consolas", 8);
            public static Font medium = new Font("Consolas", 12);
            public static Font large = new Font("Consolas", 24);
        }

        struct Text
        {
            public string value;
            public StringFormat format;
            public Font font;
            public Brush brush;
            public PointF position;
        }

        int targetTexture;
        int shader;
        int uniform_sampler;

        Bitmap targetImage;
        Graphics gdiGraphics;

        List<Text> texts = new List<Text>();
        Dictionary<Color, BrushCacheEntry> cachedBrushes = new Dictionary<Color, BrushCacheEntry>();

        protected virtual int GetShader() => GraphicsUtil.GetShaderProgram("Resources/Shaders/Fullscreen.vert.glsl", "Resources/Shaders/TextOverlay.frag.glsl");

        public TextRenderer()
        {
            AccessScope<MapTab>.content.graphics.DoGLInit(() =>
            {
                shader = GetShader();
                uniform_sampler = GL.GetUniformLocation(shader, "sampler");

                targetTexture = GL.GenTexture();
                ResizeIfNecessary();
            });
        }

        private void ResizeIfNecessary()
        {
            var size = AccessScope<MapTab>.content.glControlMap2D;
            if (size.Width == targetImage?.Size.Width && size.Height == targetImage.Size.Height)
                return;

            GL.BindTexture(TextureTarget.Texture2D, targetTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, size.Width, size.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            targetImage?.Dispose();
            gdiGraphics?.Dispose();
            targetImage = new Bitmap(size.Width, size.Height, Imaging.PixelFormat.Format32bppArgb);
            gdiGraphics = Graphics.FromImage(targetImage);
            gdiGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        }

        public override void SetDrawCalls(MapGraphics graphics)
        {
            ResizeIfNecessary();
            graphics.drawLayers[(int)MapGraphics.DrawLayers.BakeText].Add(() =>
            {
                // Dispose of brushes that haven't been used in a while
                var old = cachedBrushes;
                cachedBrushes = new Dictionary<Color, BrushCacheEntry>();
                foreach (var kvp in old)
                    if (kvp.Value.Value.unusedCount++ < BRUSH_UNUSED_COUNT_LIMIT)
                        cachedBrushes.Add(kvp.Key, kvp.Value);
                    else
                        kvp.Value.Value.brush.Dispose();

                // Draw all collected text instances, then clear the list for the next frame
                gdiGraphics.Clear(Color.FromArgb(0));
                foreach (var t in texts)
                    gdiGraphics.DrawString(t.value, t.font, t.brush, t.position, t.format);

                texts.Clear();
                gdiGraphics.Flush();

                // Retrieve the rendered image data into a GL compatible array
                // This process swaps the red and blue channels due to GDI's storage format - the shader will swap these channels back.
                var gdiData = targetImage.LockBits(new Rectangle(Point.Empty, targetImage.Size), Imaging.ImageLockMode.ReadOnly, Imaging.PixelFormat.Format32bppArgb);
                var glData = new byte[targetImage.Width * targetImage.Height * 4];
                int glStride = targetImage.Width * 4;
                for (int row = 0; row < targetImage.Height; row++)
                    Marshal.Copy(gdiData.Scan0 + row * gdiData.Stride, glData, row * glStride, glStride);
                targetImage.UnlockBits(gdiData);

                // Write the retrieved data to the texture
                var gcHandle = GCHandle.Alloc(glData);
                GL.BindTexture(TextureTarget.Texture2D, targetTexture);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, targetImage.Size.Width, targetImage.Size.Height, PixelFormat.Rgba, PixelType.UnsignedByte, Marshal.UnsafeAddrOfPinnedArrayElement(glData, 0));
                gcHandle.Free();
            });
            graphics.drawLayers[(int)MapGraphics.DrawLayers.Overlay].Add(() =>
            {
                GL.UseProgram(shader);
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, targetTexture);
                GL.Uniform1(uniform_sampler, (int)0);
                GL.BindVertexArray(graphics.emptyVAO);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            });
        }

        public void AddText(string text, Vector3 position, Color color, StringAlignment alignment, Font font = null)
        {
            var graphics = AccessScope<MapTab>.content.graphics;
            var ssp = Vector4.TransformRow(new Vector4(position.X, position.Y, position.Z, 1.0f), graphics.ViewMatrix);

            // clip texts behind the camera
            if (ssp.W < 0)
                return;

            Vector3 screenspacePoint = ssp.Xyz / ssp.W;
            texts.Add(new Text()
            {
                value = text,
                brush = GetBrush(color),
                font = font ?? Fonts.medium,
                format = new StringFormat() { Alignment = alignment },
                position = new PointF((screenspacePoint.X + 1) * targetImage.Width / 2f, (-screenspacePoint.Y + 1) * targetImage.Height / 2f),
            });
        }

        Brush GetBrush(Color color)
        {
            BrushCacheEntry result;
            if (!cachedBrushes.TryGetValue(color, out result))
                cachedBrushes[color] = result = new BrushCacheEntry((new SolidBrush(color), 0));
            else
                result.Value.unusedCount = 0;

            return result.Value.brush;
        }
    }
}
