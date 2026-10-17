namespace StroopNext.Memory;

public static class RdramSignature
{
    public const int Length = 0x190;

    private static readonly byte?[] Vector = [null, 0x80, 0x1A, 0x3C, null, null, 0x5A, 0x27, 0x08, 0x00, 0x40, 0x03, 0x00, 0x00, 0x00, 0x00];

    public static bool Matches(ReadOnlySpan<byte> swappedRamStart)
    {
        if (swappedRamStart.Length < Length)
            return false;
        for (int vector = 0; vector < 4; vector++)
            if (!MatchesVector(swappedRamStart.Slice(vector * 0x80, Vector.Length)))
                return false;
        return true;
    }

    public static IEnumerable<int> FindAll(byte[] haystack, int length)
    {
        for (int i = 0; i + Length <= length; i += 4)
            if (haystack[i + 1] == 0x80 && haystack[i + 2] == 0x1A && haystack[i + 3] == 0x3C && Matches(haystack.AsSpan(i, Length)))
                yield return i;
    }

    private static bool MatchesVector(ReadOnlySpan<byte> bytes)
    {
        for (int i = 0; i < Vector.Length; i++)
            if (Vector[i] is byte expected && bytes[i] != expected)
                return false;
        return true;
    }
}
