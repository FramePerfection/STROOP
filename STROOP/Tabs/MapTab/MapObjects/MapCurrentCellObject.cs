﻿using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Structs.Configurations;
using STROOP.Structs;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Current Cell", "Current")]
    public class MapCurrentCellObject : MapQuadObject
    {
        public MapCurrentCellObject()
            : base()
        {
            Opacity = 0.5;
            Color = Color.Yellow;
        }


        protected override List<(float xMin, float zMin, float xMax, float zMax)> GetQuadList()
        {
            (int cellX, int cellZ) = WatchVariableSpecialUtilities.GetMarioCell();
            int xMin = (cellX - 8) * 1024;
            int xMax = xMin + 1024;
            int zMin = (cellZ - 8) * 1024;
            int zMax = zMin + 1024;
            return new List<(float, float, float, float)>(new[] { ((float)xMin, (float)xMax, (float)zMin, (float)zMax) });
        }

        public override string GetName()
        {
            return "Current Cell";
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.CurrentCellImage;
    }
}
