using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using STROOP.Models;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Custom Ceiling Triangles", "Triangles", nameof(Create))]
    public class MapCustomCeilingObject : MapCeilingObject
    {
        private readonly List<uint> _triAddressList;

        public MapCustomCeilingObject(List<uint> triAddressList)
            : base()
        {
            _triAddressList = triAddressList;
        }

        public static MapCustomCeilingObject Create(ObjectCreateParams creationParameters)
        {
            var lst = GetTrianglesFromDialog(Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.WallTriangleOffset));
            return lst != null ? new MapCustomCeilingObject(lst) : null;
        }

        protected override List<TriangleDataModel> GetTrianglesOfAnyDist()
        {
            return MapUtilities.GetTriangles(_triAddressList);
        }

        public override string GetName()
        {
            return "Custom Ceiling Tris";
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.TriangleCeilingImage;
    }
}
