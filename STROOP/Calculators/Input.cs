﻿using STROOP.Utilities;
using System;

namespace STROOP.Structs
{
    // Y value is inputted and stored in sm64 convention
    // Y value is displayed in mupen convention
    public class Input
    {
        public readonly int X;
        public readonly int Y;

        public Input(int x, int y)
        {
            X = x;
            Y = y;
        }

        public float GetScaledMagnitude()
        {
            return MoreMath.GetScaledInputMagnitude(X, Y, false);
        }

        public override string ToString()
        {
            return String.Format("({0},{1})", X, -1 * Y);
        }
    }
}
