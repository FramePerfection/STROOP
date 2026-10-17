using System.Buffers.Binary;

namespace StroopNext.Memory;

public sealed class RamImage
{
    private readonly byte[] _data;

    public RamImage(byte[] data, int length)
    {
        if ((uint)length > (uint)data.Length)
            throw new ArgumentOutOfRangeException(nameof(length));
        _data = data;
        Length = length;
    }

    public RamImage(byte[] data) : this(data, data.Length) { }

    public int Length { get; }

    public ReadOnlySpan<byte> Raw => _data.AsSpan(0, Length);

    public bool IsValid(uint address, int size = 1) => (ulong)N64.ToOffset(address) + (ulong)size <= (ulong)Length;

    public bool IsPointer(uint value) => (value & 0xFF000000) == 0x80000000 && IsValid(value, 4);

    public byte U8(uint address)
    {
        uint offset = N64.ToOffset(address);
        return offset < (uint)Length ? _data[N64.SwappedByteOffset(offset)] : (byte)0;
    }

    public sbyte S8(uint address) => (sbyte)U8(address);

    public ushort U16(uint address)
    {
        uint offset = N64.ToOffset(address);
        if ((offset & 1) != 0)
            return (ushort)((U8(address) << 8) | U8(address + 1));
        if (offset + 2 > (uint)Length)
            return 0;
        return BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan((int)(offset ^ 2), 2));
    }

    public short S16(uint address) => (short)U16(address);

    public uint U32(uint address)
    {
        uint offset = N64.ToOffset(address);
        if ((offset & 3) != 0)
            return ((uint)U16(address) << 16) | U16(address + 2);
        if (offset + 4 > (uint)Length)
            return 0;
        return BinaryPrimitives.ReadUInt32LittleEndian(_data.AsSpan((int)offset, 4));
    }

    public int S32(uint address) => (int)U32(address);

    public float F32(uint address) => BitConverter.Int32BitsToSingle(S32(address));

    public double F64(uint address) => BitConverter.Int64BitsToDouble((long)(((ulong)U32(address) << 32) | U32(address + 4)));

    public void ReadBytes(uint address, Span<byte> destination)
    {
        for (int i = 0; i < destination.Length; i++)
            destination[i] = U8(address + (uint)i);
    }
}
