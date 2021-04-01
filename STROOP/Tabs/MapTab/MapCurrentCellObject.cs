﻿using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Structs.Configurations;
using STROOP.Structs;

namespace STROOP.Tabs.MapTab
{
    [ObjectDescription("Current Cell")]
    public class MapCurrentCellObject : MapQuadObject
    {
        public MapCurrentCellObject()
            : base()
        {
            Opacity = 0.5;
            Color = Color.Yellow;
        }

        protected override List<List<(float x, float y, float z)>> GetQuadList()
        {
            float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
            (int cellX, int cellZ) = WatchVariableSpecialUtilities.GetMarioCell();
            int xMin = (cellX - 8) * 1024;
            int xMax = xMin + 1024;
            int zMin = (cellZ - 8) * 1024;
            int zMax = zMin + 1024;
            List<(float x, float y, float z)> quad =
                new List<(float x, float y, float z)>()
                {
                    (xMin, marioY, zMin),
                    (xMin, marioY, zMax),
                    (xMax, marioY, zMax),
                    (xMax, marioY, zMin),
                };
            return new List<List<(float x, float y, float z)>>() { quad };
        }

        public override string GetName()
        {
            return "Current Cell";
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.CurrentCellImage;
    }
}
