using System.Numerics;
using System.Runtime.InteropServices;

namespace StroopNext.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct GfxVertex
{
    public const int Size = 36;

    public Vector3 Position;
    public Vector2 Uv;
    public uint Color;
    public uint Ambient;
    public uint Normal;
    public uint LightDirection;
}

public enum CullMode : byte { None, Back, Front }

public readonly record struct MaterialKey(int Texture, int Layer, bool Decal, CullMode Cull, int UvAnimation, int TexGen, ulong Fog)
{
    public int Phase => Layer <= 3 ? 0 : Layer == 4 ? 1 : 2;
}

public readonly record struct GfxBatch(MaterialKey Material, int First, int Count);

public sealed class GfxMesh
{
    private static int _nextId;

    public GfxMesh(GfxVertex[] vertices, GfxBatch[] batches)
    {
        Id = Interlocked.Increment(ref _nextId);
        Vertices = vertices;
        Batches = batches;
    }

    public static readonly GfxMesh Empty = new([], []);

    public bool Transient { get; init; }

    public int Id { get; }
    public GfxVertex[] Vertices { get; }
    public GfxBatch[] Batches { get; }
    public int TriangleCount => Vertices.Length / 3;
}

public sealed class GfxMeshBuilder
{
    private readonly Dictionary<MaterialKey, VertexList<GfxVertex>> _materials = new();

    public int TriangleCount { get; private set; }

    public VertexList<GfxVertex> For(in MaterialKey key)
    {
        if (!_materials.TryGetValue(key, out var list))
            _materials[key] = list = new VertexList<GfxVertex>();
        return list;
    }

    internal void CountTriangle() => TriangleCount++;

    public GfxMesh Build(bool transient = false)
    {
        if (TriangleCount == 0)
            return GfxMesh.Empty;
        var keys = _materials.Where(kv => kv.Value.Count > 0).Select(kv => kv.Key)
            .OrderBy(k => k.Phase).ThenBy(k => k.Layer).ThenBy(k => k.Texture).ToArray();
        var vertices = new GfxVertex[keys.Sum(k => _materials[k].Count)];
        var batches = new GfxBatch[keys.Length];
        int at = 0;
        for (int i = 0; i < keys.Length; i++)
        {
            var span = _materials[keys[i]].Span;
            span.CopyTo(vertices.AsSpan(at));
            batches[i] = new GfxBatch(keys[i], at, span.Length);
            at += span.Length;
        }
        return new GfxMesh(vertices, batches) { Transient = transient };
    }
}

public enum TextureWrap : byte { Repeat, MirroredRepeat, Clamp }

public sealed record DecodedTexture(int Id, int Width, int Height, byte[] Rgba, TextureWrap WrapS, TextureWrap WrapT);

public readonly record struct TextureKey(uint Address, int Format, int Size, int Width, int Height, uint Tlut, int Palette, int TlutType, int Cms, int Cmt);

public sealed class TextureLibrary
{
    private readonly Dictionary<TextureKey, DecodedTexture> _byKey = new();
    private readonly List<DecodedTexture> _all = new();

    public int Generation { get; private set; }
    public IReadOnlyList<DecodedTexture> All => _all;

    public int Get(GfxMemory memory, in TextureKey key)
    {
        if (_byKey.TryGetValue(key, out var texture))
            return texture.Id;
        texture = new DecodedTexture(_all.Count, key.Width, key.Height, TextureDecoder.Decode(memory, key), Wrap(key.Cms), Wrap(key.Cmt));
        _byKey[key] = texture;
        _all.Add(texture);
        return texture.Id;
    }

    public DecodedTexture? Find(int id) => id >= 0 && id < _all.Count ? _all[id] : null;

    public void Clear()
    {
        _byKey.Clear();
        _all.Clear();
        Generation++;
    }

    private static TextureWrap Wrap(int cm) => (cm & 1) != 0 ? TextureWrap.MirroredRepeat : (cm & 2) != 0 ? TextureWrap.Clamp : TextureWrap.Repeat;
}

public static class TextureDecoder
{
    public const int FormatRgba = 0, FormatYuv = 1, FormatCi = 2, FormatIa = 3, FormatI = 4;

    private static int C5(int v) => (v << 3) | (v >> 2);

    private static void Rgba16(int v, Span<byte> o)
    {
        o[0] = (byte)C5((v >> 11) & 31);
        o[1] = (byte)C5((v >> 6) & 31);
        o[2] = (byte)C5((v >> 1) & 31);
        o[3] = (byte)((v & 1) != 0 ? 255 : 0);
    }

    public static byte[] Decode(GfxMemory memory, in TextureKey key)
    {
        int bits = 4 << key.Size, w = key.Width, h = key.Height;
        uint src = memory.Phys(key.Address);
        bool hasTlut = key.Tlut != uint.MaxValue;
        uint tlut = hasTlut ? memory.Phys(key.Tlut) : uint.MaxValue;
        var output = new byte[w * h * 4];
        if (src == uint.MaxValue)
            return output;
        for (int i = 0; i < w * h; i++)
        {
            int v = bits switch
            {
                4 => (i & 1) == 0 ? memory.PhysU8(src + (uint)(i >> 1)) >> 4 : memory.PhysU8(src + (uint)(i >> 1)) & 15,
                8 => memory.PhysU8(src + (uint)i),
                16 => memory.PhysU16(src + (uint)(2 * i)),
                _ => (int)memory.PhysU32(src + (uint)(4 * i)),
            };
            var o = output.AsSpan(4 * i, 4);
            switch (key.Format)
            {
                case FormatRgba:
                    if (bits == 16)
                        Rgba16(v, o);
                    else
                    {
                        o[0] = (byte)(v >> 24);
                        o[1] = (byte)(v >> 16);
                        o[2] = (byte)(v >> 8);
                        o[3] = (byte)v;
                    }
                    break;
                case FormatCi:
                    int index = v + (bits == 4 ? key.Palette * 16 : 0);
                    if (!hasTlut)
                    {
                        o[0] = o[1] = o[2] = (byte)(key.Size == 0 ? index * 17 : index);
                        o[3] = 255;
                    }
                    else
                    {
                        int p = memory.PhysU16(tlut + (uint)(2 * index));
                        if (key.TlutType == 3)
                        {
                            o[0] = o[1] = o[2] = (byte)(p >> 8);
                            o[3] = (byte)p;
                        }
                        else
                            Rgba16(p, o);
                    }
                    break;
                case FormatIa:
                    if (bits == 4)
                    {
                        int i3 = (v >> 1) & 7;
                        o[0] = o[1] = o[2] = (byte)((i3 << 5) | (i3 << 2) | (i3 >> 1));
                        o[3] = (byte)((v & 1) != 0 ? 255 : 0);
                    }
                    else if (bits == 8)
                    {
                        o[0] = o[1] = o[2] = (byte)((v >> 4) * 17);
                        o[3] = (byte)((v & 15) * 17);
                    }
                    else
                    {
                        o[0] = o[1] = o[2] = (byte)(v >> 8);
                        o[3] = (byte)v;
                    }
                    break;
                default:
                    o[0] = o[1] = o[2] = o[3] = (byte)(bits == 4 ? v * 17 : v & 255);
                    break;
            }
        }
        return output;
    }
}
