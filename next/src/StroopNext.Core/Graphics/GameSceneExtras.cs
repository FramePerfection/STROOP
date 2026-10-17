using System.Numerics;
using StroopNext.Game;
using StroopNext.Session;

namespace StroopNext.Graphics;

public sealed partial class GameSceneBuilder
{
    private const int LayerTransparentInter = 7;
    private const uint MovtexJrbInitialMist = 0x1205, MovtexHmcToxicMist = 0x0702, MovtexSslQuicksandMist = 0x0851;
    private const int MovtexTextureMist = 1, FormatRgba = 0, FormatIa = 3;
    private const uint MovtexObjectSize = 0x24, MovtexQuadSize = 0x1C, MovtexRegionSize = 12;
    private const float PaintingSize = 614;

    private Extras _extras;
    private readonly List<(uint Node, Matrix4x4 M)> _movtexNodes = new();
    private readonly List<(uint Parameter, Matrix4x4 M)> _paintingNodes = new();
    private uint _envFxNode;
    private readonly byte[] _vertexScratch = new byte[64 * 16];
    private readonly Dictionary<(uint Function, uint Parameter, int Layer), CachedMesh> _objectMovtex = new();

    internal readonly record struct Extras(
        uint EnvironmentRegions, uint MovtexIdToTexture, uint QuadCollectionFromId,
        uint GeoMovtexWaterRegions, uint GeoMovtexNoColor, uint GeoMovtexColored, uint GeoMovtexColoredNoUpdate, uint GeoMovtexColored2NoUpdate,
        uint MovtexNonColored, uint MovtexColored, uint MovtexColored2,
        uint GeoPaintingDraw, uint PaintingGroups, uint DisplayPaintingRippling, uint PaintingRippleImage, uint PaintingRippleEnvMapped,
        uint GeoEnvFxMain, uint EnvFxMode, uint EnvFxBuffer, uint SnowParticleCount, uint BubbleParticleMaxCount,
        uint GeoFlyingCarpetCreate, uint FlyingCarpetRippleTimer,
        uint GeoInsideCastleLight, uint HudDisplay, uint SaveFileGetFlags)
    {
        public static Extras Resolve(GameFrame frame)
        {
            var s = Sm64Symbols.For(frame.Version);
            uint S(string name) => s?.Get(name) ?? 0;
            uint bubbleConfig = S("gEnvFxBubbleConfig");
            uint bubbleMax = S("sBubbleParticleMaxCount");
            return new Extras(
                S("gEnvironmentRegions"), S("gMovtexIdToTexture"), S("get_quad_collection_from_id"),
                S("geo_movtex_draw_water_regions"), S("geo_movtex_draw_nocolor"), S("geo_movtex_draw_colored"),
                S("geo_movtex_draw_colored_no_update"), S("geo_movtex_draw_colored_2_no_update"),
                S("gMovtexNonColored"), S("gMovtexColored"), S("gMovtexColored2"),
                S("geo_painting_draw"), S("sPaintingGroups"), S("display_painting_rippling"), S("painting_ripple_image"), S("painting_ripple_env_mapped"),
                S("geo_envfx_main"), S("gEnvFxMode"), S("gEnvFxBuffer"), S("gSnowParticleCount"),
                bubbleMax != 0 ? bubbleMax : bubbleConfig != 0 ? bubbleConfig + 0x1C : 0,
                S("geo_exec_flying_carpet_create"), S("sFlyingCarpetRippleTimer"),
                S("geo_exec_inside_castle_light"), S("gHudDisplay"), S("save_file_get_flags"));
        }
    }

    private bool IsMovtex(uint function) => function != 0 && (function == _extras.GeoMovtexWaterRegions || function == _extras.GeoMovtexNoColor
        || function == _extras.GeoMovtexColored || function == _extras.GeoMovtexColoredNoUpdate || function == _extras.GeoMovtexColored2NoUpdate);

    private uint StaticGeneratedNode(uint n, Matrix4x4 m)
    {
        uint function = _memory.U32(n + 0x14);
        if (function == 0)
            return 0;
        if (function == _extras.GeoPaintingDraw)
            _paintingNodes.Add((_memory.U32(n + 0x18), m));
        else if (IsMovtex(function))
            _movtexNodes.Add((n, m));
        else if (function == _extras.GeoEnvFxMain)
            _envFxNode = n;
        else if (function == _extras.GeoInsideCastleLight && CastleLightVisible())
            return MipsLeaf.FindSegmentedConstants(_memory, function, 0x07) is [var light, ..] ? light : 0;
        return 0;
    }

    private bool CastleLightVisible()
    {
        if (_extras.HudDisplay == 0 || _extras.SaveFileGetFlags == 0 || _extras.GeoInsideCastleLight == 0)
            return false;
        uint? flags = MipsLeaf.Call(_memory, _extras.SaveFileGetFlags);
        return _memory.S16(_extras.HudDisplay + 4) >= 10 && flags is uint f && (f & 2) == 0;
    }

    private int ExtrasKey()
    {
        var hash = new HashCode();
        uint regions = _extras.EnvironmentRegions != 0 ? _memory.U32(_extras.EnvironmentRegions) : 0;
        if (_memory.IsPointer(regions))
        {
            int count = Math.Min((int)_memory.S16(regions), 64);
            for (int i = 0; i < count; i++)
                hash.Add(_memory.S16(regions + 2 + (uint)i * MovtexRegionSize + 10));
        }
        hash.Add(CastleLightVisible());
        return hash.ToHashCode();
    }

    private static void WriteVertex(Span<byte> vertices, int index, short x, short y, short z, short s, short t, byte r, byte g, byte b, byte a)
    {
        var o = vertices.Slice(index * 16, 16);
        static void S16(Span<byte> span, int at, short value)
        {
            span[at] = (byte)(value >> 8);
            span[at + 1] = (byte)value;
        }
        S16(o, 0, x);
        S16(o, 2, y);
        S16(o, 4, z);
        S16(o, 6, 0);
        S16(o, 8, s);
        S16(o, 10, t);
        o[12] = r;
        o[13] = g;
        o[14] = b;
        o[15] = a;
    }


    private void EmitMovtex(F3DInterpreter f3d, uint node, Matrix4x4 m)
    {
        uint function = _memory.U32(node + 0x14), parameter = _memory.U32(node + 0x18);
        if (_extras.MovtexIdToTexture == 0)
            return;
        if (function == _extras.GeoMovtexWaterRegions)
            EmitWaterRegions(f3d, parameter, m);
        else if (function == _extras.GeoMovtexNoColor)
            EmitMovtexObject(f3d, _extras.MovtexNonColored, parameter, false, m);
        else if (function == _extras.GeoMovtexColored || function == _extras.GeoMovtexColoredNoUpdate)
            EmitMovtexObject(f3d, _extras.MovtexColored, parameter, true, m);
        else if (function == _extras.GeoMovtexColored2NoUpdate)
            EmitMovtexObject(f3d, _extras.MovtexColored2, parameter, true, m);
    }

    private void EmitWaterRegions(F3DInterpreter f3d, uint parameter, Matrix4x4 m)
    {
        uint regions = _memory.U32(_extras.EnvironmentRegions);
        if (!_memory.IsPointer(regions) || parameter == MovtexJrbInitialMist)
            return;
        uint? collection = MipsLeaf.Call(_memory, _extras.QuadCollectionFromId, parameter);
        if (collection is not uint quadsTable || quadsTable == 0)
            return;

        bool mist = parameter is MovtexHmcToxicMist or MovtexSslQuicksandMist;
        byte r = 255, g = 255, b = 255;
        if (parameter == MovtexHmcToxicMist)
            b = 0;
        else if (parameter == MovtexSslQuicksandMist)
            g = b = 0;

        const int layer = LayerTransparentInter;
        f3d.Command(0xB6000000, 0x00022000, layer, m);
        f3d.Command(0xFC121824, 0xFF33FFFF, layer, m);
        f3d.Command(0xBB000001, 0xFFFFFFFF, layer, m);
        f3d.Command(0xF5000000 | (uint)(mist ? FormatIa : FormatRgba) << 21 | 2 << 19 | 8 << 9, 0x00014050, layer, m);
        f3d.Command(0xF2000000, 0x0007C07C, layer, m);

        int count = Math.Min((int)_memory.S16(regions), 64);
        var vertices = _vertexScratch.AsSpan(0, 4 * 16);
        for (int i = 0; i < count; i++)
        {
            uint region = regions + 2 + (uint)i * MovtexRegionSize;
            short id = _memory.S16(region), y = _memory.S16(region + 10);
            uint quads = FindQuadArray(quadsTable, id);
            if (quads == 0)
                continue;
            int quadCount = Math.Min((int)_memory.S16(quads), 64);
            for (int q = 0; q < quadCount; q++)
            {
                uint quad = quads + 2 + (uint)q * MovtexQuadSize;
                short rot = _memory.S16(quad), scale = _memory.S16(quad + 4);
                byte alpha = (byte)_memory.S16(quad + 24);
                int textureId = _memory.S16(quad + 26);
                int direction = _memory.S16(quad + 22) == 0 ? 1 : -1;
                for (int v = 0; v < 4; v++)
                {
                    int rotOffset = v == 0 ? 0 : v == 2 ? -32768 : direction * (v == 1 ? 16384 : -16384);
                    double angle = (ushort)(short)(rot + rotOffset) / 65536.0 * 2 * Math.PI;
                    double radius = 32.0 * (32.0 * scale - 1.0);
                    WriteVertex(vertices, v, _memory.S16(quad + 6 + 4 * (uint)v), y, _memory.S16(quad + 8 + 4 * (uint)v),
                        (short)(radius * Math.Sin(angle)), (short)(radius * Math.Cos(angle)), r, g, b, alpha);
                }
                LoadMovtexTexture(f3d, textureId, textureId == MovtexTextureMist ? FormatIa : FormatRgba, layer, m);
                f3d.LoadVertices(vertices, 4, 0, m);
                f3d.UvAnimation = AddUvAnimation(quad, true, Vector2.Zero);
                f3d.Command(0xBF000000, 10 << 8 | 20, layer, m);
                f3d.Command(0xBF000000, 20 << 8 | 30, layer, m);
                f3d.UvAnimation = -1;
            }
        }
    }

    private uint FindQuadArray(uint collection, short id)
    {
        for (uint i = 0; i < 64; i++)
        {
            short entryId = _memory.S16(collection + 8 * i);
            if (entryId == -1)
                return 0;
            if (entryId == id)
                return _memory.U32(collection + 8 * i + 4);
        }
        return 0;
    }

    private void EmitMovtexObject(F3DInterpreter f3d, uint table, uint parameter, bool colored, Matrix4x4 m)
    {
        if (table == 0)
            return;
        for (uint entry = table; entry < table + 64 * MovtexObjectSize; entry += MovtexObjectSize)
        {
            uint verts = _memory.U32(entry + 0x0C);
            if (verts == 0)
                return;
            if (_memory.U32(entry) != parameter)
                continue;

            int textureId = _memory.S32(entry + 4), count = Math.Clamp(_memory.S32(entry + 8), 0, 16);
            uint beginDl = _memory.U32(entry + 0x10), endDl = _memory.U32(entry + 0x14), triDl = _memory.U32(entry + 0x18);
            byte r = _memory.U8(entry + 0x1C), g = _memory.U8(entry + 0x1D), b = _memory.U8(entry + 0x1E), a = _memory.U8(entry + 0x1F);
            int layer = _memory.S32(entry + 0x20);
            uint stride = colored ? 8u : 5u, st = colored ? 7u : 4u;
            short baseS = _memory.S16(verts + 2 * st), baseT = _memory.S16(verts + 2 * (st + 1));
            var vertices = _vertexScratch.AsSpan(0, count * 16);
            for (int i = 0; i < count; i++)
            {
                uint v = verts + 2 * (uint)i * stride;
                short s = baseS, t = baseT;
                if (i > 0)
                {
                    s = (short)(baseS + _memory.S16(v + 2 * st) * 1024);
                    t = (short)(baseT + _memory.S16(v + 2 * (st + 1)) * 1024);
                }
                if (colored)
                    (r, g, b) = ((byte)_memory.S16(v + 8), (byte)_memory.S16(v + 10), (byte)_memory.S16(v + 12));
                WriteVertex(vertices, i, _memory.S16(v + 2), _memory.S16(v + 4), _memory.S16(v + 6), s, t, r, g, b, a);
            }
            f3d.Run(beginDl, layer, m);
            LoadMovtexTexture(f3d, textureId, FormatRgba, layer, m);
            f3d.LoadVertices(vertices, count, 0, m);
            f3d.UvAnimation = AddUvAnimation(verts + 2 * st, false, f3d.TexCoordScale());
            f3d.Run(triDl, layer, m);
            f3d.UvAnimation = -1;
            f3d.Run(endDl, layer, m);
            return;
        }
    }

    private void LoadMovtexTexture(F3DInterpreter f3d, int textureId, int format, int layer, in Matrix4x4 m)
    {
        uint texture = _memory.U32(_extras.MovtexIdToTexture + 4 * (uint)textureId);
        f3d.Command(0xFD000000 | (uint)format << 21 | 2 << 19, texture, layer, m);
        f3d.Command(0xF5000000 | (uint)format << 21 | 2 << 19, 0x07000000, layer, m);
        f3d.Command(0xF3000000, 0x07000000 | 1023 << 12, layer, m);
    }

    private int AddUvAnimation(uint address, bool rotate, Vector2 scale)
    {
        _scene.UvAnimations.Add(new UvAnimation(address, rotate, _memory.S16(address), scale));
        return _scene.UvAnimations.Count - 1;
    }

    private void UpdateUvAnimations()
    {
        foreach (var a in _scene.UvAnimations)
            a.Update(_memory.S16(a.Address));
    }

    private void EmitObjectMovtex(uint n, uint function, int layer, in NodeFrame frame)
    {
        uint parameter = _memory.U32(n + 0x18);
        var key = (function, parameter, layer);
        if (!_objectMovtex.TryGetValue(key, out var entry))
        {
            var builder = new GfxMeshBuilder();
            var f3d = new F3DInterpreter(_memory, _scene.Textures, builder) { Ucode = _ucode };
            f3d.Load(_defaultState!);
            EmitMovtex(f3d, n, Matrix4x4.Identity);
            _objectMovtex[key] = entry = new CachedMesh(builder.Build(), _defaultState!);
        }
        entry.LastUsed = _updates;
        if (entry.Mesh.TriangleCount > 0)
            _scene.Instances.Add(new GfxInstance(entry.Mesh, frame.Matrix, frame.Billboard, frame.Anchor, frame.AnchorScale, false, false));
    }


    private void EmitPaintings()
    {
        if (_extras.PaintingGroups == 0)
            return;
        foreach (var (parameter, m) in _paintingNodes)
            EmitPainting(parameter, m);
    }

    private void EmitPainting(uint parameter, Matrix4x4 m)
    {
        uint group = _memory.U32(_extras.PaintingGroups + 4 * ((parameter >> 8) & 0xFF));
        if (group == 0)
            return;
        uint painting = _memory.U32(group + 4 * (parameter & 0xFF));
        if (painting == 0 || !_memory.Ok(_memory.Phys(painting), 0x78))
            return;
        float size = _memory.F32(painting + 0x74);
        if (!(size > 0))
            return;
        var transform = Matrix4x4.CreateScale(size / PaintingSize)
                        * Matrix4x4.CreateRotationY(_memory.F32(painting + 0x0C) * MathF.PI / 180)
                        * Matrix4x4.CreateRotationX(_memory.F32(painting + 0x08) * MathF.PI / 180)
                        * Matrix4x4.CreateTranslation(_memory.F32(painting + 0x10), _memory.F32(painting + 0x14), _memory.F32(painting + 0x18))
                        * m;
        int layer = _memory.U8(painting + 0x6D) == 0xFF ? LayerOpaque : LayerTransparent;
        if ((sbyte)_memory.U8(painting + 0x07) != 0)
        {
            EmitRipplingPainting(painting, layer, transform);
            return;
        }

        uint dl = _memory.U32(painting + 0x58);
        if (dl == 0)
            return;
        var key = (dl, 0x500 | layer, _defaultState!);
        if (!_meshes.TryGetValue(key, out var entry))
        {
            var builder = new GfxMeshBuilder();
            var f3d = new F3DInterpreter(_memory, _scene.Textures, builder) { Ucode = _ucode };
            f3d.Load(_defaultState!);
            f3d.Run(dl, layer, Matrix4x4.Identity);
            _meshes[key] = entry = new CachedMesh(builder.Build(), _defaultState!);
        }
        entry.LastUsed = _updates;
        if (entry.Mesh.TriangleCount > 0)
            _scene.Instances.Add(new GfxInstance(entry.Mesh, transform, false, default, Vector3.One, false, false, Level: true));
    }

    private void EmitRipplingPainting(uint painting, int layer, Matrix4x4 transform)
    {
        bool envMapped = (sbyte)_memory.U8(painting + 0x03) == 1;
        var meshData = MipsLeaf.FindSegmentedConstants(_memory, _extras.DisplayPaintingRippling, 0x02);
        var beginEnd = MipsLeaf.FindSegmentedConstants(_memory, envMapped ? _extras.PaintingRippleEnvMapped : _extras.PaintingRippleImage, 0x02);
        if (meshData.Count < 2 || beginEnd.Count < 2)
            return;
        uint mesh = meshData[0], neighborTris = meshData[1];
        int numVtx = Math.Clamp((int)_memory.S16(mesh), 0, 1024);
        int numTris = Math.Clamp((int)_memory.S16(mesh + 2 * (uint)(numVtx * 3 + 1)), 0, 2048);
        if (numVtx == 0 || numTris == 0)
            return;

        var pos = new short[numVtx * 3];
        var normal = new sbyte[numVtx * 3];
        float sizeRatio = _memory.F32(painting + 0x74) / PaintingSize;
        float magnitude = _memory.F32(painting + 0x1C), rate = _memory.F32(painting + 0x34), dispersion = _memory.F32(painting + 0x40);
        float timer = _memory.F32(painting + 0x4C), rippleX = _memory.F32(painting + 0x50), rippleY = _memory.F32(painting + 0x54);
        for (int i = 0; i < numVtx; i++)
        {
            uint v = mesh + 2 * (uint)(i * 3 + 1);
            pos[3 * i] = _memory.S16(v);
            pos[3 * i + 1] = _memory.S16(v + 2);
            if (_memory.S16(v + 4) == 0)
                continue;
            float x = pos[3 * i] * sizeRatio, y = pos[3 * i + 1] * sizeRatio;
            float distance = MathF.Sqrt((x - rippleX) * (x - rippleX) + (y - rippleY) * (y - rippleY)) / dispersion;
            if (timer < distance)
                continue;
            float z = magnitude * (float)Math.Cos(rate * (2 * Math.PI) * (timer - distance));
            pos[3 * i + 2] = (short)(z >= 0 ? z + 0.5 : z - 0.5);
        }

        var triNormals = new Vector3[numTris];
        for (int i = 0; i < numTris; i++)
        {
            uint tri = mesh + 2 * (uint)(numVtx * 3 + i * 3 + 2);
            int v0 = Math.Clamp((int)_memory.S16(tri), 0, numVtx - 1), v1 = Math.Clamp((int)_memory.S16(tri + 2), 0, numVtx - 1), v2 = Math.Clamp((int)_memory.S16(tri + 4), 0, numVtx - 1);
            float x0 = pos[3 * v0], y0 = pos[3 * v0 + 1], z0 = pos[3 * v0 + 2];
            float x1 = pos[3 * v1], y1 = pos[3 * v1 + 1], z1 = pos[3 * v1 + 2];
            float x2 = pos[3 * v2], y2 = pos[3 * v2 + 1], z2 = pos[3 * v2 + 2];
            triNormals[i] = new Vector3((y1 - y0) * (z2 - z1) - (z1 - z0) * (y2 - y1), (z1 - z0) * (x2 - x1) - (x1 - x0) * (z2 - z1), (x1 - x0) * (y2 - y1) - (y1 - y0) * (x2 - x1));
        }
        static sbyte Component(float c) => c > 0 ? (sbyte)(c * 127.0 + 0.5) : c < 0 ? (sbyte)(c * 128.0 - 0.5) : (sbyte)0;
        uint neighbors = neighborTris;
        for (int i = 0; i < numVtx; i++)
        {
            int count = Math.Clamp((int)_memory.S16(neighbors), 0, 64);
            var sum = Vector3.Zero;
            for (int j = 0; j < count; j++)
                sum += triNormals[Math.Clamp((int)_memory.S16(neighbors + 2 * (uint)(j + 1)), 0, numTris - 1)];
            neighbors += 2 * (uint)(count + 1);
            if (count == 0 || sum.LengthSquared() == 0)
                continue;
            var n = Vector3.Normalize(sum);
            normal[3 * i] = Component(n.X);
            normal[3 * i + 1] = Component(n.Y);
            normal[3 * i + 2] = Component(n.Z);
        }

        uint rippleDl = _memory.U32(painting + 0x68), textureMaps = _memory.U32(painting + 0x5C), textureArray = _memory.U32(painting + 0x60);
        if (rippleDl == 0 || textureMaps == 0 || textureArray == 0)
            return;
        var builder = new GfxMeshBuilder();
        var f3d = new F3DInterpreter(_memory, _scene.Textures, builder) { Ucode = _ucode };
        f3d.Load(_defaultState!);
        f3d.Run(beginEnd[0], layer, transform);
        f3d.Run(rippleDl, layer, transform);
        int images = envMapped ? 1 : Math.Clamp((int)(sbyte)_memory.U8(painting + 0x02), 0, 16);
        int width = _memory.S16(painting + 0x64), height = _memory.S16(painting + 0x66);
        byte alpha = _memory.U8(painting + 0x6D);
        var vertices = _vertexScratch.AsSpan(0, 3 * 16);
        for (int image = 0; image < images; image++)
        {
            uint map = _memory.U32(textureMaps + 4 * (uint)image);
            if (map == 0)
                continue;
            f3d.Command(0xFD000000 | 2 << 19, _memory.U32(textureArray + 4 * (uint)image), layer, transform);
            f3d.Command(0xF5000000 | 2 << 19, 0x07000000, layer, transform);
            f3d.Command(0xF3000000, 0x07000000 | (uint)Math.Clamp(width * height - 1, 0, 4095) << 12, layer, transform);
            int mapVerts = Math.Clamp((int)_memory.S16(map), 0, 1024);
            int mapTris = Math.Clamp((int)_memory.S16(map + 2 * (uint)(mapVerts * 3 + 1)), 0, 2048);
            if (mapVerts == 0)
                continue;
            for (int t = 0; t < mapTris; t++)
            {
                for (int k = 0; k < 3; k++)
                {
                    int mapping = Math.Clamp((int)_memory.S16(map + 2 * (uint)(mapVerts * 3 + 2 + t * 3 + k)), 0, mapVerts - 1);
                    uint mapped = map + 2 * (uint)(mapping * 3 + 1);
                    int vtx = Math.Clamp((int)_memory.S16(mapped), 0, numVtx - 1);
                    WriteVertex(vertices, k, pos[3 * vtx], pos[3 * vtx + 1], pos[3 * vtx + 2], _memory.S16(mapped + 2), _memory.S16(mapped + 4),
                        (byte)normal[3 * vtx], (byte)normal[3 * vtx + 1], (byte)normal[3 * vtx + 2], alpha);
                }
                f3d.LoadVertices(vertices, 3, 0, transform);
                f3d.Command(0xBF000000, 10 << 8 | 20, layer, transform);
            }
        }
        f3d.Run(beginEnd[^1], layer, transform);
        var built = builder.Build(transient: true);
        if (built.TriangleCount > 0)
            _scene.Instances.Add(new GfxInstance(built, Matrix4x4.Identity, false, default, Vector3.One, false, false, Level: true));
    }


    private const uint EnvFxParticleSize = 0x38;
    private const uint DlSnowGrayBegin = 0x0B006A50, DlSnowBlueBegin = 0x0B006CD8, DlParticlesEnd = 0x0B006AB0;
    private const uint DlBubblesBegin = 0x0B006D38, DlBubbleTexture = 0x0B006D68;
    private const uint FlowerTextures = 0x0B002008, LavaBubbleTextures = 0x0B006020, BubbleTextures = 0x0B006848;

    private void EmitEnvFx()
    {
        if (_envFxNode == 0 || _extras.EnvFxMode == 0 || _extras.EnvFxBuffer == 0)
            return;
        int mode = (sbyte)_memory.U8(_extras.EnvFxMode);
        uint buffer = _memory.U32(_extras.EnvFxBuffer);
        if (mode == 0 || !_memory.IsPointer(buffer) || (_memory.U16(_envFxNode + 2) & RenderActive) == 0)
            return;
        bool bubbles = mode >= 10;
        uint countAddress = bubbles ? _extras.BubbleParticleMaxCount : _extras.SnowParticleCount;
        if (countAddress == 0)
            return;
        int count = bubbles ? _memory.S32(countAddress) : _memory.S16(countAddress);
        count = Math.Clamp((count + 4) / 5 * 5, 0, 200);
        for (int i = 0; i < count; i++)
        {
            uint particle = buffer + (uint)i * EnvFxParticleSize;
            var mesh = EnvFxMesh(mode, bubbles ? _memory.S16(buffer + (uint)(i - i % 5) * EnvFxParticleSize + 2) : 0);
            if (mesh is null)
                return;
            var position = new Vector3(_memory.S32(particle + 4), _memory.S32(particle + 8), _memory.S32(particle + 12));
            _scene.Instances.Add(new GfxInstance(mesh, Matrix4x4.Identity, true, position, Vector3.One, false, false));
        }
    }

    private GfxMesh? EnvFxMesh(int mode, int frame)
    {
        const int layer = 4;
        var key = ((uint)frame, 0x300 | layer | (mode << 12), _defaultState!);
        if (_meshes.TryGetValue(key, out var cached))
        {
            cached.LastUsed = _updates;
            return cached.Mesh;
        }
        uint begin, texture = 0;
        switch (mode)
        {
            case 1 or 2 or 3:
                begin = mode == 2 ? DlSnowBlueBegin : DlSnowGrayBegin;
                break;
            case 11 or 12 or 13 or 14:
                begin = DlBubblesBegin;
                uint table = mode == 11 ? FlowerTextures : mode == 12 ? LavaBubbleTextures : BubbleTextures;
                texture = _memory.U32(table + 4 * (uint)(mode is 11 or 12 ? Math.Clamp(frame, 0, 63) : 0));
                break;
            default:
                return null;
        }
        var vertices = _vertexScratch.AsSpan(0, 3 * 16);
        if (mode < 10)
        {
            WriteVertex(vertices, 0, -5, 5, 0, 0, 0, 0x7F, 0x7F, 0x7F, 0xFF);
            WriteVertex(vertices, 1, -5, -5, 0, 0, 960, 0x7F, 0x7F, 0x7F, 0xFF);
            WriteVertex(vertices, 2, 5, 5, 0, 960, 0, 0x7F, 0x7F, 0x7F, 0xFF);
        }
        else
        {
            var (spread, height) = mode == 11 ? ((short)50, (short)75) : mode == 12 ? ((short)100, (short)150) : ((short)40, (short)60);
            WriteVertex(vertices, 0, spread, 0, 0, 1544, 964, 0xFF, 0xFF, 0xFF, 0xFF);
            WriteVertex(vertices, 1, 0, height, 0, 522, -568, 0xFF, 0xFF, 0xFF, 0xFF);
            WriteVertex(vertices, 2, (short)-spread, 0, 0, -498, 964, 0xFF, 0xFF, 0xFF, 0xFF);
        }
        var builder = new GfxMeshBuilder();
        var f3d = new F3DInterpreter(_memory, _scene.Textures, builder) { Ucode = _ucode };
        f3d.Load(_defaultState!);
        f3d.Run(begin, layer, Matrix4x4.Identity);
        if (texture != 0)
        {
            f3d.Command(0xFD000000 | 2 << 19, texture, layer, Matrix4x4.Identity);
            f3d.Run(DlBubbleTexture, layer, Matrix4x4.Identity);
        }
        f3d.LoadVertices(vertices, 3, 0, Matrix4x4.Identity);
        f3d.Command(0xBF000000, 10 << 8 | 20, layer, Matrix4x4.Identity);
        f3d.Run(DlParticlesEnd, layer, Matrix4x4.Identity);
        var mesh = builder.Build();
        _meshes[key] = new CachedMesh(mesh, _defaultState!) { LastUsed = _updates };
        return mesh;
    }


    private uint _carpetTimer;

    private void EmitFlyingCarpet(in NodeFrame frame)
    {
        if (_carpetTimer == 0)
            _carpetTimer = _extras.FlyingCarpetRippleTimer != 0 ? _extras.FlyingCarpetRippleTimer : MipsLeaf.FindFirstHalfwordLoad(_memory, _extras.GeoFlyingCarpetCreate);
        if (_carpetTimer == 0)
            return;
        var constants = MipsLeaf.FindSegmentedConstants(_memory, _extras.GeoFlyingCarpetCreate, 0x07);
        if (constants.Count < 4)
            return;
        uint data = constants[0], begin = constants[1], half = constants[2], end = constants[^1];
        ushort timer = _memory.U16(_carpetTimer);
        var vertices = _vertexScratch.AsSpan(0, 21 * 16);
        for (int n = 0; n < 21; n++)
        {
            int row = n / 3, col = n % 3;
            double y = Math.Sin(((ushort)(timer + (row << 12) + (col << 14)) >> 4) * (2 * Math.PI / 4096)) * 20.0;
            uint v = data + 8 * (uint)n;
            WriteVertex(vertices, n, _memory.S16(v), (short)(y >= 0 ? y + 0.5 : y - 0.5), _memory.S16(v + 2), _memory.S16(v + 4), _memory.S16(v + 6), 0, 127, 0, 255);
        }
        var builder = new GfxMeshBuilder();
        var f3d = new F3DInterpreter(_memory, _scene.Textures, builder) { Ucode = _ucode };
        f3d.Load(_defaultState!);
        f3d.Run(begin, LayerOpaque, Matrix4x4.Identity);
        f3d.LoadVertices(vertices, 12, 0, Matrix4x4.Identity);
        f3d.Run(half, LayerOpaque, Matrix4x4.Identity);
        f3d.LoadVertices(vertices[(9 * 16)..], 12, 0, Matrix4x4.Identity);
        f3d.Run(half, LayerOpaque, Matrix4x4.Identity);
        f3d.Run(end, LayerOpaque, Matrix4x4.Identity);
        var mesh = builder.Build(transient: true);
        if (mesh.TriangleCount > 0)
            _scene.Instances.Add(new GfxInstance(mesh, frame.Matrix, frame.Billboard, frame.Anchor, frame.AnchorScale, false, false));
    }
}
