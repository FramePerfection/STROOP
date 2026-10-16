using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public abstract class MapQuadObject : MapObject
    {
        public MapQuadObject() : base()
        {
        }

        protected override void DrawTopDown(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() => AddQuads(graphics));
        }

        protected override void DrawOrthogonal(MapGraphics graphics) => DrawTopDown(graphics);

        protected override void Draw3D(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                var boxes = GetBoxList3D(graphics);
                if (boxes == null)
                {
                    AddQuads(graphics);
                    return;
                }

                var color = Utilities.OpenTKUtilities.ColorToVec4(Color, OpacityByte);
                var outlineColor = Utilities.OpenTKUtilities.ColorToVec4(OutlineColor);
                foreach (var box in boxes)
                    AddBox(graphics, box, color, outlineColor);
            });
        }

        void AddQuads(MapGraphics graphics)
        {
            var color = Utilities.OpenTKUtilities.ColorToVec4(Color, OpacityByte);
            var outlineColor = Utilities.OpenTKUtilities.ColorToVec4(OutlineColor);

            foreach (var quad in GetQuadList())
            {
                Matrix4 transform = Matrix4.CreateRotationX((float)Math.PI / 2)
                                    * Matrix4.CreateScale((quad.xMax - quad.xMin) * 0.5f, 1, (quad.zMax - quad.zMin) * 0.5f)
                                    * Matrix4.CreateTranslation((quad.xMin + quad.xMax) * 0.5f, quad.y, (quad.zMin + quad.zMax) * 0.5f);
                graphics.circleRenderer.AddInstance(
                    graphics.viewMode != MapGraphics.ViewMode.TopDown,
                    transform,
                    OutlineWidth,
                    color,
                    outlineColor,
                    Renderers.ShapeRenderer.Shapes.Quad);
            }
        }

        void AddBox(MapGraphics graphics, (float xMin, float xMax, float zMin, float zMax, float yMin, float yMax) box, Vector4 color, Vector4 outlineColor)
        {
            Vector3 Corner(float x, float y, float z) => new Vector3(x, y, z);
            float x0 = box.xMin, x1 = box.xMax, z0 = box.zMin, z1 = box.zMax;
            float y0 = Math.Min(box.yMin, box.yMax), y1 = Math.Max(box.yMin, box.yMax);

            if (y1 - y0 < 1e-3f)
            {
                AddFace(graphics, Corner(x0, y0, z0), Corner(x1, y0, z0), Corner(x1, y0, z1), Corner(x0, y0, z1), color, outlineColor);
                return;
            }

            AddFace(graphics, Corner(x0, y0, z0), Corner(x1, y0, z0), Corner(x1, y0, z1), Corner(x0, y0, z1), color, outlineColor);
            AddFace(graphics, Corner(x0, y1, z0), Corner(x1, y1, z0), Corner(x1, y1, z1), Corner(x0, y1, z1), color, outlineColor);
            AddFace(graphics, Corner(x0, y0, z0), Corner(x1, y0, z0), Corner(x1, y1, z0), Corner(x0, y1, z0), color, outlineColor);
            AddFace(graphics, Corner(x0, y0, z1), Corner(x1, y0, z1), Corner(x1, y1, z1), Corner(x0, y1, z1), color, outlineColor);
            AddFace(graphics, Corner(x0, y0, z0), Corner(x0, y0, z1), Corner(x0, y1, z1), Corner(x0, y1, z0), color, outlineColor);
            AddFace(graphics, Corner(x1, y0, z0), Corner(x1, y0, z1), Corner(x1, y1, z1), Corner(x1, y1, z0), color, outlineColor);
        }

        void AddFace(MapGraphics graphics, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector4 color, Vector4 outlineColor)
        {
            graphics.triangleRenderer.Add(p0, p1, p2, false, color, outlineColor, new Vector3(OutlineWidth, 0, OutlineWidth), true);
            graphics.triangleRenderer.Add(p0, p2, p3, false, color, outlineColor, new Vector3(OutlineWidth, OutlineWidth, 0), true);
        }

        protected abstract List<(float xMin, float xMax, float zMin, float zMax, float y)> GetQuadList();

        protected virtual List<(float xMin, float xMax, float zMin, float zMax, float yMin, float yMax)> GetBoxList3D(MapGraphics graphics) => null;
    }
}
