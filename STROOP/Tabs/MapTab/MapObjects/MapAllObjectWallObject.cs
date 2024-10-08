﻿using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Models;
using System.Windows.Forms;
using STROOP.Tabs.MapTab.DataUtil;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("All Object Wall Triangles", "Triangles")]
    public class MapAllObjectWallObject : MapWallObject
    {
        CustomTriangleList customTris = new CustomTriangleList(() => TriangleUtilities.GetObjectTriangles().FindAll(tri => tri.IsWall()));

        protected override List<TriangleDataModel> GetTrianglesOfAnyDist() => customTris.GetTriangles();

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            var _contextMenuStrip = new ContextMenuStrip();

            customTris.AddToContextStrip(_contextMenuStrip.Items);
            _contextMenuStrip.Items.Add(new ToolStripSeparator());
            GetWallToolStripMenuItems(targetTracker).ForEach(item => _contextMenuStrip.Items.Add(item));
            _contextMenuStrip.Items.Add(new ToolStripSeparator());
            GetTriangleToolStripMenuItems().ForEach(item => _contextMenuStrip.Items.Add(item));

            return _contextMenuStrip;
        }

        public override string GetName()
        {
            return "All Object Wall Tris";
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.TriangleWallImage;
    }
}
