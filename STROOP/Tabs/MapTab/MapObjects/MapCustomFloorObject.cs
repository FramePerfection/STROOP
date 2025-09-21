using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using STROOP.Models;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Custom Floor Triangles", "Triangles", nameof(Create))]
    public class MapCustomFloorObject : MapFloorObject
    {
        private readonly List<uint> _triAddressList;

        protected MapCustomFloorObject(List<uint> triAddressList) : this(triAddressList, null)
        {
        }

        protected MapCustomFloorObject(List<uint> triAddressList, ObjectCreateParams creationParameters)
            : base(creationParameters)
        {
            _triAddressList = triAddressList;
        }

        public static MapCustomFloorObject Create(ObjectCreateParams creationParameters)
        {
            List<uint> lst = GetCreationAddressList(ref creationParameters, Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset));
            return lst != null ? new MapCustomFloorObject(lst, creationParameters) : null;
        }

        protected override List<TriangleDataModel> GetTrianglesOfAnyDist()
        {
            return MapUtilities.GetTriangles(_triAddressList);
        }

        public override string GetName()
        {
            return "Custom Floor Tris";
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.TriangleFloorImage;
    }
}
