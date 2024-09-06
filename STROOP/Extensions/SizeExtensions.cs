﻿using System.Drawing;

namespace STROOP.Extensions
{
    public static class SizeExtensions
    {
        public static Size Divide (this Size a, int b)
        {
            return new Size(a.Width / b, a.Height / b);
        }
    }
}
