namespace StroopNext.Memory;

public interface IRamSource : IDisposable
{
    string Name { get; }

    int Size { get; }

    bool IsWritable { get; }

    bool IsAlive { get; }

    bool TryRead(Span<byte> destination);

    bool TryReadRange(int offset, Span<byte> destination);

    bool TryWrite(int offset, ReadOnlySpan<byte> swappedBytes);

    IDisposable Freeze() => NoFreeze.Instance;

    bool CanInvalidateCode => false;

    void InvalidateCode(int offset, int length) { }
}

internal sealed class NoFreeze : IDisposable
{
    public static readonly NoFreeze Instance = new();
    public void Dispose() { }
}

public sealed class BufferRamSource : IRamSource
{
    private readonly byte[] _ram;
    private readonly object _lock = new();

    public BufferRamSource(string name, byte[] swappedRam)
    {
        Name = name;
        _ram = swappedRam;
    }

    public string Name { get; }
    public int Size => _ram.Length;
    public bool IsWritable => true;
    public bool IsAlive => true;

    public bool TryRead(Span<byte> destination)
    {
        lock (_lock)
            _ram.CopyTo(destination);
        return true;
    }

    public bool TryReadRange(int offset, Span<byte> destination)
    {
        if (offset < 0 || offset + destination.Length > _ram.Length)
            return false;
        lock (_lock)
            _ram.AsSpan(offset, destination.Length).CopyTo(destination);
        return true;
    }

    public bool TryWrite(int offset, ReadOnlySpan<byte> swappedBytes)
    {
        if (offset < 0 || offset + swappedBytes.Length > _ram.Length)
            return false;
        lock (_lock)
            swappedBytes.CopyTo(_ram.AsSpan(offset));
        return true;
    }

    public void Dispose() { }
}
