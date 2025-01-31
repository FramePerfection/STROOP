using OpenTK.Graphics.OpenGL;
using STROOP.Utilities;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Imaging = System.Drawing.Imaging;

namespace STROOP.Tabs.MapTab.Renderers
{
    public class TextRenderer : Renderer
    {
        int targetTexture;
        int shader;
        int uniform_sampler;

        Bitmap targetImage;
        Graphics gdiGraphics;
        Font font;

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
            font = new Font("Consolas", 60);
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
                // Draw some test content with GDI - we will later render text "instances" here
                gdiGraphics.Clear(Color.FromArgb(0));
                gdiGraphics.DrawString("Test", font, Brushes.DarkGray, new Point(10, 20));
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
                GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            });
        }

        public void AddText(params object[] ignored) { }
    }
}
