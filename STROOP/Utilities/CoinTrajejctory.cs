﻿using System;

namespace STROOP.Utilities
{
    public class CoinTrajectory
    {
        public readonly double HSpeed;
        public readonly double VSpeed;
        public readonly ushort Angle;

        public CoinTrajectory(
            double hSpeed,
            double vSpeed,
            ushort angle)
        {
            HSpeed = hSpeed;
            VSpeed = vSpeed;
            Angle = angle;
        }

        public override string ToString()
        {
            return String.Format(
                "HSpeed:{0}, VSpeed:{1}, Angle:{2}",
                HSpeed, VSpeed, Angle);
        }

    }
}
