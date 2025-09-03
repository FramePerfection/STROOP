using System.Windows.Forms;
using System.Drawing;
using OpenTK;
using OpenTK.Mathematics;
using STROOP.Models;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public abstract class MapCeilingObject : MapHorizontalTriangleObject
    {
        public MapCeilingObject()
            : base(null)
        {
            Size = 160;
            Opacity = 0.5;
            Color = Color.Red;
        }

        protected override Vector3[] GetVolumeDisplacements(TriangleDataModel tri) => new[] { new Vector3(0, -Size, 0) };
        protected override (Vector3 low, Vector3 high)[] GetOrthogonalBoundaryProjection(MapGraphics graphics, TriangleDataModel tri, Vector3 projectionA, Vector3 projectionB)
            => new[] { (Vector3.Zero, new Vector3(0, -160, 0)) };

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            var _contextMenuStrip = new ContextMenuStrip();

            GetHorizontalTriangleToolStripMenuItems(targetTracker).ForEach(item => _contextMenuStrip.Items.Add(item));
            _contextMenuStrip.Items.Add(new ToolStripSeparator());
            GetTriangleToolStripMenuItems().ForEach(item => _contextMenuStrip.Items.Add(item));

            return _contextMenuStrip;
        }
    }
}
