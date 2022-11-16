﻿using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using System.Windows.Forms;
using OpenTK;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public interface IPositionCalculatorProvider
    {
        IEnumerable<(string name, Func<Vector3> func)> GetPositionCalculators();
    }

    [ObjectDescription("Tape Measure", "Custom")]
    public class MapTapeMeasureObject : MapLineObject
    {
        class TapeHoverData : IHoverData
        {
            MapTapeMeasureObject parent;
            public bool dragA;
            float cursorY;
            public TapeHoverData(MapTapeMeasureObject parent)
            {
                this.parent = parent;
            }

            public bool CanDrag() => parent.itemEnableDragging.Checked;

            public void DragTo(Vector3 newPosition, bool setY)
            {
                if (dragA)
                    parent.a = newPosition;
                else
                    parent.b = newPosition;
                parent.targetTracker.textBoxSize.Text = (parent.Size = (parent.a - parent.b).Length).ToString();
            }

            public void LeftClick(Vector3 position) { }

            public void RightClick(Vector3 position)
            {
                cursorY = position.Y;
            }

            public void AddContextMenuItems(MapTab tab, ContextMenuStrip menu)
            {
                var ABString = (dragA ? "A" : "B");
                var myItem = new ToolStripMenuItem($"Tape Measure ({ABString})");
                var copyPositionItem = new ToolStripMenuItem("Copy Position");
                copyPositionItem.Click += (_, __) => CopyUtilities.CopyPosition(dragA ? parent.a : parent.b);
                myItem.DropDownItems.Add(copyPositionItem);

                var pastePositionItem = new ToolStripMenuItem("Paste Position");
                pastePositionItem.Click += (_, __) =>
                {
                    if (CopyUtilities.TryPastePosition(out Vector3 textVector))
                    {
                        if (dragA)
                            parent.a = textVector;
                        else
                            parent.b = textVector;
                    }
                };
                myItem.DropDownItems.Add(pastePositionItem);

                foreach (var data in tab.hoverData)
                    if (data is IPositionCalculatorProvider calcProvider)
                        foreach (var calculator in calcProvider.GetPositionCalculators())
                        {
                            var itemAttach = new ToolStripMenuItem($"Attach {ABString} to {calculator.name}");
                            var funcCapture = calculator.func;
                            itemAttach.Click += (_, __) =>
                            {
                                if (dragA)
                                    parent.aProvider = funcCapture;
                                else
                                    parent.bProvider = funcCapture;
                            };
                            myItem.DropDownItems.Add(itemAttach);
                        }

                var itemDetach = new ToolStripMenuItem($"Detach {ABString}");
                itemDetach.Click += (_, __) =>
                {
                    if (dragA)
                        parent.aProvider = null;
                    else
                        parent.bProvider = null;
                };
                myItem.DropDownItems.Add(itemDetach);

                menu.Items.Add(myItem);
            }
        }

        Vector3 a, b;

        Func<Vector3> aProvider, bProvider;
        TapeHoverData hoverData;

        public MapTapeMeasureObject()
        {
            OutlineColor = Color.Orange;
            OutlineWidth = 3;
            a = new Vector3(currentMapTab.graphics.view.position.X - 50, 0, currentMapTab.graphics.view.position.Z);
            b = new Vector3(currentMapTab.graphics.view.position.X + 50, 0, currentMapTab.graphics.view.position.Z);
            hoverData = new TapeHoverData(this);
        }

        MapTracker targetTracker;
        ToolStripMenuItem itemEnableDragging;

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            this.targetTracker = targetTracker;

            itemEnableDragging = new ToolStripMenuItem("Enable dragging");
            var capturedMapTab = currentMapTab;
            itemEnableDragging.Click += (sender, e) =>
            {
                itemEnableDragging.Checked = !itemEnableDragging.Checked;
            };

            var _contextMenuStrip = new ContextMenuStrip();
            _contextMenuStrip.Items.Add(itemEnableDragging);
            itemEnableDragging.PerformClick();
            return _contextMenuStrip;
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.ArrowImage;

        public override string GetName() => "Tape Measure";

        protected override List<Vector3> GetVertices(MapGraphics graphics) =>
            new List<Vector3>(new[] { aProvider?.Invoke() ?? a, bProvider?.Invoke() ?? b });

        protected override void Draw3D(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                Vector3 _a = aProvider?.Invoke() ?? a;
                Vector3 _b = bProvider?.Invoke() ?? b;
                List<Vector3> ends = new List<Vector3>();
                ends.AddRange(new[] {
                    new Vector3(float.NaN),
                    new Vector3(float.NaN, 0, float.NaN),
                    new Vector3(0, 0, float.NaN),
                    new Vector3(1, 0, float.NaN),
                    new Vector3(float.NaN, 0, 0),
                    new Vector3(float.NaN, 0, 1),
                    new Vector3(float.NaN, float.NaN, 1),
                    new Vector3(1, float.NaN, float.NaN),
                    new Vector3(1, float.NaN, 1),
                });
                Color[] colors = new[] { Color.FromArgb(255, 100, 100), Color.LightGreen, Color.Yellow, Color.LightBlue, Color.Pink, Color.Cyan, Color.LightGray };

                foreach (var end in ends)
                {
                    string nameString = "";
                    var p1 = _a;
                    var p2 = _b;
                    int colorIndex = 0;
                    if (!float.IsNaN(end.X))
                        p1.X = p2.X = end.X == 0 ? p1.X : p2.X;
                    else
                    {
                        nameString += "x";
                        colorIndex |= 1;
                    }
                    if (!float.IsNaN(end.Y))
                        p1.Y = p2.Y = end.Y == 0 ? p1.Y : p2.Y;
                    else
                    {
                        nameString += "y";
                        colorIndex |= 2;
                    }
                    if (!float.IsNaN(end.Z))
                        p1.Z = p2.Z = end.Z == 0 ? p1.Z : p2.Z;
                    else
                    {
                        nameString += "z";
                        colorIndex |= 4;
                    }

                    var lineColor = colors[colorIndex - 1];
                    graphics.lineRenderer.Add(p1, p2, ColorUtilities.ColorToVec4(lineColor), OutlineWidth);

                    Vector3 middlePoint = (p1 + p2) * 0.5f;
                    var ssp = Vector4.Transform(new Vector4(middlePoint.X, middlePoint.Y, middlePoint.Z, 1), graphics.ViewMatrix);

                    Vector3 screenspacePoint = ssp.Xyz / ssp.W;
                    if (ssp.W < 0)
                        continue;
                    graphics.textRenderer.AddText(
                        new[] { ($"{nameString}: {(p1 - p2).Length}", Vector3.Zero) },
                        lineColor,
                        Matrix4.CreateTranslation(-16, 8, 0) * Matrix4.CreateScale(1.0f / graphics.glControl.Height) * Matrix4.CreateTranslation(screenspacePoint),
                        screenSpace: true,
                        align: QuickFont.QFontAlignment.Right);
                }
            });
        }

        public override IHoverData GetHoverData(MapGraphics graphics, ref Vector3 position)
        {
            float magicConst = 15;
            Vector3 _a = aProvider?.Invoke() ?? a;
            Vector3 _b = bProvider?.Invoke() ?? b;
            if (graphics.view.mode == MapView.ViewMode.TopDown)
            {
                var rad = (magicConst / graphics.MapViewScaleValue);
                if (graphics.HoverTopDown(_a, rad))
                {
                    hoverData.dragA = true;
                    position.Y = _a.Y;
                    return hoverData;
                }
                else if (graphics.HoverTopDown(_b, rad))
                {
                    hoverData.dragA = false;
                    position.Y = _b.Y;
                    return hoverData;
                }
            }
            else if (graphics.view.mode == MapView.ViewMode.ThreeDimensional)
            {
                bool prioritizeA = (_a - graphics.view.position).LengthSquared < (_b - graphics.view.position).LengthSquared;
                bool hoverA = graphics.Hover3D(_a, magicConst * Get3DIconScale(graphics, _a.X, _a.Y, _a.Z));
                bool hoverB = graphics.Hover3D(_b, magicConst * Get3DIconScale(graphics, _b.X, _b.Y, _b.Z));
                if (hoverA && (!hoverB || prioritizeA))
                {
                    hoverData.dragA = true;
                    position = _a;
                    return hoverData;
                }
                else if (hoverB)
                {
                    hoverData.dragA = false;
                    position = _b;
                    return hoverData;
                }
            }
            return null;
        }
    }
}
