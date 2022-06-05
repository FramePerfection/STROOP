﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using OpenTK;
using System.Drawing.Imaging;
using STROOP.Models;
using System.Windows.Forms;
using STROOP.Forms;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("All Object Ceiling Triangles", "Triangles")]
    public class MapAllObjectCeilingObject : MapCeilingObject
    {
        CustomTriangleList customTris = new CustomTriangleList(() => TriangleUtilities.GetObjectTriangles().FindAll(tri => tri.IsCeiling()));
        protected override List<TriangleDataModel> GetTrianglesOfAnyDist() => customTris.GetTriangles();

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            if (_contextMenuStrip == null)
            {
                _contextMenuStrip = new ContextMenuStrip();
                customTris.AddToContextStrip(_contextMenuStrip.Items);
                _contextMenuStrip.Items.Add(new ToolStripSeparator());
                GetHorizontalTriangleToolStripMenuItems(targetTracker).ForEach(item => _contextMenuStrip.Items.Add(item));
                _contextMenuStrip.Items.Add(new ToolStripSeparator());
                GetTriangleToolStripMenuItems().ForEach(item => _contextMenuStrip.Items.Add(item));
            }

            return _contextMenuStrip;
        }

        public override string GetName()
        {
            return "All Object Ceiling Tris";
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.TriangleCeilingImage;
    }
}

