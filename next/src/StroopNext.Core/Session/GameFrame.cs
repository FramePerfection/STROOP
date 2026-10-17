using StroopNext.Game;
using StroopNext.Memory;

namespace StroopNext.Session;

public sealed class GameServices
{
    public SurfaceCache Surfaces { get; } = new();
    public ActionTable Actions { get; init; } = ActionTable.Default;
    public ObjectNames ObjectNames { get; init; } = ObjectNames.Default;
}

public sealed class GameFrame
{
    private readonly FrameBuffer? _buffer;
    private readonly GameServices _services;
    private int _refs = 1;

    private MarioState? _mario;
    private bool _marioRead;
    private ObjectList? _objects;
    private SurfaceTable? _surfaces;
    private CameraState? _camera;
    private bool _cameraRead;

    internal GameFrame(long sequence, RamImage ram, FrameBuffer? buffer, GameFrame? previous, GameServices services)
    {
        Sequence = sequence;
        Ram = ram;
        _buffer = buffer;
        _services = services;
        Previous = previous;
        Version = GameVersionDetector.Detect(ram);
        Layout = Version is { } v ? Sm64Layout.For(v) : null;
        GlobalTimer = Layout?.GlobalTimer is uint t ? ram.U32(t) : 0;
    }

    public static GameFrame Create(RamImage ram, GameFrame? previous = null, GameServices? services = null)
        => new(0, ram, null, previous, services ?? new GameServices());

    public long Sequence { get; internal set; }

    public RamImage Ram { get; }
    public GameVersion? Version { get; }
    public Sm64Layout? Layout { get; }
    public uint GlobalTimer { get; }
    public GameServices Services => _services;

    public GameFrame? Previous { get; internal set; }

    public bool IsKnownGame => Layout is not null;

    public MarioState? Mario
    {
        get
        {
            if (!_marioRead)
            {
                _mario = Layout?.MarioStruct is uint m ? MarioState.Read(Ram, m) : null;
                _marioRead = true;
            }
            return _mario;
        }
    }

    public ObjectList Objects => _objects ??= Layout is null ? ObjectList.Empty : ObjectList.Read(Ram, Layout, _services.ObjectNames);

    public SurfaceTable Surfaces => _surfaces ??= Layout is null ? SurfaceTable.Empty : _services.Surfaces.Build(Ram, Layout);

    public CameraState? Camera
    {
        get
        {
            if (!_cameraRead)
            {
                _camera = Layout is null ? null : CameraState.Read(Ram, Layout);
                _cameraRead = true;
            }
            return _camera;
        }
    }

    public short? LevelIndex => Layout?.CurrentLevel is uint a ? Ram.S16(a) : null;

    public int? AreaIndex
    {
        get
        {
            if (Layout?.CurrentAreaPointer is not uint pointer || Layout.Areas is not uint areas)
                return null;
            uint current = Ram.U32(pointer);
            if (current < areas || (current - areas) % Sm64Addresses.AreaStructSize != 0)
                return null;
            uint index = (current - areas) / Sm64Addresses.AreaStructSize;
            return index < 8 ? (int)index : null;
        }
    }

    internal GameFrame Retain()
    {
        if (_buffer is not null)
            Interlocked.Increment(ref _refs);
        return this;
    }

    public void Release()
    {
        if (_buffer is null)
            return;
        int refs = Interlocked.Decrement(ref _refs);
        if (refs == 0)
        {
            _buffer.Release();
            Previous?.Release();
        }
        else if (refs < 0)
            throw new InvalidOperationException("GameFrame released more often than acquired.");
    }

    internal GameFrame AsPrevious()
    {
        _buffer?.Retain();
        return new GameFrame(Sequence, Ram, _buffer, null, _services);
    }
}

internal sealed class FrameBuffer
{
    private readonly FramePool _pool;
    private int _refs;

    public FrameBuffer(FramePool pool, int size)
    {
        _pool = pool;
        Data = new byte[size];
    }

    public byte[] Data { get; }

    internal void Rent() => _refs = 1;
    public void Retain() => Interlocked.Increment(ref _refs);

    public void Release()
    {
        if (Interlocked.Decrement(ref _refs) == 0)
            _pool.Return(this);
    }
}

internal sealed class FramePool
{
    private readonly Stack<FrameBuffer> _free = new();
    private readonly object _lock = new();

    private int _rented;

    public int Allocated
    {
        get
        {
            lock (_lock)
                return _rented + _free.Count;
        }
    }

    public int Outstanding
    {
        get
        {
            lock (_lock)
                return _rented;
        }
    }

    public FrameBuffer Rent(int size)
    {
        FrameBuffer? buffer = null;
        lock (_lock)
        {
            while (_free.Count > 0 && buffer is null)
            {
                var candidate = _free.Pop();
                if (candidate.Data.Length == size)
                    buffer = candidate;
            }
            _rented++;
        }
        buffer ??= new FrameBuffer(this, size);
        buffer.Rent();
        return buffer;
    }

    public void Return(FrameBuffer buffer)
    {
        lock (_lock)
        {
            _rented--;
            _free.Push(buffer);
        }
    }

    public void Trim()
    {
        lock (_lock)
            _free.Clear();
    }
}
