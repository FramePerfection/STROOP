using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using OpenTK;
using STROOP.Models;

using System.Windows.Forms;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public abstract class MapHorizontalTriangleObject : MapTriangleObject
    {
        private float? _minHeight;
        private float? _maxHeight;
        private float? _minNormalY;
        private float? _maxNormalY;
        protected bool _enableQuarterFrameLandings;

        protected MapHorizontalTriangleObject(ObjectCreateParams creationParameters)
            : base(creationParameters)
        {
            _minHeight = null;
            _maxHeight = null;
        }

        public override IHoverData GetHoverData(MapGraphics graphics, ref Vector3 position)
        {
            if (graphics.view.mode == MapView.ViewMode.TopDown)
                foreach (var tri in GetTrianglesWithinDist())
                {
                    if (tri.GetTruncatedHeightOnTriangleIfInsideTriangle(graphics.mapCursorPosition.X, graphics.mapCursorPosition.Z) != null)
                    {
                        hoverData.triangle = tri;
                        return hoverData;
                    }
                }
            return base.GetHoverData(graphics, ref position);
        }

        protected override Predicate<TriangleDataModel> FilterTriangles()
        {
            Predicate<TriangleDataModel> pred = base.FilterTriangles();
            if (_minHeight != null)
                pred = PredicateAnd(tri => _minHeight <= tri.GetMaxY(), pred);
            if (_maxHeight != null)
                pred = PredicateAnd(tri => _maxHeight >= tri.GetMinY(), pred);
            if (_minNormalY != null)
                pred = PredicateAnd(tri => _minNormalY <= tri.NormY, pred);
            if (_maxNormalY != null)
                pred = PredicateAnd(tri => _maxNormalY >= tri.NormY, pred);
            return pred;
        }

        static Predicate<T> PredicateAnd<T>(Predicate<T> A, Predicate<T> B) => _ => A(_) && B(_);

        protected override void DrawTopDown(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                foreach (var tri in this.GetTrianglesWithinDist())
                {
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

        protected List<ToolStripMenuItem> GetHorizontalTriangleToolStripMenuItems(MapTracker targetTracker)
        {
            bool parseFloatNullable(string text, out float? value)
            {
                value = text == "" ?
                    Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset) :
                    ParsingUtilities.ParseFloatNullable(text);
                return value.HasValue;
            }

            var itemFilters = new ToolStripMenuItem("Filters");

            ToolStripMenuItem itemSetMinHeight = new ToolStripMenuItem("Set Min Height");
            itemSetMinHeight.Click += (sender, e) => DialogUtilities.UpdateNumberFromDialog(
                ref _minHeight,
                labelText: "Enter the min height.",
                parser: parseFloatNullable);

            ToolStripMenuItem itemClearMinHeight = new ToolStripMenuItem("Clear Min Height");
            itemClearMinHeight.Click += (sender, e) => _minHeight = null;

            ToolStripMenuItem itemSetMaxHeight = new ToolStripMenuItem("Set Max Height");
            itemSetMaxHeight.Click += (sender, e) => DialogUtilities.UpdateNumberFromDialog(
                ref _maxHeight,
                labelText: "Enter the max height.",
                parser: parseFloatNullable
                );

            ToolStripMenuItem itemClearMaxHeight = new ToolStripMenuItem("Clear Max Height");
            itemClearMaxHeight.Click += (sender, e) => _maxHeight = null;

            ToolStripMenuItem itemSetMinNormalY = new ToolStripMenuItem("Set Min Normal Y");
            itemSetMinNormalY.Click += (sender, e) => DialogUtilities.UpdateNumberFromDialog(
                ref _minNormalY,
                labelText: "Enter the min Normal Y component.",
                parser: parseFloatNullable
                );

            ToolStripMenuItem itemClearMinNormalY = new ToolStripMenuItem("Clear Min Normal Y");
            itemClearMinNormalY.Click += (sender, e) => _minNormalY = null;

            ToolStripMenuItem itemSetMaxNormalY = new ToolStripMenuItem("Set Max Normal Y");
            itemSetMaxNormalY.Click += (sender, e) => DialogUtilities.UpdateNumberFromDialog(
                ref _maxNormalY,
                labelText: "Enter the Max Normal Y component.",
                parser: parseFloatNullable
                );

            ToolStripMenuItem itemClearMaxNormalY = new ToolStripMenuItem("Clear Max Normal Y");
            itemClearMaxNormalY.Click += (sender, e) => _maxNormalY = null;

            itemFilters.DropDownItems.AddRange(new[] {
                itemSetMinHeight,
                itemClearMinHeight,
                itemSetMaxHeight,
                itemClearMaxHeight,
                itemSetMinNormalY,
                itemClearMinNormalY,
                itemSetMaxNormalY,
                itemClearMaxNormalY,
            });
            return new List<ToolStripMenuItem>() { itemFilters };
        }

        public override (SaveSettings save, LoadSettings load) SettingsSaveLoad => (
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.save(node);
                SaveValueNode(node, "MinHeight", _minHeight.ToString());
                SaveValueNode(node, "MaxHeight", _maxHeight.ToString());
            }
        ,
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.load(node);
                if (float.TryParse(LoadValueNode(node, "MinHeight"), out float minHeight))
                    _minHeight = minHeight;
                if (float.TryParse(LoadValueNode(node, "MaxHeight"), out float maxHeight))
                    _maxHeight = maxHeight;
            }
        );

        private List<List<(float x, float y, float z)>> GetVertexListsWithSplicing(float? minHeight, float? maxHeight)
        {
            List<List<(float x, float y, float z)>> vertexLists = GetVertexLists();
            if (!minHeight.HasValue && !maxHeight.HasValue) return vertexLists; // short circuit

            List<List<(float x, float y, float z)>> splicedVertexLists = new List<List<(float x, float y, float z)>>();
            foreach (List<(float x, float y, float z)> vertexList in vertexLists)
            {
                List<(float x, float y, float z)> splicedVertexList = new List<(float x, float y, float z)>();
                splicedVertexList.AddRange(vertexList);

                float minY = splicedVertexList.Min(vertex => vertex.y);
                float maxY = splicedVertexList.Max(vertex => vertex.y);

                if (minHeight.HasValue)
                {
                    if (minHeight.Value > maxY) continue; // don't add anything
                    if (minHeight.Value > minY)
                    {
                        List<(float x, float y, float z)> tempVertexList = new List<(float x, float y, float z)>();
                        for (int i = 0; i < splicedVertexList.Count; i++)
                        {
                            (float x1, float y1, float z1) = splicedVertexList[i];
                            (float x2, float y2, float z2) = splicedVertexList[(i + 1) % splicedVertexList.Count];
                            bool isValid1 = y1 >= minHeight.Value;
                            bool isValid2 = y2 >= minHeight.Value;
                            if (isValid1)
                                tempVertexList.Add((x1, y1, z1));
                            if (isValid1 != isValid2)
                                tempVertexList.Add(InterpolatePointForY(x1, y1, z1, x2, y2, z2, minHeight.Value));
                        }
                        splicedVertexList.Clear();
                        splicedVertexList.AddRange(tempVertexList);
                    }
                }

                if (maxHeight.HasValue)
                {
                    if (maxHeight.Value < minY) continue; // don't add anything
                    if (maxHeight.Value < maxY)
                    {
                        List<(float x, float y, float z)> tempVertexList = new List<(float x, float y, float z)>();
                        for (int i = 0; i < splicedVertexList.Count; i++)
                        {
                            (float x1, float y1, float z1) = splicedVertexList[i];
                            (float x2, float y2, float z2) = splicedVertexList[(i + 1) % splicedVertexList.Count];
                            bool isValid1 = y1 <= maxHeight.Value;
                            bool isValid2 = y2 <= maxHeight.Value;
                            if (isValid1)
                                tempVertexList.Add((x1, y1, z1));
                            if (isValid1 != isValid2)
                                tempVertexList.Add(InterpolatePointForY(x1, y1, z1, x2, y2, z2, maxHeight.Value));
                        }
                        splicedVertexList.Clear();
                        splicedVertexList.AddRange(tempVertexList);
                    }
                }

                splicedVertexLists.Add(splicedVertexList);
            }
            return splicedVertexLists;
        }

        private (float x, float y, float z) InterpolatePointForY(
            float x1, float y1, float z1, float x2, float y2, float z2, float y)
        {
            float proportion = (y - y1) / (y2 - y1);
            float x = x1 + proportion * (x2 - x1);
            float z = z1 + proportion * (z2 - z1);
            return (x, y, z);
        }
    }
}
