using System.Numerics;
using StroopNext.Memory;

namespace StroopNext.Game;

public enum SurfaceClass { Floor, Wall, Ceiling }

public sealed record Surface(
    int Index,
    uint Address,
    bool IsStatic,
    short Type,
    byte Flags,
    sbyte Room,
    Vector3 V1,
    Vector3 V2,
    Vector3 V3,
    Vector3 Normal,
    float OriginOffset,
    uint Object)
{
    public const int Size = 0x30;

    public SurfaceClass Class => Classify(Normal.Y);

    public bool ProjectsOnX => (Flags & 0x08) != 0;

    public Vector2[] PointsXZ { get; } = [new(V1.X, V1.Z), new(V2.X, V2.Z), new(V3.X, V3.Z)];

    public float MinY => MathF.Min(V1.Y, MathF.Min(V2.Y, V3.Y));
    public float MaxY => MathF.Max(V1.Y, MathF.Max(V2.Y, V3.Y));

    public static SurfaceClass Classify(float normalY) => normalY > 0.01f ? SurfaceClass.Floor : normalY < -0.01f ? SurfaceClass.Ceiling : SurfaceClass.Wall;

    public float HeightAt(float x, float z) => -(x * Normal.X + Normal.Z * z + OriginOffset) / Normal.Y;

    public bool ContainsXZ(float x, float z)
    {
        float c1 = (V1.Z - z) * (V2.X - V1.X) - (V1.X - x) * (V2.Z - V1.Z);
        float c2 = (V2.Z - z) * (V3.X - V2.X) - (V2.X - x) * (V3.Z - V2.Z);
        float c3 = (V3.Z - z) * (V1.X - V3.X) - (V3.X - x) * (V1.Z - V3.Z);
        return (c1 >= 0 && c2 >= 0 && c3 >= 0) || (c1 <= 0 && c2 <= 0 && c3 <= 0);
    }

    public static Surface Read(RamImage ram, uint address, int index, bool isStatic)
    {
        Vector3 Vertex(uint o) => new(ram.S16(address + o), ram.S16(address + o + 2), ram.S16(address + o + 4));
        return new Surface(
            index,
            address,
            isStatic,
            ram.S16(address + 0x00),
            ram.U8(address + 0x04),
            ram.S8(address + 0x05),
            Vertex(0x0A),
            Vertex(0x10),
            Vertex(0x16),
            new Vector3(ram.F32(address + 0x1C), ram.F32(address + 0x20), ram.F32(address + 0x24)),
            ram.F32(address + 0x28),
            ram.U32(address + 0x2C));
    }
}

public sealed class SurfaceTable
{
    public static readonly SurfaceTable Empty = new(StaticSurfaces.Empty, []);

    private readonly StaticSurfaces _static;
    private readonly Surface[] _dynamic;
    private ConcatList? _all;

    internal SurfaceTable(StaticSurfaces statics, Surface[] dynamic)
    {
        _static = statics;
        _dynamic = dynamic;
    }

    public SurfaceTable(IReadOnlyList<Surface> all, int staticCount)
        : this(new StaticSurfaces(all.Take(staticCount).ToArray()), all.Skip(staticCount).ToArray()) { }

    public IReadOnlyList<Surface> Static => _static.Items;

    public IReadOnlyList<Surface> Dynamic => _dynamic;

    public object StaticVersion => _static;

    public int StaticCount => _static.Items.Length;

    public IReadOnlyList<Surface> All => _all ??= new ConcatList(_static.Items, _dynamic);

    public Surface? Find(uint address)
    {
        if (address == 0)
            return null;
        foreach (var s in _dynamic)
            if (s.Address == address)
                return s;
        return _static.Find(address);
    }

    private sealed class ConcatList(Surface[] first, Surface[] second) : IReadOnlyList<Surface>
    {
        public int Count => first.Length + second.Length;
        public Surface this[int index] => index < first.Length ? first[index] : second[index - first.Length];

        public IEnumerator<Surface> GetEnumerator()
        {
            foreach (var s in first)
                yield return s;
            foreach (var s in second)
                yield return s;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

internal sealed class StaticSurfaces(Surface[] items)
{
    public static readonly StaticSurfaces Empty = new([]);

    private Dictionary<uint, Surface>? _byAddress;

    public Surface[] Items { get; } = items;

    public Surface? Find(uint address)
    {
        var index = Volatile.Read(ref _byAddress);
        if (index is null)
        {
            index = new Dictionary<uint, Surface>(Items.Length);
            foreach (var s in Items)
                index[s.Address] = s;
            Volatile.Write(ref _byAddress, index);
        }
        return index.GetValueOrDefault(address);
    }
}

public sealed class SurfaceCache
{
    public const int MaxSurfaces = 0x4000;

    private readonly object _lock = new();
    private (uint Pool, int Count, int Hash) _staticKey;
    private StaticSurfaces _static = StaticSurfaces.Empty;

    public SurfaceTable Build(RamImage ram, Sm64Layout layout)
    {
        if (layout.SurfacePool is not uint poolPtr || layout.StaticSurfaceCount is not uint staticAddr || layout.TotalSurfaceCount is not uint totalAddr)
            return SurfaceTable.Empty;

        uint pool = ram.U32(poolPtr);
        int staticCount = ram.S32(staticAddr);
        int total = ram.S32(totalAddr);
        if (!ram.IsPointer(pool) || staticCount < 0 || total < staticCount || total > MaxSurfaces || !ram.IsValid(pool, total * Surface.Size))
            return SurfaceTable.Empty;

        StaticSurfaces statics;
        var key = (pool, staticCount, HashRange(ram, pool, staticCount * Surface.Size));
        lock (_lock)
        {
            if (key != _staticKey)
            {
                var parsed = new Surface[staticCount];
                for (int i = 0; i < staticCount; i++)
                    parsed[i] = Surface.Read(ram, pool + (uint)(i * Surface.Size), i, isStatic: true);
                _static = new StaticSurfaces(parsed);
                _staticKey = key;
            }
            statics = _static;
        }

        var dynamic = new Surface[total - staticCount];
        for (int i = 0; i < dynamic.Length; i++)
            dynamic[i] = Surface.Read(ram, pool + (uint)((staticCount + i) * Surface.Size), staticCount + i, isStatic: false);
        return new SurfaceTable(statics, dynamic);
    }

    private static int HashRange(RamImage ram, uint address, int length)
    {
        int offset = (int)N64.ToOffset(address);
        var hash = new HashCode();
        hash.AddBytes(ram.Raw.Slice(offset, length));
        return hash.ToHashCode();
    }
}
