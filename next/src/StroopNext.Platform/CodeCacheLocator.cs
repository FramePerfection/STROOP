using System.Buffers.Binary;

namespace StroopNext.Platform;

internal static class CodeCacheLocator
{
    public const int Pages = 0x100000;

    public static nint? Find(IProcessMemory memory, string processName)
    {
        if (memory.Symbol("invalid_code") is nint symbol && symbol != 0 && LooksLikeTable(memory, symbol))
            return symbol;
        return Scan(memory, processName);
    }

    public static (nint? Symbol, nint? Signature) BothWays(IProcessMemory memory, string processName)
        => (memory.Symbol("invalid_code") is nint s && s != 0 && LooksLikeTable(memory, s) ? s : null, Scan(memory, processName));

    private static bool LooksLikeTable(IProcessMemory memory, nint address)
    {
        var table = new byte[Pages];
        return memory.Read(address, table) && IsTable(table, 0);
    }

    internal static bool IsTable(ReadOnlySpan<byte> buffer, int at)
    {
        if (at < 0 || at + Pages > buffer.Length)
            return false;
        var table = buffer.Slice(at, Pages);
        if (table[..0x80000].ContainsAnyExcept((byte)1))
            return false;
        if (table.ContainsAnyExcept((byte)0, (byte)1))
            return false;
        return table[0x80200..0x80400].Count((byte)0) >= 8;
    }

    private static nint? Scan(IProcessMemory memory, string processName)
    {
        string module = processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? processName : processName + ".exe";
        if (memory.ModuleBase(module) is not nint image || image == 0)
            return null;
        Span<byte> header = stackalloc byte[0x400];
        if (!memory.Read(image, header))
            return null;
        int pe = BinaryPrimitives.ReadInt32LittleEndian(header[0x3C..]);
        if (pe <= 0 || pe + 0x58 > header.Length)
            return null;
        long size = BinaryPrimitives.ReadUInt32LittleEndian(header[(pe + 0x50)..]);
        if (size is < Pages or > 512L << 20)
            return null;

        nint? found = null;
        foreach (var region in memory.Regions())
        {
            if (!region.Readable || region.End <= image || region.Base >= image + (nint)size || region.Size < Pages)
                continue;
            var bytes = new byte[region.Size];
            if (!memory.Read(region.Base, bytes))
                continue;
            int run = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                run = bytes[i] == 1 ? run + 1 : 0;
                if (run != 0x80000)
                    continue;
                int start = i - 0x80000 + 1;
                if ((start == 0 || bytes[start - 1] != 1) && IsTable(bytes, start))
                {
                    if (found is not null)
                        return null;
                    found = region.Base + start;
                }
            }
        }
        return found;
    }
}
