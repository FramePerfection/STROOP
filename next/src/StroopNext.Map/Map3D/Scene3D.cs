using System.Numerics;
using StroopNext.Map;

namespace StroopNext.Map3D;

public abstract record Primitive3D
{
    public IMapHit? Hit { get; init; }
}

public sealed record Triangle3D(Vector3 A, Vector3 B, Vector3 C, Argb8Color Color, bool Edges = true) : Primitive3D;

public sealed record Cylinder3D(Vector3 Base, float Radius, float Height, Argb8Color Color, int Segments = 24) : Primitive3D;

public sealed record Line3D(Vector3[] Points, Argb8Color Color) : Primitive3D;

public sealed record Arrow3D(Vector3 Origin, double Yaw, float Length, float Width, Argb8Color Color) : Primitive3D;

public sealed record Point3D(Vector3 Position, float SizePx, Argb8Color Color) : Primitive3D;

public sealed record GameVisualsOptions(bool Level, bool Objects, bool Sky, bool Fog, bool CullBackFaces, float MaxY, bool KeepMarioVisible = true)
{
    public static readonly GameVisualsOptions Default = new(true, true, true, true, false, float.PositiveInfinity);
}

public sealed class Scene3D
{
    private sealed class Section
    {
        public object? Key = new();
        public readonly List<Primitive3D> Primitives = new();
    }

    private readonly List<Primitive3D> _items = new();
    private readonly Dictionary<object, Section> _sections = new(ReferenceEqualityComparer.Instance);
    private readonly List<Section> _order = new();
    private List<Section> _previousOrder = new();
    private readonly List<Primitive3D> _background = new();
    private Section? _current;
    private bool _dirty = true;
    private bool _finalized;
    private long _version;

    public IReadOnlyList<Primitive3D> Items => _items;

    public Graphics.GameScene? GameVisuals { get; set; }

    public GameVisualsOptions GameVisualsOptions { get; set; } = GameVisualsOptions.Default;

    public List<System.Numerics.Vector3> MarioModelCopies { get; } = new();

    public List<(System.Numerics.Vector3 Position, ushort Yaw)> MarioModelGhosts { get; } = new();

    public List<System.Numerics.Vector3> LevelCopies { get; } = new();

    public IReadOnlyList<Primitive3D> Background
    {
        get
        {
            Consolidate();
            return _background;
        }
    }

    public IEnumerable<Primitive3D> All => Background.Concat(_items);
    public int Count => Background.Count + _items.Count;

    public long BackgroundVersion
    {
        get
        {
            Consolidate();
            return _version;
        }
    }

    public void Add(Primitive3D primitive) => _items.Add(primitive);

    public bool BeginBackground(object owner, object? key)
    {
        if (!_sections.TryGetValue(owner, out var section))
            _sections[owner] = section = new Section();
        _order.Add(section);
        _finalized = false;
        if (Equals(section.Key, key))
        {
            _current = null;
            return false;
        }
        section.Key = key;
        section.Primitives.Clear();
        _current = section;
        _dirty = true;
        return true;
    }

    public void AddBackground(Primitive3D primitive)
        => (_current ?? throw new InvalidOperationException("AddBackground outside a rebuilt section (see BeginBackground)."))
            .Primitives.Add(primitive);

    public void Clear()
    {
        Consolidate();
        GameVisuals = null;
        MarioModelCopies.Clear();
        MarioModelGhosts.Clear();
        LevelCopies.Clear();
        _items.Clear();
        _previousOrder = new List<Section>(_order);
        _order.Clear();
        _current = null;
        _finalized = false;
    }

    private void Consolidate()
    {
        if (_finalized)
            return;
        _finalized = true;
        if (!_dirty && _order.SequenceEqual(_previousOrder))
            return;
        foreach (var unused in _sections.Where(kv => !_order.Contains(kv.Value)).Select(kv => kv.Key).ToList())
            _sections.Remove(unused);
        _background.Clear();
        foreach (var section in _order)
            _background.AddRange(section.Primitives);
        _dirty = false;
        _version++;
    }
}
