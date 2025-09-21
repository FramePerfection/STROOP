using System.Collections.Generic;
using STROOP.Models;
using System;
using System.Windows.Forms;

namespace STROOP.Tabs.MapTab.DataUtil
{
    class CustomTriangleList
    {
        private bool _autoUpdate = true;
        public List<TriangleDataModel> keptTris = new List<TriangleDataModel>();
        Func<List<TriangleDataModel>> getTriangles;

        public CustomTriangleList(Func<List<TriangleDataModel>> getTriangles)
        {
            this.getTriangles = getTriangles;
        }

        public void AddToContextStrip(ToolStripItemCollection target)
        {
            var addTrisItem = new ToolStripMenuItem("Keep current");
            addTrisItem.Click += (_, __) => keptTris.AddRange(getTriangles());
            target.Add(addTrisItem);

            var clearKeptItem = new ToolStripMenuItem("Clear kept triangles");
            clearKeptItem.Click += (_, __) => keptTris.Clear();
            target.Add(clearKeptItem);

            var itemAutoUpdate = new ToolStripMenuItem("Auto Update");
            itemAutoUpdate.Click += (sender, e) =>
            {
                _autoUpdate = !_autoUpdate;
                itemAutoUpdate.Checked = _autoUpdate;
            };
            itemAutoUpdate.Checked = _autoUpdate;

            target.Add(itemAutoUpdate);
        }

        public List<TriangleDataModel> GetTriangles()
        {
            if (_autoUpdate)
            {
                var lst = new List<TriangleDataModel>(keptTris);
                lst.AddRange(getTriangles());
                return lst;
            }

            return keptTris;
        }
    }
}
