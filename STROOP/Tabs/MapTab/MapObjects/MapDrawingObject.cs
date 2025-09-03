using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Structs.Configurations;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Drawing", "Custom")]
    public class MapDrawingObject : MapLineObject
    {
        class Drawing : IHoverData
        {
            MapDrawingObject parent;
            public Drawing(MapDrawingObject target) { this.parent = target; }

            public void AddContextMenuItems(MapTab tab, ContextMenuStrip menu)
            {
                var myItem = new ToolStripMenuItem("Drawing");
                var clearDrawingItem = new ToolStripMenuItem("Clear");
                clearDrawingItem.Click += (_, __) => parent.ClearDrawing();
                myItem.DropDownItems.Add(clearDrawingItem);

                var stopDrawingItem = new ToolStripMenuItem("Stop Drawing");
                stopDrawingItem.Click += (_, __) => parent.drawingEnabled = false;
                myItem.DropDownItems.Add(stopDrawingItem);
                menu.Items.Add(myItem);
            }

            public DragMask CanDrag() => DragMask.All & ~DragMask.Angle;

            public void DragTo(Vector3 newPosition, bool setY)
            {
                Vector3 currentVertex = newPosition;
                if (currentVertex != parent._lastVertex)
                {
                    parent._vertices.Add(parent._lastVertex);
                    parent._vertices.Add(currentVertex);
                }
                parent._lastVertex = currentVertex;
            }

            public void SetLookAt(Vector3 lookAt) { }

            public void LeftClick(Vector3 position)
            {
                parent._lastVertex = position;
            }

            public void RightClick(Vector3 position) { }
        }

        private readonly List<Vector3> _vertices;

        ToolStripMenuItem itemEnableDrawing;
        private bool drawingEnabled { get { return itemEnableDrawing.Checked; } set { itemEnableDrawing.Checked = value; } }

        private Vector3 _lastVertex;

        public MapDrawingObject()
            : base()
        {
            OutlineWidth = 3;
            OutlineColor = Color.Red;

            _vertices = new List<Vector3>();
        }

        protected override List<Vector3> GetVertices(MapGraphics graphics) => _vertices;

        public override string GetName() => "Drawing";

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.PathImage;

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            itemEnableDrawing = new ToolStripMenuItem("Enable Drawing");
            var capturedMapTab = currentMapTab;
            itemEnableDrawing.Click += (sender, e) => drawingEnabled = !drawingEnabled;

            ToolStripMenuItem itemClearDrawing = new ToolStripMenuItem("Clear Drawing");
            itemClearDrawing.Click += (sender, e) => ClearDrawing();

            var _contextMenuStrip = new ContextMenuStrip();
            _contextMenuStrip.Items.Add(itemEnableDrawing);
            _contextMenuStrip.Items.Add(itemClearDrawing);
            return _contextMenuStrip;
        }

        public void ClearDrawing() => _vertices.Clear();

        public override IHoverData GetHoverData(MapGraphics graphics, ref Vector3 position) => drawingEnabled ? new Drawing(this) : null;
    }
}
