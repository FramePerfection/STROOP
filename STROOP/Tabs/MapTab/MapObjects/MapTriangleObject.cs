using System.Collections.Generic;
using System.Linq;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using OpenTK;
using STROOP.Models;
using System.Windows.Forms;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public abstract class MapTriangleObject : MapObject
    {
        protected class TriangleHoverData : IHoverData
        {
            MapTriangleObject parent;
            public TriangleDataModel triangle;
            Vector3 mapCursorOnRightClick;

            public TriangleHoverData(MapTriangleObject parent)
            {
                this.parent = parent;
            }

            public void DragTo(Vector3 position, bool setY) { }

            public void SetLookAt(Vector3 lookAt) { }

            public void LeftClick(Vector3 position) { }

            public void RightClick(Vector3 position)
            {
                mapCursorOnRightClick = position;
            }

            public DragMask CanDrag() => DragMask.None;

            public void AddContextMenuItems(MapTab tab, ContextMenuStrip menu)
            {
                var myItem = new ToolStripMenuItem($"{triangle.Classification} Triangle 0x{triangle.Address.ToString("x8")}");

                var itemCopyTriangleAddress = new ToolStripMenuItem("Copy Triangle Address");
                itemCopyTriangleAddress.Click += (_, __) =>
                {
                    if (triangle != null)
                        Clipboard.SetText($"0x{triangle.Address.ToString("x8")}");
                };
                myItem.DropDownItems.Add(itemCopyTriangleAddress);

                var itemCopyPosition = new ToolStripMenuItem("Copy Position");
                itemCopyPosition.Click += (_, __) =>
                {
                    if (triangle != null)
                    {
                        if (tab.graphics.view.mode == MapView.ViewMode.TopDown)
                        {
                            float y = triangle.IsWall() ? mapCursorOnRightClick.Y : (float)triangle.GetHeightOnTriangle(mapCursorOnRightClick.X, mapCursorOnRightClick.Z);
                            CopyUtilities.CopyPosition(new Vector3(mapCursorOnRightClick.X, y, mapCursorOnRightClick.Z));
                        }
                    }
                };
                myItem.DropDownItems.Add(itemCopyPosition);

                var itemAssignColor = new ToolStripMenuItem("Assign Custom Color");
                itemAssignColor.Click += (_, __) =>
                {
                    var dlg = new ColorDialog();
                    if (dlg.ShowDialog() == DialogResult.OK)
                        parent.individualTriangleColors[triangle.Address] = ColorUtilities.ColorToVec4(dlg.Color);
                };
                myItem.DropDownItems.Add(itemAssignColor);

                var itemRemoveColor = new ToolStripMenuItem("Remove Custom Color");
                itemRemoveColor.Click += (_, __) => parent.individualTriangleColors.Remove(triangle.Address);
                myItem.DropDownItems.Add(itemRemoveColor);

                menu.Items.Add(myItem);
            }
        }

        protected static List<uint> GetTrianglesFromDialog(uint defaultTriangle)
        {
            string text = DialogUtilities.GetStringFromDialog(labelText: "Enter triangle addresses as hex uints.");
            if (text == null) return null;
            if (text == "")
            {
                if (defaultTriangle == 0) return null;
                return new List<uint>() { defaultTriangle };
            }
            List<uint?> nullableUIntList = ParsingUtilities.ParseStringList(text)
                .ConvertAll(word => ParsingUtilities.ParseHexNullable(word));
            if (nullableUIntList.Any(nullableUInt => !nullableUInt.HasValue))
                return null;
            return nullableUIntList.ConvertAll(nullableUInt => nullableUInt.Value);
        }

        protected static List<uint> GetCreationAddressList(ref ObjectCreateParams creationParameters, uint defaultTriangle)
        {
            List<uint> lst = null;
            if (creationParameters != null)
                lst = ParsingUtilities.ParseHexList(creationParameters.GetValue("Addresses"));
            if (lst == null)
                lst = GetTrianglesFromDialog(Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset));
            return lst;
        }

        protected List<TriangleDataModel> _bufferedTris { get; private set; } = new List<TriangleDataModel>();
        private float? _withinDist;
        private float? _withinCenter;
        protected bool _excludeDeathBarriers;
        protected TriangleHoverData hoverData;
        float _projectionAlphaMultiplier = 0.5f;
        protected Dictionary<uint, Vector4> individualTriangleColors = new Dictionary<uint, Vector4>();
        protected bool useRandomColors => itemUseRandomColors.Checked;
        ToolStripMenuItem itemUseRandomColors;

        protected MapTriangleObject(ObjectCreateParams creationParameters)
            : base(creationParameters)
        {
            hoverData = new TriangleHoverData(this);
            _withinDist = null;
            _withinCenter = null;
            _excludeDeathBarriers = false;
        }

        protected List<List<(float x, float y, float z)>> GetVertexLists()
        {
            return GetTrianglesWithinDist().ConvertAll(tri => tri.Get3DVertices());
        }

        protected List<TriangleDataModel> GetTrianglesWithinDist()
        {
            return _bufferedTris.FindAll(FilterTriangles());
        }

        protected virtual System.Predicate<TriangleDataModel> FilterTriangles()
        {
            float centerY = _withinCenter ?? Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
            if (_excludeDeathBarriers)
                return tri => tri.SurfaceType != 0x0A && tri.IsTriWithinVerticalDistOfCenter(_withinDist, centerY);
            else
                return tri => tri.IsTriWithinVerticalDistOfCenter(_withinDist, centerY);
        }

        protected abstract List<TriangleDataModel> GetTrianglesOfAnyDist();

        public override void Update()
        {
            base.Update();
            var newList = GetTrianglesOfAnyDist();
            if (newList != null)
                _bufferedTris = newList;
        }

        public override IHoverData GetHoverData(MapGraphics graphics, ref Vector3 position)
        {
            if (graphics.view.mode == MapView.ViewMode.ThreeDimensional)
            {
                if (graphics.hoverTriangle != null && _bufferedTris.Any(_ => _.Address == graphics.hoverTriangle.Address))
                {
                    hoverData.triangle = graphics.hoverTriangle;
                    return hoverData;
                }
            }
            return null;
        }

        bool projectOnPlane(Vector3 v1, Vector3 v2, Vector3 pNor, float pD, out Vector3 projection)
        {
            Vector3 diff = v2 - v1;
            float vd = Vector3.Dot(v1, pNor) + pD;
            float vOrthogonal = -Vector3.Dot(diff, pNor);
            float f = vd / vOrthogonal;
            projection = v1 + diff * f;

            float newDot = Vector3.Dot(projection, pNor) + pD;
            return f >= 0 && f < 1;
        }

        void DrawOrthogonalProjection(MapGraphics graphics, TriangleDataModel tri, (Vector3 pNor, float pD) plane, Vector4 color)
        {
            Vector3[] projections = new Vector3[3];
            int projIndex = 0;
            Vector3 projection;
            if (projectOnPlane(tri.p1, tri.p2, plane.pNor, plane.pD, out projection))
                projections[projIndex++] = projection;
            if (projectOnPlane(tri.p2, tri.p3, plane.pNor, plane.pD, out projection))
                projections[projIndex++] = projection;
            if (projectOnPlane(tri.p3, tri.p1, plane.pNor, plane.pD, out projection))
                projections[projIndex++] = projection;
            if (projIndex >= 2) // we have a near plane projection
                foreach ((Vector3 low, Vector3 high) offset in GetOrthogonalBoundaryProjection(graphics, tri, projections[0], projections[1]))
                {
                    graphics.triangleOverlayRenderer.Add(
                        projections[0] + offset.low,
                        projections[1] + offset.low,
                        projections[0] + offset.high,
                        false,
                        new Vector4(color.Xyz, color.W),
                        new Vector4(0, 0, 0, 1),
                        new Vector3(0, 1, 1) * OutlineWidth,
                        false);
                    graphics.triangleOverlayRenderer.Add(
                        projections[1] + offset.low,
                        projections[0] + offset.high,
                        projections[1] + offset.high,
                        false,
                        new Vector4(color.Xyz, color.W),
                        new Vector4(0, 0, 0, 1),
                        new Vector3(1, 1, 0) * OutlineWidth,
                        false);
                }
        }

        protected override void DrawOrthogonal(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                var colorMultipliers = new float[] { 1.0f, 0.5f, 0.25f };
                var baseColor = new Vector4(Color.R / 255f, Color.G / 255f, Color.B / 255f, OpacityByte / 255f);
                foreach (var tri in GetTrianglesWithinDist())
                {
                    if (graphics.view.displayOrthoLevelGeometry)
                        graphics.triangleRenderer.Add(
                            tri.p1,
                            tri.p2,
                            tri.p3,
                            false,
                            baseColor,
                            new Vector4(OutlineColor.R / 255f, OutlineColor.G / 255f, OutlineColor.B / 255f, OutlineColor.A / 255f),
                            new Vector3(OutlineWidth),
                            true);
                    DrawOrthogonalProjection(graphics, tri, graphics.orthographicZero, baseColor);
                }
            });
        }

        protected abstract (Vector3 low, Vector3 high)[] GetOrthogonalBoundaryProjection(MapGraphics graphics, TriangleDataModel tri, Vector3 projectionA, Vector3 projectionB);

        protected override void Draw3D(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                var colorMultipliers = new float[] { 1.0f, 0.5f, 0.25f };
                var regularBaseColor = new Vector4(Color.R / 255f, Color.G / 255f, Color.B / 255f, OpacityByte / 255f);
                foreach (var tri in GetTrianglesWithinDist())
                {
                    Vector4 baseColor;
                    if (!individualTriangleColors.TryGetValue(tri.Address, out baseColor))
                        if (useRandomColors)
                            baseColor = ColorUtilities.GetRandomColor((int)tri.Address);
                        else
                            baseColor = regularBaseColor;
                    baseColor.W = OpacityByte / 255f;
                    var projectionColor = new Vector4(baseColor.Xyz, _projectionAlphaMultiplier * baseColor.W);

                    if (!graphics.view.display3DLevelGeometry)
                        graphics.triangleRenderer.Add(
                            tri.p1,
                            tri.p2,
                            tri.p3,
                            false,
                            baseColor,
                            new Vector4(OutlineColor.R / 255f, OutlineColor.G / 255f, OutlineColor.B / 255f, OutlineColor.A / 255f),
                            new Vector3(OutlineWidth),
                            true);

                    Vector4 outlineColor = new Vector4(OutlineColor.R / 255f, OutlineColor.G / 255f, OutlineColor.B / 255f, OutlineColor.A / 255f);
                    foreach (var baseDisplacement in GetBaseFaceVertices(tri))
                    {
                        Vector3[] baseVectors = baseDisplacement.faceVertices;
                        foreach (Vector3 displacement in GetVolumeDisplacements(tri))
                        {

                            for (int i = 0; i < baseVectors.Length; i++)
                            {
                                if (i > 1)
                                    graphics.triangleRenderer.Add(
                                        baseVectors[0] + displacement,
                                        baseVectors[i - 1] + displacement,
                                        baseVectors[i] + displacement,
                                        false,
                                        baseColor,
                                        outlineColor,
                                        new Vector3(OutlineWidth),
                                        true);

                                Vector3 a = baseVectors[i], b = baseVectors[(i + 1) % baseVectors.Length];

                                graphics.triangleRenderer.Add(
                                    a,
                                    b,
                                    a + displacement,
                                    false, projectionColor, outlineColor,
                                    new Vector3(0, OutlineWidth, OutlineWidth),
                                    true);

                                graphics.triangleRenderer.Add(
                                    b,
                                    b + displacement,
                                    a + displacement,
                                    false, projectionColor, outlineColor,
                                    new Vector3(OutlineWidth, 0, OutlineWidth),
                                    true);
                            }
                        }
                    }
                }
            });
        }

        protected abstract Vector3[] GetVolumeDisplacements(TriangleDataModel tri);
        protected virtual (Vector3[] faceVertices, Vector4 color)[] GetBaseFaceVertices(TriangleDataModel tri) => new[] { (new[] { tri.p1, tri.p2, tri.p3 }, new Vector4(1)) };

        protected List<ToolStripItem> GetTriangleToolStripMenuItems()
        {
            ToolStripMenuItem itemSetWithinDist = new ToolStripMenuItem("Set Within Dist");
            itemSetWithinDist.Click += (sender, e) =>
            {
                string text = DialogUtilities.GetStringFromDialog(labelText: "Enter the vertical distance from the center (default: Mario) within which to show tris.");
                float? withinDistNullable = ParsingUtilities.ParseFloatNullable(text);
                if (!withinDistNullable.HasValue) return;
                _withinDist = withinDistNullable.Value;
            };

            ToolStripMenuItem itemClearWithinDist = new ToolStripMenuItem("Clear Within Dist");
            itemClearWithinDist.Click += (sender, e) =>
            {
                _withinDist = null;
            };

            ToolStripMenuItem itemSetWithinCenter = new ToolStripMenuItem("Set Within Center");
            itemSetWithinCenter.Click += (sender, e) =>
            {
                string text = DialogUtilities.GetStringFromDialog(labelText: "Enter the center y of the within-dist range.");
                float? withinCenterNullable =
                    text == "" ?
                    Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset) :
                    ParsingUtilities.ParseFloatNullable(text);
                if (!withinCenterNullable.HasValue) return;
                _withinCenter = withinCenterNullable.Value;
            };

            ToolStripMenuItem itemClearWithinCenter = new ToolStripMenuItem("Clear Within Center");
            itemClearWithinCenter.Click += (sender, e) =>
            {
                _withinCenter = null;
            };

            ToolStripMenuItem itemSetProjectionColorMultiplier = new ToolStripMenuItem("Set Projection Alpha Multiplier");
            itemSetProjectionColorMultiplier.Click += (sender, e) =>
            {
                string text = DialogUtilities.GetStringFromDialog(labelText: "Enter the value by which the alpha of projection side faces are multiplied (Default is 0.5).");
                float? projectionColorMultiplierNullable = ParsingUtilities.ParseFloatNullable(text);
                if (!projectionColorMultiplierNullable.HasValue) return;
                _projectionAlphaMultiplier = projectionColorMultiplierNullable.Value;
            };

            itemUseRandomColors = new ToolStripMenuItem("Use random colors");
            itemUseRandomColors.Click += (sender, e) => itemUseRandomColors.Checked = !itemUseRandomColors.Checked;

            var itemClearCustomColors = new ToolStripMenuItem("Clear Custom Colors");
            itemClearCustomColors.Click += (sender, e) => individualTriangleColors.Clear();

            return new List<ToolStripItem>()
            {
                itemSetWithinDist,
                itemClearWithinDist,
                itemSetWithinCenter,
                itemClearWithinCenter,
                new ToolStripSeparator(),
                itemSetProjectionColorMultiplier,
                itemUseRandomColors,
                itemClearCustomColors,
            };
        }

        public override (SaveSettings save, LoadSettings load) SettingsSaveLoad => (
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.save(node);
                SaveValueNode(node, "WithinDist", _withinDist.ToString());
                SaveValueNode(node, "WithinCenter", _withinCenter.ToString());
                SaveValueNode(node, "ProjectionAlphaMultiplier", _projectionAlphaMultiplier.ToString());
                SaveValueNode(node, "UseRandomColors", useRandomColors.ToString());
            }
        ,
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.load(node);
                if (float.TryParse(LoadValueNode(node, "WithinDist"), out var withinDist))
                    _withinDist = withinDist;
                if (float.TryParse(LoadValueNode(node, "WithinCenter"), out var withinCenter))
                    _withinCenter = withinCenter;
                if (float.TryParse(LoadValueNode(node, "ProjectionAlphaMultiplier"), out var projectionAlphaMultiplier))
                    _projectionAlphaMultiplier = projectionAlphaMultiplier;
                if (bool.TryParse(LoadValueNode(node, "UseRandomColors"), out var randomColors))
                    itemUseRandomColors.Checked = randomColors;
            }
        );
    }
}
