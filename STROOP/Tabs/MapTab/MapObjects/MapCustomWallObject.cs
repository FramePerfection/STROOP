using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using STROOP.Models;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Custom Wall Triangles", "Triangles", nameof(Create))]
    public class MapCustomWallObject : MapWallObject
    {
        private readonly List<uint> _triAddressList;

        public MapCustomWallObject(List<uint> triAddressList)
            : base()
        {
            _triAddressList = triAddressList;
        }

        public static MapCustomWallObject Create(ObjectCreateParams creationParameters)
        {
            var lst = GetCreationAddressList(ref creationParameters, Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.WallTriangleOffset));
            return lst != null ? new MapCustomWallObject(lst) : null;
        }

        protected override List<TriangleDataModel> GetTrianglesOfAnyDist()
        {
            return MapUtilities.GetTriangles(_triAddressList);
        }

        public override string GetName()
        {
            return "Custom Wall Tris";
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.TriangleWallImage;
    }
}
