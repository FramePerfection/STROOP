namespace StroopNext.Session;

public readonly record struct Sample<T>(uint GlobalTimer, T Value);

public interface ISampleTrack
{
    string Id { get; }
    void Record(GameFrame frame);
    void Clear();
}

public sealed class SampleTrack<T> : ISampleTrack where T : struct
{
    private readonly Func<GameFrame, T?> _sampler;
    private readonly Sample<T>[] _ring;
    private readonly object _lock = new();
    private int _start, _count;

    public SampleTrack(string id, Func<GameFrame, T?> sampler, int capacity = 4096)
    {
        Id = id;
        _sampler = sampler;
        _ring = new Sample<T>[capacity];
    }

    public string Id { get; }
    public int Capacity => _ring.Length;

    public void Record(GameFrame frame)
    {
        if (_sampler(frame) is not T value)
            return;
        lock (_lock)
        {
            if (_count > 0)
            {
                uint last = _ring[(_start + _count - 1) % _ring.Length].GlobalTimer;
                if (frame.GlobalTimer == last)
                    return;
                if (frame.GlobalTimer < last)
                    _start = _count = 0;
            }
            int index = (_start + _count) % _ring.Length;
            _ring[index] = new Sample<T>(frame.GlobalTimer, value);
            if (_count < _ring.Length)
                _count++;
            else
                _start = (_start + 1) % _ring.Length;
        }
    }

    public void Clear()
    {
        lock (_lock)
            _start = _count = 0;
    }

    public Sample<T>[] Snapshot()
    {
        lock (_lock)
        {
            var result = new Sample<T>[_count];
            for (int i = 0; i < _count; i++)
                result[i] = _ring[(_start + i) % _ring.Length];
            return result;
        }
    }
}
