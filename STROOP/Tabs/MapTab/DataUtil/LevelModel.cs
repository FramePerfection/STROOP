using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.DataUtil
{
    public class LevelModel
    {
        public class GameAddresses
        {
            public uint marioObjectPointer, marioState;

            public uint geoSwitchAnimState;

            public uint skyboxTextures;

            public uint geoPaintingDraw, paintingGroups;

            public uint coloredHatsGenerator, coloredHatsLights;

            public bool ghostHack;

            public uint geoSwitchMarioCapEffect, geoSwitchMarioCapOnOff, geoMirrorMarioSetAlpha, bodyStates;

            public uint geoUpdateLayerTransparency;

            public uint environmentRegions, movtexIdToTexture, movtexQuadCollectionFromId;
            public uint geoMovtexDrawWaterRegions, geoMovtexDrawNoColor, geoMovtexDrawColored, geoMovtexDrawColoredNoUpdate, geoMovtexDrawColored2NoUpdate;
            public uint movtexNonColored, movtexColored, movtexColored2;

            public uint geoRenderMirrorMario, geoBitsBowserColoring, geoExecInsideCastleLight, hudDisplay, saveFileGetFlags;

            public uint geoEnvFxMain, envFxMode, envFxBuffer, snowParticleCount, bubbleParticleMaxCount;

            public uint geoExecFlyingCarpetCreate, flyingCarpetRippleTimer;

            public uint displayPaintingRippling, paintingRippleImage, paintingRippleEnvMapped;

            public static GameAddresses FromSymbols(Func<string, uint?> symbol, uint marioObjectPointer, uint marioState)
            {
                uint S(string name) => symbol(name) ?? 0;
                uint envFxBubbleConfig = S("gEnvFxBubbleConfig");
                return new GameAddresses
                {
                    marioObjectPointer = marioObjectPointer,
                    marioState = marioState,
                    geoSwitchAnimState = S("geo_switch_anim_state"),
                    skyboxTextures = S("sSkyboxTextures"),
                    geoPaintingDraw = S("geo_painting_draw"),
                    paintingGroups = S("sPaintingGroups"),
                    geoSwitchMarioCapEffect = S("geo_switch_mario_cap_effect"),
                    geoSwitchMarioCapOnOff = S("geo_switch_mario_cap_on_off"),
                    geoMirrorMarioSetAlpha = S("geo_mirror_mario_set_alpha"),
                    bodyStates = S("gBodyStates"),
                    geoUpdateLayerTransparency = S("geo_update_layer_transparency"),
                    environmentRegions = S("gEnvironmentRegions"),
                    movtexIdToTexture = S("gMovtexIdToTexture"),
                    movtexQuadCollectionFromId = S("get_quad_collection_from_id"),
                    geoMovtexDrawWaterRegions = S("geo_movtex_draw_water_regions"),
                    geoMovtexDrawNoColor = S("geo_movtex_draw_nocolor"),
                    geoMovtexDrawColored = S("geo_movtex_draw_colored"),
                    geoMovtexDrawColoredNoUpdate = S("geo_movtex_draw_colored_no_update"),
                    geoMovtexDrawColored2NoUpdate = S("geo_movtex_draw_colored_2_no_update"),
                    movtexNonColored = S("gMovtexNonColored"),
                    movtexColored = S("gMovtexColored"),
                    movtexColored2 = S("gMovtexColored2"),
                    geoRenderMirrorMario = S("geo_render_mirror_mario"),
                    geoBitsBowserColoring = S("geo_bits_bowser_coloring"),
                    geoExecInsideCastleLight = S("geo_exec_inside_castle_light"),
                    hudDisplay = S("gHudDisplay"),
                    saveFileGetFlags = S("save_file_get_flags"),
                    geoEnvFxMain = S("geo_envfx_main"),
                    envFxMode = S("gEnvFxMode"),
                    envFxBuffer = S("gEnvFxBuffer"),
                    snowParticleCount = S("gSnowParticleCount"),
                    bubbleParticleMaxCount = symbol("sBubbleParticleMaxCount") ?? (envFxBubbleConfig != 0 ? envFxBubbleConfig + 0x1C : 0),
                    geoExecFlyingCarpetCreate = S("geo_exec_flying_carpet_create"),
                    flyingCarpetRippleTimer = S("sFlyingCarpetRippleTimer"),
                    displayPaintingRippling = S("display_painting_rippling"),
                    paintingRippleImage = S("painting_ripple_image"),
                    paintingRippleEnvMapped = S("painting_ripple_env_mapped"),
                };
            }
        }

        public struct Shadow
        {
            public Vector3 position;
            public float size, opacity;
            public bool square;
        }

        public struct Instance
        {
            public Mesh mesh;
            public Matrix4 matrix;
            public bool billboard;
            public Vector3 anchor, anchorScale;

            public bool frame;

            public bool cap;
        }

        public readonly MeshStore store = new MeshStore();
        public readonly TextureSet textures = new TextureSet();
        public Mesh level = new Mesh();
        public readonly List<Instance> instances = new List<Instance>();

        public readonly List<Instance> marioInstances = new List<Instance>();
        public Vector3 marioModelPosition;

        public int marioInstancesStart = -1;
        public readonly List<Shadow> shadows = new List<Shadow>();

        public Texture skybox;
        public Vector3? backgroundColor;

        public int displayListCount;

        public Vector2 perspectivePlanes = new Vector2(100, 20000);
        public uint area;

        public bool IsEmpty => level.triangleCount == 0;

        public static readonly LevelModel None = new LevelModel();

        const int GN_ROOT = 0x001, GN_ORTHO = 0x002, GN_PERSPECTIVE = 0x103, GN_LOD = 0x00B, GN_SWITCH = 0x10C, GN_CAMERA = 0x114;
        const int GN_TRANSROT = 0x015, GN_TRANS = 0x016, GN_ROT = 0x017, GN_OBJECT = 0x018, GN_ANIMPART = 0x019, GN_BILLBOARD = 0x01A, GN_DL = 0x01B, GN_SCALE = 0x01C;
        const int GN_SHADOW = 0x028, GN_OBJPARENT = 0x029, GN_GENLIST = 0x12A, GN_BG = 0x12C, GN_HELD = 0x12E;

        const int GRAPH_RENDER_ACTIVE = 0x01, GRAPH_RENDER_BILLBOARD = 0x04, GRAPH_RENDER_INVISIBLE = 0x10;

        sealed class GraphScheme
        {
            public static readonly GraphScheme Vanilla = new GraphScheme(null, null, 0x20);

            const int HACKER_ROOT = 20, HACKER_MASTER_LIST = 2, MASTER_LIST_HEADER = 0x14;

            static readonly int[] HackerTypes =
            {
                GN_ORTHO, GN_PERSPECTIVE, 0x004, GN_LOD, GN_SWITCH, GN_CAMERA, GN_TRANSROT, GN_TRANS, GN_ROT, GN_OBJECT,
                GN_ANIMPART, GN_BILLBOARD, GN_DL, GN_SCALE, GN_SHADOW, GN_OBJPARENT, GN_GENLIST, GN_BG, GN_HELD, 0x02F,
                GN_ROOT, 0x00A,
            };

            static readonly int[] HackerLayers = { 0, 1, 3, 2, 4, 6, 5, 7 };
            static readonly int[] HackerSilhouetteLayers = { 0, 1, 3, 2, 4, 4, 1, 4, 1, 4, 6, 5, 7 };

            readonly int[] types, layers;
            public readonly uint perspectiveNear;

            GraphScheme(int[] types, int[] layers, uint perspectiveNear)
            {
                this.types = types;
                this.layers = layers;
                this.perspectiveNear = perspectiveNear;
            }

            public bool EmulatorAutoLod => types != null;

            public int Type(int raw) => types == null ? raw : raw >= 0 && raw < types.Length ? types[raw] : -1;

            public int Layer(int raw) => layers == null ? raw : raw < layers.Length ? layers[raw] : LAYER_OPAQUE;

            public static GraphScheme Detect(Rdram ram, uint root)
            {
                if (ram.U16(root) != HACKER_ROOT)
                    return Vanilla;
                int layerCount = 8;
                foreach (uint c in Children(ram, root))
                    if (ram.U16(c) == HACKER_MASTER_LIST)
                    {
                        long size = (long)ram.U32(c + 0x10) - c;
                        if (size == MASTER_LIST_HEADER + 8 * HackerSilhouetteLayers.Length)
                            layerCount = HackerSilhouetteLayers.Length;
                        break;
                    }

                return new GraphScheme(HackerTypes, layerCount == 8 ? HackerLayers : HackerSilhouetteLayers, 0x1C);
            }
        }

        GraphScheme scheme = GraphScheme.Vanilla;

        int NodeType(Rdram ram, uint n) => scheme.Type(ram.U16(n));

        Microcode ucode = Microcode.F3D;

        F3D NewF3D(Rdram ram, MeshBuilder builder) => new F3D(ram, textures, builder) { ucode = ucode };

        F3D.State DefaultState(Rdram ram)
        {
            var f3d = NewF3D(ram, new MeshBuilder());
            f3d.combine1 = F3D.COMBINE_SHADE_W1;
            return f3d.Save();
        }

        uint rootNode, cameraNode, objectGroup, mirrorParent, envFxNode;

        readonly Dictionary<(uint dl, int layer, int state), (Mesh mesh, F3D.State state)> objectMeshes = new Dictionary<(uint, int, int), (Mesh, F3D.State)>();

        readonly F3D.State[] layerStates = new F3D.State[8];

        F3D.State defaultState;

        #region matrices (row vectors, v' = v . M: the game's convention)

        static Matrix4 Affine(double m00, double m01, double m02, double m10, double m11, double m12, double m20, double m21, double m22, double tx, double ty, double tz) =>
            new Matrix4(
                (float)m00, (float)m01, (float)m02, 0,
                (float)m10, (float)m11, (float)m12, 0,
                (float)m20, (float)m21, (float)m22, 0,
                (float)tx, (float)ty, (float)tz, 1);

        static Matrix4 RotZXYTrans(double tx, double ty, double tz, short rx, short ry, short rz)
        {
            double sx = Math.Sin(rx * Math.PI / 32768), cx = Math.Cos(rx * Math.PI / 32768);
            double sy = Math.Sin(ry * Math.PI / 32768), cy = Math.Cos(ry * Math.PI / 32768);
            double sz = Math.Sin(rz * Math.PI / 32768), cz = Math.Cos(rz * Math.PI / 32768);
            return Affine(
                cy * cz + sx * sy * sz, cx * sz, -sy * cz + sx * cy * sz,
                -cy * sz + sx * sy * cz, cx * cz, sy * sz + sx * cy * cz,
                cx * sy, -sx, cx * cy,
                tx, ty, tz);
        }

        static Matrix4 RotXYZTrans(double tx, double ty, double tz, short rx, short ry, short rz)
        {
            double sx = Math.Sin(rx * Math.PI / 32768), cx = Math.Cos(rx * Math.PI / 32768);
            double sy = Math.Sin(ry * Math.PI / 32768), cy = Math.Cos(ry * Math.PI / 32768);
            double sz = Math.Sin(rz * Math.PI / 32768), cz = Math.Cos(rz * Math.PI / 32768);
            return Affine(
                cy * cz, cy * sz, -sy,
                sx * sy * cz - cx * sz, sx * sy * sz + cx * cz, sx * cy,
                cx * sy * cz + sx * sz, cx * sy * sz - sx * cz, cx * cy,
                tx, ty, tz);
        }

        static Matrix4 ScaleRows(Matrix4 m, Vector3 s)
        {
            m.Row0 *= s.X;
            m.Row1 *= s.Y;
            m.Row2 *= s.Z;
            m.Row0.W = m.Row1.W = m.Row2.W = 0;
            return m;
        }

        #endregion

        #region the static level

        public static LevelModel Build(Rdram ram, uint currentAreaPointer, GameAddresses addresses)
        {
            var model = new LevelModel();
            model.area = ram.U32(currentAreaPointer);
            if (!Rdram.IsPointer(model.area))
                return model;
            uint root = ram.U32(model.area + 4);
            if (!Rdram.IsPointer(root))
                return model;
            model.rootNode = root;
            model.addresses = addresses;
            model.scheme = GraphScheme.Detect(ram, root);

            var dls = new List<(uint dl, int layer, Matrix4 m)>();
            model.CollectDisplayLists(ram, root, Matrix4.Identity, 0, dls);
            model.displayListCount = dls.Count;
            model.ucode = F3D.Detect(ram, dls.Select(d => d.dl));

            var builder = new MeshBuilder();
            var f3d = model.NewF3D(ram, builder);
            foreach (var (dl, layer, m) in dls)
            {
                f3d.geo = F3D.GEO_INIT;
                f3d.Run(dl, layer, m);
            }

            foreach (var (node, m) in model.movtexNodes)
            {
                f3d.geo = F3D.GEO_INIT;
                model.EmitMovtex(ram, f3d, node, m);
            }

            model.level = model.store.Add(builder);
            model.ReadBackground(ram, root, addresses);
            return model;
        }

        const float PAINTING_SIZE = 614;


        const int PAINTING_IDLE = 0, PAINTING_ENV_MAP = 1;

        public readonly List<Instance> levelInstances = new List<Instance>();

        public readonly MeshStore frameStore = new MeshStore();

        readonly List<(uint parameter, Matrix4 m)> paintingNodes = new List<(uint, Matrix4)>();

        public bool TryGetGameCamera(Rdram ram, out Vector3 position, out Vector3 focus)
        {
            position = focus = Vector3.Zero;
            if (!Rdram.IsPointer(cameraNode))
                return false;
            position = new Vector3(ram.F32(cameraNode + 0x1C), ram.F32(cameraNode + 0x20), ram.F32(cameraNode + 0x24));
            focus = new Vector3(ram.F32(cameraNode + 0x28), ram.F32(cameraNode + 0x2C), ram.F32(cameraNode + 0x30));
            bool Finite(Vector3 v) => float.IsFinite(v.X) && float.IsFinite(v.Y) && float.IsFinite(v.Z);
            return Finite(position) && Finite(focus) && (focus - position).LengthSquared > 1e-4f;
        }

        public void UpdateLevelGeometry(Rdram ram, GameAddresses addresses)
        {
            this.addresses = addresses;
            levelInstances.Clear();
            frameStore.length = 0;
            defaultState ??= DefaultState(ram);
            foreach (var (parameter, m) in paintingNodes)
                EmitPainting(ram, parameter, m);
        }

        void EmitPainting(Rdram ram, uint parameter, Matrix4 m)
        {
            uint group = ram.U32(addresses.paintingGroups + 4 * ((parameter >> 8) & 0xFF));
            if (group == 0)
                return;
            uint painting = ram.U32(group + 4 * (parameter & 0xFF));
            if (painting == 0 || !ram.Ok(ram.Phys(painting), 0x78))
                return;
            float size = ram.F32(painting + 0x74);
            if (!(size > 0))
                return;

            var transform = Matrix4.CreateScale(size / PAINTING_SIZE)
                            * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(ram.F32(painting + 0x0C)))
                            * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(ram.F32(painting + 0x08)))
                            * Matrix4.CreateTranslation(ram.F32(painting + 0x10), ram.F32(painting + 0x14), ram.F32(painting + 0x18))
                            * m;
            int layer = ram.U8(painting + 0x6D) == 0xFF ? LAYER_OPAQUE : LAYER_TRANSPARENT;

            if ((sbyte)ram.U8(painting + 0x07) != PAINTING_IDLE)
            {
                EmitRipplingPainting(ram, painting, layer, transform);
                return;
            }

            uint dl = ram.U32(painting + 0x58);
            if (dl == 0)
                return;
            var key = (dl, 0x500 | layer, 0);
            if (!objectMeshes.TryGetValue(key, out var entry))
            {
                var builder = new MeshBuilder();
                var f3d = NewF3D(ram, builder);
                f3d.Load(defaultState);
                f3d.Run(dl, layer, Matrix4.Identity);
                objectMeshes[key] = entry = (store.Add(builder), null);
            }

            if (entry.mesh.triangleCount > 0)
                levelInstances.Add(new Instance { mesh = entry.mesh, matrix = transform });
        }

        void EmitRipplingPainting(Rdram ram, uint painting, int layer, Matrix4 transform)
        {
            bool envMapped = (sbyte)ram.U8(painting + 0x03) == PAINTING_ENV_MAP;
            var meshData = FindSegmentedConstants(ram, addresses.displayPaintingRippling, 0x02);
            var beginEnd = FindSegmentedConstants(ram, envMapped ? addresses.paintingRippleEnvMapped : addresses.paintingRippleImage, 0x02);
            if (meshData.Count < 2 || beginEnd.Count < 2)
                return;
            uint mesh = meshData[0], neighborTris = meshData[1];

            int numVtx = Math.Clamp((int)ram.S16(mesh), 0, 1024);
            int numTris = Math.Clamp((int)ram.S16(mesh + 2 * (uint)(numVtx * 3 + 1)), 0, 2048);
            if (numVtx == 0 || numTris == 0)
                return;
            var pos = new short[numVtx, 3];
            var normal = new sbyte[numVtx, 3];

            float sizeRatio = ram.F32(painting + 0x74) / PAINTING_SIZE;
            float magnitude = ram.F32(painting + 0x1C), rate = ram.F32(painting + 0x34), dispersion = ram.F32(painting + 0x40);
            float timer = ram.F32(painting + 0x4C), rippleX = ram.F32(painting + 0x50), rippleY = ram.F32(painting + 0x54);
            for (int i = 0; i < numVtx; i++)
            {
                uint v = mesh + 2 * (uint)(i * 3 + 1);
                pos[i, 0] = ram.S16(v);
                pos[i, 1] = ram.S16(v + 2);
                if (ram.S16(v + 4) == 0)
                    continue;
                float x = pos[i, 0] * sizeRatio, y = pos[i, 1] * sizeRatio;
                float distance = MathF.Sqrt((x - rippleX) * (x - rippleX) + (y - rippleY) * (y - rippleY)) / dispersion;
                if (timer < distance)
                    continue;
                float z = magnitude * (float)Math.Cos(rate * (2 * Math.PI) * (timer - distance));
                pos[i, 2] = (short)(z >= 0 ? z + 0.5 : z - 0.5);
            }

            var triNormals = new Vector3[numTris];
            for (int i = 0; i < numTris; i++)
            {
                uint tri = mesh + 2 * (uint)(numVtx * 3 + i * 3 + 2);
                int v0 = Math.Clamp((int)ram.S16(tri), 0, numVtx - 1);
                int v1 = Math.Clamp((int)ram.S16(tri + 2), 0, numVtx - 1);
                int v2 = Math.Clamp((int)ram.S16(tri + 4), 0, numVtx - 1);
                float x0 = pos[v0, 0], y0 = pos[v0, 1], z0 = pos[v0, 2];
                float x1 = pos[v1, 0], y1 = pos[v1, 1], z1 = pos[v1, 2];
                float x2 = pos[v2, 0], y2 = pos[v2, 1], z2 = pos[v2, 2];
                triNormals[i] = new Vector3(
                    (y1 - y0) * (z2 - z1) - (z1 - z0) * (y2 - y1),
                    (z1 - z0) * (x2 - x1) - (x1 - x0) * (z2 - z1),
                    (x1 - x0) * (y2 - y1) - (y1 - y0) * (x2 - x1));
            }

            static sbyte Component(float c) => c > 0 ? (sbyte)(c * 127.0 + 0.5) : c < 0 ? (sbyte)(c * 128.0 - 0.5) : (sbyte)0;
            uint entry = neighborTris;
            for (int i = 0; i < numVtx; i++)
            {
                int neighbors = Math.Clamp((int)ram.S16(entry), 0, 64);
                var sum = Vector3.Zero;
                for (int j = 0; j < neighbors; j++)
                    sum += triNormals[Math.Clamp((int)ram.S16(entry + 2 * (uint)(j + 1)), 0, numTris - 1)];
                entry += 2 * (uint)(neighbors + 1);
                if (neighbors == 0 || sum.Length == 0)
                    continue;
                var n = sum.Normalized();
                normal[i, 0] = Component(n.X);
                normal[i, 1] = Component(n.Y);
                normal[i, 2] = Component(n.Z);
            }

            uint rippleDl = ram.U32(painting + 0x68), textureMaps = ram.U32(painting + 0x5C), textureArray = ram.U32(painting + 0x60);
            if (rippleDl == 0 || textureMaps == 0 || textureArray == 0)
                return;

            var builder = new MeshBuilder();
            var f3d = NewF3D(ram, builder);
            f3d.Load(defaultState);
            f3d.Run(beginEnd[0], layer, transform);
            f3d.Run(rippleDl, layer, transform);

            int images = envMapped ? 1 : Math.Clamp((int)(sbyte)ram.U8(painting + 0x02), 0, 16);
            int width = ram.S16(painting + 0x64), height = ram.S16(painting + 0x66);
            byte alpha = ram.U8(painting + 0x6D);
            var vertices = new byte[3 * 16];
            for (int image = 0; image < images; image++)
            {
                uint map = ram.U32(textureMaps + 4 * (uint)image);
                if (map == 0)
                    continue;
                f3d.Command(0xFD000000 | 2 << 19, ram.U32(textureArray + 4 * (uint)image), layer, transform);
                f3d.Command(0xF5000000 | 2 << 19, 0x07000000, layer, transform);
                f3d.Command(0xF3000000, 0x07000000 | (uint)Math.Clamp(width * height - 1, 0, 4095) << 12, layer, transform);

                int mapVerts = Math.Clamp((int)ram.S16(map), 0, 1024);
                int mapTris = Math.Clamp((int)ram.S16(map + 2 * (uint)(mapVerts * 3 + 1)), 0, 2048);
                if (mapVerts == 0)
                    continue;
                for (int t = 0; t < mapTris; t++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        int mapping = Math.Clamp((int)ram.S16(map + 2 * (uint)(mapVerts * 3 + 2 + t * 3 + k)), 0, mapVerts - 1);
                        uint mapped = map + 2 * (uint)(mapping * 3 + 1);
                        int vtx = Math.Clamp((int)ram.S16(mapped), 0, numVtx - 1);
                        WriteVertex(vertices, k, pos[vtx, 0], pos[vtx, 1], pos[vtx, 2], ram.S16(mapped + 2), ram.S16(mapped + 4),
                            (byte)normal[vtx, 0], (byte)normal[vtx, 1], (byte)normal[vtx, 2], alpha);
                    }

                    f3d.LoadVertices(vertices, 3, 0, transform);
                    f3d.Command(0xBF000000, 10 << 8 | 20, layer, transform);
                }
            }

            f3d.Run(beginEnd[beginEnd.Count - 1], layer, transform);
            var built = frameStore.Add(builder);
            if (built.triangleCount > 0)
                levelInstances.Add(new Instance { mesh = built, matrix = Matrix4.Identity, frame = true });
        }

        const uint SAVE_FLAG_HAVE_WING_CAP = 1 << 1;

        static bool CastleLightVisible(Rdram ram, GameAddresses addresses)
        {
            if (addresses.hudDisplay == 0 || addresses.saveFileGetFlags == 0)
                return false;
            uint? flags = MipsCall.Call(ram, addresses.saveFileGetFlags);
            return ram.S16(addresses.hudDisplay + 4) >= 10 && flags.HasValue && (flags.Value & SAVE_FLAG_HAVE_WING_CAP) == 0;
        }

        public static int StaticStateKey(Rdram ram, GameAddresses addresses) =>
            addresses.geoExecInsideCastleLight != 0 && CastleLightVisible(ram, addresses) ? 1 : 0;

        static uint FindSegmentedConstant(Rdram ram, uint function, int segment)
        {
            var constants = FindSegmentedConstants(ram, function, segment);
            return constants.Count > 0 ? constants[0] : 0;
        }

        const uint JR_RA = 0x03E00008;

        static int WrittenRegister(uint i)
        {
            int op = (int)(i >> 26);
            if (op == 0x00)
                return (int)(i >> 11) & 31;
            if (op == 0x03)
                return 31;
            if ((op >= 0x08 && op <= 0x0F) || (op >= 0x20 && op <= 0x27))
                return (int)(i >> 16) & 31;
            return 0;
        }

        static uint FindFirstHalfwordLoad(Rdram ram, uint function)
        {
            if (!Rdram.IsPointer(function))
                return 0;
            var upper = new uint?[32];
            uint end = function + 4 * 512;
            for (uint pc = function; pc < end; pc += 4)
            {
                uint i = ram.U32(pc);
                int op = (int)(i >> 26), rs = (int)(i >> 21) & 31, rt = (int)(i >> 16) & 31;
                uint imm = i & 0xFFFF;
                if ((op == 0x21 || op == 0x25) && upper[rs].HasValue)
                    return upper[rs].Value + (uint)(short)imm;
                upper[WrittenRegister(i)] = null;
                if (op == 0x0F)
                    upper[rt] = (imm >> 8) == 0x80 ? imm << 16 : null;
                else if (i == JR_RA)
                    end = pc + 8;
            }

            return 0;
        }

        static List<uint> FindSegmentedConstants(Rdram ram, uint function, int segment)
        {
            var found = new List<uint>();
            if (!Rdram.IsPointer(function))
                return found;
            var upper = new uint?[32];
            uint end = function + 4 * 512;
            for (uint pc = function; pc < end; pc += 4)
            {
                uint i = ram.U32(pc);
                int op = (int)(i >> 26), rs = (int)(i >> 21) & 31, rt = (int)(i >> 16) & 31;
                uint imm = i & 0xFFFF;
                if ((op == 0x09 || op == 0x0D) && upper[rs].HasValue)
                {
                    uint value = op == 0x09 ? upper[rs].Value + (uint)(short)imm : upper[rs].Value | imm;
                    if (found.Count == 0 || found[found.Count - 1] != value)
                        found.Add(value);
                }

                upper[WrittenRegister(i)] = null;
                if (op == 0x0F)
                    upper[rt] = (imm >> 8) == segment ? imm << 16 : null;
                else if (i == JR_RA)
                    end = pc + 8;
            }

            return found;
        }


        const int LAYER_TRANSPARENT_INTER = 7;
        const uint MOVTEX_JRB_INITIAL_MIST = 0x1205, MOVTEX_HMC_TOXIC_MAZE_MIST = 0x0702, MOVTEX_SSL_TOXBOX_QUICKSAND_MIST = 0x0851;
        const int MOVTEX_TEXTURE_MIST = 1;
        const int G_IM_FMT_RGBA = 0, G_IM_FMT_IA = 3;
        const uint MOVTEX_OBJECT_SIZE = 0x24, MOVTEX_QUAD_SIZE = 0x1C, MOVTEX_REGION_SIZE = 12;

        readonly List<(uint node, Matrix4 m)> movtexNodes = new List<(uint, Matrix4)>();

        public class UvAnimation
        {
            public uint address;
            public bool rotate;
            public short built;
            public Vector2 scale;
            public Matrix3 transform = Matrix3.Identity;
        }

        public readonly List<UvAnimation> uvAnimations = new List<UvAnimation>();

        int AddUvAnimation(Rdram ram, uint address, bool rotate, Vector2 scale)
        {
            uvAnimations.Add(new UvAnimation { address = address, rotate = rotate, built = ram.S16(address), scale = scale });
            return uvAnimations.Count - 1;
        }

        public void UpdateTextureAnimations(Rdram ram)
        {
            foreach (var a in uvAnimations)
            {
                int delta = (short)(ram.S16(a.address) - a.built);
                if (a.rotate)
                {
                    float angle = (float)(delta / 65536.0 * 2 * Math.PI), sin = MathF.Sin(angle), cos = MathF.Cos(angle);
                    a.transform = new Matrix3(cos, -sin, 0, sin, cos, 0, 0, 0, 1);
                }
                else
                    a.transform = new Matrix3(1, 0, 0, 0, 1, 0, delta * a.scale.X, 0, 1);
            }
        }

        bool IsMovtex(uint function) => function != 0 && (function == addresses.geoMovtexDrawWaterRegions
                                                           || function == addresses.geoMovtexDrawNoColor
                                                           || function == addresses.geoMovtexDrawColored
                                                           || function == addresses.geoMovtexDrawColoredNoUpdate
                                                           || function == addresses.geoMovtexDrawColored2NoUpdate);

        void EmitMovtex(Rdram ram, F3D f3d, uint node, Matrix4 m)
        {
            uint function = ram.U32(node + 0x14), parameter = ram.U32(node + 0x18);
            if (addresses.movtexIdToTexture == 0)
                return;
            if (function == addresses.geoMovtexDrawWaterRegions)
                EmitWaterRegions(ram, f3d, parameter, m);
            else if (function == addresses.geoMovtexDrawNoColor)
                EmitMovtexObject(ram, f3d, addresses.movtexNonColored, parameter, false, m);
            else if (function == addresses.geoMovtexDrawColored || function == addresses.geoMovtexDrawColoredNoUpdate)
                EmitMovtexObject(ram, f3d, addresses.movtexColored, parameter, true, m);
            else if (function == addresses.geoMovtexDrawColored2NoUpdate)
                EmitMovtexObject(ram, f3d, addresses.movtexColored2, parameter, true, m);
        }

        void EmitWaterRegions(Rdram ram, F3D f3d, uint parameter, Matrix4 m)
        {
            uint regions = ram.U32(addresses.environmentRegions);
            if (!Rdram.IsPointer(regions) || parameter == MOVTEX_JRB_INITIAL_MIST)
                return;
            uint? collection = MipsCall.Call(ram, addresses.movtexQuadCollectionFromId, parameter);
            if (!collection.HasValue || collection.Value == 0)
                return;

            bool mist = parameter == MOVTEX_JRB_INITIAL_MIST || parameter == MOVTEX_HMC_TOXIC_MAZE_MIST || parameter == MOVTEX_SSL_TOXBOX_QUICKSAND_MIST;
            byte r = 255, g = 255, b = 255;
            if (parameter == MOVTEX_HMC_TOXIC_MAZE_MIST)
                b = 0;
            else if (parameter == MOVTEX_SSL_TOXBOX_QUICKSAND_MIST)
                g = b = 0;

            const int layer = LAYER_TRANSPARENT_INTER;
            f3d.Command(0xB6000000, 0x00022000, layer, m);
            f3d.Command(0xFC121824, 0xFF33FFFF, layer, m);
            f3d.Command(0xBB000001, 0xFFFFFFFF, layer, m);
            f3d.Command(0xF5000000 | (uint)(mist ? G_IM_FMT_IA : G_IM_FMT_RGBA) << 21 | 2 << 19 | 8 << 9, 0x00014050, layer, m);
            f3d.Command(0xF2000000, 0x0007C07C, layer, m);

            int count = Math.Min((int)ram.S16(regions), 64);
            var vertices = new byte[4 * 16];
            for (int i = 0; i < count; i++)
            {
                uint region = regions + 2 + (uint)i * MOVTEX_REGION_SIZE;
                short id = ram.S16(region), y = ram.S16(region + 10);
                uint quads = FindQuadArray(ram, collection.Value, id);
                if (quads == 0)
                    continue;
                int quadCount = Math.Min((int)ram.S16(quads), 64);
                for (int q = 0; q < quadCount; q++)
                {
                    uint quad = quads + 2 + (uint)q * MOVTEX_QUAD_SIZE;
                    short rot = ram.S16(quad), scale = ram.S16(quad + 4);
                    byte alpha = (byte)ram.S16(quad + 24);
                    int textureId = ram.S16(quad + 26);
                    int direction = ram.S16(quad + 22) == 0 ? 1 : -1;
                    for (int v = 0; v < 4; v++)
                    {
                        int rotOffset = v == 0 ? 0 : v == 2 ? -32768 : direction * (v == 1 ? 16384 : -16384);
                        double angle = (ushort)(short)(rot + rotOffset) / 65536.0 * 2 * Math.PI;
                        double radius = 32.0 * (32.0 * scale - 1.0);
                        WriteVertex(vertices, v, ram.S16(quad + 6 + 4 * (uint)v), y, ram.S16(quad + 8 + 4 * (uint)v),
                            (short)(radius * Math.Sin(angle)), (short)(radius * Math.Cos(angle)), r, g, b, alpha);
                    }

                    LoadMovtexTexture(ram, f3d, textureId, textureId == MOVTEX_TEXTURE_MIST ? G_IM_FMT_IA : G_IM_FMT_RGBA, layer, m);
                    f3d.LoadVertices(vertices, 4, 0, m);
                    f3d.uvAnimation = AddUvAnimation(ram, quad, true, Vector2.Zero);
                    f3d.Command(0xBF000000, 10 << 8 | 20, layer, m);
                    f3d.Command(0xBF000000, 20 << 8 | 30, layer, m);
                    f3d.uvAnimation = -1;
                }
            }
        }

        static uint FindQuadArray(Rdram ram, uint collection, short id)
        {
            for (uint i = 0; i < 64; i++)
            {
                short entryId = ram.S16(collection + 8 * i);
                if (entryId == -1)
                    return 0;
                if (entryId == id)
                    return ram.U32(collection + 8 * i + 4);
            }

            return 0;
        }

        void EmitMovtexObject(Rdram ram, F3D f3d, uint table, uint parameter, bool colored, Matrix4 m)
        {
            if (table == 0)
                return;
            for (uint entry = table; entry < table + 64 * MOVTEX_OBJECT_SIZE; entry += MOVTEX_OBJECT_SIZE)
            {
                uint verts = ram.U32(entry + 0x0C);
                if (verts == 0)
                    return;
                if (ram.U32(entry) != parameter)
                    continue;

                int textureId = ram.S32(entry + 4), count = Math.Clamp(ram.S32(entry + 8), 0, 16);
                uint beginDl = ram.U32(entry + 0x10), endDl = ram.U32(entry + 0x14), triDl = ram.U32(entry + 0x18);
                byte r = ram.U8(entry + 0x1C), g = ram.U8(entry + 0x1D), b = ram.U8(entry + 0x1E), a = ram.U8(entry + 0x1F);
                int layer = ram.S32(entry + 0x20);

                uint stride = colored ? 8u : 5u, st = colored ? 7u : 4u;
                short baseS = ram.S16(verts + 2 * st), baseT = ram.S16(verts + 2 * (st + 1));
                var vertices = new byte[count * 16];
                for (int i = 0; i < count; i++)
                {
                    uint v = verts + 2 * (uint)i * stride;
                    short s = baseS, t = baseT;
                    if (i > 0)
                    {
                        s = (short)(baseS + ram.S16(v + 2 * st) * 1024);
                        t = (short)(baseT + ram.S16(v + 2 * (st + 1)) * 1024);
                    }

                    if (colored)
                        (r, g, b) = ((byte)ram.S16(v + 8), (byte)ram.S16(v + 10), (byte)ram.S16(v + 12));
                    WriteVertex(vertices, i, ram.S16(v + 2), ram.S16(v + 4), ram.S16(v + 6), s, t, r, g, b, a);
                }

                f3d.Run(beginDl, layer, m);
                LoadMovtexTexture(ram, f3d, textureId, G_IM_FMT_RGBA, layer, m);
                f3d.LoadVertices(vertices, count, 0, m);
                f3d.uvAnimation = AddUvAnimation(ram, verts + 2 * st, false, f3d.TexCoordScale());
                f3d.Run(triDl, layer, m);
                f3d.uvAnimation = -1;
                f3d.Run(endDl, layer, m);
                return;
            }
        }

        void LoadMovtexTexture(Rdram ram, F3D f3d, int textureId, int format, int layer, in Matrix4 m)
        {
            uint texture = ram.U32(addresses.movtexIdToTexture + 4 * (uint)textureId);
            f3d.Command(0xFD000000 | (uint)format << 21 | 2 << 19, texture, layer, m);
            f3d.Command(0xF5000000 | (uint)format << 21 | 2 << 19, 0x07000000, layer, m);
            f3d.Command(0xF3000000, 0x07000000 | 1023 << 12, layer, m);
        }

        static void WriteVertex(byte[] vertices, int index, short x, short y, short z, short s, short t, byte r, byte g, byte b, byte a)
        {
            int o = index * 16;
            void S16(int at, short value)
            {
                vertices[o + at] = (byte)(value >> 8);
                vertices[o + at + 1] = (byte)value;
            }

            S16(0, x);
            S16(2, y);
            S16(4, z);
            S16(6, 0);
            S16(8, s);
            S16(10, t);
            vertices[o + 12] = r;
            vertices[o + 13] = g;
            vertices[o + 14] = b;
            vertices[o + 15] = a;
        }

        const int SKYBOX_COLS = 10, SKYBOX_ROWS = 8;

        void ReadBackground(Rdram ram, uint root, GameAddresses addresses)
        {
            var stack = new Stack<uint>();
            var seen = new HashSet<uint>();
            stack.Push(root);
            while (stack.Count > 0 && seen.Count < 4096)
            {
                uint n = stack.Pop();
                if (!Rdram.IsPointer(n) || !seen.Add(n))
                    continue;
                if (NodeType(ram, n) == GN_BG)
                {
                    uint function = ram.U32(n + 0x14), background = ram.U32(n + 0x1C);
                    if (function == 0)
                    {
                        int c = (int)(background & 0xFFFF);
                        backgroundColor = new Vector3(((c >> 11) & 31) / 31f, ((c >> 6) & 31) / 31f, ((c >> 1) & 31) / 31f);
                    }
                    else if (addresses.skyboxTextures != 0)
                    {
                        uint tiles = ram.U32(addresses.skyboxTextures + 4 * (background & 0xFF));
                        var image = new byte[SKYBOX_COLS * 32 * SKYBOX_ROWS * 32 * 4];
                        for (int t = 0; t < SKYBOX_COLS * SKYBOX_ROWS; t++)
                        {
                            byte[] tile = TextureSet.DecodeTexture(ram, ram.U32(tiles + (uint)(4 * t)), 0, 2, 32, 32, null, 0, 0);
                            int x0 = t % SKYBOX_COLS * 32, y0 = t / SKYBOX_COLS * 32;
                            for (int y = 0; y < 32; y++)
                                Buffer.BlockCopy(tile, y * 128, image, ((y0 + y) * SKYBOX_COLS * 32 + x0) * 4, 128);
                        }

                        skybox = new Texture { width = SKYBOX_COLS * 32, height = SKYBOX_ROWS * 32, rgba = image, wrapS = 0, wrapT = 2 };
                    }

                    return;
                }

                foreach (uint c in Children(ram, n))
                    stack.Push(c);
            }
        }

        static IEnumerable<uint> Children(Rdram ram, uint n)
        {
            uint first = ram.U32(n + 0x10), c = first;
            for (int i = 0; i < 1024 && Rdram.IsPointer(c); i++)
            {
                yield return c;
                c = ram.U32(c + 0x08);
                if (c == first)
                    break;
            }
        }

        void CollectDisplayLists(Rdram ram, uint n, Matrix4 m, int depth, List<(uint, int, Matrix4)> output)
        {
            if (depth > 40)
                return;
            int t = NodeType(ram, n);
            ushort flags = ram.U16(n + 2);
            if ((flags & GRAPH_RENDER_ACTIVE) == 0)
                return;
            if (t == GN_CAMERA)
                cameraNode = n;
            if (t == GN_PERSPECTIVE)
                perspectivePlanes = new Vector2(ram.U16(n + scheme.perspectiveNear), ram.U16(n + scheme.perspectiveNear + 2));
            if (t == GN_OBJPARENT)
                objectGroup = ram.U32(n + 0x14);
            if (t == GN_GENLIST && addresses.geoPaintingDraw != 0 && ram.U32(n + 0x14) == addresses.geoPaintingDraw)
                paintingNodes.Add((ram.U32(n + 0x18), m));
            if (t == GN_GENLIST && IsMovtex(ram.U32(n + 0x14)))
                movtexNodes.Add((n, m));
            if (t == GN_GENLIST && addresses.geoRenderMirrorMario != 0 && ram.U32(n + 0x14) == addresses.geoRenderMirrorMario)
                mirrorParent = n;
            if (t == GN_GENLIST && addresses.geoEnvFxMain != 0 && ram.U32(n + 0x14) == addresses.geoEnvFxMain)
                envFxNode = n;
            if (t == GN_GENLIST && addresses.geoExecInsideCastleLight != 0 && ram.U32(n + 0x14) == addresses.geoExecInsideCastleLight
                && CastleLightVisible(ram, addresses))
            {
                uint lightDl = FindSegmentedConstant(ram, addresses.geoExecInsideCastleLight, 0x07);
                if (lightDl != 0)
                    output.Add((lightDl, LAYER_TRANSPARENT, m));
            }
            if (t == GN_OBJPARENT || t == GN_GENLIST || t == GN_BG || t == GN_HELD || t == GN_OBJECT || t == GN_SHADOW || t == GN_ORTHO)
                return;
            int layer = scheme.Layer((flags >> 8) & 0xFF);
            uint dl = 0;
            switch (t)
            {
                case GN_DL:
                    dl = ram.U32(n + 0x14);
                    break;
                case GN_TRANSROT:
                    m = RotZXYTrans(ram.S16(n + 0x18), ram.S16(n + 0x1A), ram.S16(n + 0x1C), ram.S16(n + 0x1E), ram.S16(n + 0x20), ram.S16(n + 0x22)) * m;
                    dl = ram.U32(n + 0x14);
                    break;
                case GN_TRANS:
                case GN_ANIMPART:
                case GN_BILLBOARD:
                    m = RotZXYTrans(ram.S16(n + 0x18), ram.S16(n + 0x1A), ram.S16(n + 0x1C), 0, 0, 0) * m;
                    dl = ram.U32(n + 0x14);
                    break;
                case GN_ROT:
                    m = RotZXYTrans(0, 0, 0, ram.S16(n + 0x18), ram.S16(n + 0x1A), ram.S16(n + 0x1C)) * m;
                    dl = ram.U32(n + 0x14);
                    break;
                case GN_SCALE:
                    m = Matrix4.CreateScale(ram.F32(n + 0x18)) * m;
                    dl = ram.U32(n + 0x14);
                    break;
            }

            if (dl != 0)
                output.Add((dl, layer, m));
            if (t == GN_LOD && !(ram.S16(n + 0x14) <= 0 && 0 < ram.S16(n + 0x16)))
                return;

            foreach (uint c in Children(ram, n))
                CollectDisplayLists(ram, c, m, depth + 1, output);
        }

        #endregion

        #region the objects (Mario included), as rendered on the last frame

        const int ANIM_NONE = 0, ANIM_TRANSLATION = 1, ANIM_VERTICAL_TRANSLATION = 2, ANIM_LATERAL_TRANSLATION = 3, ANIM_NO_TRANSLATION = 4, ANIM_ROTATION = 5;

        struct AnimState
        {
            public int type, frame;
            public uint attributes, values;
            public float translationMultiplier;
        }

        AnimState anim;
        Vector3 gameCamera, gameCameraForward;

        float LodDistance(Vector3 position) =>
            scheme.EmulatorAutoLod ? 50
            : gameCameraForward != Vector3.Zero ? Vector3.Dot(position - gameCamera, gameCameraForward)
            : (position - gameCamera).Length;
        byte areaIndex;
        int nodeBudget;
        GameAddresses addresses;
        uint currentObject, marioObject, mirrorMario;
        bool inHeldObject;
        ISet<uint> roomCulledObjects;

        const int ACTIVE_FLAG_ACTIVE = 0x01, ACTIVE_FLAG_IN_DIFFERENT_ROOM = 0x08;

        public void UpdateObjects(Rdram ram, GameAddresses addresses, ISet<uint> roomCulledObjects = null)
        {
            instances.Clear();
            shadows.Clear();
            if (!Rdram.IsPointer(objectGroup))
                return;
            this.addresses = addresses;
            this.roomCulledObjects = roomCulledObjects;
            defaultState ??= DefaultState(ram);
            for (int i = 0; i < layerStates.Length; i++)
                layerStates[i] = defaultState;
            marioObject = ram.U32(addresses.marioObjectPointer);
            areaIndex = ram.U8(rootNode + 0x14);
            gameCamera = Rdram.IsPointer(cameraNode)
                ? new Vector3(ram.F32(cameraNode + 0x1C), ram.F32(cameraNode + 0x20), ram.F32(cameraNode + 0x24))
                : Vector3.Zero;
            var focus = Rdram.IsPointer(cameraNode)
                ? new Vector3(ram.F32(cameraNode + 0x28), ram.F32(cameraNode + 0x2C), ram.F32(cameraNode + 0x30))
                : Vector3.Zero;
            gameCameraForward = (focus - gameCamera).LengthSquared > 0 ? (focus - gameCamera).Normalized() : Vector3.Zero;
            marioInstances.Clear();
            marioInstancesStart = -1;
            foreach (uint obj in Children(ram, objectGroup))
            {
                nodeBudget = 4096;
                int first = instances.Count;
                DrawObject(ram, obj);
                if (obj == marioObject)
                {
                    marioInstances.AddRange(instances.GetRange(first, instances.Count - first));
                    marioInstancesStart = first;
                    marioModelPosition = new Vector3(ram.F32(obj + 0x20), ram.F32(obj + 0x24), ram.F32(obj + 0x28));
                }
            }

            if (envFxNode != 0)
                EmitEnvFx(ram);

            if (Rdram.IsPointer(mirrorParent))
                foreach (uint obj in Children(ram, mirrorParent))
                {
                    mirrorMario = obj;
                    nodeBudget = 4096;
                    DrawObject(ram, obj);
                }
        }

        void DrawObject(Rdram ram, uint n)
        {
            if (NodeType(ram, n) != GN_OBJECT)
                return;
            ushort flags = ram.U16(n + 2);
            if ((flags & GRAPH_RENDER_INVISIBLE) != 0)
                return;
            if ((flags & GRAPH_RENDER_ACTIVE) == 0)
            {
                if (n == mirrorMario)
                    return;
                int activeFlags = ram.U16(n + 0x74);
                bool roomCulled = (activeFlags & ACTIVE_FLAG_IN_DIFFERENT_ROOM) != 0 || (roomCulledObjects?.Contains(n) ?? false);
                if ((activeFlags & ACTIVE_FLAG_ACTIVE) == 0 || !roomCulled)
                    return;
            }

            if (ram.U8(n + 0x18) != areaIndex)
                return;
            uint sharedChild = ram.U32(n + 0x14);
            if (!Rdram.IsPointer(sharedChild))
                return;

            var pos = new Vector3(ram.F32(n + 0x20), ram.F32(n + 0x24), ram.F32(n + 0x28));
            var scale = new Vector3(ram.F32(n + 0x2C), ram.F32(n + 0x30), ram.F32(n + 0x34));
            var frame = new Frame();
            uint throwMatrix = ram.U32(n + 0x50);
            if (Rdram.IsPointer(throwMatrix))
                frame.matrix = ScaleRows(ReadMatrix(ram, throwMatrix), scale);
            else if ((flags & GRAPH_RENDER_BILLBOARD) != 0)
            {
                frame.matrix = Matrix4.Identity;
                frame.billboard = true;
                frame.anchor = pos;
                frame.anchorScale = scale;
            }
            else
            {
                var angle = (ram.S16(n + 0x1A), ram.S16(n + 0x1C), ram.S16(n + 0x1E));
                frame.matrix = ScaleRows(RotZXYTrans(pos.X, pos.Y, pos.Z, angle.Item1, angle.Item2, angle.Item3), scale);
            }

            frame.objectMatrix = frame.matrix;
            currentObject = n;
            SetAnimation(ram, n);
            DrawNodeAndSiblings(ram, sharedChild, frame, 0);
            anim.type = ANIM_NONE;
        }

        void AddShadow(Rdram ram, uint n, bool held, in Frame frame)
        {
            float size = ram.S16(n + 0x14);
            byte solidity = ram.U8(n + 0x16), type = ram.U8(n + 0x17);
            Vector3 position;
            if (held)
                position = frame.WorldPosition;
            else
            {
                position = new Vector3(ram.F32(currentObject + 0x20), ram.F32(currentObject + 0x24), ram.F32(currentObject + 0x28));
                size *= ram.F32(currentObject + 0x2C);
            }

            float floor = IsMario && addresses.marioState != 0
                ? ram.F32(addresses.marioState + 0x70)
                : ram.F32(currentObject + 0xE8);
            float height = position.Y - floor;
            if (float.IsNaN(floor) || floor < -10000 || height < -50 || height > 2000)
                return;

            float opacity = solidity / 255f;
            if (height > 0)
            {
                size *= height >= 600 ? 0.5f : 1 - 0.5f * height / 600;
                if (height > 600)
                    opacity *= Math.Max(0, 1 - (height - 600) / 600);
            }

            if (size > 0 && opacity > 0)
                shadows.Add(new Shadow
                {
                    position = new Vector3(position.X, floor, position.Z),
                    size = size,
                    opacity = opacity,
                    square = type >= 10 && type <= 12,
                });
        }

        static Matrix4 ReadMatrix(Rdram ram, uint a)
        {
            var m = new Matrix4();
            for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                m[r, c] = ram.F32(a + (uint)(16 * r + 4 * c));
            return m;
        }

        void SetAnimation(Rdram ram, uint objectNode)
        {
            anim = default;
            uint curAnim = ram.U32(objectNode + 0x3C);
            if (!Rdram.IsPointer(curAnim) && (curAnim >> 24) == 0)
                return;
            int animFlags = ram.U16(curAnim);
            short divisor = ram.S16(curAnim + 0x02);
            if ((animFlags & (1 << 3)) != 0)
                anim.type = ANIM_VERTICAL_TRANSLATION;
            else if ((animFlags & (1 << 4)) != 0)
                anim.type = ANIM_LATERAL_TRANSLATION;
            else if ((animFlags & (1 << 6)) != 0)
                anim.type = ANIM_NO_TRANSLATION;
            else
                anim.type = ANIM_TRANSLATION;
            anim.frame = ram.S16(objectNode + 0x40);
            anim.values = ram.U32(curAnim + 0x0C);
            anim.attributes = ram.U32(curAnim + 0x10);
            anim.translationMultiplier = divisor == 0 ? 1 : ram.S16(objectNode + 0x3A) / (float)divisor;
        }

        short NextAnimationValue(Rdram ram)
        {
            int count = ram.U16(anim.attributes), start = ram.U16(anim.attributes + 2);
            int index = anim.frame < count ? start + anim.frame : start + count - 1;
            anim.attributes += 4;
            return ram.S16(anim.values + (uint)(2 * index));
        }

        struct Frame
        {
            public Matrix4 matrix, objectMatrix;
            public bool billboard;
            public Vector3 anchor, anchorScale;

            public Vector3 WorldPosition => billboard ? anchor + matrix.Row3.Xyz : matrix.Row3.Xyz;
        }

        void DrawNodeAndSiblings(Rdram ram, uint first, Frame frame, int depth)
        {
            uint c = first;
            for (int i = 0; i < 256 && Rdram.IsPointer(c); i++)
            {
                DrawNode(ram, c, frame, depth);
                c = ram.U32(c + 0x08);
                if (c == first)
                    break;
            }
        }

        void Emit(uint dl, int layer, in Frame frame, Rdram ram)
        {
            if (!Rdram.IsPointer(dl) && (dl >> 24) == 0)
                return;
            int slot = layer & 7;
            var incoming = layerStates[slot];
            var key = (dl, layer, incoming.Hash);
            if (!objectMeshes.TryGetValue(key, out var entry))
            {
                var builder = new MeshBuilder();
                var f3d = NewF3D(ram, builder);
                f3d.Load(incoming);
                f3d.Run(dl, layer, Matrix4.Identity);
                objectMeshes[key] = entry = (store.Add(builder), f3d.Save());
            }

            layerStates[slot] = entry.state;
            if (entry.mesh.triangleCount > 0)
                instances.Add(new Instance { mesh = entry.mesh, matrix = frame.matrix, billboard = frame.billboard, anchor = frame.anchor, anchorScale = frame.anchorScale, cap = inCap });
        }

        bool inCap;

        void DrawNode(Rdram ram, uint n, Frame frame, int depth)
        {
            if (depth > 64 || --nodeBudget < 0)
                return;
            int t = NodeType(ram, n);
            ushort flags = ram.U16(n + 2);
            bool active = IsMario && !IsGhost && capWings.Contains(n) ? wingsShown : (flags & GRAPH_RENDER_ACTIVE) != 0;
            if (!active)
                return;
            int layer = scheme.Layer((flags >> 8) & 0xFF);
            uint dl = 0;
            switch (t)
            {
                case GN_DL:
                    dl = ram.U32(n + 0x14);
                    break;
                case GN_TRANSROT:
                    frame.matrix = RotZXYTrans(ram.S16(n + 0x18), ram.S16(n + 0x1A), ram.S16(n + 0x1C), ram.S16(n + 0x1E), ram.S16(n + 0x20), ram.S16(n + 0x22)) * frame.matrix;
                    dl = ram.U32(n + 0x14);
                    break;
                case GN_TRANS:
                    frame.matrix = RotZXYTrans(ram.S16(n + 0x18), ram.S16(n + 0x1A), ram.S16(n + 0x1C), 0, 0, 0) * frame.matrix;
                    dl = ram.U32(n + 0x14);
                    break;
                case GN_ROT:
                    frame.matrix = RotZXYTrans(0, 0, 0, ram.S16(n + 0x18), ram.S16(n + 0x1A), ram.S16(n + 0x1C)) * frame.matrix;
                    dl = ram.U32(n + 0x14);
                    break;
                case GN_SCALE:
                    frame.matrix = Matrix4.CreateScale(ram.F32(n + 0x18)) * frame.matrix;
                    dl = ram.U32(n + 0x14);
                    break;
                case GN_ANIMPART:
                    frame.matrix = AnimatedPart(ram, n) * frame.matrix;
                    dl = ram.U32(n + 0x14);
                    break;
                case GN_BILLBOARD:
                    var position = Vector3.TransformPosition(new Vector3(ram.S16(n + 0x18), ram.S16(n + 0x1A), ram.S16(n + 0x1C)), frame.matrix);
                    frame.anchor = frame.billboard ? frame.anchor + position : position;
                    frame.anchorScale = Vector3.One;
                    frame.billboard = true;
                    frame.matrix = Matrix4.Identity;
                    dl = ram.U32(n + 0x14);
                    break;
                case GN_SHADOW:
                    AddShadow(ram, n, inHeldObject, frame);
                    break;
                case GN_SWITCH:
                    int selected = ram.S16(n + 0x1E), i = 0;
                    uint switchFunction = ram.U32(n + 0x14);
                    bool capSwitch = false;
                    if (addresses.geoSwitchAnimState != 0 && switchFunction == addresses.geoSwitchAnimState)
                    {
                        selected = ram.S32(currentObject + 0xF0);
                        if (selected >= ram.S16(n + 0x1C) || selected < 0)
                            selected = 0;
                    }
                    else if (addresses.geoSwitchMarioCapEffect != 0 && switchFunction == addresses.geoSwitchMarioCapEffect)
                        selected = MarioCapEffect(ram);
                    else if (IsCapSwitch(ram, n, switchFunction))
                    {
                        selected = CapSwitchCase(ram, n);
                        capSwitch = true;
                    }

                    uint selectedChild = ram.U32(n + 0x10);
                    for (i = 0; Rdram.IsPointer(selectedChild) && selected > i && i < 256; i++)
                        selectedChild = ram.U32(selectedChild + 0x08);
                    bool wasInCap = inCap;
                    inCap |= capSwitch;
                    if (Rdram.IsPointer(selectedChild))
                        DrawNode(ram, selectedChild, frame, depth + 1);
                    inCap = wasInCap;
                    return;
                case GN_LOD:
                    float distance = LodDistance(frame.WorldPosition);
                    if (!(ram.S16(n + 0x14) <= distance && distance < ram.S16(n + 0x16)))
                        return;
                    break;
                case GN_HELD:
                    if (IsMario && !inHeldObject)
                        DrawHeldObject(ram, n, frame, depth);
                    return;
                case GN_GENLIST:
                    uint generator = ram.U32(n + 0x14);
                    if (addresses.coloredHatsGenerator != 0 && generator == addresses.coloredHatsGenerator)
                        EmitColoredHat(ram, layer, frame);
                    else if (addresses.geoMirrorMarioSetAlpha != 0 && generator == addresses.geoMirrorMarioSetAlpha)
                        SetMarioAlpha(ram);
                    else if (addresses.geoUpdateLayerTransparency != 0 && generator == addresses.geoUpdateLayerTransparency)
                    {
                        int opacity = ram.S32(currentObject + 0x17C) & 0xFF;
                        int target = ram.U32(n + 0x18) == 20 ? 6 : opacity == 0xFF ? LAYER_OPAQUE : LAYER_TRANSPARENT;
                        SetEnvAlpha(ram, target, opacity);
                    }
                    else if (IsMovtex(generator))
                        EmitObjectMovtex(ram, n, generator, layer, frame);
                    else if (addresses.geoExecFlyingCarpetCreate != 0 && generator == addresses.geoExecFlyingCarpetCreate)
                        EmitFlyingCarpet(ram, frame);
                    else if (addresses.geoBitsBowserColoring != 0 && generator == addresses.geoBitsBowserColoring)
                    {
                        int slot = (ram.S32(currentObject + 0x17C) & 0xFF) == 0xFF ? LAYER_OPAQUE : LAYER_TRANSPARENT;
                        if (ram.S16(currentObject + 0x1B2) != 0)
                            ClearLighting(ram, slot);
                    }

                    break;
                case GN_OBJECT:
                case GN_OBJPARENT:
                case GN_BG:
                    return;
            }

            if (dl != 0)
                Emit(dl, layer, frame, ram);
            uint children = ram.U32(n + 0x10);
            if (Rdram.IsPointer(children))
                DrawNodeAndSiblings(ram, children, frame, depth + 1);
        }

        const int LAYER_OPAQUE = 1, LAYER_TRANSPARENT = 5;

        bool IsMario => currentObject == marioObject || (currentObject == mirrorMario && mirrorMario != 0);

        bool IsGhost => addresses.ghostHack && !IsMario;

        uint BodyState(Rdram ram) =>
            addresses.bodyStates != 0 ? addresses.bodyStates
            : addresses.marioState != 0 ? ram.U32(addresses.marioState + 0x98)
            : 0;

        int MarioCapEffect(Rdram ram) =>
            IsGhost ? (sbyte)ram.U8(currentObject + 0x61) : ram.U16(BodyState(ram) + 8) >> 8;

        const int MARIO_HAS_DEFAULT_CAP_OFF = 1, MARIO_HAS_WING_CAP_ON = 2;

        readonly HashSet<uint> capWings = new HashSet<uint>();
        bool wingsShown;

        bool IsCapSwitch(Rdram ram, uint n, uint function)
        {
            if (!IsMario || IsGhost)
                return false;
            if (addresses.geoSwitchMarioCapOnOff != 0)
                return function == addresses.geoSwitchMarioCapOnOff;
            for (uint s = ram.U32(n + 0x08), i = 0; s != n && Rdram.IsPointer(s) && i < 64; s = ram.U32(s + 0x08), i++)
                if (NodeType(ram, s) == GN_TRANSROT)
                    return true;
            return false;
        }

        int CapSwitchCase(Rdram ram, uint n)
        {
            uint bodyState = BodyState(ram);
            int capState = Rdram.IsPointer(bodyState) ? (sbyte)ram.U8(bodyState + 4) : 0;
            wingsShown = (capState & MARIO_HAS_WING_CAP_ON) != 0;
            for (uint s = ram.U32(n + 0x08), i = 0; s != n && Rdram.IsPointer(s) && i < 64; s = ram.U32(s + 0x08), i++)
                if (NodeType(ram, s) == GN_TRANSROT)
                    capWings.Add(s);
            return capState & MARIO_HAS_DEFAULT_CAP_OFF;
        }

        readonly Dictionary<(int state, int alpha), F3D.State> alphaStates = new Dictionary<(int, int), F3D.State>();

        void SetMarioAlpha(Rdram ram)
        {
            int alpha;
            if (IsGhost)
                alpha = (sbyte)ram.U8(currentObject + 0x61) != 0 ? 0x7F : 0xFF;
            else
            {
                int modelState = ram.U16(BodyState(ram) + 8);
                alpha = (modelState & 0x100) != 0 ? modelState & 0xFF : 0xFF;
            }

            SetEnvAlpha(ram, alpha == 0xFF ? LAYER_OPAQUE : LAYER_TRANSPARENT, alpha);
        }

        void SetEnvAlpha(Rdram ram, int slot, int alpha)
        {
            slot &= 7;
            var key = (layerStates[slot].Hash, alpha);
            if (!alphaStates.TryGetValue(key, out var state))
            {
                var f3d = NewF3D(ram, new MeshBuilder());
                f3d.Load(layerStates[slot]);
                f3d.SetEnvColor(255, 255, 255, (byte)alpha);
                alphaStates[key] = state = f3d.Save();
            }

            layerStates[slot] = state;
        }


        const int ENVFX_SNOW_NORMAL = 1, ENVFX_SNOW_WATER = 2, ENVFX_SNOW_BLIZZARD = 3, ENVFX_BUBBLE_START = 10;
        const int ENVFX_FLOWERS = 11, ENVFX_LAVA_BUBBLES = 12, ENVFX_WHIRLPOOL_BUBBLES = 13, ENVFX_JETSTREAM_BUBBLES = 14;
        const uint ENVFX_PARTICLE_SIZE = 0x38;
        const uint DL_SNOW_GRAY_BEGIN = 0x0B006A50, DL_SNOW_BLUE_BEGIN = 0x0B006CD8, DL_PARTICLES_END = 0x0B006AB0;
        const uint DL_BUBBLES_BEGIN = 0x0B006D38, DL_BUBBLE_TEXTURE = 0x0B006D68;
        const uint FLOWER_TEXTURES = 0x0B002008, LAVA_BUBBLE_TEXTURES = 0x0B006020, BUBBLE_TEXTURES = 0x0B006848;

        void EmitEnvFx(Rdram ram)
        {
            if (addresses.envFxMode == 0 || addresses.envFxBuffer == 0)
                return;
            int mode = (sbyte)ram.U8(addresses.envFxMode);
            uint buffer = ram.U32(addresses.envFxBuffer);
            if (mode == 0 || !Rdram.IsPointer(buffer) || (ram.U16(envFxNode + 2) & GRAPH_RENDER_ACTIVE) == 0)
                return;

            bool bubbles = mode >= ENVFX_BUBBLE_START;
            uint countAddress = bubbles ? addresses.bubbleParticleMaxCount : addresses.snowParticleCount;
            if (countAddress == 0)
                return;
            int count = bubbles ? ram.S32(countAddress) : ram.S16(countAddress);
            count = Math.Clamp((count + 4) / 5 * 5, 0, 200);
            for (int i = 0; i < count; i++)
            {
                uint particle = buffer + (uint)i * ENVFX_PARTICLE_SIZE;
                var mesh = EnvFxMesh(ram, mode, bubbles ? ram.S16(buffer + (uint)(i - i % 5) * ENVFX_PARTICLE_SIZE + 2) : 0);
                if (mesh == null)
                    return;
                var position = new Vector3(ram.S32(particle + 4), ram.S32(particle + 8), ram.S32(particle + 12));
                instances.Add(new Instance { mesh = mesh, matrix = Matrix4.Identity, billboard = true, anchor = position, anchorScale = Vector3.One });
            }
        }

        Mesh EnvFxMesh(Rdram ram, int mode, int frame)
        {
            const int layer = 4;
            var key = ((uint)frame, 0x300 | layer, mode);
            if (objectMeshes.TryGetValue(key, out var entry))
                return entry.mesh;

            uint begin, texture = 0;
            short spread, height;
            (short s, short t)[] st;
            byte shade;
            switch (mode)
            {
                case ENVFX_SNOW_NORMAL:
                case ENVFX_SNOW_BLIZZARD:
                case ENVFX_SNOW_WATER:
                    begin = mode == ENVFX_SNOW_WATER ? DL_SNOW_BLUE_BEGIN : DL_SNOW_GRAY_BEGIN;
                    break;
                case ENVFX_FLOWERS:
                case ENVFX_LAVA_BUBBLES:
                case ENVFX_WHIRLPOOL_BUBBLES:
                case ENVFX_JETSTREAM_BUBBLES:
                    begin = DL_BUBBLES_BEGIN;
                    uint textures = mode == ENVFX_FLOWERS ? FLOWER_TEXTURES : mode == ENVFX_LAVA_BUBBLES ? LAVA_BUBBLE_TEXTURES : BUBBLE_TEXTURES;
                    texture = ram.U32(textures + 4 * (uint)(mode == ENVFX_FLOWERS || mode == ENVFX_LAVA_BUBBLES ? Math.Clamp(frame, 0, 63) : 0));
                    break;
                default:
                    return null;
            }

            var vertices = new byte[3 * 16];
            if (mode < ENVFX_BUBBLE_START)
            {
                WriteVertex(vertices, 0, -5, 5, 0, 0, 0, 0x7F, 0x7F, 0x7F, 0xFF);
                WriteVertex(vertices, 1, -5, -5, 0, 0, 960, 0x7F, 0x7F, 0x7F, 0xFF);
                WriteVertex(vertices, 2, 5, 5, 0, 960, 0, 0x7F, 0x7F, 0x7F, 0xFF);
            }
            else
            {
                (spread, height) = mode == ENVFX_FLOWERS ? ((short)50, (short)75) : mode == ENVFX_LAVA_BUBBLES ? ((short)100, (short)150) : ((short)40, (short)60);
                st = new[] { ((short)1544, (short)964), ((short)522, (short)-568), ((short)-498, (short)964) };
                shade = 0xFF;
                WriteVertex(vertices, 0, spread, 0, 0, st[0].s, st[0].t, shade, shade, shade, 0xFF);
                WriteVertex(vertices, 1, 0, height, 0, st[1].s, st[1].t, shade, shade, shade, 0xFF);
                WriteVertex(vertices, 2, (short)-spread, 0, 0, st[2].s, st[2].t, shade, shade, shade, 0xFF);
            }

            var builder = new MeshBuilder();
            var f3d = NewF3D(ram, builder);
            f3d.Load(defaultState);
            f3d.Run(begin, layer, Matrix4.Identity);
            if (texture != 0)
            {
                f3d.Command(0xFD000000 | 2 << 19, texture, layer, Matrix4.Identity);
                f3d.Run(DL_BUBBLE_TEXTURE, layer, Matrix4.Identity);
            }

            f3d.LoadVertices(vertices, 3, 0, Matrix4.Identity);
            f3d.Command(0xBF000000, 10 << 8 | 20, layer, Matrix4.Identity);
            f3d.Run(DL_PARTICLES_END, layer, Matrix4.Identity);
            var mesh = store.Add(builder);
            objectMeshes[key] = (mesh, null);
            return mesh;
        }

        const int FLYING_CARPET_VERTICES = 21;

        void EmitFlyingCarpet(Rdram ram, in Frame frame)
        {
            if (addresses.flyingCarpetRippleTimer == 0)
                addresses.flyingCarpetRippleTimer = FindFirstHalfwordLoad(ram, addresses.geoExecFlyingCarpetCreate);
            if (addresses.flyingCarpetRippleTimer == 0)
                return;
            ushort timer = ram.U16(addresses.flyingCarpetRippleTimer);
            var key = ((uint)timer, 0x400 | LAYER_OPAQUE, (int)addresses.geoExecFlyingCarpetCreate);
            if (!objectMeshes.TryGetValue(key, out var entry))
            {
                var constants = FindSegmentedConstants(ram, addresses.geoExecFlyingCarpetCreate, 0x07);
                if (constants.Count < 4)
                    return;
                uint data = constants[0], begin = constants[1], half = constants[2], end = constants[constants.Count - 1];

                var vertices = new byte[FLYING_CARPET_VERTICES * 16];
                for (int n = 0; n < FLYING_CARPET_VERTICES; n++)
                {
                    int row = n / 3, col = n % 3;
                    double sin = Math.Sin(((ushort)(timer + (row << 12) + (col << 14)) >> 4) * (2 * Math.PI / 4096));
                    double y = sin * 20.0;
                    uint v = data + 8 * (uint)n;
                    WriteVertex(vertices, n, ram.S16(v), (short)(y >= 0 ? y + 0.5 : y - 0.5), ram.S16(v + 2), ram.S16(v + 4), ram.S16(v + 6), 0, 127, 0, 255);
                }

                var builder = new MeshBuilder();
                var f3d = NewF3D(ram, builder);
                f3d.Load(defaultState);
                f3d.Run(begin, LAYER_OPAQUE, Matrix4.Identity);
                f3d.LoadVertices(vertices, 12, 0, Matrix4.Identity);
                f3d.Run(half, LAYER_OPAQUE, Matrix4.Identity);
                f3d.LoadVertices(vertices.AsSpan(9 * 16).ToArray(), 12, 0, Matrix4.Identity);
                f3d.Run(half, LAYER_OPAQUE, Matrix4.Identity);
                f3d.Run(end, LAYER_OPAQUE, Matrix4.Identity);
                objectMeshes[key] = entry = (store.Add(builder), null);
            }

            if (entry.mesh.triangleCount > 0)
                instances.Add(new Instance { mesh = entry.mesh, matrix = frame.matrix, billboard = frame.billboard, anchor = frame.anchor, anchorScale = frame.anchorScale });
        }

        void EmitObjectMovtex(Rdram ram, uint n, uint function, int layer, in Frame frame)
        {
            uint parameter = ram.U32(n + 0x18);
            var key = (parameter, layer | 0x200, (int)function);
            if (!objectMeshes.TryGetValue(key, out var entry))
            {
                var builder = new MeshBuilder();
                var f3d = NewF3D(ram, builder);
                f3d.Load(defaultState);
                EmitMovtex(ram, f3d, n, Matrix4.Identity);
                objectMeshes[key] = entry = (store.Add(builder), null);
            }

            if (entry.mesh.triangleCount > 0)
                instances.Add(new Instance { mesh = entry.mesh, matrix = frame.matrix, billboard = frame.billboard, anchor = frame.anchor, anchorScale = frame.anchorScale });
        }

        void ClearLighting(Rdram ram, int slot)
        {
            slot &= 7;
            var key = (layerStates[slot].Hash, -1);
            if (!alphaStates.TryGetValue(key, out var state))
            {
                var f3d = NewF3D(ram, new MeshBuilder());
                f3d.Load(layerStates[slot]);
                f3d.Command(0xB6000000, 0x00020000, slot, Matrix4.Identity);
                alphaStates[key] = state = f3d.Save();
            }

            layerStates[slot] = state;
        }

        const uint COLORED_HAT_DL_1 = 0x040119A0, COLORED_HAT_DL_2 = 0x04011978;

        void EmitColoredHat(Rdram ram, int layer, in Frame frame)
        {
            uint index = IsMario ? 0u : ram.U8(currentObject + 0x60);
            uint lights = addresses.coloredHatsLights + 0x20 * index;
            int slot = layer & 7;
            var incoming = layerStates[slot];
            var key = (lights, layer | 0x100, HashCode.Combine(incoming.Hash, ram.U32(lights), ram.U32(lights + 8), ram.U32(lights + 12)));
            if (!objectMeshes.TryGetValue(key, out var entry))
            {
                var builder = new MeshBuilder();
                var f3d = NewF3D(ram, builder);
                f3d.Load(incoming);
                f3d.MoveMemLight(0x88, lights);
                f3d.MoveMemLight(0x86, lights + 8);
                f3d.Run(COLORED_HAT_DL_1, layer, Matrix4.Identity);
                f3d.MoveMemLight(0x88, lights);
                f3d.MoveMemLight(0x86, lights + 8);
                f3d.Run(COLORED_HAT_DL_2, layer, Matrix4.Identity);
                objectMeshes[key] = entry = (store.Add(builder), f3d.Save());
            }

            layerStates[slot] = entry.state;
            if (entry.mesh.triangleCount > 0)
                instances.Add(new Instance { mesh = entry.mesh, matrix = frame.matrix, billboard = frame.billboard, anchor = frame.anchor, anchorScale = frame.anchorScale });
        }

        Matrix4 AnimatedPart(Rdram ram, uint n)
        {
            double tx = ram.S16(n + 0x18), ty = ram.S16(n + 0x1A), tz = ram.S16(n + 0x1C);
            short rx = 0, ry = 0, rz = 0;
            switch (anim.type)
            {
                case ANIM_TRANSLATION:
                    tx += NextAnimationValue(ram) * anim.translationMultiplier;
                    ty += NextAnimationValue(ram) * anim.translationMultiplier;
                    tz += NextAnimationValue(ram) * anim.translationMultiplier;
                    anim.type = ANIM_ROTATION;
                    break;
                case ANIM_LATERAL_TRANSLATION:
                    tx += NextAnimationValue(ram) * anim.translationMultiplier;
                    anim.attributes += 4;
                    tz += NextAnimationValue(ram) * anim.translationMultiplier;
                    anim.type = ANIM_ROTATION;
                    break;
                case ANIM_VERTICAL_TRANSLATION:
                    anim.attributes += 4;
                    ty += NextAnimationValue(ram) * anim.translationMultiplier;
                    anim.attributes += 4;
                    anim.type = ANIM_ROTATION;
                    break;
                case ANIM_NO_TRANSLATION:
                    anim.attributes += 12;
                    anim.type = ANIM_ROTATION;
                    break;
            }

            if (anim.type == ANIM_ROTATION)
            {
                rx = NextAnimationValue(ram);
                ry = NextAnimationValue(ram);
                rz = NextAnimationValue(ram);
            }

            return RotXYZTrans(tx, ty, tz, rx, ry, rz);
        }

        void DrawHeldObject(Rdram ram, uint n, Frame frame, int depth)
        {
            uint obj = ram.U32(n + 0x1C);
            if (!Rdram.IsPointer(obj))
                return;
            uint sharedChild = ram.U32(obj + 0x14);
            if (!Rdram.IsPointer(sharedChild))
                return;

            var hand = frame.WorldPosition;
            var holder = frame.objectMatrix;
            holder.Row3 = new Vector4(hand, 1);
            var translation = Matrix4.CreateTranslation(ram.S16(n + 0x20) / 4f, ram.S16(n + 0x22) / 4f, ram.S16(n + 0x24) / 4f);
            var scale = new Vector3(ram.F32(obj + 0x2C), ram.F32(obj + 0x30), ram.F32(obj + 0x34));
            var held = new Frame { matrix = ScaleRows(translation * holder, scale) };
            held.objectMatrix = held.matrix;

            var holderAnimation = anim;
            uint holderObject = currentObject;
            bool wasInHeldObject = inHeldObject;
            currentObject = obj;
            inHeldObject = true;
            SetAnimation(ram, obj);
            DrawNodeAndSiblings(ram, sharedChild, held, depth + 1);
            anim = holderAnimation;
            currentObject = holderObject;
            inHeldObject = wasInHeldObject;
        }

        #endregion
    }
}
