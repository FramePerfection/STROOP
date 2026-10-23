using System.Buffers.Binary;

namespace StroopNext.TasEngine;

public static class Movie
{
    public static IReadOnlyList<int[]> Inputs(byte[] m64)
    {
        if (m64.Length < 0x400)
            throw new InvalidDataException("not a .m64 movie (too short)");
        int count = (int)Math.Min(BinaryPrimitives.ReadUInt32LittleEndian(m64.AsSpan(0x18)), (uint)((m64.Length - 0x400) / 4));
        var frames = new int[count][];
        for (int i = 0; i < count; i++)
        {
            int at = 0x400 + 4 * i;
            frames[i] = [(sbyte)m64[at + 2], (sbyte)m64[at + 3], m64[at], m64[at + 1]];
        }
        return frames;
    }

    public static IReadOnlyList<int[]> Inputs(string path) => Inputs(File.ReadAllBytes(path));
}
