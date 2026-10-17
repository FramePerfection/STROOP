using System.Globalization;
using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;

namespace StroopNext.Platform;

internal readonly record struct LinuxMapping(nint Start, nint End, string Permissions, string Path)
{
    public bool Readable => Permissions.Length > 0 && Permissions[0] == 'r';

    public MemoryRegion ToRegion() => new(Start, End - Start, Readable && !(Path.StartsWith("/dev/", StringComparison.Ordinal) && !Path.StartsWith("/dev/shm/", StringComparison.Ordinal)));
}

internal sealed class LinuxProcessMemory : IProcessMemory
{
    private readonly int _pid;
    private SafeFileHandle? _mem;

    private LinuxProcessMemory(int pid, SafeFileHandle mem, bool writable)
    {
        _pid = pid;
        _mem = mem;
        IsWritable = writable;
    }

    [SupportedOSPlatform("linux")]
    public static LinuxProcessMemory Open(int pid)
    {
        string path = $"/proc/{pid}/mem";
        try
        {
            return new LinuxProcessMemory(pid, File.OpenHandle(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite), true);
        }
        catch (Exception e) when (IsAccessError(e))
        {
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
        {
            throw new InvalidOperationException("the process has exited.", e);
        }
        try
        {
            return new LinuxProcessMemory(pid, File.OpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), false);
        }
        catch (Exception e) when (IsAccessError(e))
        {
            throw new InvalidOperationException(
                "Linux denied access to its memory (ptrace restrictions). Either allow it for this session with "
                + "'sudo sysctl kernel.yama.ptrace_scope=0', run STROOP Next with sudo, or grant the capability once with "
                + "'sudo setcap cap_sys_ptrace=eip <path to STROOP Next executable>'.", e);
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
        {
            throw new InvalidOperationException("the process has exited.", e);
        }
    }

    private static bool IsAccessError(Exception e)
        => e is UnauthorizedAccessException
           || (e is IOException io and not FileNotFoundException and not DirectoryNotFoundException
               && (io.HResult is 13 or 1 || io.Message.Contains("denied", StringComparison.OrdinalIgnoreCase) || io.Message.Contains("not permitted", StringComparison.OrdinalIgnoreCase)));

    public bool IsWritable { get; }
    public int PointerSize => IntPtr.Size;

    public bool IsAlive
    {
        get
        {
            try
            {
                string stat = File.ReadAllText($"/proc/{_pid}/stat");
                int close = stat.LastIndexOf(')');
                return close >= 0 && close + 2 < stat.Length && stat[close + 2] is not ('Z' or 'X');
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                return false;
            }
        }
    }

    public bool Read(nint address, Span<byte> buffer)
    {
        if (_mem is not { } mem || address < 0)
            return false;
        try
        {
            while (!buffer.IsEmpty)
            {
                int read = RandomAccess.Read(mem, buffer, address);
                if (read <= 0)
                    return false;
                buffer = buffer[read..];
                address += read;
            }
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ObjectDisposedException)
        {
            return false;
        }
    }

    public bool Write(nint address, ReadOnlySpan<byte> buffer)
    {
        if (_mem is not { } mem || !IsWritable || address < 0)
            return false;
        try
        {
            RandomAccess.Write(mem, buffer, address);
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ObjectDisposedException)
        {
            return false;
        }
    }

    public IEnumerable<MemoryRegion> Regions() => ReadMaps().Select(m => m.ToRegion());

    public nint? ModuleBase(string moduleName)
    {
        nint? lowest = null;
        foreach (var mapping in ReadMaps())
            if (mapping.Path.Length > 0 && string.Equals(System.IO.Path.GetFileName(mapping.Path), moduleName, StringComparison.Ordinal)
                && (lowest is null || mapping.Start < lowest))
                lowest = mapping.Start;
        return lowest;
    }

    public nint? Symbol(string name) => null;

    private List<LinuxMapping> ReadMaps()
    {
        try
        {
            return ParseMaps(File.ReadAllText($"/proc/{_pid}/maps"));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return [];
        }
    }

    public static List<LinuxMapping> ParseMaps(string text)
    {
        var result = new List<LinuxMapping>();
        foreach (string rawLine in text.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');
            int position = 0;
            if (NextField(line, ref position) is not { } range
                || NextField(line, ref position) is not { } permissions
                || NextField(line, ref position) is null
                || NextField(line, ref position) is null
                || NextField(line, ref position) is null)
                continue;
            int dash = range.IndexOf('-');
            if (dash <= 0
                || !ulong.TryParse(range.AsSpan(0, dash), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong start)
                || !ulong.TryParse(range.AsSpan(dash + 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong end)
                || end <= start || end > long.MaxValue)
                continue;
            string path = position < line.Length ? line[position..].TrimStart(' ', '\t') : "";
            result.Add(new LinuxMapping((nint)(long)start, (nint)(long)end, permissions, path));
        }
        return result;
    }

    private static string? NextField(string line, ref int position)
    {
        while (position < line.Length && line[position] is ' ' or '\t')
            position++;
        int start = position;
        while (position < line.Length && line[position] is not (' ' or '\t'))
            position++;
        return position > start ? line[start..position] : null;
    }

    public void Dispose()
    {
        _mem?.Dispose();
        _mem = null;
    }
}
