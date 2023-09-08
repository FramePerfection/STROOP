﻿using STROOP.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace STROOP.Utilities
{
    public static class ImageUtilities
    {
        public static void Dispose(this Lazy<Image> image)
        {
            if (image != null && image.IsValueCreated)
                image.Value?.Dispose();
        }

        public static readonly Lazy<Image> NullImage = new Lazy<Image>(() => null);

        public static Lazy<Image> FromPathOrNull(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                System.Diagnostics.Debugger.Break();
                return NullImage;
            }
            return new Lazy<Image>(() => Image.FromFile(path));
        }

        public static Image CreateMultiImage(List<Image> images, int width, int height)
        {
            Image multiBitmap = new Bitmap(width, height);
            using (Graphics gfx = Graphics.FromImage(multiBitmap))
            {
                int count = images.Count();
                int numCols = (int)Math.Ceiling(Math.Sqrt(count));
                int numRows = (int)Math.Ceiling(count / (double)numCols);
                int imageSize = Math.Min(width, height) / numCols;
                foreach (int row in Enumerable.Range(0, numRows))
                {
                    foreach (int col in Enumerable.Range(0, numCols))
                    {
                        int index = row * numCols + col;
                        if (index >= count) break;
                        Image image = images[index];
                        Rectangle rect = new Rectangle(col * imageSize, row * imageSize, imageSize, imageSize);
                        Rectangle zoomedRect = rect.Zoom(image.Size);
                        gfx.DrawImage(image, zoomedRect);
                    }
                }
            }
            return multiBitmap;
        }

        public static Image ChangeTransparency(Image image, byte alpha)
        {
            Bitmap originalBitmap = new Bitmap(image);
            Bitmap transparentBitmap = new Bitmap(image.Width, image.Height);

            Color originalColor = Color.Black;
            Color transparentColor = Color.Black;

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    originalColor = originalBitmap.GetPixel(x, y);
                    transparentColor = Color.FromArgb(alpha, originalColor.R, originalColor.G, originalColor.B);
                    transparentBitmap.SetPixel(x, y, transparentColor);
                }
            }

            return transparentBitmap;
        }
    }
}
