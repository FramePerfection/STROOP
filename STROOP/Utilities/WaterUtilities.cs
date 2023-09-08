﻿using STROOP.Structs.Configurations;
using System.Collections.Generic;

namespace STROOP.Structs
{
    public static class WaterUtilities
    {
        public static List<(int y, int xMin, int xMax, int zMin, int zMax)> GetWaterLevels()
        {
            uint waterAddress = Config.Stream.GetUInt32(MiscConfig.WaterPointerAddress);
            int numWaterLevels = waterAddress == 0 ? 0 : Config.Stream.GetInt16(waterAddress);

            if (numWaterLevels > 100) numWaterLevels = 100;

            uint baseOffset = 0x04;
            uint waterStructSize = 0x0C;

            List<(int y, int xMin, int xMax, int zMin, int zMax)> output =
                new List<(int y, int xMin, int xMax, int zMin, int zMax)>();
            for (int i = 0; i < numWaterLevels; i++)
            {
                int xMin = Config.Stream.GetInt16((uint)(waterAddress + baseOffset + i * waterStructSize + 0x00));
                int zMin = Config.Stream.GetInt16((uint)(waterAddress + baseOffset + i * waterStructSize + 0x02));
                int xMax = Config.Stream.GetInt16((uint)(waterAddress + baseOffset + i * waterStructSize + 0x04));
                int zMax = Config.Stream.GetInt16((uint)(waterAddress + baseOffset + i * waterStructSize + 0x06));
                int y = Config.Stream.GetInt16((uint)(waterAddress + baseOffset + i * waterStructSize + 0x08));
                output.Add((y, xMin, xMax, zMin, zMax));
            }
            return output;
        }

        public static int GetCurrentWater()
        {
            float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
            float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
            List<(int y, int xMin, int xMax, int zMin, int zMax)> waterLevels = GetWaterLevels();

            for (int i = 0; i < waterLevels.Count; i++)
            {
                var w = waterLevels[i];
                if (marioX > w.xMin && marioX < w.xMax && marioZ > w.zMin && marioZ < w.zMax)
                {
                    return i + 1;
                }
            }
            return 0;
        }
    }
}
