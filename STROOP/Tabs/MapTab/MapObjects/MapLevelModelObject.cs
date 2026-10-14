using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using STROOP.Core;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Tabs.MapTab.DataUtil;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public class MapLevelModelObject : MapObject
    {
        const int STABLE_UPDATES_BEFORE_REBUILD = 3;

        readonly MapCurrentMapObject fallback = new MapCurrentMapObject();
        readonly Renderers.LevelModelRenderer renderer = new Renderers.LevelModelRenderer();
        readonly Rdram ram = new Rdram(new byte[Config.RamSize]);

        LevelModel model;
        int levelKey;
        int stableUpdates = -1;
        List<(int y, int xMin, int xMax, int zMin, int zMax)> waters;

        public bool showModel = true;
        public bool showObjects = true;
        public bool showWaterBoxes = false;
        public bool showSky = true;
        public bool showFog = false;
        public bool keepMarioVisible = true;

        public static (Vector3 position, Vector3 focus)? GameCamera { get; private set; }

        const float MARIO_MIN_PIXELS = 40;
        public bool cullFaces = true;
        public bool hideAboveMario = false;
        public float hideAboveMarioOffset = 200;

        public LevelModel Model => model;

        public MapLevelModelObject() : base()
        {
        }

        public override string GetName() => "Level Model";

        public override Lazy<Image> GetInternalImage() => fallback.GetInternalImage();

        int ComputeLevelKey(in LevelModel.GameAddresses addresses)
        {
            static bool Known(uint us, uint jp, uint sh, uint eu) => MappingConfig.IsMapped(RomVersionConfig.SwitchOnly(us, jp, sh, eu));

            uint area = ram.U32(AreaConfig.CurrentAreaPointerAddress);
            var hash = new HashCode();
            if (Known(MiscConfig.LevelIndexAddressUS, MiscConfig.LevelIndexAddressJP, MiscConfig.LevelIndexAddressSH, MiscConfig.LevelIndexAddressEU))
                hash.Add(ram.U8(MiscConfig.LevelIndexAddress));
            hash.Add(area);
            hash.Add(Rdram.IsPointer(area) ? ram.U8(area) : 0);
            hash.Add(Rdram.IsPointer(area) ? ram.U32(area + 4) : 0);
            if (Known(TriangleConfig.LevelTriangleCountAddressUS, TriangleConfig.LevelTriangleCountAddressJP, TriangleConfig.LevelTriangleCountAddressSH, 0))
                hash.Add(ram.S32(TriangleConfig.LevelTriangleCountAddress));
            hash.Add(ram.U32(Config.ObjectAssociations.SegmentTable + 4 * 7));
            uint waters = Known(MiscConfig.WaterPointerAddressUS, MiscConfig.WaterPointerAddressJP, 0, MiscConfig.WaterPointerAddressEU)
                ? ram.U32(MiscConfig.WaterPointerAddress)
                : 0;
            if (Rdram.IsPointer(waters))
                for (uint i = 0, count = (uint)Math.Clamp((int)ram.S16(waters), 0, 64); i < count; i++)
                    hash.Add(ram.S16(waters + 12 + 12 * i));
            hash.Add(LevelModel.StaticStateKey(ram, addresses));
            return hash.ToHashCode();
        }

        public override void Update()
        {
            base.Update();
            fallback.Update();
            if (Config.Stream == null || !currentMapTab.IsMapVisible)
                return;

            Config.Stream.CopyCachedRam(ram.data);
            ram.LoadSegmentTable(Config.ObjectAssociations.SegmentTable);

            var addresses = GetGameAddresses();
            int key = ComputeLevelKey(addresses);
            if (key != levelKey)
            {
                levelKey = key;
                stableUpdates = 0;
            }
            else if (stableUpdates >= 0)
                stableUpdates++;
            else if (model == null)
                stableUpdates = 0;

            if (currentMapTab.NeedsGeometryRefresh())
                stableUpdates = STABLE_UPDATES_BEFORE_REBUILD;

            if (stableUpdates >= STABLE_UPDATES_BEFORE_REBUILD)
            {
                stableUpdates = -1;
                Rebuild();
            }

            if (model != null)
                try
                {
                    model.UpdateLevelGeometry(ram, addresses);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"Paintings could not be drawn: {e}");
                    model.levelInstances.Clear();
                }

            if (model != null && showObjects)
                try
                {
                    model.UpdateObjects(ram, addresses, GetDoors());
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"Objects could not be placed: {e}");
                    model.instances.Clear();
                }

            model?.UpdateTextureAnimations(ram);
            GameCamera = model != null && model.TryGetGameCamera(ram, out var cameraPosition, out var cameraFocus)
                ? (cameraPosition, cameraFocus)
                : null;
            waters = showWaterBoxes ? WaterUtilities.GetWaterLevels() : null;
        }

        readonly HashSet<uint> doors = new HashSet<uint>();

        static readonly uint[] DoorBehaviors = { 0x13000000, 0x13000AFC, 0x13000B0C };

        HashSet<uint> GetDoors()
        {
            doors.Clear();
            foreach (var obj in Config.StroopMainForm.ObjectSlotsManager.GetLoadedObjectsWithPredicate(
                         o => Array.IndexOf(DoorBehaviors, o.BehaviorCriteria.BehaviorAddress) >= 0))
                doors.Add(obj.Address);
            return doors;
        }

        static LevelModel.GameAddresses GetGameAddresses()
        {
            var addresses = LevelModel.GameAddresses.FromSymbols(MappingConfig.GetSymbolAddress, MarioObjectConfig.PointerAddress, MarioConfig.StructAddress);
            if (RomVersionConfig.Version == RomVersion.US)
            {
                var ghostTab = Config.StroopMainForm.GetTab<GhostTab.GhostTab>();
                addresses.ghostHack = ghostTab.GhostHackInstalled;
                if (ghostTab.ColoredHatsInstalled)
                {
                    addresses.coloredHatsGenerator = ghostTab.ColoredHatsGeneratorAddress;
                    addresses.coloredHatsLights = ghostTab.ColoredHatsLightsAddress;
                }
            }

            return addresses;
        }

        public void Rebuild()
        {
            try
            {
                model = LevelModel.Build(ram, AreaConfig.CurrentAreaPointerAddress, GetGameAddresses());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Level model could not be built: {e}");
                model = LevelModel.None;
            }
        }

        bool UseFallback => !currentMapTab.IsLevelModelShown;

        float GetMaxY()
        {
            if (!hideAboveMario)
                return float.PositiveInfinity;
            return (Config.Stream?.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset) ?? 0) + hideAboveMarioOffset;
        }

        void DrawModel(MapGraphics graphics)
        {
            var drawnModel = model;
            var drawnWaters = waters;
            float maxY = GetMaxY();
            var levelCopies = graphics.MapViewEnablePuView ? GetPuOffsets(graphics) : null;
            graphics.drawLayers[(int)MapGraphics.DrawLayers.Background].Add(() =>
            {
                bool marioShown = graphics.viewMode == MapGraphics.ViewMode.ThreeDimensional && showObjects
                                  && drawnModel != null && drawnModel.marioInstances.Count > 0;
                renderer.Draw(graphics, drawnModel, maxY, cullFaces, showObjects, drawnWaters,
                    showSky && graphics.viewMode == MapGraphics.ViewMode.ThreeDimensional,
                    showFog && graphics.viewMode == MapGraphics.ViewMode.ThreeDimensional,
                    marioShown ? graphics.marioModelCopies : null, levelCopies, keepMarioVisible ? MARIO_MIN_PIXELS : 0);
                if (marioShown)
                    graphics.ReportMarioModelDrawn();
                if (graphics.viewMode != MapGraphics.ViewMode.ThreeDimensional)
                {
                    GL.Clear(ClearBufferMask.DepthBufferBit);
                    GL.Disable(EnableCap.DepthTest);
                }
            });
        }

        const int MAX_PU_COPIES = 81;

        List<Vector3> GetPuOffsets(MapGraphics graphics)
        {
            var center = graphics.currentView.position;
            return currentMapTab.GetPuCenters(graphics)
                .Select(c => new Vector3(c.x, 0, c.z))
                .OrderBy(c => (c.Xz - center.Xz).LengthSquared)
                .Take(MAX_PU_COPIES)
                .ToList();
        }

        const float PU_SIZE = 65536, PU_HALF = 32768, BEACON_HEIGHT = 20000;

        static int PuIndex(float coordinate) => (int)Math.Floor((coordinate + PU_HALF) / PU_SIZE);

        void DrawMarioPuMarker(MapGraphics graphics)
        {
            if (!graphics.MapViewEnablePuView || Config.Stream == null)
                return;
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                var mario = new Vector3(Models.DataModels.Mario.X, Models.DataModels.Mario.Y, Models.DataModels.Mario.Z);
                if (float.IsNaN(mario.X) || float.IsNaN(mario.Z))
                    return;
                int puX = PuIndex(mario.X), puZ = PuIndex(mario.Z);
                var color = new Vector4(1, 0.2f, 1, 1);

                if (graphics.viewMode == MapGraphics.ViewMode.ThreeDimensional)
                    graphics.lineRenderer.Add(mario, mario + new Vector3(0, BEACON_HEIGHT, 0), color, 3);

                float cx = puX * PU_SIZE, cz = puZ * PU_SIZE;
                var corners = new[]
                {
                    new Vector3(cx - PU_HALF, mario.Y, cz - PU_HALF), new Vector3(cx + PU_HALF, mario.Y, cz - PU_HALF),
                    new Vector3(cx + PU_HALF, mario.Y, cz + PU_HALF), new Vector3(cx - PU_HALF, mario.Y, cz + PU_HALF),
                };
                for (int i = 0; i < corners.Length; i++)
                    graphics.lineRenderer.Add(corners[i], corners[(i + 1) % corners.Length], color, 2);

                graphics.textRenderer.AddText($"Mario  PU ({puX}, {puZ})", mario + new Vector3(0, 200, 0), Color.Magenta,
                    StringAlignment.Center, Renderers.TextRenderer.Fonts.large, StringAlignment.Far);
            });
        }

        protected override void DrawTopDown(MapGraphics graphics)
        {
            if (UseFallback)
                fallback.Draw(graphics);
            else
                DrawModel(graphics);
            DrawMarioPuMarker(graphics);
        }

        protected override void DrawOrthogonal(MapGraphics graphics)
        {
            if (!UseFallback)
                DrawModel(graphics);
        }

        protected override void Draw3D(MapGraphics graphics)
        {
            if (UseFallback)
                fallback.Draw(graphics);
            else
                DrawModel(graphics);
            DrawMarioPuMarker(graphics);
        }
    }
}
