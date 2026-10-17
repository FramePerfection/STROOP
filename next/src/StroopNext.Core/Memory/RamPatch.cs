using System.Buffers.Binary;

namespace StroopNext.Memory;

public sealed class RamPatch
{
    public readonly record struct Write(uint Address, byte[] Bytes);

    private readonly List<Write> _writes = new();

    public IReadOnlyList<Write> Writes => _writes;
    public bool IsEmpty => _writes.Count == 0;

    public RamPatch WriteBytes(uint address, ReadOnlySpan<byte> bigEndianBytes)
    {
        _writes.Add(new Write(address, bigEndianBytes.ToArray()));
        return this;
    }

    public RamPatch WriteU8(uint address, byte value) => WriteBytes(address, [value]);
    public RamPatch WriteS8(uint address, sbyte value) => WriteU8(address, (byte)value);

    public RamPatch WriteU16(uint address, ushort value)
    {
        Span<byte> b = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(b, value);
        return WriteBytes(address, b);
    }

    public RamPatch WriteS16(uint address, short value) => WriteU16(address, (ushort)value);

    public RamPatch WriteU32(uint address, uint value)
    {
        Span<byte> b = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(b, value);
        return WriteBytes(address, b);
    }

    public RamPatch WriteS32(uint address, int value) => WriteU32(address, (uint)value);
    public RamPatch WriteF32(uint address, float value) => WriteU32(address, (uint)BitConverter.SingleToInt32Bits(value));

    public RamPatch WriteF64(uint address, double value)
    {
        ulong bits = (ulong)BitConverter.DoubleToInt64Bits(value);
        WriteU32(address, (uint)(bits >> 32));
        return WriteU32(address + 4, (uint)bits);
    }

    public RamPatch Append(RamPatch other)
    {
        _writes.AddRange(other._writes);
        return this;
    }

    public IReadOnlyList<(int Offset, byte[] Bytes)> ToSwappedRuns()
    {
        var bytes = new SortedDictionary<int, byte>();
        foreach (var write in _writes)
            for (int i = 0; i < write.Bytes.Length; i++)
                bytes[N64.SwappedByteOffset(N64.ToOffset(write.Address + (uint)i))] = write.Bytes[i];

        var runs = new List<(int, byte[])>();
        var current = new List<byte>();
        int start = -1, previous = -2;
        foreach (var (offset, value) in bytes)
        {
            if (offset != previous + 1 && current.Count > 0)
            {
                runs.Add((start, current.ToArray()));
                current.Clear();
            }
            if (current.Count == 0)
                start = offset;
            current.Add(value);
            previous = offset;
        }
        if (current.Count > 0)
            runs.Add((start, current.ToArray()));
        return runs;
    }

    public void ApplyTo(Span<byte> swappedRam)
    {
        foreach (var (offset, data) in ToSwappedRuns())
            if (offset + data.Length <= swappedRam.Length)
                data.CopyTo(swappedRam[offset..]);
    }
}
