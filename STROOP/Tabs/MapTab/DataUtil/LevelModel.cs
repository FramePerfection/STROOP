using System;
using System.Collections.Generic;
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

            public uint geoSwitchMarioCapEffect, geoMirrorMarioSetAlpha, bodyStates;

            public uint geoUpdateLayerTransparency;

            public static GameAddresses US(uint marioObjectPointer, uint marioState) => new GameAddresses
            {
                marioObjectPointer = marioObjectPointer,
                marioState = marioState,
                geoSwitchAnimState = 0x8029DB48,
                skyboxTextures = 0x80330F00,
                geoPaintingDraw = 0x802D5B98,
                paintingGroups = 0x8033134C,
                geoSwitchMarioCapEffect = 0x802776D8,
                geoMirrorMarioSetAlpha = 0x802770A4,
                bodyStates = 0x8033B3B0,
                geoUpdateLayerTransparency = 0x8029D924,
            };
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
        }

        public readonly MeshStore store = new MeshStore();
        public readonly TextureSet textures = new TextureSet();
        public Mesh level = new Mesh();
        public readonly List<Instance> instances = new List<Instance>();
        public readonly List<Shadow> shadows = new List<Shadow>();

        public Texture skybox;
        public Vector3? backgroundColor;

        public int displayListCount;
        public uint area;

        public bool IsEmpty => level.triangleCount == 0;

        public static readonly LevelModel None = new LevelModel();

        const int GN_ROOT = 0x001, GN_ORTHO = 0x002, GN_LOD = 0x00B, GN_SWITCH = 0x10C, GN_CAMERA = 0x114;
        const int GN_TRANSROT = 0x015, GN_TRANS = 0x016, GN_ROT = 0x017, GN_OBJECT = 0x018, GN_ANIMPART = 0x019, GN_BILLBOARD = 0x01A, GN_DL = 0x01B, GN_SCALE = 0x01C;
        const int GN_SHADOW = 0x028, GN_OBJPARENT = 0x029, GN_GENLIST = 0x12A, GN_BG = 0x12C, GN_HELD = 0x12E;

        const int GRAPH_RENDER_ACTIVE = 0x01, GRAPH_RENDER_BILLBOARD = 0x04, GRAPH_RENDER_INVISIBLE = 0x10;

        uint rootNode, cameraNode, objectGroup;

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

            var dls = new List<(uint dl, int layer, Matrix4 m)>();
            model.CollectDisplayLists(ram, root, Matrix4.Identity, 0, dls);
            model.displayListCount = dls.Count;

            var builder = new MeshBuilder();
            var f3d = new F3D(ram, model.textures, builder);
            foreach (var (dl, layer, m) in dls)
            {
                f3d.geo = F3D.GEO_INIT;
                f3d.Run(dl, layer, m);
            }

            model.level = model.store.Add(builder);
            model.ReadBackground(ram, root, addresses);
            return model;
        }

        const float PAINTING_SIZE = 614;

        void AddPainting(Rdram ram, uint parameter, int layer, Matrix4 m, List<(uint, int, Matrix4)> output)
        {
            uint group = ram.U32(addresses.paintingGroups + 4 * ((parameter >> 8) & 0xFF));
            if (group == 0)
                return;
            uint painting = ram.U32(group + 4 * (parameter & 0xFF));
            if (painting == 0 || !ram.Ok(ram.Phys(painting), 0x78))
                return;
            uint dl = ram.U32(painting + 0x58);
            float size = ram.F32(painting + 0x74);
            if (dl == 0 || !(size > 0))
                return;
            var transform = Matrix4.CreateScale(size / PAINTING_SIZE)
                            * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(ram.F32(painting + 0x08)))
                            * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(ram.F32(painting + 0x0C)))
                            * Matrix4.CreateTranslation(ram.F32(painting + 0x10), ram.F32(painting + 0x14), ram.F32(painting + 0x18));
            output.Add((dl, layer, transform * m));
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
                if (ram.U16(n) == GN_BG)
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
            int t = ram.U16(n);
            ushort flags = ram.U16(n + 2);
            if ((flags & GRAPH_RENDER_ACTIVE) == 0)
                return;
            if (t == GN_CAMERA)
                cameraNode = n;
            if (t == GN_OBJPARENT)
                objectGroup = ram.U32(n + 0x14);
            if (t == GN_GENLIST && addresses.geoPaintingDraw != 0 && ram.U32(n + 0x14) == addresses.geoPaintingDraw)
                AddPainting(ram, ram.U32(n + 0x18), (flags >> 8) & 0xFF, m, output);
            if (t == GN_OBJPARENT || t == GN_GENLIST || t == GN_BG || t == GN_HELD || t == GN_OBJECT || t == GN_SHADOW || t == GN_ORTHO)
                return;
            int layer = (flags >> 8) & 0xFF;
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
        Vector3 gameCamera;
        byte areaIndex;
        int nodeBudget;
        GameAddresses addresses;
        uint currentObject, marioObject;
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
            defaultState ??= new F3D(ram, textures, new MeshBuilder()) { combine1 = F3D.COMBINE_SHADE_W1 }.Save();
            for (int i = 0; i < layerStates.Length; i++)
                layerStates[i] = defaultState;
            marioObject = ram.U32(addresses.marioObjectPointer);
            areaIndex = ram.U8(rootNode + 0x14);
            gameCamera = Rdram.IsPointer(cameraNode)
                ? new Vector3(ram.F32(cameraNode + 0x1C), ram.F32(cameraNode + 0x20), ram.F32(cameraNode + 0x24))
                : Vector3.Zero;
            foreach (uint obj in Children(ram, objectGroup))
            {
                nodeBudget = 4096;
                DrawObject(ram, obj);
            }
        }

        void DrawObject(Rdram ram, uint n)
        {
            if (ram.U16(n) != GN_OBJECT)
                return;
            ushort flags = ram.U16(n + 2);
            if ((flags & GRAPH_RENDER_INVISIBLE) != 0)
                return;
            if ((flags & GRAPH_RENDER_ACTIVE) == 0)
            {
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

            float floor = currentObject == marioObject && addresses.marioState != 0
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
                var f3d = new F3D(ram, textures, builder);
                f3d.Load(incoming);
                f3d.Run(dl, layer, Matrix4.Identity);
                objectMeshes[key] = entry = (store.Add(builder), f3d.Save());
            }

            layerStates[slot] = entry.state;
            if (entry.mesh.triangleCount > 0)
                instances.Add(new Instance { mesh = entry.mesh, matrix = frame.matrix, billboard = frame.billboard, anchor = frame.anchor, anchorScale = frame.anchorScale });
        }

        void DrawNode(Rdram ram, uint n, Frame frame, int depth)
        {
            if (depth > 64 || --nodeBudget < 0)
                return;
            int t = ram.U16(n);
            ushort flags = ram.U16(n + 2);
            if ((flags & GRAPH_RENDER_ACTIVE) == 0)
                return;
            int layer = (flags >> 8) & 0xFF;
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
                    if (addresses.geoSwitchAnimState != 0 && switchFunction == addresses.geoSwitchAnimState)
                    {
                        selected = ram.S32(currentObject + 0xF0);
                        if (selected >= ram.S16(n + 0x1C) || selected < 0)
                            selected = 0;
                    }
                    else if (addresses.geoSwitchMarioCapEffect != 0 && switchFunction == addresses.geoSwitchMarioCapEffect)
                        selected = MarioCapEffect(ram);

                    uint selectedChild = ram.U32(n + 0x10);
                    for (i = 0; Rdram.IsPointer(selectedChild) && selected > i && i < 256; i++)
                        selectedChild = ram.U32(selectedChild + 0x08);
                    if (Rdram.IsPointer(selectedChild))
                        DrawNode(ram, selectedChild, frame, depth + 1);
                    return;
                case GN_LOD:
                    float distance = (frame.WorldPosition - gameCamera).Length;
                    if (!(ram.S16(n + 0x14) <= distance && distance < ram.S16(n + 0x16)))
                        return;
                    break;
                case GN_HELD:
                    if (currentObject == marioObject && !inHeldObject)
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

        bool IsGhost => addresses.ghostHack && currentObject != marioObject;

        int MarioCapEffect(Rdram ram) =>
            IsGhost ? (sbyte)ram.U8(currentObject + 0x61) : ram.U16(addresses.bodyStates + 8) >> 8;

        readonly Dictionary<(int state, int alpha), F3D.State> alphaStates = new Dictionary<(int, int), F3D.State>();

        void SetMarioAlpha(Rdram ram)
        {
            int alpha;
            if (IsGhost)
                alpha = (sbyte)ram.U8(currentObject + 0x61) != 0 ? 0x7F : 0xFF;
            else
            {
                int modelState = ram.U16(addresses.bodyStates + 8);
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
                var f3d = new F3D(ram, textures, new MeshBuilder());
                f3d.Load(layerStates[slot]);
                f3d.SetEnvColor(255, 255, 255, (byte)alpha);
                alphaStates[key] = state = f3d.Save();
            }

            layerStates[slot] = state;
        }

        const uint COLORED_HAT_DL_1 = 0x040119A0, COLORED_HAT_DL_2 = 0x04011978;

        void EmitColoredHat(Rdram ram, int layer, in Frame frame)
        {
            uint index = currentObject == marioObject ? 0u : ram.U8(currentObject + 0x60);
            uint lights = addresses.coloredHatsLights + 0x20 * index;
            int slot = layer & 7;
            var incoming = layerStates[slot];
            var key = (lights, layer | 0x100, HashCode.Combine(incoming.Hash, ram.U32(lights), ram.U32(lights + 8), ram.U32(lights + 12)));
            if (!objectMeshes.TryGetValue(key, out var entry))
            {
                var builder = new MeshBuilder();
                var f3d = new F3D(ram, textures, builder);
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
