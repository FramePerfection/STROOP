using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Mathematics;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public abstract class MapMapObject : MapBackgroundObject
    {
        Renderers.SpriteRenderer renderer;

        public MapMapObject() : base()
        {
            InternalRotates = true;
            renderer = new Renderers.SpriteRenderer(MapGraphics.DrawLayers.Background);
            renderer.texture = 0;
        }

        public abstract MapLayout GetMapLayout();
        protected override BackgroundImage GetBackgroundImage() => GetMapLayout().MapImage.Value;

        protected override void DrawTopDown(MapGraphics graphics)
        {
            renderer.SetDrawCalls(graphics);
            var img = GetInternalImage();
            if (img == null)
                return;
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                renderer.texture = GraphicsUtil.TextureFromImage(img.Value);
                foreach (var dim in GetDimensions(graphics))
                    renderer.AddInstance(
                        graphics.BillboardMatrix
                        * Matrix4.CreateScale(dim.size.Width, 1, dim.size.Height)
                        * Matrix4.CreateTranslation(dim.loc.X, 0, dim.loc.Y),
                        graphics.rendererCollection.GetObjectTextureLayer(GetInternalImage().Value),
                        new Vector4(1)
                    );
            });
        }

        protected override void DrawOrthogonal(MapGraphics graphics)
        {
        }

        protected override void Draw3D(MapGraphics graphics)
        {
            renderer.SetDrawCalls(graphics);
            var img = GetInternalImage();
            if (img == null)
                return;
            var flat = new Matrix4(1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1);
            float y = GetPlaneY3D();
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                renderer.texture = GraphicsUtil.TextureFromImage(img.Value);
                foreach (var dim in GetDimensions(graphics))
                    renderer.AddInstance(
                        flat
                        * Matrix4.CreateScale(dim.size.Width, 1, dim.size.Height)
                        * Matrix4.CreateTranslation(dim.loc.X, y, dim.loc.Y),
                        graphics.rendererCollection.GetObjectTextureLayer(GetInternalImage().Value),
                        new Vector4(1)
                    );
            });
        }

        MapLayout planeYLayout;
        int planeYTriangleCount = -1;
        float? planeY;

        float GetPlaneY3D()
        {
            if (Config.Stream == null)
                return 0;
            MapLayout layout = GetMapLayout();
            if (layout != null && layout.Y != float.MinValue && layout.Y != 0)
                return layout.Y;

            if (!MappingConfig.IsMapped(RomVersionConfig.SwitchOnly(TriangleConfig.LevelTriangleCountAddressUS, TriangleConfig.LevelTriangleCountAddressJP, TriangleConfig.LevelTriangleCountAddressSH))
                || !MappingConfig.IsMapped(RomVersionConfig.SwitchOnly(TriangleConfig.TriangleListPointerAddressUS, TriangleConfig.TriangleListPointerAddressJP)))
                return Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);

            int triangleCount = Config.Stream.GetInt32(TriangleConfig.LevelTriangleCountAddress);
            if (!Equals(layout, planeYLayout) || triangleCount != planeYTriangleCount)
            {
                planeYLayout = layout;
                planeYTriangleCount = triangleCount;
                planeY = null;
                foreach (var tri in TriangleUtilities.GetLevelTriangles())
                    if (tri.Classification == TriangleClassification.Floor && (!planeY.HasValue || tri.GetMinY() < planeY.Value))
                        planeY = tri.GetMinY();
            }

            return planeY ?? Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
        }

        protected List<(PointF loc, SizeF size)> GetDimensions(MapGraphics graphics)
        {
            RectangleF rectangle = GetMapLayout().Coordinates;
            float rectangleCenterX = rectangle.X + rectangle.Width / 2;
            float rectangleCenterZ = rectangle.Y + rectangle.Height / 2;
            List<(float x, float z)> rectangleCenters = graphics.MapViewEnablePuView ? currentMapTab.GetPuCoordinates(graphics, rectangleCenterX, rectangleCenterZ) : new List<(float x, float z)>() { (rectangleCenterX, rectangleCenterZ) };

            List<(PointF loc, SizeF size)> dimensions = rectangleCenters.ConvertAll(
                rectangleCenter => (new PointF(rectangleCenter.x, rectangleCenter.z), new SizeF(rectangle.Width, rectangle.Height)));
            return dimensions;
        }
    }
}
