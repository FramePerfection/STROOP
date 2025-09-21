#define ThrowExceptions
using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;

namespace STROOP.Tabs.MapTab
{
    public static class GraphicsUtil
    {
        static void ThrowOrLog(string debug)
        {
#if ThrowExceptions
            System.Diagnostics.Debugger.Break();
#else
            System.Diagnostics.Debug.WriteLine(debug);
#endif
            ;
        }

        static Dictionary<(string, string, string), int> simpleShaders = new Dictionary<(string, string, string), int>();

        public static int GetShaderProgram(string vertexShaderFile, string fragmentShaderFile, string geometryShaderFile = null)
        {
            (string, string, string) tuple = (vertexShaderFile, fragmentShaderFile, geometryShaderFile);
            if (simpleShaders.TryGetValue(tuple, out int existing))
                return existing;

            var program = GL.CreateProgram();

            string debug;

            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, System.IO.File.ReadAllText(vertexShaderFile));
            GL.CompileShader(vertexShader);
            if ((debug = GL.GetShaderInfoLog(vertexShader)) != string.Empty)
                ThrowOrLog(debug);
            GL.AttachShader(program, vertexShader);

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, System.IO.File.ReadAllText(fragmentShaderFile));
            GL.CompileShader(fragmentShader);
            if ((debug = GL.GetShaderInfoLog(fragmentShader)) != string.Empty)
                ThrowOrLog(debug);
            GL.AttachShader(program, fragmentShader);

            int geometryShader = 0;
            if (geometryShaderFile != null)
            {
                geometryShader = GL.CreateShader(ShaderType.GeometryShader);
                GL.ShaderSource(geometryShader, System.IO.File.ReadAllText(geometryShaderFile));
                GL.CompileShader(geometryShader);
                if ((debug = GL.GetShaderInfoLog(geometryShader)) != string.Empty)
                    ThrowOrLog(debug);
                GL.AttachShader(program, geometryShader);
            }

            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int linked);
            if (linked == 0)
                ThrowOrLog(GL.GetProgramInfoLog(program));

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            if (geometryShaderFile != null)
                GL.DeleteShader(geometryShader);

            simpleShaders[tuple] = program;
            return program;
        }

        public static Bitmap ConvertBitmap(Image orig, System.Drawing.Imaging.PixelFormat newFormat, int newWidth, int newHeight)
        {
            Bitmap clone = new Bitmap(Math.Max(1, newWidth), Math.Max(1, newHeight), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(clone))
            {
                g.Clear(Color.FromArgb(0, 0, 0, 0));
                if (newWidth != orig.Width || newHeight != orig.Height)
                {
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                }

                g.DrawImage(orig, new Rectangle(0, 0, newWidth, newHeight));
            }

            return clone;
        }

        public static byte[] GetPixelData(Image bmp, int width, int height)
        {
            Bitmap tmp = bmp as Bitmap;
            bool useTemporary = tmp == null || bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb || bmp.Width != width || bmp.Height != height;
            if (useTemporary)
                tmp = ConvertBitmap(tmp, System.Drawing.Imaging.PixelFormat.Format32bppArgb, width, height);
            BitmapData dat = tmp.LockBits(new Rectangle(0, 0, tmp.Width, tmp.Height), ImageLockMode.ReadOnly, tmp.PixelFormat);

            byte[] bitmapData = new byte[tmp.Width * tmp.Height * 4];
            for (int i = 0; i < tmp.Height; i++)
                Marshal.Copy(IntPtr.Add(dat.Scan0, dat.Stride * i), bitmapData, tmp.Width * 4 * (tmp.Height - i - 1), tmp.Width * 4);
            tmp.UnlockBits(dat);

            for (int i = 0; i < bitmapData.Length; i += 4)
            {
                byte tmp1 = bitmapData[i], tmp2 = bitmapData[i + 1];
                bitmapData[i] = bitmapData[i + 2];
                bitmapData[i + 1] = tmp2;
                bitmapData[i + 2] = tmp1;
            }

            if (useTemporary)
                tmp.Dispose();
            return bitmapData;
        }

        static void GenTexture(Image[] bmp, int texture, int width = 0, int height = 0)
        {
            if (width == 0 || height == 0)
            {
                width = bmp[0].Width;
                height = bmp[0].Height;
            }

            var format = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
            var pixelType = PixelType.UnsignedByte;

            GL.BindTexture(TextureTarget.Texture2DArray, texture);

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.LinearMipmapNearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba8, width, height, bmp.Length, 0, format, pixelType, IntPtr.Zero);


            for (int layer = 0; layer < bmp.Length; layer++)
                GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, layer, width, height, 1, format, pixelType, GetPixelData(bmp[layer], width, height));

            // Generate mipmaps for texture filtering
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

            GL.BindTexture(TextureTarget.Texture2DArray, 0);
        }

        static Dictionary<string, int> loadedTextures = new Dictionary<string, int>();

        public static int TextureFromFile(string file)
        {
            if (loadedTextures.TryGetValue(file, out int existing))
                return existing;

            int texture = GL.GenTexture();
            using (Bitmap bmp = new Bitmap(file))
            {
                GenTexture(new[] { bmp }, texture);
            }

            loadedTextures[file] = texture;
            return texture;
        }

        static Dictionary<Image, int> imageTextures = new Dictionary<Image, int>();

        public static int TextureFromImage(Image image)
        {
            if (imageTextures.TryGetValue(image, out int existing))
                return existing;

            int texture = GL.GenTexture();
            GenTexture(new[] { image }, texture);

            imageTextures[image] = texture;
            return texture;
        }
    }
}
