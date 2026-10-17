using StroopNext.Memory;

namespace StroopNext.Platform;

public sealed class ProcessRamSource : IRamSource
{
    private IProcessMemory? _memory;

    private readonly int _processId;

    public int ProcessId => _processId;
    private readonly string _processName;

    private ProcessRamSource(IProcessMemory memory, string name, nint ramBase, int size, string method, int processId)
    {
        _memory = memory;
        _processId = processId;
        _processName = name.Split(' ')[0];
        Name = name;
        RamBase = ramBase;
        Size = size;
        DetectionMethod = method;
    }

    public string Name { get; }
    public int Size { get; }
    public bool IsWritable => _memory?.IsWritable ?? false;
    public nint RamBase { get; }

    public string DetectionMethod { get; }

    public bool IsAlive => _memory?.IsAlive ?? false;

    public static ProcessRamSource Attach(EmulatorCandidate candidate)
    {
        IProcessMemory memory;
        try
        {
            memory = ProcessMemory.Open(candidate.ProcessId);
        }
        catch (InvalidOperationException e)
        {
            throw new InvalidOperationException($"Cannot open {candidate.ProcessName}: {e.Message}", e);
        }
        return Attach(memory, candidate);
    }

    internal static ProcessRamSource Attach(IProcessMemory memory, EmulatorCandidate candidate)
    {
        try
        {
            var (ramBase, method) = RamBaseLocator.Locate(memory, candidate);
            int size = memory.Read(ramBase + N64.RamSize8MB - 4, stackalloc byte[4]) ? N64.RamSize8MB : N64.RamSize4MB;
            return new ProcessRamSource(memory, $"{candidate.ProcessName} ({candidate.ProcessId})", ramBase, size, method, candidate.ProcessId);
        }
        catch
        {
            memory.Dispose();
            throw;
        }
    }

    public bool TryRead(Span<byte> destination) => _memory is { } m && m.Read(RamBase, destination[..Size]);

    public bool TryReadRange(int offset, Span<byte> destination)
        => _memory is { } m && offset >= 0 && offset + destination.Length <= Size && m.Read(RamBase + offset, destination);

    public IDisposable Freeze() => ProcessFreeze.Freeze(_processId);

    private nint? _codeTable;
    private bool _codeTableSearched;
    private readonly object _codeTableLock = new();

    private nint? CodeTable
    {
        get
        {
            lock (_codeTableLock)
            {
                if (!_codeTableSearched && _memory is { } m)
                {
                    _codeTableSearched = true;
                    try
                    {
                        _codeTable = CodeCacheLocator.Find(m, _processName);
                    }
                    catch (Exception e) when (e is IOException or InvalidOperationException or OutOfMemoryException)
                    {
                        _codeTable = null;
                    }
                }
                return _codeTable;
            }
        }
    }

    public bool CanInvalidateCode => CodeTable is not null;

    public string DescribeCodeCache()
    {
        if (_memory is not { } m)
            return "detached";
        var (symbol, signature) = CodeCacheLocator.BothWays(m, _processName);
        return $"code cache table: symbol {(symbol is { } a ? $"0x{a:X}" : "none")}, signature {(signature is { } b ? $"0x{b:X}" : "none")}";
    }

    public void InvalidateCode(int offset, int length)
    {
        if (CodeTable is not nint table || _memory is not { } m || length <= 0)
            return;
        ReadOnlySpan<byte> one = [1];
        for (int page = offset >> 12; page <= (offset + length - 1) >> 12; page++)
        {
            m.Write(table + (0x80000 + page), one);
            m.Write(table + (0xA0000 + page), one);
        }
    }

    public bool TryWrite(int offset, ReadOnlySpan<byte> swappedBytes)
        => _memory is { } m && offset >= 0 && offset + swappedBytes.Length <= Size && m.Write(RamBase + offset, swappedBytes);

    public void Dispose()
    {
        _memory?.Dispose();
        _memory = null;
    }
}
