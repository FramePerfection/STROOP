﻿using System.Drawing;

namespace STROOP.Utilities
{
    public static class SizeExtensions
    {
        public static Size Divide (this Size a, int b)
        {
            return new Size(a.Width / b, a.Height / b);
        }
    }
}
