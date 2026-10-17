namespace StroopNext.Memory;

public static class N64
{
    public const int RamSize4MB = 0x400000;
    public const int RamSize8MB = 0x800000;

    public static uint ToOffset(uint address) => address & 0x1FFFFFFF;

    public static uint ToVirtual(uint offset) => offset | 0x80000000;

    public static int SwappedByteOffset(uint offset) => (int)(offset ^ 3);

    public static string Hex(uint address) => $"0x{address:X8}";
}
