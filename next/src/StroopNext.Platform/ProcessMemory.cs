namespace StroopNext.Platform;

internal readonly record struct MemoryRegion(nint Base, nint Size, bool Readable)
{
    public nint End => Base + Size;
}

internal interface IProcessMemory : IDisposable
{
    bool IsWritable { get; }

    bool IsAlive { get; }

    int PointerSize { get; }

    bool Read(nint address, Span<byte> buffer);

    bool Write(nint address, ReadOnlySpan<byte> buffer);

    IEnumerable<MemoryRegion> Regions();

    nint? ModuleBase(string moduleName);

    nint? Symbol(string name);
}

internal static class ProcessMemory
{
    public static IProcessMemory Open(int pid)
    {
        if (OperatingSystem.IsWindows())
            return WindowsProcessMemory.Open(pid);
        if (OperatingSystem.IsLinux())
            return LinuxProcessMemory.Open(pid);
        if (OperatingSystem.IsMacOS())
            return MacProcessMemory.Open(pid);
        throw new InvalidOperationException("Reading emulator memory is only supported on Windows, Linux and macOS.");
    }
}
