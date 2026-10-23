namespace StroopNext.Graphics;

public sealed class VertexList<T> where T : struct
{
    private T[] _items = new T[256];

    public int Count { get; private set; }
    public T[] Array => _items;
    public ReadOnlySpan<T> Span => _items.AsSpan(0, Count);

    public void Add(in T item)
    {
        if (Count == _items.Length)
            System.Array.Resize(ref _items, _items.Length * 2);
        _items[Count++] = item;
    }

    public void Clear() => Count = 0;
}
