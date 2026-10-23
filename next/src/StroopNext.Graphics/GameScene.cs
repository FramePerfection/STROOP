using System.Numerics;
using StroopNext.Game;
using StroopNext.Ghosts;
using StroopNext.Session;

namespace StroopNext.Graphics;

public readonly record struct GfxInstance(GfxMesh Mesh, Matrix4x4 Matrix, bool Billboard, Vector3 Anchor, Vector3 AnchorScale, bool IsMario, bool Cap, bool Level = false, bool Ghost = false);

public readonly record struct VirtualGhost(Vector3 Position, short Pitch, short Yaw, short Roll, short AnimationId, short AnimationFrame, byte[]? Body = null);

public readonly record struct GfxShadow(Vector3 Position, float Size, float Opacity, bool Square);

public sealed class GameScene
{
    public GfxMesh Level { get; internal set; } = GfxMesh.Empty;

    public int LevelVersion { get; internal set; }

    public TextureLibrary Textures { get; } = new();
    public List<GfxInstance> Instances { get; } = new();
    public List<GfxShadow> Shadows { get; } = new();

    public DecodedTexture? Skybox { get; internal set; }
    public Vector3? BackgroundColor { get; internal set; }

    public Vector2 PerspectivePlanes { get; internal set; } = new(100, 20000);

    public Vector3 MarioModelPosition { get; internal set; }

    public Func<VirtualGhost, bool>? AddVirtualGhost { get; internal set; }

    public ushort MarioModelYaw { get; internal set; }
    public bool HasLevel => Level.TriangleCount > 0;

    public List<UvAnimation> UvAnimations { get; } = new();
}

public sealed class UvAnimation(uint address, bool rotate, short built, Vector2 scale)
{
    public uint Address { get; } = address;
    public bool Rotate { get; } = rotate;
    public short Built { get; } = built;
    public Vector2 Scale { get; } = scale;
    public float[] Transform { get; } = [1, 0, 0, 0, 1, 0, 0, 0, 1];

    internal void Update(short now)
    {
        int delta = (short)(now - Built);
        if (Rotate)
        {
            float angle = (float)(delta / 65536.0 * 2 * Math.PI), sin = MathF.Sin(angle), cos = MathF.Cos(angle);
            Transform[0] = cos; Transform[1] = -sin; Transform[2] = 0;
            Transform[3] = sin; Transform[4] = cos; Transform[5] = 0;
            Transform[6] = 0; Transform[7] = 0; Transform[8] = 1;
        }
        else
        {
            Transform[0] = 1; Transform[1] = 0; Transform[2] = 0;
            Transform[3] = 0; Transform[4] = 1; Transform[5] = 0;
            Transform[6] = delta * Scale.X; Transform[7] = 0; Transform[8] = 1;
        }
    }
}

public sealed partial class GameSceneBuilder
{
    internal const int GN_ROOT = 0x001, GN_ORTHO = 0x002, GN_PERSPECTIVE = 0x103, GN_LOD = 0x00B, GN_SWITCH = 0x10C, GN_CAMERA = 0x114;
    internal const int GN_TRANSROT = 0x015, GN_TRANS = 0x016, GN_ROT = 0x017, GN_OBJECT = 0x018, GN_ANIMPART = 0x019, GN_BILLBOARD = 0x01A, GN_DL = 0x01B, GN_SCALE = 0x01C;
    internal const int GN_SHADOW = 0x028, GN_OBJPARENT = 0x029, GN_GENLIST = 0x12A, GN_BG = 0x12C, GN_HELD = 0x12E;
    private const int RenderActive = 0x01, RenderBillboard = 0x04, RenderInvisible = 0x10;
    private const int LayerOpaque = 1, LayerTransparent = 5;
    private const int MeshCacheLimit = 4096, MeshMaxAge = 600;

    private readonly GameScene _scene = new();
    private object? _levelKey;
    private int _updates;

    private GraphScheme _scheme = GraphScheme.Vanilla;
    private Microcode _ucode = Microcode.F3D;
    private uint _root, _cameraNode, _objectGroup, _mirrorParent;

    private readonly Dictionary<(uint Dl, int Layer, F3DState State), CachedMesh> _meshes = new();
    private readonly Dictionary<(F3DState State, int Alpha), F3DState> _derivedStates = new();
    private readonly F3DState[] _layerStates = new F3DState[8];
    private F3DState? _defaultState;
    private GfxMemory _memory = null!;
    private Addresses _addresses;
    private readonly HashSet<uint> _capWings = new();
    private readonly HashSet<uint> _roomCulled = new();
    private uint _currentObject, _marioObject, _mirrorMario;
    private byte _areaIndex;
    private int _nodeBudget;
    private bool _inHeldObject, _inCap, _wingsShown, _emittingGhost;
    private bool _emittingRealMario;
    private bool _stateDerived;
    private readonly F3DState[] _statesBeforeObject = new F3DState[8];
    private int _graphBudget;
    private Vector3 _gameCamera, _gameCameraForward;
    private Animation _anim;

    private sealed class CachedMesh(GfxMesh mesh, F3DState output)
    {
        public GfxMesh Mesh { get; } = mesh;
        public F3DState Output { get; } = output;
        public int LastUsed;
    }

    public GameScene Scene => _scene;

    public int CachedMeshCount => _meshes.Count;

    public GameSceneBuilder() => _scene.AddVirtualGhost = DrawVirtualGhost;

    private const uint GhostNode = 0x80000000 | GfxMemory.OverlayBase, GhostBody = GhostNode + 0x80, GhostAnimation = GhostNode + 0x100;
    private readonly Dictionary<short, byte[]> _ghostOverlays = new();

    private bool DrawVirtualGhost(VirtualGhost g)
    {
        if (!_memory.IsPointer(_marioObject) || MarioAnimations.Get(g.AnimationId) is not { } animation)
            return false;
        if (!_ghostOverlays.TryGetValue(g.AnimationId, out var overlay))
        {
            overlay = new byte[0x100 + animation.Blob.Length];
            animation.Blob.CopyTo(overlay, 0x100);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(overlay.AsSpan(0x100 + 0x0C), (GhostAnimation & 0x1FFFFFFF) + animation.ValuesOffset);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(overlay.AsSpan(0x100 + 0x10), (GhostAnimation & 0x1FFFFFFF) + animation.IndexOffset);
            if (_ghostOverlays.Count > 64)
                _ghostOverlays.Clear();
            _ghostOverlays[g.AnimationId] = overlay;
        }
        overlay.AsSpan(0, 0x100).Clear();
        void U16(int at, int v) => System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(overlay.AsSpan(at), (ushort)v);
        void U32(int at, uint v) => System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(overlay.AsSpan(at), v);
        void F32(int at, float v) => System.Buffers.Binary.BinaryPrimitives.WriteSingleBigEndian(overlay.AsSpan(at), v);
        U16(0x00, _memory.U16(_marioObject));
        U16(0x02, (_memory.U16(_marioObject + 2) | RenderActive) & ~RenderInvisible & ~0x40);
        U32(0x14, _memory.U32(_marioObject + 0x14));
        overlay[0x18] = (byte)_areaIndex;
        U16(0x1A, g.Pitch);
        U16(0x1C, g.Yaw);
        U16(0x1E, g.Roll);
        F32(0x20, g.Position.X);
        F32(0x24, g.Position.Y);
        F32(0x28, g.Position.Z);
        F32(0x2C, 1);
        F32(0x30, 1);
        F32(0x34, 1);
        U16(0x38, g.AnimationId);
        U16(0x3A, _memory.U16(_marioObject + 0x3A));
        U32(0x3C, GhostAnimation);
        U16(0x40, g.AnimationFrame);
        g.Body?.AsSpan(0, Math.Min(g.Body.Length, 0x14)).CopyTo(overlay.AsSpan(0x80 + 4));

        _memory.SetOverlay(overlay);
        _emittingGhost = true;
        try
        {
            _nodeBudget = 4096;
            DrawObject(GhostNode);
        }
        finally
        {
            _emittingGhost = false;
            _memory.SetOverlay(null);
        }
        return true;
    }

    public bool Update(GameFrame frame)
    {
        _updates++;
        if (frame.Layout is not { SegmentTable: uint segments, CurrentAreaPointer: uint areaPointer } layout)
            return false;
        _memory = new GfxMemory(frame.Ram, segments);
        _addresses = Addresses.Resolve(frame, layout);
        _extras = Extras.Resolve(frame);

        uint area = _memory.U32(areaPointer);
        if (!_memory.IsPointer(area) || !_memory.IsPointer(_memory.U32(area + 4)))
            return false;

        var key = (area, _memory.U8(area), _memory.U32(area + 4), frame.LevelIndex, layout.StaticSurfaceCount is uint c ? _memory.S32(c) : 0, _memory.Segment(7), ExtrasKey());
        if (!Equals(key, _levelKey))
        {
            BuildLevel(area);
            _levelKey = key;
        }
        UpdateObjects(frame);
        return true;
    }


    private void BuildLevel(uint area)
    {
        _scene.Textures.Clear();
        _meshes.Clear();
        _derivedStates.Clear();
        _capWings.Clear();
        _defaultState = null;
        _cameraNode = _objectGroup = _mirrorParent = _envFxNode = 0;
        _carpetTimer = 0;
        _movtexNodes.Clear();
        _paintingNodes.Clear();
        _scene.UvAnimations.Clear();
        _scene.Skybox = null;
        _scene.BackgroundColor = null;

        _root = _memory.U32(area + 4);
        _scheme = GraphScheme.Detect(_memory, _root);
        _graphBudget = 16384;
        var displayLists = new List<(uint Dl, int Layer, Matrix4x4 M)>();
        CollectDisplayLists(_root, Matrix4x4.Identity, 0, displayLists);
        _ucode = F3DInterpreter.Detect(_memory, displayLists.Select(d => d.Dl));

        var builder = new GfxMeshBuilder();
        var f3d = new F3DInterpreter(_memory, _scene.Textures, builder) { Ucode = _ucode };
        foreach (var (dl, layer, m) in displayLists)
        {
            f3d.Geo = F3DInterpreter.GeoInit;
            f3d.Run(dl, layer, m);
        }
        foreach (var (node, m) in _movtexNodes)
        {
            f3d.Geo = F3DInterpreter.GeoInit;
            EmitMovtex(f3d, node, m);
        }
        _scene.Level = builder.Build();
        _scene.LevelVersion++;
        ReadBackground();
    }

    private IEnumerable<uint> Children(uint node)
    {
        uint first = _memory.U32(node + 0x10), c = first;
        for (int i = 0; i < 1024 && _memory.IsPointer(c); i++)
        {
            yield return c;
            c = _memory.U32(c + 0x08);
            if (c == first)
                break;
        }
    }

    private int NodeType(uint node) => _scheme.Type(_memory.U16(node));

    private void CollectDisplayLists(uint n, Matrix4x4 m, int depth, List<(uint, int, Matrix4x4)> output)
    {
        if (depth > 40 || --_graphBudget < 0)
            return;
        int t = NodeType(n);
        ushort flags = _memory.U16(n + 2);
        if ((flags & RenderActive) == 0)
            return;
        switch (t)
        {
            case GN_CAMERA:
                _cameraNode = n;
                break;
            case GN_PERSPECTIVE:
                _scene.PerspectivePlanes = new Vector2(_memory.U16(n + _scheme.PerspectiveNear), _memory.U16(n + _scheme.PerspectiveNear + 2));
                break;
            case GN_OBJPARENT:
                _objectGroup = _memory.U32(n + 0x14);
                break;
            case GN_GENLIST when _addresses.GeoRenderMirrorMario != 0 && _memory.U32(n + 0x14) == _addresses.GeoRenderMirrorMario:
                _mirrorParent = n;
                break;
            case GN_GENLIST:
                if (StaticGeneratedNode(n, m) is var light and not 0)
                    output.Add((light, LayerTransparent, m));
                break;
        }
        if (t is GN_OBJPARENT or GN_GENLIST or GN_BG or GN_HELD or GN_OBJECT or GN_SHADOW or GN_ORTHO)
            return;

        int layer = _scheme.Layer((flags >> 8) & 0xFF);
        uint dl = 0;
        switch (t)
        {
            case GN_DL:
                dl = _memory.U32(n + 0x14);
                break;
            case GN_TRANSROT:
                m = Gfx.RotZXYTrans(_memory.S16(n + 0x18), _memory.S16(n + 0x1A), _memory.S16(n + 0x1C), _memory.S16(n + 0x1E), _memory.S16(n + 0x20), _memory.S16(n + 0x22)) * m;
                dl = _memory.U32(n + 0x14);
                break;
            case GN_TRANS:
            case GN_ANIMPART:
            case GN_BILLBOARD:
                m = Gfx.RotZXYTrans(_memory.S16(n + 0x18), _memory.S16(n + 0x1A), _memory.S16(n + 0x1C), 0, 0, 0) * m;
                dl = _memory.U32(n + 0x14);
                break;
            case GN_ROT:
                m = Gfx.RotZXYTrans(0, 0, 0, _memory.S16(n + 0x18), _memory.S16(n + 0x1A), _memory.S16(n + 0x1C)) * m;
                dl = _memory.U32(n + 0x14);
                break;
            case GN_SCALE:
                m = Matrix4x4.CreateScale(_memory.F32(n + 0x18)) * m;
                dl = _memory.U32(n + 0x14);
                break;
        }
        if (dl != 0)
            output.Add((dl, layer, m));
        if (t == GN_LOD && !(_memory.S16(n + 0x14) <= 0 && 0 < _memory.S16(n + 0x16)))
            return;
        foreach (uint c in Children(n))
            CollectDisplayLists(c, m, depth + 1, output);
    }

    private void ReadBackground()
    {
        var stack = new Stack<uint>();
        var seen = new HashSet<uint>();
        stack.Push(_root);
        while (stack.Count > 0 && seen.Count < 4096)
        {
            uint n = stack.Pop();
            if (!_memory.IsPointer(n) || !seen.Add(n))
                continue;
            if (NodeType(n) == GN_BG)
            {
                uint function = _memory.U32(n + 0x14), background = _memory.U32(n + 0x1C);
                if (function == 0)
                {
                    int c = (int)(background & 0xFFFF);
                    _scene.BackgroundColor = new Vector3(((c >> 11) & 31) / 31f, ((c >> 6) & 31) / 31f, ((c >> 1) & 31) / 31f);
                }
                else if (_addresses.SkyboxTextures != 0)
                    _scene.Skybox = DecodeSkybox(_memory.U32(_addresses.SkyboxTextures + 4 * (background & 0xFF)));
                return;
            }
            foreach (uint c in Children(n))
                stack.Push(c);
        }
    }

    private DecodedTexture? DecodeSkybox(uint tiles)
    {
        const int cols = 10, rows = 8;
        if (_memory.Phys(tiles) == uint.MaxValue)
            return null;
        var image = new byte[cols * 32 * rows * 32 * 4];
        for (int t = 0; t < cols * rows; t++)
        {
            var tile = TextureDecoder.Decode(_memory, new TextureKey(_memory.U32(tiles + (uint)(4 * t)), 0, 2, 32, 32, F3DInterpreter.NoTlut, 0, 0, 0, 0));
            int x0 = t % cols * 32, y0 = t / cols * 32;
            for (int y = 0; y < 32; y++)
                Buffer.BlockCopy(tile, y * 128, image, ((y0 + y) * cols * 32 + x0) * 4, 128);
        }
        return new DecodedTexture(-1, cols * 32, rows * 32, image, TextureWrap.Repeat, TextureWrap.Clamp);
    }


    private struct Animation
    {
        public int Type, Frame;
        public uint Attributes, Values;
        public float TranslationMultiplier;
    }

    private const int AnimNone = 0, AnimTranslation = 1, AnimVertical = 2, AnimLateral = 3, AnimNoTranslation = 4, AnimRotation = 5;

    private struct NodeFrame
    {
        public Matrix4x4 Matrix, ObjectMatrix;
        public bool Billboard;
        public Vector3 Anchor, AnchorScale;
        public Vector3 WorldPosition => Billboard ? Anchor + Matrix.Translation : Matrix.Translation;
    }

    private void UpdateObjects(GameFrame frame)
    {
        _scene.Instances.Clear();
        _scene.Shadows.Clear();
        AgeCaches();
        _defaultState ??= DefaultState();
        UpdateUvAnimations();
        EmitPaintings();
        if (!_memory.IsPointer(_objectGroup))
            return;

        Array.Fill(_layerStates, _defaultState);
        _marioObject = _addresses.MarioObjectPointer != 0 ? _memory.U32(_addresses.MarioObjectPointer) : 0;
        _globalTimer = frame.GlobalTimer;
        CollectGhostNodes();
        _areaIndex = _memory.U8(_root + 0x14);
        _gameCamera = _gameCameraForward = Vector3.Zero;
        if (_memory.IsPointer(_cameraNode))
        {
            _gameCamera = new Vector3(_memory.F32(_cameraNode + 0x1C), _memory.F32(_cameraNode + 0x20), _memory.F32(_cameraNode + 0x24));
            var focus = new Vector3(_memory.F32(_cameraNode + 0x28), _memory.F32(_cameraNode + 0x2C), _memory.F32(_cameraNode + 0x30));
            if ((focus - _gameCamera).LengthSquared() > 0)
                _gameCameraForward = Vector3.Normalize(focus - _gameCamera);
        }

        _roomCulled.Clear();
        foreach (var o in frame.Objects.All)
            if (o.SegmentedBehavior is 0x13000000 or 0x13000AFC or 0x13000B0C)
                _roomCulled.Add(o.Address);

        _mirrorMario = 0;
        foreach (uint obj in Children(_objectGroup))
        {
            _nodeBudget = 4096;
            _emittingRealMario = obj == _marioObject;
            DrawObject(obj);
            _emittingRealMario = false;
            if (obj == _marioObject)
            {
                MarioAnimations.Capture(_memory.Ram, obj);
                _scene.MarioModelPosition = new Vector3(_memory.F32(obj + 0x20), _memory.F32(obj + 0x24), _memory.F32(obj + 0x28));
                _scene.MarioModelYaw = _memory.U16(obj + 0x1C);
            }
        }
        if (_memory.IsPointer(_mirrorParent))
            foreach (uint obj in Children(_mirrorParent))
            {
                _mirrorMario = obj;
                _nodeBudget = 4096;
                DrawObject(obj);
            }
        EmitEnvFx();
    }

    private F3DState DefaultState()
    {
        var f3d = new F3DInterpreter(_memory, _scene.Textures, new GfxMeshBuilder()) { Ucode = _ucode, Combine1 = F3DInterpreter.CombineShadeW1 };
        return f3d.Save();
    }

    private void AgeCaches()
    {
        if (_meshes.Count <= MeshCacheLimit)
            return;
        foreach (var stale in _meshes.Where(kv => _updates - kv.Value.LastUsed > MeshMaxAge).Select(kv => kv.Key).ToList())
            _meshes.Remove(stale);
        if (_meshes.Count > MeshCacheLimit)
        {
            _meshes.Clear();
            _objectMovtex.Clear();
        }
        if (_scene.UvAnimations.Count > 4096)
            _levelKey = null;
        if (_derivedStates.Count > MeshCacheLimit)
            _derivedStates.Clear();
    }

    private bool IsMario => _currentObject == _marioObject || (_currentObject == _mirrorMario && _mirrorMario != 0) || _emittingGhost;

    private void DrawObject(uint n)
    {
        if (NodeType(n) != GN_OBJECT)
            return;
        ushort flags = _memory.U16(n + 2);
        if ((flags & RenderInvisible) != 0)
            return;
        if ((flags & RenderActive) == 0)
        {
            if (n == _mirrorMario)
                return;
            int activeFlags = _memory.U16(n + 0x74);
            bool roomCulled = (activeFlags & 0x08) != 0 || _roomCulled.Contains(n);
            if ((activeFlags & 0x01) == 0 || !roomCulled)
                return;
        }
        if (_memory.U8(n + 0x18) != _areaIndex)
            return;
        uint sharedChild = _memory.U32(n + 0x14);
        if (!_memory.IsPointer(sharedChild))
            return;

        var position = new Vector3(_memory.F32(n + 0x20), _memory.F32(n + 0x24), _memory.F32(n + 0x28));
        var scale = new Vector3(_memory.F32(n + 0x2C), _memory.F32(n + 0x30), _memory.F32(n + 0x34));
        var nodeFrame = new NodeFrame();
        uint throwMatrix = _memory.U32(n + 0x50);
        if (_memory.IsPointer(throwMatrix))
            nodeFrame.Matrix = Gfx.ScaleRows(ReadMatrix(throwMatrix), scale);
        else if ((flags & RenderBillboard) != 0)
        {
            nodeFrame.Matrix = Matrix4x4.Identity;
            nodeFrame.Billboard = true;
            nodeFrame.Anchor = position;
            nodeFrame.AnchorScale = scale;
        }
        else
            nodeFrame.Matrix = Gfx.ScaleRows(Gfx.RotZXYTrans(position.X, position.Y, position.Z, _memory.S16(n + 0x1A), _memory.S16(n + 0x1C), _memory.S16(n + 0x1E)), scale);
        nodeFrame.ObjectMatrix = nodeFrame.Matrix;

        _currentObject = n;
        SetAnimation(n);
        _layerStates.CopyTo(_statesBeforeObject, 0);
        _stateDerived = false;
        DrawSiblings(sharedChild, nodeFrame, 0);
        if (_stateDerived)
            _statesBeforeObject.CopyTo(_layerStates, 0);
        _anim.Type = AnimNone;
    }

    private Matrix4x4 ReadMatrix(uint a)
    {
        var m = new Matrix4x4();
        for (int r = 0; r < 4; r++)
        for (int c = 0; c < 4; c++)
            m[r, c] = _memory.F32(a + (uint)(16 * r + 4 * c));
        return m;
    }

    private void SetAnimation(uint objectNode)
    {
        _anim = default;
        uint current = _memory.U32(objectNode + 0x3C);
        if (!_memory.IsPointer(current) && (current >> 24) == 0)
            return;
        int animFlags = _memory.U16(current);
        short divisor = _memory.S16(current + 0x02);
        _anim.Type = (animFlags & (1 << 3)) != 0 ? AnimVertical
            : (animFlags & (1 << 4)) != 0 ? AnimLateral
            : (animFlags & (1 << 6)) != 0 ? AnimNoTranslation
            : AnimTranslation;
        _anim.Frame = _memory.S16(objectNode + 0x40);
        _anim.Values = _memory.U32(current + 0x0C);
        _anim.Attributes = _memory.U32(current + 0x10);
        _anim.TranslationMultiplier = divisor == 0 ? 1 : _memory.S16(objectNode + 0x3A) / (float)divisor;
    }

    private short NextAnimationValue()
    {
        int count = _memory.U16(_anim.Attributes), start = _memory.U16(_anim.Attributes + 2);
        int index = _anim.Frame < count ? start + _anim.Frame : start + count - 1;
        _anim.Attributes += 4;
        return _memory.S16(_anim.Values + (uint)(2 * index));
    }

    private Matrix4x4 AnimatedPart(uint n)
    {
        double tx = _memory.S16(n + 0x18), ty = _memory.S16(n + 0x1A), tz = _memory.S16(n + 0x1C);
        short rx = 0, ry = 0, rz = 0;
        float k = _anim.TranslationMultiplier;
        switch (_anim.Type)
        {
            case AnimTranslation:
                tx += NextAnimationValue() * k;
                ty += NextAnimationValue() * k;
                tz += NextAnimationValue() * k;
                _anim.Type = AnimRotation;
                break;
            case AnimLateral:
                tx += NextAnimationValue() * k;
                _anim.Attributes += 4;
                tz += NextAnimationValue() * k;
                _anim.Type = AnimRotation;
                break;
            case AnimVertical:
                _anim.Attributes += 4;
                ty += NextAnimationValue() * k;
                _anim.Attributes += 4;
                _anim.Type = AnimRotation;
                break;
            case AnimNoTranslation:
                _anim.Attributes += 12;
                _anim.Type = AnimRotation;
                break;
        }
        if (_anim.Type == AnimRotation)
        {
            rx = NextAnimationValue();
            ry = NextAnimationValue();
            rz = NextAnimationValue();
        }
        return Gfx.RotXYZTrans(tx, ty, tz, rx, ry, rz);
    }

    private void DrawSiblings(uint first, NodeFrame frame, int depth)
    {
        uint c = first;
        for (int i = 0; i < 256 && _memory.IsPointer(c); i++)
        {
            DrawNode(c, frame, depth);
            c = _memory.U32(c + 0x08);
            if (c == first)
                break;
        }
    }

    private void Emit(uint dl, int layer, in NodeFrame frame)
    {
        if (!_memory.IsPointer(dl) && (dl >> 24) == 0)
            return;
        int slot = layer & 7;
        var incoming = _layerStates[slot];
        var key = (dl, layer, incoming);
        if (!_meshes.TryGetValue(key, out var entry))
        {
            var builder = new GfxMeshBuilder();
            var f3d = new F3DInterpreter(_memory, _scene.Textures, builder) { Ucode = _ucode };
            f3d.Load(incoming);
            f3d.Run(dl, layer, Matrix4x4.Identity);
            _meshes[key] = entry = new CachedMesh(builder.Build(), f3d.Save());
        }
        entry.LastUsed = _updates;
        _layerStates[slot] = entry.Output;
        if (entry.Mesh.TriangleCount > 0)
            _scene.Instances.Add(new GfxInstance(entry.Mesh, frame.Matrix, frame.Billboard, frame.Anchor, frame.AnchorScale, _emittingRealMario, _inCap, Ghost: _emittingGhost));
    }

    private void DrawNode(uint n, NodeFrame frame, int depth)
    {
        if (depth > 64 || --_nodeBudget < 0)
            return;
        int t = NodeType(n);
        ushort flags = _memory.U16(n + 2);
        bool active = IsMario && _capWings.Contains(n) ? _wingsShown : (flags & RenderActive) != 0;
        if (!active)
            return;
        int layer = _scheme.Layer((flags >> 8) & 0xFF);
        uint dl = 0;
        switch (t)
        {
            case GN_DL:
                dl = _memory.U32(n + 0x14);
                break;
            case GN_TRANSROT:
                frame.Matrix = Gfx.RotZXYTrans(_memory.S16(n + 0x18), _memory.S16(n + 0x1A), _memory.S16(n + 0x1C), _memory.S16(n + 0x1E), _memory.S16(n + 0x20), _memory.S16(n + 0x22)) * frame.Matrix;
                dl = _memory.U32(n + 0x14);
                break;
            case GN_TRANS:
                frame.Matrix = Gfx.RotZXYTrans(_memory.S16(n + 0x18), _memory.S16(n + 0x1A), _memory.S16(n + 0x1C), 0, 0, 0) * frame.Matrix;
                dl = _memory.U32(n + 0x14);
                break;
            case GN_ROT:
                frame.Matrix = Gfx.RotZXYTrans(0, 0, 0, _memory.S16(n + 0x18), _memory.S16(n + 0x1A), _memory.S16(n + 0x1C)) * frame.Matrix;
                dl = _memory.U32(n + 0x14);
                break;
            case GN_SCALE:
                frame.Matrix = Matrix4x4.CreateScale(_memory.F32(n + 0x18)) * frame.Matrix;
                dl = _memory.U32(n + 0x14);
                break;
            case GN_ANIMPART:
                frame.Matrix = AnimatedPart(n) * frame.Matrix;
                dl = _memory.U32(n + 0x14);
                break;
            case GN_BILLBOARD:
                var position = Vector3.Transform(new Vector3(_memory.S16(n + 0x18), _memory.S16(n + 0x1A), _memory.S16(n + 0x1C)), frame.Matrix);
                frame.Anchor = frame.Billboard ? frame.Anchor + position : position;
                frame.AnchorScale = Vector3.One;
                frame.Billboard = true;
                frame.Matrix = Matrix4x4.Identity;
                dl = _memory.U32(n + 0x14);
                break;
            case GN_SHADOW:
                AddShadow(n, frame);
                break;
            case GN_SWITCH:
                DrawSwitch(n, frame, depth);
                return;
            case GN_LOD:
                float distance = _scheme.EmulatorAutoLod ? 50
                    : _gameCameraForward != Vector3.Zero ? Vector3.Dot(frame.WorldPosition - _gameCamera, _gameCameraForward)
                    : (frame.WorldPosition - _gameCamera).Length();
                if (distance < 0)
                    distance = (frame.WorldPosition - _gameCamera).Length();
                if (!(_memory.S16(n + 0x14) <= distance && distance < _memory.S16(n + 0x16)))
                    return;
                break;
            case GN_HELD:
                if (IsMario && !_inHeldObject && !_emittingGhost)
                    DrawHeldObject(n, frame, depth);
                return;
            case GN_GENLIST:
                GeneratedNode(n, layer, frame);
                break;
            case GN_OBJECT:
            case GN_OBJPARENT:
            case GN_BG:
                return;
        }
        if (dl != 0)
            Emit(dl, layer, frame);
        uint children = _memory.U32(n + 0x10);
        if (_memory.IsPointer(children))
            DrawSiblings(children, frame, depth + 1);
    }

    private void DrawSwitch(uint n, NodeFrame frame, int depth)
    {
        int selected = _memory.S16(n + 0x1E);
        uint function = _memory.U32(n + 0x14);
        bool capSwitch = false;
        if (_addresses.GeoSwitchAnimState != 0 && function == _addresses.GeoSwitchAnimState)
        {
            selected = _memory.S32(_currentObject + 0xF0);
            if (selected >= _memory.S16(n + 0x1C) || selected < 0)
                selected = 0;
        }
        else if (_addresses.GeoSwitchMarioCapEffect != 0 && function == _addresses.GeoSwitchMarioCapEffect && IsMario)
            selected = _memory.U16(BodyState() + 8) >> 8;
        else if (IsCapSwitch(n, function))
        {
            uint bodyState = BodyState();
            int capState = _memory.IsPointer(bodyState) ? (sbyte)_memory.U8(bodyState + 4) : 0;
            _wingsShown = (capState & 2) != 0;
            for (uint s = _memory.U32(n + 0x08), i = 0; s != n && _memory.IsPointer(s) && i < 64; s = _memory.U32(s + 0x08), i++)
                if (NodeType(s) == GN_TRANSROT)
                    _capWings.Add(s);
            selected = capState & 1;
            capSwitch = true;
        }

        uint child = _memory.U32(n + 0x10);
        for (int i = 0; _memory.IsPointer(child) && selected > i && i < 256; i++)
            child = _memory.U32(child + 0x08);
        bool wasInCap = _inCap;
        _inCap |= capSwitch;
        if (_memory.IsPointer(child))
            DrawNode(child, frame, depth + 1);
        _inCap = wasInCap;
    }

    private uint BodyState()
    {
        if (_emittingGhost)
            return GhostBody;
        if (_ghostNodes.Contains(_currentObject))
            return 0x80460000 + (uint)((_memory.U8(_currentObject + 0x60) - 1) & 0xF) * 0x1000 + (_globalTimer & 0x7F) * 0x20 - 4;
        return _addresses.BodyStates != 0 ? _addresses.BodyStates
            : _addresses.MarioState != 0 ? _memory.U32(_addresses.MarioState + 0x98) : 0;
    }

    private uint _globalTimer;

    private bool IsCapSwitch(uint n, uint function)
    {
        if (!IsMario)
            return false;
        if (_addresses.GeoSwitchMarioCapOnOff != 0)
            return function == _addresses.GeoSwitchMarioCapOnOff;
        for (uint s = _memory.U32(n + 0x08), i = 0; s != n && _memory.IsPointer(s) && i < 64; s = _memory.U32(s + 0x08), i++)
            if (NodeType(s) == GN_TRANSROT)
                return true;
        return false;
    }

    private void GeneratedNode(uint n, int layer, in NodeFrame frame)
    {
        uint generator = _memory.U32(n + 0x14);
        if (generator == 0)
            return;
        if (IsMovtex(generator))
            EmitObjectMovtex(n, generator, layer, frame);
        else if (generator == _extras.GeoFlyingCarpetCreate)
            EmitFlyingCarpet(frame);
        else if (IsGhostHackCapGenerator(generator))
            EmitGhostHackCap(generator, layer, frame);
        else if (generator == _addresses.GeoMirrorMarioSetAlpha)
        {
            int modelState = _memory.U16(BodyState() + 8);
            int alpha = (modelState & 0x100) != 0 ? modelState & 0xFF : 0xFF;
            if (_currentObject != _marioObject && _ghostNodes.Contains(_currentObject))
                alpha = _memory.U8(_currentObject + 0x61) != 0 ? 0x7F : 0xFF;
            SetEnvAlpha(alpha == 0xFF ? LayerOpaque : LayerTransparent, alpha);
        }
        else if (generator == _addresses.GeoUpdateLayerTransparency)
        {
            int opacity = _memory.S32(_currentObject + 0x17C) & 0xFF;
            int target = _memory.U32(n + 0x18) == 20 ? 6 : opacity == 0xFF ? LayerOpaque : LayerTransparent;
            SetEnvAlpha(target, opacity);
        }
        else if (generator == _addresses.GeoBitsBowserColoring)
        {
            int slot = (_memory.S32(_currentObject + 0x17C) & 0xFF) == 0xFF ? LayerOpaque : LayerTransparent;
            if (_memory.S16(_currentObject + 0x1B2) != 0)
                DeriveState(slot, -1, f => f.Command(0xB6000000, F3DInterpreter.G_LIGHTING, slot, Matrix4x4.Identity));
        }
    }

    private readonly HashSet<uint> _ghostNodes = new();

    private void CollectGhostNodes()
    {
        _ghostNodes.Clear();
        uint first = ((uint)(InGameGhosts.InstalledBaseHi(_memory.U32) ?? InGameGhosts.DefaultBaseHi) << 16) + 0x7FF8;
        for (uint i = 0, slot = first; i < 40 && _memory.IsPointer(slot); i++, slot -= 0x68)
        {
            uint node = _memory.U32(slot);
            if (!_memory.IsPointer(node))
                break;
            _ghostNodes.Add(node);
        }
    }

    private bool IsGhostHackCapGenerator(uint generator)
        => (generator & 0xFFFF) == 0x8200 && (generator >> 16) is >= 0x8040 and <= 0x807F
        && _memory.U32(generator) == 0x27BDFFC0 && _memory.U32(generator + 8) == 0x3C090401;

    private void EmitGhostHackCap(uint generator, int layer, in NodeFrame frame)
    {
        int index = _currentObject == _marioObject ? 0 : (sbyte)_memory.U8(_currentObject + 0x60);
        uint lights = (generator & 0xFFFF0000) + 0x8300 + (uint)(index << 5);
        int variant = HashCode.Combine(lights, _memory.U32(lights), _memory.U32(lights + 8));
        void SetLights() => DeriveState(layer, variant, f =>
        {
            f.MoveMemLight(0x88, lights);
            f.MoveMemLight(0x86, lights + 8);
        });
        SetLights();
        Emit(0x040119A0, layer, frame);
        SetLights();
        Emit(0x04011978, layer, frame);
    }

    private void SetEnvAlpha(int slot, int alpha) => DeriveState(slot, alpha, f => f.SetEnvColor(255, 255, 255, (byte)alpha));

    private void DeriveState(int slot, int variant, Action<F3DInterpreter> change)
    {
        slot &= 7;
        var key = (_layerStates[slot], variant);
        if (!_derivedStates.TryGetValue(key, out var state))
        {
            var f3d = new F3DInterpreter(_memory, _scene.Textures, new GfxMeshBuilder()) { Ucode = _ucode };
            f3d.Load(_layerStates[slot]);
            change(f3d);
            _derivedStates[key] = state = f3d.Save();
        }
        _layerStates[slot] = state;
        _stateDerived = true;
    }

    private void AddShadow(uint n, in NodeFrame frame)
    {
        if (_emittingGhost)
            return;
        float size = _memory.S16(n + 0x14);
        byte solidity = _memory.U8(n + 0x16), type = _memory.U8(n + 0x17);
        Vector3 position;
        if (_inHeldObject)
            position = frame.WorldPosition;
        else
        {
            position = new Vector3(_memory.F32(_currentObject + 0x20), _memory.F32(_currentObject + 0x24), _memory.F32(_currentObject + 0x28));
            size *= _memory.F32(_currentObject + 0x2C);
        }
        float floor = IsMario && _addresses.MarioState != 0 ? _memory.F32(_addresses.MarioState + 0x70) : _memory.F32(_currentObject + 0xE8);
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
            _scene.Shadows.Add(new GfxShadow(new Vector3(position.X, floor, position.Z), size, opacity, type is >= 10 and <= 12));
    }

    private void DrawHeldObject(uint n, NodeFrame frame, int depth)
    {
        uint obj = _memory.U32(n + 0x1C);
        if (!_memory.IsPointer(obj))
            return;
        uint sharedChild = _memory.U32(obj + 0x14);
        if (!_memory.IsPointer(sharedChild))
            return;
        var holder = frame.ObjectMatrix;
        holder.Translation = frame.WorldPosition;
        var translation = Matrix4x4.CreateTranslation(_memory.S16(n + 0x20) / 4f, _memory.S16(n + 0x22) / 4f, _memory.S16(n + 0x24) / 4f);
        var scale = new Vector3(_memory.F32(obj + 0x2C), _memory.F32(obj + 0x30), _memory.F32(obj + 0x34));
        var held = new NodeFrame { Matrix = Gfx.ScaleRows(translation * holder, scale) };
        held.ObjectMatrix = held.Matrix;

        var holderAnimation = _anim;
        uint holderObject = _currentObject;
        bool wasHeld = _inHeldObject;
        _currentObject = obj;
        _inHeldObject = true;
        SetAnimation(obj);
        DrawSiblings(sharedChild, held, depth + 1);
        _anim = holderAnimation;
        _currentObject = holderObject;
        _inHeldObject = wasHeld;
    }

    internal readonly record struct Addresses(
        uint MarioObjectPointer,
        uint MarioState,
        uint SkyboxTextures,
        uint GeoSwitchAnimState,
        uint GeoSwitchMarioCapEffect,
        uint GeoSwitchMarioCapOnOff,
        uint GeoMirrorMarioSetAlpha,
        uint BodyStates,
        uint GeoUpdateLayerTransparency,
        uint GeoBitsBowserColoring,
        uint GeoRenderMirrorMario)
    {
        public static Addresses Resolve(GameFrame frame, Sm64Layout layout)
        {
            var s = Sm64Symbols.For(frame.Version);
            uint S(string name) => s?.Get(name) ?? 0;
            return new Addresses(
                layout.MarioObjectPointer ?? 0,
                layout.MarioStruct ?? 0,
                S("sSkyboxTextures"),
                S("geo_switch_anim_state"),
                S("geo_switch_mario_cap_effect"),
                S("geo_switch_mario_cap_on_off"),
                S("geo_mirror_mario_set_alpha"),
                S("gBodyStates"),
                S("geo_update_layer_transparency"),
                S("geo_bits_bowser_coloring"),
                S("geo_render_mirror_mario"));
        }
    }
}

internal sealed class GraphScheme
{
    public static readonly GraphScheme Vanilla = new(null, null, 0x20);

    private const int HackerRoot = 20, HackerMasterList = 2, MasterListHeader = 0x14;

    private static readonly int[] HackerTypes =
    [
        GameSceneBuilder.GN_ORTHO, GameSceneBuilder.GN_PERSPECTIVE, 0x004, GameSceneBuilder.GN_LOD, GameSceneBuilder.GN_SWITCH, GameSceneBuilder.GN_CAMERA,
        GameSceneBuilder.GN_TRANSROT, GameSceneBuilder.GN_TRANS, GameSceneBuilder.GN_ROT, GameSceneBuilder.GN_OBJECT, GameSceneBuilder.GN_ANIMPART,
        GameSceneBuilder.GN_BILLBOARD, GameSceneBuilder.GN_DL, GameSceneBuilder.GN_SCALE, GameSceneBuilder.GN_SHADOW, GameSceneBuilder.GN_OBJPARENT,
        GameSceneBuilder.GN_GENLIST, GameSceneBuilder.GN_BG, GameSceneBuilder.GN_HELD, 0x02F, GameSceneBuilder.GN_ROOT, 0x00A,
    ];

    private static readonly int[] HackerLayers = [0, 1, 3, 2, 4, 6, 5, 7];
    private static readonly int[] HackerSilhouetteLayers = [0, 1, 3, 2, 4, 4, 1, 4, 1, 4, 6, 5, 7];

    private readonly int[]? _types, _layers;

    private GraphScheme(int[]? types, int[]? layers, uint perspectiveNear)
    {
        _types = types;
        _layers = layers;
        PerspectiveNear = perspectiveNear;
    }

    public uint PerspectiveNear { get; }
    public bool EmulatorAutoLod => _types is not null;

    public int Type(int raw) => _types is null ? raw : raw >= 0 && raw < _types.Length ? _types[raw] : -1;
    public int Layer(int raw) => _layers is null ? raw : raw < _layers.Length ? _layers[raw] : 1;

    public static GraphScheme Detect(GfxMemory memory, uint root)
    {
        if (memory.U16(root) != HackerRoot)
            return Vanilla;
        int layerCount = 8;
        uint first = memory.U32(root + 0x10), c = first;
        for (int i = 0; i < 64 && memory.IsPointer(c); i++)
        {
            if (memory.U16(c) == HackerMasterList)
            {
                long size = (long)memory.U32(c + 0x10) - c;
                if (size == MasterListHeader + 8 * HackerSilhouetteLayers.Length)
                    layerCount = HackerSilhouetteLayers.Length;
                break;
            }
            c = memory.U32(c + 0x08);
            if (c == first)
                break;
        }
        return new GraphScheme(HackerTypes, layerCount == 8 ? HackerLayers : HackerSilhouetteLayers, 0x1C);
    }
}

public static class Gfx
{
    private static Matrix4x4 Affine(double m00, double m01, double m02, double m10, double m11, double m12, double m20, double m21, double m22, double tx, double ty, double tz) =>
        new((float)m00, (float)m01, (float)m02, 0, (float)m10, (float)m11, (float)m12, 0, (float)m20, (float)m21, (float)m22, 0, (float)tx, (float)ty, (float)tz, 1);

    public static Matrix4x4 RotZXYTrans(double tx, double ty, double tz, short rx, short ry, short rz)
    {
        double sx = Math.Sin(rx * Math.PI / 32768), cx = Math.Cos(rx * Math.PI / 32768);
        double sy = Math.Sin(ry * Math.PI / 32768), cy = Math.Cos(ry * Math.PI / 32768);
        double sz = Math.Sin(rz * Math.PI / 32768), cz = Math.Cos(rz * Math.PI / 32768);
        return Affine(cy * cz + sx * sy * sz, cx * sz, -sy * cz + sx * cy * sz,
            -cy * sz + sx * sy * cz, cx * cz, sy * sz + sx * cy * cz,
            cx * sy, -sx, cx * cy,
            tx, ty, tz);
    }

    public static Matrix4x4 RotXYZTrans(double tx, double ty, double tz, short rx, short ry, short rz)
    {
        double sx = Math.Sin(rx * Math.PI / 32768), cx = Math.Cos(rx * Math.PI / 32768);
        double sy = Math.Sin(ry * Math.PI / 32768), cy = Math.Cos(ry * Math.PI / 32768);
        double sz = Math.Sin(rz * Math.PI / 32768), cz = Math.Cos(rz * Math.PI / 32768);
        return Affine(cy * cz, cy * sz, -sy,
            sx * sy * cz - cx * sz, sx * sy * sz + cx * cz, sx * cy,
            cx * sy * cz + sx * sz, cx * sy * sz - sx * cz, cx * cy,
            tx, ty, tz);
    }

    public static Matrix4x4 ScaleRows(Matrix4x4 m, Vector3 s)
    {
        m.M11 *= s.X; m.M12 *= s.X; m.M13 *= s.X; m.M14 = 0;
        m.M21 *= s.Y; m.M22 *= s.Y; m.M23 *= s.Y; m.M24 = 0;
        m.M31 *= s.Z; m.M32 *= s.Z; m.M33 *= s.Z; m.M34 = 0;
        return m;
    }
}
