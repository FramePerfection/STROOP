namespace StroopNext.Session;

public sealed class Selection
{
    private readonly HashSet<uint> _objects = new();

    public event Action? Changed;

    public IReadOnlyCollection<uint> Objects => _objects;

    public bool IsSelected(uint objectAddress) => _objects.Contains(objectAddress);

    public void SelectOnly(uint objectAddress)
    {
        _objects.Clear();
        _objects.Add(objectAddress);
        Changed?.Invoke();
    }

    public void Toggle(uint objectAddress)
    {
        if (!_objects.Remove(objectAddress))
            _objects.Add(objectAddress);
        Changed?.Invoke();
    }

    public void Clear()
    {
        if (_objects.Count == 0)
            return;
        _objects.Clear();
        Changed?.Invoke();
    }
}
