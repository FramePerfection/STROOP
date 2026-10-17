using System.Numerics;
using StroopNext.Map3D;

namespace StroopNext.Graphics;

public enum Microcode : byte { F3D, F3DEX, F3DEX2 }

public record struct TileState
{
    public int Fmt, Siz, Tmem, Pal, Cms, Cmt, MaskS, MaskT, Uls, Ult, Lrs, Lrt;

    public static TileState Default => new() { Siz = 2 };
}

public readonly record struct GfxLight(byte R, byte G, byte B, sbyte Dx, sbyte Dy, sbyte Dz)
{
    public static readonly GfxLight Default = new(255, 255, 255, 0x28, 0x28, 0x28);
}

public sealed class F3DState : IEquatable<F3DState>
{
    internal uint Geo, Timg, Combine0, Combine1, FogColor, FogFactors, Prim, Env;
    internal bool TexOn;
    internal double TexScaleS, TexScaleT;
    internal TileState[] Tiles = new TileState[8];
    internal (int Tmem, uint Address)[] TmemSources = [];
    internal uint Tlut;
    internal int TlutType, NumLights;
    internal GfxLight[] Lights = new GfxLight[9];
    internal ushort LightMask;
    internal Microcode Ucode;
    private int _hash;

    public bool Equals(F3DState? o)
    {
        if (o is null)
            return false;
        if (ReferenceEquals(this, o))
            return true;
        if (GetHashCode() != o.GetHashCode())
            return false;
        return Geo == o.Geo && Timg == o.Timg && Combine0 == o.Combine0 && Combine1 == o.Combine1 && FogColor == o.FogColor
               && FogFactors == o.FogFactors && Prim == o.Prim && Env == o.Env && TexOn == o.TexOn && TexScaleS == o.TexScaleS
               && TexScaleT == o.TexScaleT && Tlut == o.Tlut && TlutType == o.TlutType && NumLights == o.NumLights
               && LightMask == o.LightMask && Ucode == o.Ucode && Tiles.AsSpan().SequenceEqual(o.Tiles)
               && TmemSources.AsSpan().SequenceEqual(o.TmemSources) && Lights.AsSpan().SequenceEqual(o.Lights);
    }

    public override bool Equals(object? obj) => Equals(obj as F3DState);

    public override int GetHashCode()
    {
        if (_hash != 0)
            return _hash;
        var h = new HashCode();
        h.Add(Geo);
        h.Add(Timg);
        h.Add(Combine0);
        h.Add(Combine1);
        h.Add(FogColor);
        h.Add(FogFactors);
        h.Add(Prim);
        h.Add(Env);
        h.Add(TexOn);
        h.Add(TexScaleS);
        h.Add(TexScaleT);
        foreach (var t in Tiles)
            h.Add(HashCode.Combine(t.Fmt, t.Siz, t.Tmem, t.Pal, HashCode.Combine(t.Cms, t.Cmt, t.MaskS, t.MaskT), HashCode.Combine(t.Uls, t.Ult, t.Lrs, t.Lrt)));
        foreach (var s in TmemSources)
            h.Add(s);
        h.Add(Tlut);
        h.Add(TlutType);
        h.Add(NumLights);
        h.Add(LightMask);
        foreach (var l in Lights)
            h.Add(l);
        h.Add(Ucode);
        return _hash = h.ToHashCode() | 1;
    }
}

public sealed class F3DInterpreter
{
    private const int G_MOVEMEM = 0x03, G_VTX = 0x04, G_DL = 0x06;
    private const int EX_G_BRANCH_Z = 0xB0, EX_G_TRI2 = 0xB1, EX_G_RDPHALF_1 = 0xB4, EX_G_QUAD = 0xB5;
    private const int EX2_G_VTX = 0x01, EX2_G_BRANCH_Z = 0x04, EX2_G_TRI1 = 0x05, EX2_G_TRI2 = 0x06, EX2_G_QUAD = 0x07;
    private const int EX2_G_TEXTURE = 0xD7, EX2_G_GEOMETRYMODE = 0xD9, EX2_G_MOVEWORD = 0xDB, EX2_G_MOVEMEM = 0xDC;
    private const int EX2_G_DL = 0xDE, EX2_G_ENDDL = 0xDF, EX2_G_RDPHALF_1 = 0xE1, EX2_G_SETOTHERMODE_H = 0xE3;
    private const int EX2_G_MV_LIGHT = 0x0A;
    private const uint EX2_G_CULL_FRONT = 0x200, EX2_G_CULL_BACK = 0x400, EX2_G_SHADING_SMOOTH = 0x200000;
    private const uint F3D_G_SHADING_SMOOTH = 0x200;
    private const uint SharedGeoBits = 0x1 | 0x4 | 0x10000 | 0x20000 | 0x40000 | 0x80000 | 0x100000;
    private const int G_CLEARGEOMETRYMODE = 0xB6, G_SETGEOMETRYMODE = 0xB7, G_ENDDL = 0xB8, G_SETOTHERMODE_H = 0xBA;
    private const int G_TEXTURE = 0xBB, G_MOVEWORD = 0xBC, G_TRI1 = 0xBF;
    private const int G_LOADTLUT = 0xF0, G_SETTILESIZE = 0xF2, G_LOADBLOCK = 0xF3, G_LOADTILE = 0xF4, G_SETTILE = 0xF5;
    private const int G_SETFOGCOLOR = 0xF8, G_SETPRIMCOLOR = 0xFA, G_SETENVCOLOR = 0xFB, G_SETCOMBINE = 0xFC, G_SETTIMG = 0xFD;
    private const uint G_TEXTURE_GEN = 0x40000, G_TEXTURE_GEN_LINEAR = 0x80000, G_FOG = 0x10000;
    public const uint G_LIGHTING = 0x20000;
    private const uint G_CULL_FRONT = 0x1000, G_CULL_BACK = 0x2000;
    private const int G_MW_NUMLIGHT = 0x02, G_MW_FOG = 0x08, G_MW_LIGHTCOL = 0x0A;

    public const uint GeoInit = 0x4 | 0x200 | G_CULL_BACK | G_LIGHTING;
    public const uint CombineShadeW1 = 4 << 15;
    public const uint NoTlut = uint.MaxValue;

    private struct Vtx
    {
        public Vector3 Position;
        public short S, T;
        public uint Rgba;
        public bool Lit, Valid;
        public sbyte Nx, Ny, Nz, Lx, Ly, Lz;
        public byte Ar, Ag, Ab;
    }

    private readonly GfxMemory _memory;
    private readonly TextureLibrary _textures;
    private GfxMeshBuilder _output;

    public Microcode Ucode = Microcode.F3D;
    public uint Geo = GeoInit;
    public int UvAnimation = -1;
    private uint _rdpHalf1;
    private bool _texOn;
    private double _texScaleS = 1, _texScaleT = 1;
    private readonly TileState[] _tiles = new TileState[8];
    private uint _timg;
    private readonly List<(int Tmem, uint Address)> _tmem = new();
    private uint _tlut = NoTlut;
    private int _tlutType;
    private uint _prim = 0xFFFFFFFF, _env = 0xFFFFFFFF;
    public uint Combine0, Combine1;
    private uint _fogColor, _fogFactors;
    private readonly GfxLight[] _lights = new GfxLight[9];
    private ushort _lightMask;
    private int _numLights = 1;
    private readonly Vtx[] _vbuf = new Vtx[64];
    private readonly byte[] _raw = new byte[16];

    public F3DInterpreter(GfxMemory memory, TextureLibrary textures, GfxMeshBuilder output)
    {
        _memory = memory;
        _textures = textures;
        _output = output;
        for (int i = 0; i < _tiles.Length; i++)
            _tiles[i] = TileState.Default;
    }

    public GfxMeshBuilder Output
    {
        get => _output;
        set => _output = value;
    }


    public F3DState Save() => new()
    {
        Geo = Geo, Timg = _timg, Combine0 = Combine0, Combine1 = Combine1, FogColor = _fogColor, FogFactors = _fogFactors,
        Prim = _prim, Env = _env, TexOn = _texOn, TexScaleS = _texScaleS, TexScaleT = _texScaleT, Tiles = (TileState[])_tiles.Clone(),
        TmemSources = _tmem.OrderBy(t => t.Tmem).ToArray(), Tlut = _tlut, TlutType = _tlutType, NumLights = _numLights,
        Lights = (GfxLight[])_lights.Clone(), LightMask = _lightMask, Ucode = Ucode,
    };

    public void Load(F3DState s)
    {
        Geo = s.Geo;
        _timg = s.Timg;
        Combine0 = s.Combine0;
        Combine1 = s.Combine1;
        _fogColor = s.FogColor;
        _fogFactors = s.FogFactors;
        _prim = s.Prim;
        _env = s.Env;
        _texOn = s.TexOn;
        _texScaleS = s.TexScaleS;
        _texScaleT = s.TexScaleT;
        s.Tiles.CopyTo(_tiles, 0);
        _tmem.Clear();
        _tmem.AddRange(s.TmemSources);
        _tlut = s.Tlut;
        _tlutType = s.TlutType;
        _numLights = s.NumLights;
        s.Lights.CopyTo(_lights, 0);
        _lightMask = s.LightMask;
        Ucode = s.Ucode;
    }

    public void SetEnvColor(byte r, byte g, byte b, byte a) => _env = Pack(r, g, b, a);

    private static uint Pack(byte r, byte g, byte b, byte a) => r | ((uint)g << 8) | ((uint)b << 16) | ((uint)a << 24);

    private static uint FromRdp(uint w1) => Pack((byte)(w1 >> 24), (byte)(w1 >> 16), (byte)(w1 >> 8), (byte)w1);


    private void Combiner(out int a0, out int b0, out int c0, out int d0)
    {
        a0 = (int)(Combine0 >> 20) & 15;
        c0 = (int)(Combine0 >> 15) & 31;
        b0 = (int)(Combine1 >> 28) & 15;
        d0 = (int)(Combine1 >> 15) & 7;
    }

    private int ColorSource(out bool usesTexture)
    {
        Combiner(out int a, out int b, out int c, out int d);
        usesTexture = a == 1 || b == 1 || c == 1 || d == 1;
        if (a == 4 || b == 4 || c == 4 || d == 4) return 1;
        if (a == 3 || b == 3 || c == 3 || d == 3) return 2;
        if (a == 5 || b == 5 || c == 5 || d == 5) return 3;
        return 0;
    }

    private byte AlphaSource(byte shadeAlpha)
    {
        int a = (int)(Combine0 >> 12) & 7, c = (int)(Combine0 >> 9) & 7, b = (int)(Combine1 >> 12) & 7, d = (int)(Combine1 >> 9) & 7;
        if (a == 5 || b == 5 || c == 5 || d == 5) return (byte)(_env >> 24);
        if (a == 3 || b == 3 || c == 3 || d == 3) return (byte)(_prim >> 24);
        if (a == 4 || b == 4 || c == 4 || d == 4) return shadeAlpha;
        return 255;
    }

    private bool Decal()
    {
        Combiner(out int a, out int b, out int c, out int d);
        return a == 1 && b == 4 && c == 8 && d == 4;
    }


    private GfxLight LightOrDefault(int index) => (_lightMask & (1 << index)) != 0 ? _lights[index] : GfxLight.Default;

    private void Shade(ref Vtx v, in Matrix4x4 m)
    {
        if ((Geo & G_LIGHTING) == 0)
        {
            v.Rgba = Pack(_raw[12], _raw[13], _raw[14], _raw[15]);
            return;
        }

        var n = Vector3.TransformNormal(new Vector3((sbyte)_raw[12], (sbyte)_raw[13], (sbyte)_raw[14]), m);
        if (n.LengthSquared() > 0)
            n = Vector3.Normalize(n) * 127;
        v.Lit = true;
        v.Nx = (sbyte)Math.Round(n.X);
        v.Ny = (sbyte)Math.Round(n.Y);
        v.Nz = (sbyte)Math.Round(n.Z);

        double ar = 128, ag = 128, ab = 128;
        if ((_lightMask & (1 << (_numLights + 1))) != 0 && _numLights + 1 < _lights.Length)
        {
            var ambient = _lights[_numLights + 1];
            (ar, ag, ab) = (ambient.R, ambient.G, ambient.B);
        }
        var first = LightOrDefault(1);
        v.Rgba = Pack(first.R, first.G, first.B, _raw[15]);
        (v.Lx, v.Ly, v.Lz) = (first.Dx, first.Dy, first.Dz);

        for (int i = 2; i <= _numLights && i < _lights.Length; i++)
        {
            var l = LightOrDefault(i);
            double length = Math.Sqrt(l.Dx * l.Dx + l.Dy * l.Dy + l.Dz * l.Dz);
            if (length == 0)
                length = 1;
            double k = Math.Max(0, ((sbyte)_raw[12] * l.Dx + (sbyte)_raw[13] * l.Dy + (sbyte)_raw[14] * l.Dz) / (127.0 * length));
            ar += l.R * k;
            ag += l.G * k;
            ab += l.B * k;
        }
        v.Ar = (byte)Math.Min(255, (int)ar);
        v.Ag = (byte)Math.Min(255, (int)ag);
        v.Ab = (byte)Math.Min(255, (int)ab);
    }

    private void LoadVertices(uint address, int count, int first, in Matrix4x4 m)
    {
        uint p = _memory.Phys(address);
        for (int i = 0; i < count; i++, p += 16)
        {
            if (!_memory.Ok(p, 16))
                return;
            for (uint k = 0; k < 16; k++)
                _raw[k] = _memory.PhysU8(p + k);
            StoreVertex(first + i, m);
        }
    }

    public void LoadVertices(ReadOnlySpan<byte> vertices, int count, int first, in Matrix4x4 m)
    {
        for (int i = 0; i < count && (i + 1) * 16 <= vertices.Length; i++)
        {
            vertices.Slice(i * 16, 16).CopyTo(_raw);
            StoreVertex(first + i, m);
        }
    }

    private void StoreVertex(int index, in Matrix4x4 m)
    {
        if (index < 0 || index >= _vbuf.Length)
            return;
        var local = new Vector3((short)(_raw[0] << 8 | _raw[1]), (short)(_raw[2] << 8 | _raw[3]), (short)(_raw[4] << 8 | _raw[5]));
        var v = new Vtx
        {
            Position = Vector3.Transform(local, m),
            S = (short)(_raw[8] << 8 | _raw[9]),
            T = (short)(_raw[10] << 8 | _raw[11]),
            Valid = true,
        };
        Shade(ref v, m);
        _vbuf[index] = v;
    }


    private int CurrentTexture(bool usesTexture, out int w, out int h, out int uls, out int ult)
    {
        ref var t = ref _tiles[0];
        w = ((t.Lrs - t.Uls) >> 2) + 1;
        h = ((t.Lrt - t.Ult) >> 2) + 1;
        uls = t.Uls;
        ult = t.Ult;
        if (!(_texOn && usesTexture) || !(w is > 0 and <= 256 && h is > 0 and <= 256))
            return -1;
        uint address = 0;
        bool found = false;
        foreach (var (tmem, source) in _tmem)
            if (tmem == t.Tmem)
            {
                address = source;
                found = true;
            }
        if (!found)
            return -1;
        var key = new TextureKey(address, t.Fmt, t.Siz, w, h, t.Fmt == TextureDecoder.FormatCi ? _tlut : NoTlut,
            t.Siz == 0 ? t.Pal : 0, _tlutType, t.Cms | (t.MaskS == 0 ? 2 : 0), t.Cmt | (t.MaskT == 0 ? 2 : 0));
        return _textures.Get(_memory, key);
    }

    public Vector2 TexCoordScale()
    {
        ref var t = ref _tiles[0];
        int w = ((t.Lrs - t.Uls) >> 2) + 1, h = ((t.Lrt - t.Ult) >> 2) + 1;
        return w > 0 && h > 0 ? new Vector2((float)(_texScaleS / (32.0 * w)), (float)(_texScaleT / (32.0 * h))) : Vector2.Zero;
    }

    private void Triangle(int i0, int i1, int i2, int layer)
    {
        if ((uint)i0 >= _vbuf.Length || (uint)i1 >= _vbuf.Length || (uint)i2 >= _vbuf.Length || !_vbuf[i0].Valid || !_vbuf[i1].Valid || !_vbuf[i2].Valid)
            return;
        var cull = (Geo & G_CULL_BACK) != 0 ? CullMode.Back : CullMode.None;
        if ((Geo & G_CULL_FRONT) != 0)
        {
            if (cull == CullMode.Back)
                return;
            cull = CullMode.Front;
        }

        int source = ColorSource(out bool usesTexture);
        int texture = CurrentTexture(usesTexture, out int w, out int h, out int uls, out int ult);
        bool decal = texture >= 0 && Decal();
        int texGen = texture >= 0 && (Geo & G_TEXTURE_GEN) != 0 ? (Geo & G_TEXTURE_GEN_LINEAR) != 0 ? 2 : 1 : 0;
        ulong fog = (Geo & G_FOG) != 0 ? (ulong)_fogColor << 32 | _fogFactors : 0;
        var list = _output.For(new MaterialKey(texture, layer, decal, cull, UvAnimation, texGen, fog));

        float fu = 0, fv = 0, ou = 0, ov = 0;
        if (texture >= 0)
        {
            fu = (float)(_texScaleS / (32.0 * w));
            fv = (float)(_texScaleT / (32.0 * h));
            ou = uls / (4.0f * w);
            ov = ult / (4.0f * h);
        }
        list.Add(Vertex(_vbuf[i0], texture, fu, fv, ou, ov, source, texGen != 0));
        list.Add(Vertex(_vbuf[i1], texture, fu, fv, ou, ov, source, texGen != 0));
        list.Add(Vertex(_vbuf[i2], texture, fu, fv, ou, ov, source, texGen != 0));
        _output.CountTriangle();
    }

    private static uint S8x3(sbyte x, sbyte y, sbyte z) => (byte)x | ((uint)(byte)y << 8) | ((uint)(byte)z << 16);

    private GfxVertex Vertex(in Vtx v, int texture, float fu, float fv, float ou, float ov, int source, bool texGen)
    {
        uint color = source switch
        {
            1 => v.Rgba,
            2 => _prim,
            3 => _env,
            _ => 0x00FFFFFF | (v.Rgba & 0xFF000000),
        };
        color = (color & 0x00FFFFFF) | ((uint)AlphaSource((byte)(v.Rgba >> 24)) << 24);
        bool lit = source == 1 && v.Lit, normal = lit || (texGen && v.Lit);
        return new GfxVertex
        {
            Position = v.Position,
            Uv = texture < 0 ? default : texGen ? new Vector2(32768 * fu, 32768 * fv) : new Vector2(v.S * fu - ou, v.T * fv - ov),
            Color = color,
            Ambient = lit ? Pack(v.Ar, v.Ag, v.Ab, 255) : 0,
            Normal = normal ? S8x3(v.Nx, v.Ny, v.Nz) : 0,
            LightDirection = lit ? S8x3(v.Lx, v.Ly, v.Lz) : 0,
        };
    }


    public void MoveMemLight(int index, uint address)
    {
        if (index < 0x86 || index > 0x94 || index % 2 != 0)
            return;
        ReadLight((index - 0x86) / 2 + 1, address);
    }

    private void ReadLight(int n, uint address)
    {
        uint p = _memory.Phys(address);
        if (n < 0 || n >= _lights.Length || !_memory.Ok(p, 16))
            return;
        _lights[n] = new GfxLight(_memory.PhysU8(p), _memory.PhysU8(p + 1), _memory.PhysU8(p + 2),
            (sbyte)_memory.PhysU8(p + 8), (sbyte)_memory.PhysU8(p + 9), (sbyte)_memory.PhysU8(p + 10));
        _lightMask |= (ushort)(1 << n);
    }


    public const int CommandBudget = 200_000;
    private int _budget;

    public void Run(uint displayList, int layer, in Matrix4x4 m, int depth = 0)
    {
        if (depth == 0)
            _budget = CommandBudget;
        uint pc = displayList;
        while (--_budget >= 0)
        {
            uint p = _memory.Phys(pc);
            if (!_memory.Ok(p, 8))
                return;
            uint w0 = _memory.PhysU32(p), w1 = _memory.PhysU32(p + 4);
            pc += 8;
            if (!Command(w0, w1, Ucode, layer, m, depth))
                return;
        }
    }

    public bool Command(uint w0, uint w1, int layer, in Matrix4x4 m, int depth = 0) => CommandF3D(w0, w1, false, layer, m, depth);

    public bool Command(uint w0, uint w1, Microcode ucode, int layer, in Matrix4x4 m, int depth = 0) => ucode switch
    {
        Microcode.F3DEX2 => CommandF3DEX2(w0, w1, layer, m, depth),
        Microcode.F3DEX => CommandF3D(w0, w1, true, layer, m, depth),
        _ => CommandF3D(w0, w1, false, layer, m, depth),
    };

    private bool Branch(uint target, uint w0, int layer, in Matrix4x4 m, int depth)
    {
        if (depth < 16)
            Run(target, layer, m, depth + 1);
        return ((w0 >> 16) & 0xFF) == 0;
    }

    private void Triangle2x(uint w, int layer) => Triangle((int)((w >> 16) & 0xFF) / 2, (int)((w >> 8) & 0xFF) / 2, (int)(w & 0xFF) / 2, layer);

    private void OtherModeH(int shift, int length, uint w1)
    {
        if (shift <= 14 && 14 < shift + length)
            _tlutType = (int)(w1 >> 14) & 3;
    }

    private static uint GeoEx2ToF3D(uint bits)
    {
        uint r = bits & SharedGeoBits;
        if ((bits & EX2_G_CULL_FRONT) != 0) r |= G_CULL_FRONT;
        if ((bits & EX2_G_CULL_BACK) != 0) r |= G_CULL_BACK;
        if ((bits & EX2_G_SHADING_SMOOTH) != 0) r |= F3D_G_SHADING_SMOOTH;
        return r;
    }

    private void SetTextureScale(uint w1)
    {
        uint s = (w1 >> 16) & 0xFFFF, t = w1 & 0xFFFF;
        _texScaleS = s == 0xFFFF ? 1.0 : s / 65536.0;
        _texScaleT = t == 0xFFFF ? 1.0 : t / 65536.0;
    }

    private bool CommandF3DEX2(uint w0, uint w1, int layer, in Matrix4x4 m, int depth)
    {
        int op = (int)(w0 >> 24);
        if (op >= 0xE4)
        {
            Rdp(op, w0, w1);
            return true;
        }
        switch (op)
        {
            case EX2_G_ENDDL:
                return false;
            case EX2_G_DL:
                return Branch(w1, w0, layer, m, depth);
            case EX2_G_BRANCH_Z:
                if (depth < 16)
                    Run(_rdpHalf1, layer, m, depth + 1);
                return false;
            case EX2_G_RDPHALF_1:
                _rdpHalf1 = w1;
                break;
            case EX2_G_VTX:
                int n = (int)(w0 >> 12) & 0xFF;
                LoadVertices(w1, n, (int)((w0 >> 1) & 0x7F) - n, m);
                break;
            case EX2_G_TRI1:
                Triangle2x(w0, layer);
                break;
            case EX2_G_TRI2:
            case EX2_G_QUAD:
                Triangle2x(w0, layer);
                Triangle2x(w1, layer);
                break;
            case EX2_G_GEOMETRYMODE:
                Geo = (Geo & ~GeoEx2ToF3D(~w0 & 0xFFFFFF)) | GeoEx2ToF3D(w1);
                break;
            case EX2_G_TEXTURE:
                _texOn = ((w0 >> 1) & 0x7F) != 0;
                SetTextureScale(w1);
                break;
            case EX2_G_MOVEMEM:
                if ((w0 & 0xFF) == EX2_G_MV_LIGHT)
                {
                    int offset = (int)((w0 >> 8) & 0xFF) * 8;
                    if (offset % 0x18 == 0 && offset >= 0x30)
                        ReadLight(offset / 0x18 - 1, w1);
                }
                break;
            case EX2_G_MOVEWORD:
                int index = (int)(w0 >> 16) & 0xFF;
                if (index == G_MW_NUMLIGHT)
                    _numLights = (int)Math.Clamp(w1 / 24, 1, 7);
                else if (index == G_MW_FOG)
                    _fogFactors = w1;
                else if (index == G_MW_LIGHTCOL && (w0 & 0xFFFF) % 0x18 == 0)
                {
                    int light = (int)(w0 & 0xFFFF) / 0x18 + 1;
                    if (light < _lights.Length)
                    {
                        var l = LightOrDefault(light);
                        _lights[light] = l with { R = (byte)(w1 >> 24), G = (byte)(w1 >> 16), B = (byte)(w1 >> 8) };
                        _lightMask |= (ushort)(1 << light);
                    }
                }
                break;
            case EX2_G_SETOTHERMODE_H:
                int length = (int)(w0 & 0xFF) + 1;
                OtherModeH(32 - (int)((w0 >> 8) & 0xFF) - length, length, w1);
                break;
        }
        return true;
    }

    private bool CommandF3D(uint w0, uint w1, bool ex, int layer, in Matrix4x4 m, int depth)
    {
        switch ((int)(w0 >> 24))
        {
            case G_ENDDL:
                return false;
            case G_DL:
                return Branch(w1, w0, layer, m, depth);
            case G_VTX:
                if (ex)
                    LoadVertices(w1, (int)(w0 >> 10) & 0x3F, (int)((w0 >> 16) & 0xFF) / 2, m);
                else
                    LoadVertices(w1, (int)((w0 >> 20) & 15) + 1, (int)(w0 >> 16) & 15, m);
                break;
            case G_TRI1:
                if (ex)
                    Triangle2x(w1, layer);
                else
                    Triangle((int)((w1 >> 16) & 0xFF) / 10, (int)((w1 >> 8) & 0xFF) / 10, (int)(w1 & 0xFF) / 10, layer);
                break;
            case EX_G_TRI2 when ex:
            case EX_G_QUAD when ex:
                Triangle2x(w0, layer);
                Triangle2x(w1, layer);
                break;
            case EX_G_RDPHALF_1 when ex:
                _rdpHalf1 = w1;
                break;
            case EX_G_BRANCH_Z when ex:
                if (depth < 16)
                    Run(_rdpHalf1, layer, m, depth + 1);
                return false;
            case G_SETGEOMETRYMODE:
                Geo |= w1;
                break;
            case G_CLEARGEOMETRYMODE:
                Geo &= ~w1;
                break;
            case G_TEXTURE:
                _texOn = (w0 & 0xFF) != 0;
                SetTextureScale(w1);
                break;
            case G_MOVEMEM:
                MoveMemLight((int)(w0 >> 16) & 0xFF, w1);
                break;
            case G_MOVEWORD:
                if ((w0 & 0xFF) == G_MW_NUMLIGHT)
                    _numLights = (int)Math.Clamp((((long)w1 - 0x80000000L) >> 5) - 1, 1, 7);
                else if ((w0 & 0xFF) == G_MW_FOG)
                    _fogFactors = w1;
                break;
            case G_SETOTHERMODE_H:
                OtherModeH((int)(w0 >> 8) & 0xFF, (int)w0 & 0xFF, w1);
                break;
            default:
                Rdp((int)(w0 >> 24), w0, w1);
                break;
        }
        return true;
    }

    private void Rdp(int op, uint w0, uint w1)
    {
        switch (op)
        {
            case G_SETFOGCOLOR:
                _fogColor = w1;
                break;
            case G_SETTIMG:
                _timg = w1;
                break;
            case G_SETTILE:
                ref var tile = ref _tiles[(w1 >> 24) & 7];
                tile.Fmt = (int)(w0 >> 21) & 7;
                tile.Siz = (int)(w0 >> 19) & 3;
                tile.Tmem = (int)w0 & 0x1FF;
                tile.Pal = (int)(w1 >> 20) & 15;
                tile.Cmt = (int)(w1 >> 18) & 3;
                tile.Cms = (int)(w1 >> 8) & 3;
                tile.MaskT = (int)(w1 >> 14) & 15;
                tile.MaskS = (int)(w1 >> 4) & 15;
                break;
            case G_SETTILESIZE:
                ref var sized = ref _tiles[(w1 >> 24) & 7];
                sized.Uls = (int)(w0 >> 12) & 0xFFF;
                sized.Ult = (int)w0 & 0xFFF;
                sized.Lrs = (int)(w1 >> 12) & 0xFFF;
                sized.Lrt = (int)w1 & 0xFFF;
                break;
            case G_LOADBLOCK:
            case G_LOADTILE:
                int target = _tiles[(w1 >> 24) & 7].Tmem;
                _tmem.RemoveAll(t => t.Tmem == target);
                _tmem.Add((target, _timg));
                break;
            case G_LOADTLUT:
                _tlut = _timg;
                break;
            case G_SETPRIMCOLOR:
                _prim = FromRdp(w1);
                break;
            case G_SETENVCOLOR:
                _env = FromRdp(w1);
                break;
            case G_SETCOMBINE:
                Combine0 = w0 & 0xFFFFFF;
                Combine1 = w1;
                break;
        }
    }

    public static Microcode Detect(GfxMemory memory, IEnumerable<uint> displayLists, int maxCommands = 512)
    {
        int f3d = 0, f3dex = 0, f3dex2 = 0;
        foreach (uint dl in displayLists)
        {
            if (dl == 0)
                continue;
            bool ex = false;
            uint pc = dl;
            for (int i = 0; i < maxCommands; i++, pc += 8)
            {
                uint p = memory.Phys(pc);
                if (!memory.Ok(p, 8))
                    break;
                uint w0 = memory.PhysU32(p);
                int op = (int)(w0 >> 24);
                if (w0 == 0xDF000000)
                {
                    f3dex2++;
                    break;
                }
                if (w0 == 0xB8000000)
                {
                    if (ex) f3dex++;
                    else f3d++;
                    break;
                }
                if (op == G_VTX && (w0 & 0xF) == 0xF && ((w0 >> 10) & 0x3F) * 16 - 1 == (w0 & 0x3FF))
                    ex = true;
                else if (op == EX_G_TRI2)
                    ex = true;
            }
        }
        if (f3dex2 > f3d && f3dex2 > f3dex)
            return Microcode.F3DEX2;
        return f3dex > f3d ? Microcode.F3DEX : Microcode.F3D;
    }
}
