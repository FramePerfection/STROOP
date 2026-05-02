using STROOP.Structs.Configurations;
using STROOP.Variables.SM64MemoryLayout;
using System;
using System.Collections.Generic;
using System.Linq;

namespace STROOP.Utilities
{
    public static class InGameTrigUtilities
    {
        private static List<int> _inGameAngles = null;

        public static List<int> GetInGameAngles()
        {
            if (_inGameAngles == null)
            {
                _inGameAngles = new List<int>();
                List<int> allAngles = Enumerable.Range(0, 65536).ToList();
                foreach (int angle in allAngles)
                {
                    (double x, double z) = MoreMath.GetComponentsFromVector(1, angle);
                    int inGameAngle = InGameAngleTo(x, z);
                    if (!_inGameAngles.Contains(inGameAngle))
                    {
                        _inGameAngles.Add(inGameAngle);
                    }
                }
            }

            return new List<int>(_inGameAngles);
        }

        public static float InGameSine(int angle)
        {
            ushort truncated = MoreMath.NormalizeAngleTruncated(angle);
            if (truncated == 32768) return 0;
            double radians = MoreMath.AngleUnitsToRadians(truncated);
            return (float)Math.Sin(radians);
        }

        public static float InGameCosine(int angle)
        {
            ushort truncated = MoreMath.NormalizeAngleTruncated(angle);
            if (truncated == 16384 || truncated == 49152) return 0;
            double radians = MoreMath.AngleUnitsToRadians(truncated);
            return (float)Math.Cos(radians);
        }

        public static ushort InGameAngleTo(double xTo, double zTo)
        {
            return InGameAngleTo((float)xTo, (float)zTo);
        }

        public static ushort InGameAngleTo(double xFrom, double zFrom, double xTo, double zTo)
        {
            return InGameAngleTo((float)xFrom, (float)zFrom, (float)xTo, (float)zTo);
        }

        public static ushort InGameAngleTo(float xTo, float zTo)
        {
            return InGameAngleTo(0, 0, xTo, zTo);
        }

        public static ushort InGameAngleTo(float xFrom, float zFrom, float xTo, float zTo)
        {
            float xDiff = xTo - xFrom;
            float zDiff = zTo - zFrom;
            return InGameATan(zDiff, xDiff);
        }

        public static ushort InGameATan(float xComp, float yComp)
        {
            int returnValue;
            if (0 <= yComp)
                if (0 <= xComp)
                    if (yComp <= xComp)
                        returnValue = InGameATanLookup(yComp, xComp);
                    else
                        returnValue = 0x4000 - InGameATanLookup(xComp, yComp);
                else if (-xComp < yComp)
                    returnValue = 0x4000 + InGameATanLookup(-xComp, yComp);
                else
                    returnValue = 0x8000 - InGameATanLookup(yComp, -xComp);
            else if (xComp < 0)
                if (-yComp < -xComp)
                    returnValue = 0x8000 + InGameATanLookup(-yComp, -xComp);
                else
                    returnValue = 0xC000 - InGameATanLookup(-xComp, -yComp);
            else if (xComp < -yComp)
                returnValue = 0xC000 + InGameATanLookup(xComp, -yComp);
            else
                returnValue = 0x10000 - InGameATanLookup(-yComp, xComp);

            return (ushort)returnValue;
        }

        private static ushort InGameATanLookup(float yComp, float xComp)
        {
            uint offset;
            if (xComp == 0)
                offset = 0;
            else
                offset = 2 * (uint)((yComp / xComp) * 1024f + 0.5f);

            uint address = MappingConfig.HandleMapping(0x8038B000);
            return Config.Stream.GetUInt16(address + offset);
        }
    }
}
