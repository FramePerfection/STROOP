using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
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
        public bool showWater = true;
        public bool showSky = true;
        public bool cullFaces = true;
        public bool hideAboveMario = false;
        public float hideAboveMarioOffset = 200;

        public LevelModel Model => model;

        public MapLevelModelObject() : base()
        {
        }

        public override string GetName() => "Level Model";

        public override Lazy<Image> GetInternalImage() => fallback.GetInternalImage();

        int ComputeLevelKey()
        {
            uint area = ram.U32(AreaConfig.CurrentAreaPointerAddress);
            var hash = new HashCode();
            hash.Add(ram.U8(MiscConfig.LevelIndexAddress));
            hash.Add(area);
            hash.Add(Rdram.IsPointer(area) ? ram.U8(area) : 0);
            hash.Add(Rdram.IsPointer(area) ? ram.U32(area + 4) : 0);
            hash.Add(ram.S32(TriangleConfig.LevelTriangleCountAddress));
            hash.Add(ram.U32(Config.ObjectAssociations.SegmentTable + 4 * 7));
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

            int key = ComputeLevelKey();
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

            if (model != null && showObjects)
                try
                {
                    model.UpdateObjects(ram, GetGameAddresses(), GetDoors());
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"Objects could not be placed: {e}");
                    model.instances.Clear();
                }

            waters = showWater ? WaterUtilities.GetWaterLevels() : null;
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
            var addresses = RomVersionConfig.Version == RomVersion.US
                ? LevelModel.GameAddresses.US(MarioObjectConfig.PointerAddress, MarioConfig.StructAddress)
                : new LevelModel.GameAddresses { marioObjectPointer = MarioObjectConfig.PointerAddress, marioState = MarioConfig.StructAddress };
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
            return Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset) + hideAboveMarioOffset;
        }

        void DrawModel(MapGraphics graphics)
        {
            var drawnModel = model;
            var drawnWaters = waters;
            float maxY = GetMaxY();
            graphics.drawLayers[(int)MapGraphics.DrawLayers.Background].Add(() =>
            {
                renderer.Draw(graphics, drawnModel, maxY, cullFaces, showObjects, drawnWaters,
                    showSky && graphics.viewMode == MapGraphics.ViewMode.ThreeDimensional);
                if (graphics.viewMode != MapGraphics.ViewMode.ThreeDimensional)
                {
                    GL.Clear(ClearBufferMask.DepthBufferBit);
                    GL.Disable(EnableCap.DepthTest);
                }
            });
        }

        protected override void DrawTopDown(MapGraphics graphics)
        {
            if (UseFallback)
                fallback.Draw(graphics);
            else
                DrawModel(graphics);
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
        }
    }
}
