﻿using System.Drawing;

namespace STROOP.Extensions
{
    public static class ColorExtensions
    {
        public static Color Lighten(this Color color, double amount)
        {
            double red = (255 - color.R) * amount + color.R;
            double green = (255 - color.G) * amount + color.G;
            double blue = (255 - color.B) * amount + color.B;
            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }

        public static Color Darken(this Color color, double amount)
        {
            double red = (0 - color.R) * amount + color.R;
            double green = (0 - color.G) * amount + color.G;
            double blue = (0 - color.B) * amount + color.B;
            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }
    }
}
