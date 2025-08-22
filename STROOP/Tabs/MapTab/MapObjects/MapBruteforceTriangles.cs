using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using OpenTK;
using System.Windows.Forms;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Bruteforce Triangle collection", "Misc")]
    public class MapBruteforceTriangles : MapObject
    {
        HashSet<uint> triangleAddresses = new HashSet<uint>();

        class HoverData : IHoverData
        {
            bool? lastAdded = null;
            MapBruteforceTriangles parent;
            public HoverData(MapBruteforceTriangles parent)
            {
                this.parent = parent;
            }

            void IHoverData.AddContextMenuItems(MapTab tab, ContextMenuStrip menu)
            {
                menu.Items.GetSubItem("Triangle Collection").DropDownItems.AddHandlerToItem("Copy Bruteforcer json", parent.CopyJson);
            }

            DragMask IHoverData.CanDrag() => DragMask.All;

            void IHoverData.DragTo(Vector3 position, bool setY)
            {
                if (lastAdded == null)
                    return;
                if (lastAdded.Value)
                    parent.triangleAddresses.Add(parent.currentMapTab.graphics.hoverTriangle.Address);
                else
                    parent.triangleAddresses.Remove(parent.currentMapTab.graphics.hoverTriangle.Address);
            }

            void IHoverData.SetLookAt(Vector3 lookAt) { }

            void IHoverData.LeftClick(Vector3 position)
            {
                if (!parent.enableSelection)
                {
                    lastAdded = null;
                    return;
                }

                var tri = parent.currentMapTab.graphics.hoverTriangle;
                lastAdded = !parent.triangleAddresses.Contains(tri.Address);
                if (lastAdded.Value)
                    parent.triangleAddresses.Add(tri.Address);
                else
                    parent.triangleAddresses.Remove(tri.Address);
            }

            void IHoverData.RightClick(Vector3 position) { }
        }

        HoverData hoverData;
        ToolStripMenuItem itemEnableSelection;
        bool enableSelection => itemEnableSelection.Checked;

        public MapBruteforceTriangles()
        {
            hoverData = new HoverData(this);
            Color = Color.Gray;
        }

        void CopyJson() => Clipboard.SetText(GetJsonString() + "\n");

        public string GetJsonString() => TriangleUtilities.ToJsonString(triangleAddresses);

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            var strip = new ContextMenuStrip();

            itemEnableSelection = strip.Items.AddHandlerToItem("Selection mode", () => itemEnableSelection.Checked = !itemEnableSelection.Checked);
            itemEnableSelection.Checked = true;

            strip.Items.AddHandlerToItem("Copy Bruteforcer json", CopyJson);

            return strip;
        }

        public override IHoverData GetHoverData(MapGraphics graphics, ref Vector3 position) => enableSelection ? hoverData : null;

        protected override void Draw3D(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                foreach (var addr in triangleAddresses)
                {
                    var tri = Models.TriangleDataModel.Create(addr);
                    Vector3 offset = new Vector3(tri.NormX, tri.NormY, tri.NormZ) * 0.25f;
                    graphics.triangleRenderer.Add(
                    tri.p1 + offset,
                    tri.p2 + offset,
                    tri.p3 + offset,
                    false,
                    ColorUtilities.ColorToVec4(Color, 128),
                    new Vector4(OutlineColor.R / 255f, OutlineColor.G / 255f, OutlineColor.B / 255f, OutlineColor.A / 255f),
                    new Vector3(OutlineWidth),
                    true);
                }
            });
        }

        protected override void DrawOrthogonal(MapGraphics graphics) { }

        protected override void DrawTopDown(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                foreach (var addr in triangleAddresses)
                {
                    var tri = Models.TriangleDataModel.Create(addr);

                    float p1_p2_x = (tri.X2 - tri.X1);
                    float p1_p2_z = (tri.Z2 - tri.Z1);

                    float p1_p3_x = (tri.X3 - tri.X1);
                    float p1_p3_z = (tri.Z3 - tri.Z1);

                    float cross = (p1_p2_z * p1_p3_x - p1_p2_x * p1_p3_z);

                    graphics.triangleRenderer.Add(
                        new Vector3(tri.X1, 0, tri.Z1),
                        cross > 0 ? new Vector3(tri.X2, 0, tri.Z2) : new Vector3(tri.X3, 0, tri.Z3),
                        cross > 0 ? new Vector3(tri.X3, 0, tri.Z3) : new Vector3(tri.X2, 0, tri.Z2),
                        ShowTriUnits,
                        new Vector4(Color.R / 255f, Color.G / 255f, Color.B / 255f, OpacityByte / 255f),
                        new Vector4(OutlineColor.R / 255f, OutlineColor.G / 255f, OutlineColor.B / 255f, OutlineColor.A / 255f),
                        OutlineWidth,
                        graphics.view.mode != MapView.ViewMode.TopDown);
                }
            });
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.TriangleFloorImage;
        public override string GetName() => "Triangle collection";
    }
}
