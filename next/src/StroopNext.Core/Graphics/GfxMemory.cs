using StroopNext.Game;
using StroopNext.Memory;

namespace StroopNext.Graphics;

public sealed class GfxMemory
{
    private readonly RamImage _ram;
    private readonly uint[] _segments = new uint[32];

    public GfxMemory(RamImage ram, uint segmentTable)
    {
        _ram = ram;
        for (int i = 0; i < 32; i++)
            _segments[i] = ram.U32(segmentTable + (uint)(4 * i)) & 0x1FFFFFFF;
    }

    public GfxMemory(RamImage ram, ReadOnlySpan<uint> segments)
    {
        _ram = ram;
        segments.CopyTo(_segments);
    }

    public RamImage Ram => _ram;
    public uint Segment(int index) => _segments[index & 31];

    public const uint OverlayBase = 0x00800000;

    private byte[]? _overlay;

    public void SetOverlay(byte[]? overlay) => _overlay = overlay;

    private bool InOverlay(uint p) => _overlay is not null && p >= OverlayBase && p < OverlayBase + (uint)_overlay.Length;

    public uint Phys(uint address)
    {
        uint p = (address & 0x80000000) != 0
            ? address & 0x1FFFFFFF
            : _segments[(address >> 24) & 0x1F] + (address & 0xFFFFFF);
        return p < (uint)_ram.Length || InOverlay(p) ? p : uint.MaxValue;
    }

    public bool Ok(uint phys, int length) => phys != uint.MaxValue
        && ((ulong)phys + (ulong)length <= (ulong)_ram.Length || (InOverlay(phys) && InOverlay(phys + (uint)Math.Max(0, length - 1))));

    public bool IsPointer(uint value) => (value >= 0x80000000 && value < 0x80000000 + (uint)_ram.Length) || (value >= 0x80000000 && InOverlay(value & 0x1FFFFFFF));

    public byte U8(uint a) => PhysU8(Phys(a));
    public ushort U16(uint a) => PhysU16(Phys(a));
    public short S16(uint a) => (short)U16(a);
    public uint U32(uint a) => PhysU32(Phys(a));
    public int S32(uint a) => (int)U32(a);
    public float F32(uint a) => BitConverter.Int32BitsToSingle(S32(a));

    public byte PhysU8(uint p) => p == uint.MaxValue ? (byte)0
        : InOverlay(p) ? _overlay![p - OverlayBase] : _ram.U8(0x80000000 | p);

    public ushort PhysU16(uint p) => p == uint.MaxValue ? (ushort)0
        : InOverlay(p) ? (ushort)(PhysU8(p) << 8 | PhysU8(p + 1)) : _ram.U16(0x80000000 | p);

    public uint PhysU32(uint p) => p == uint.MaxValue ? 0
        : InOverlay(p) ? (uint)PhysU16(p) << 16 | PhysU16(p + 2) : _ram.U32(0x80000000 | p);
}

public sealed class Sm64Symbols
{
    private static readonly Lazy<Sm64Symbols?> Us = new(() => Load("MappingUS.map"));
    private static readonly Lazy<Sm64Symbols?> Jp = new(() => Load("MappingJP.map"));

    private readonly Dictionary<string, uint> _addresses;

    private Sm64Symbols(Dictionary<string, uint> addresses) => _addresses = addresses;

    public static Sm64Symbols? For(GameVersion? version) => version switch
    {
        GameVersion.US => Us.Value,
        GameVersion.JP => Jp.Value,
        _ => null,
    };

    public uint Get(string name) => _addresses.GetValueOrDefault(name);

    public int Count => _addresses.Count;

    private static Sm64Symbols? Load(string file)
    {
        using var stream = typeof(Sm64Symbols).Assembly.GetManifestResourceStream($"StroopNext.Data.{file}");
        if (stream is null)
            return null;
        var addresses = new Dictionary<string, uint>(16384);
        using var reader = new StreamReader(stream);
        while (reader.ReadLine() is { } line)
        {
            var span = line.AsSpan().Trim();
            int space = span.IndexOf(' ');
            if (space < 0 || !span.StartsWith("0x00000000"))
                continue;
            var name = span[space..].Trim();
            if (name.IndexOf(' ') >= 0 || name.Length == 0)
                continue;
            if (uint.TryParse(span[10..space], System.Globalization.NumberStyles.HexNumber, null, out uint address))
                addresses.TryAdd(name.ToString(), address);
        }
        return new Sm64Symbols(addresses);
    }
}
