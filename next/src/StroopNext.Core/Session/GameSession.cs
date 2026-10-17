using System.Buffers.Binary;
using System.Diagnostics;
using StroopNext.Memory;

namespace StroopNext.Session;

public readonly record struct SessionDiagnostics(int PooledBuffers, int OutstandingBuffers, long FullReads, long SkippedPolls, long UnchangedRefreshes);

public sealed class GameSession : IDisposable
{
    private sealed record TimerProbe(int Offset, uint Value);

    private readonly FramePool _pool = new();
    private readonly object _ioLock = new();
    private readonly object _frameLock = new();
    private readonly AutoResetEvent _wake = new(false);
    private readonly Thread? _thread;
    private readonly List<ISampleTrack> _tracks = new();
    private volatile bool _disposed;

    private IRamSource? _source;
    private GameFrame? _latest;
    private long _sequence;

    private long _generation;

    private volatile TimerProbe? _probe;
    private volatile bool _forceFullRead = true;
    private long _lastFullReadTimestamp;
    private long _fullReads, _skippedPolls, _unchangedRefreshes;

    public GameSession(GameServices? services = null, bool autoPoll = true)
    {
        Services = services ?? new GameServices();
        if (!autoPoll)
            return;
        _thread = new Thread(PollLoop) { IsBackground = true, Name = "STROOP poll" };
        _thread.Start();
    }

    public GameServices Services { get; }

    public double PollRate { get; set; } = 120;

    public double IdleRefreshRate { get; set; } = 10;

    public IRamSource? Source => _source;

    public event Action<IRamSource?>? SourceChanged;

    public double MeasuredFrameRate { get; private set; }

    public SessionDiagnostics Diagnostics => new(_pool.Allocated, _pool.Outstanding, Interlocked.Read(ref _fullReads), Interlocked.Read(ref _skippedPolls), Interlocked.Read(ref _unchangedRefreshes));

    public void Attach(IRamSource source)
    {
        IRamSource? old;
        lock (_ioLock)
        {
            old = _source;
            _source = source;
            _generation++;
            _probe = null;
            _forceFullRead = true;
        }
        old?.Dispose();
        ResetFrames();
        SourceChanged?.Invoke(source);
        _wake.Set();
    }

    public void Detach()
    {
        IRamSource? old;
        lock (_ioLock)
        {
            old = _source;
            _source = null;
            _generation++;
            _probe = null;
        }
        old?.Dispose();
        ResetFrames();
        _pool.Trim();
        SourceChanged?.Invoke(null);
    }

    public T AddTrack<T>(T track) where T : ISampleTrack
    {
        lock (_tracks)
            _tracks.Add(track);
        return track;
    }

    public void RemoveTrack(ISampleTrack track)
    {
        lock (_tracks)
            _tracks.Remove(track);
    }

    public GameFrame? AcquireLatest()
    {
        lock (_frameLock)
            return _latest?.Retain();
    }

    public bool Apply(RamPatch patch, bool atomic = false)
    {
        if (patch.IsEmpty)
            return true;
        if (_disposed)
            return false;
        bool ok;
        lock (_ioLock)
        {
            if (_source is not { IsWritable: true } source)
                return false;
            ok = true;
            using (atomic ? source.Freeze() : null)
                foreach (var (offset, bytes) in patch.ToSwappedRuns())
                {
                    ok &= source.TryWrite(offset, bytes);
                    if (atomic)
                        source.InvalidateCode(offset, bytes.Length);
                }
            _forceFullRead = true;
        }
        _wake.Set();
        return ok;
    }

    public bool PollOnce()
    {
        FrameBuffer buffer;
        long generation;
        lock (_ioLock)
        {
            if (_source is not { } source)
                return false;
            generation = _generation;
            if (!source.IsAlive)
            {
                _source = null;
                _generation++;
                _probe = null;
                source.Dispose();
                ResetFrames();
                SourceChanged?.Invoke(null);
                return false;
            }

            bool idleRefresh = false;
            if (!_forceFullRead && _probe is { } probe)
            {
                Span<byte> word = stackalloc byte[4];
                bool timerUnchanged = source.TryReadRange(probe.Offset, word) && BinaryPrimitives.ReadUInt32LittleEndian(word) == probe.Value;
                if (timerUnchanged && !IdleRefreshDue())
                {
                    _skippedPolls++;
                    return false;
                }
                idleRefresh = timerUnchanged;
            }

            buffer = _pool.Rent(source.Size);
            if (!source.TryRead(buffer.Data))
            {
                buffer.Release();
                return false;
            }
            _forceFullRead = false;
            _lastFullReadTimestamp = Stopwatch.GetTimestamp();
            _fullReads++;

            if (idleRefresh && IsIdenticalToLatest(buffer))
            {
                buffer.Release();
                _unchangedRefreshes++;
                return false;
            }
        }

        var ram = new RamImage(buffer.Data, buffer.Data.Length);
        var frame = new GameFrame(0, ram, buffer, null, Services);
        GameFrame? old;
        lock (_frameLock)
        {
            if (generation != Interlocked.Read(ref _generation))
            {
                frame.Release();
                return false;
            }
            frame.Sequence = ++_sequence;
            frame.Previous = _latest switch
            {
                null => null,
                var l when l.GlobalTimer != frame.GlobalTimer => l.AsPrevious(),
                var l => l.Previous?.AsPrevious(),
            };
            lock (_tracks)
                foreach (var track in _tracks)
                    track.Record(frame);
            old = _latest;
            _latest = frame;
            _probe = frame.Layout?.GlobalTimer is uint timer ? new TimerProbe((int)N64.ToOffset(timer), frame.GlobalTimer) : null;
        }
        old?.Release();
        return true;
    }

    private bool IsIdenticalToLatest(FrameBuffer buffer)
    {
        lock (_frameLock)
            return _latest is { } latest && latest.Ram.Raw.SequenceEqual(buffer.Data);
    }

    private bool IdleRefreshDue()
        => IdleRefreshRate > 0 && Stopwatch.GetElapsedTime(_lastFullReadTimestamp).TotalSeconds >= 1 / IdleRefreshRate;

    private void ResetFrames()
    {
        GameFrame? old;
        lock (_frameLock)
        {
            old = _latest;
            _latest = null;
            lock (_tracks)
                foreach (var track in _tracks)
                    track.Clear();
        }
        old?.Release();
    }

    private void PollLoop()
    {
        var clock = Stopwatch.StartNew();
        int frames = 0;
        double windowStart = 0;
        while (!_disposed)
        {
            double started = clock.Elapsed.TotalMilliseconds;
            try
            {
                if (PollOnce())
                    frames++;
            }
            catch (Exception)
            {
            }

            double now = clock.Elapsed.TotalMilliseconds;
            if (now - windowStart >= 1000)
            {
                MeasuredFrameRate = frames * 1000.0 / (now - windowStart);
                frames = 0;
                windowStart = now;
            }

            int wait = _source is null ? 250 : (int)Math.Max(1, 1000.0 / PollRate - (now - started));
            try
            {
                _wake.WaitOne(wait);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _wake.Set();
        bool stopped = _thread?.Join(2000) ?? true;
        Detach();
        if (stopped)
            _wake.Dispose();
    }
}
