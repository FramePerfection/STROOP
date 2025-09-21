using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Models;
using STROOP.Tabs.MapTab.DataUtil;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("All Object Ceiling Triangles", "Triangles")]
    public class MapAllObjectCeilingObject : MapCeilingObject
    {
        CustomTriangleList customTris = new CustomTriangleList(() => TriangleUtilities.GetObjectTriangles().FindAll(tri => tri.IsCeiling()));
        protected override List<TriangleDataModel> GetTrianglesOfAnyDist() => customTris.GetTriangles();

        public override string GetName()
        {
            return "All Object Ceiling Tris";
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.TriangleCeilingImage;
    }
}
