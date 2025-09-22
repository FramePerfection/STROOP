using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using STROOP.Models;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public class MapMarioFloorObject : MapFloorObject
    {
        public MapMarioFloorObject()
            : base(null)
        {
        }

        protected override List<TriangleDataModel> GetTrianglesOfAnyDist()
        {
            uint triAddress = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset);
            return MapUtilities.GetTriangles(triAddress);
        }

        public override string GetName()
        {
            return "Floor Tri";
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.TriangleFloorImage;
    }
}
