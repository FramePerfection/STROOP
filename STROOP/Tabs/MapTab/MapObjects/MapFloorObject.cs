using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using STROOP.Models;
using System.Windows.Forms;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public abstract class MapFloorObject : MapHorizontalTriangleObject
    {
        protected MapFloorObject(ObjectCreateParams creationParameters)
            : base(creationParameters)
        {
            Size = 78;
            Opacity = 0.5;
            Color = Color.Blue;
        }

        protected override Vector3[] GetVolumeDisplacements(TriangleDataModel tri) => new[] { new Vector3(0, -Size, 0) };
        protected override (Vector3 low, Vector3 high)[] GetOrthogonalBoundaryProjection(MapGraphics graphics, TriangleDataModel tri, Vector3 projectionA, Vector3 projectionB) =>
            new[] { (Vector3.Zero, new Vector3(0, -78, 0)) };

        protected List<ToolStripMenuItem> GetFloorToolStripMenuItems()
        {
            ToolStripMenuItem itemExcludeDeathBarriers = new ToolStripMenuItem("Exclude Death Barriers");
            itemExcludeDeathBarriers.Click += (sender, e) =>
            {
                _excludeDeathBarriers = !_excludeDeathBarriers;
                itemExcludeDeathBarriers.Checked = _excludeDeathBarriers;
            };

            ToolStripMenuItem itemEnableQuarterFrameLandings = new ToolStripMenuItem("Enable Quarter Frame Landings");
            itemEnableQuarterFrameLandings.Click += (sender, e) =>
            {
                _enableQuarterFrameLandings = !_enableQuarterFrameLandings;
                itemEnableQuarterFrameLandings.Checked = _enableQuarterFrameLandings;
            };

            return new List<ToolStripMenuItem>()
            {
                itemExcludeDeathBarriers,
                itemEnableQuarterFrameLandings,
            };
        }

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            var _contextMenuStrip = new ContextMenuStrip();

            GetFloorToolStripMenuItems().ForEach(item => _contextMenuStrip.Items.Add(item));
            _contextMenuStrip.Items.Add(new ToolStripSeparator());
            GetHorizontalTriangleToolStripMenuItems(targetTracker).ForEach(item => _contextMenuStrip.Items.Add(item));
            _contextMenuStrip.Items.Add(new ToolStripSeparator());
            GetTriangleToolStripMenuItems().ForEach(item => _contextMenuStrip.Items.Add(item));

            return _contextMenuStrip;
        }
    }
}
