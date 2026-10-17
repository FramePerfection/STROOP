using StroopNext.Memory;

namespace StroopNext.Platform;

internal static class RamBaseLocator
{
    public static (nint Base, string Method) Locate(IProcessMemory memory, EmulatorCandidate candidate)
    {
        if (FromSymbol(memory) is nint symbol && Verify(memory, symbol))
            return (symbol, "CORE_RDRAM symbol");
        if (OperatingSystem.IsWindows() && FromProfile(memory, candidate) is nint hinted && Verify(memory, hinted))
            return (hinted, "profile");
        if (Scan(memory) is nint scanned)
            return (scanned, "memory scan");
        throw new InvalidOperationException($"{candidate.ProcessName} is running but no SM64 RAM was found in it. Is a ROM loaded?");
    }

    private static bool Verify(IProcessMemory memory, nint address)
    {
        Span<byte> start = stackalloc byte[RdramSignature.Length];
        return address != 0 && memory.Read(address, start) && RdramSignature.Matches(start);
    }

    private static nint? FromSymbol(IProcessMemory memory)
    {
        if (memory.Symbol("CORE_RDRAM") is not nint symbolAddress)
            return null;
        Span<byte> pointer = stackalloc byte[memory.PointerSize == 4 ? 4 : 8];
        if (!memory.Read(symbolAddress, pointer))
            return null;
        return pointer.Length == 4 ? (nint)BitConverter.ToUInt32(pointer) : (nint)BitConverter.ToInt64(pointer);
    }

    private static nint? FromProfile(IProcessMemory memory, EmulatorCandidate candidate)
    {
        if (candidate.Profile is not { } profile)
            return null;
        if (profile.ModuleName is null)
            return (nint)profile.RamStart;
        return memory.ModuleBase(profile.ModuleName) is nint module ? module + (nint)profile.RamStart : null;
    }

    private static nint? Scan(IProcessMemory memory)
    {
        var matches = FindSignatures(memory, limit: 8);
        if (matches.Count <= 1)
            return matches.Count == 1 ? matches[0] : null;

        const int window = 0x10000, windowOffset = 0x32C000;
        var before = matches.Select(m => ReadWindow(memory, m + windowOffset, window)).ToList();
        Thread.Sleep(150);
        for (int i = 0; i < matches.Count; i++)
            if (before[i] is { } b && ReadWindow(memory, matches[i] + windowOffset, window) is { } after && !b.AsSpan().SequenceEqual(after))
                return matches[i];
        return matches[0];
    }

    private static byte[]? ReadWindow(IProcessMemory memory, nint address, int length)
    {
        var bytes = new byte[length];
        return memory.Read(address, bytes) ? bytes : null;
    }

    private static List<nint> FindSignatures(IProcessMemory memory, int limit)
    {
        var found = new List<nint>();
        const int chunk = 1 << 20;
        var buffer = new byte[chunk + RdramSignature.Length];
        foreach (var region in memory.Regions())
        {
            if (!region.Readable || region.Size < N64.RamSize4MB)
                continue;
            for (nint at = region.Base; at < region.End; at += chunk)
            {
                int length = (int)Math.Min(buffer.Length, region.End - at);
                if (length < RdramSignature.Length || !memory.Read(at, buffer.AsSpan(0, length)))
                    continue;
                foreach (int offset in RdramSignature.FindAll(buffer, length))
                {
                    if (offset >= chunk)
                        continue;
                    found.Add(at + offset);
                    if (found.Count >= limit)
                        return found;
                }
            }
        }
        return found;
    }
}
