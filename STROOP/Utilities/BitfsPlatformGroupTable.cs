﻿using System.Linq;

namespace STROOP.Utilities
{
    public static class BitfsPlatformGroupTable
    {
        private static readonly float[] _relativeHeights;
        private static readonly float _heightRange;
        private static readonly int _testStartingHeight = 3500;

        static BitfsPlatformGroupTable()
        {
            _relativeHeights = new float[256];
            float height = _testStartingHeight;
            int timer = 0;
            for (int i = 0; i < 256; i++)
            {
                _relativeHeights[i] = height - _testStartingHeight;
                height += InGameTrigUtilities.InGameSine(timer) * -0.58f;
                timer += 0x100;
            }
            _heightRange = _relativeHeights.Max() - _relativeHeights.Min();
        }

        public static float GetRelativeHeightFromMax(int timer)
        {
            int byteValue = (timer >> 8) & 0xFF;
            return _relativeHeights[byteValue];
        }

        public static float GetRelativeHeightFromMin(int timer)
        {
            return GetRelativeHeightFromMax(timer) + _heightRange;
        }

        public static float GetMaxHeight(int timer, float height)
        {
            return height - GetRelativeHeightFromMax(timer);
        }

        public static float GetMinHeight(int timer, float height)
        {
            return GetMaxHeight(timer, height) - _heightRange;
        }

        public static float GetDisplacedHeight(int timer, float height, float homeHeight)
        {
            float shouldBeHeight = homeHeight + GetRelativeHeightFromMax(timer);
            return height - shouldBeHeight;
        }
    }
}
