using System.Text.Json;
using System.Text.Json.Nodes;

namespace StroopNext.Settings;

public abstract class Setting
{
    protected Setting(string key, string label, string? description)
    {
        Key = key;
        Label = label;
        Description = description;
    }

    public string Key { get; }
    public string Label { get; }
    public string? Description { get; }
    public abstract Type ValueType { get; }
}

public sealed class Setting<T>(string key, string label, T defaultValue, string? description = null) : Setting(key, label, description)
{
    public T Default { get; } = defaultValue;
    public override Type ValueType => typeof(T);

    public (double Min, double Max, double Step)? Range { get; init; }

    public IReadOnlyList<T>? Choices { get; init; }
}

public sealed class SettingsScope
{
    private readonly SettingsStore _store;

    internal SettingsScope(SettingsStore store, string prefix)
    {
        _store = store;
        Prefix = prefix;
    }

    public string Prefix { get; }

    public SettingsScope Child(string name) => new(_store, $"{Prefix}.{name}");

    public T Get<T>(Setting<T> setting) => _store.Get(Key(setting), setting.Default);

    public void Set<T>(Setting<T> setting, T value) => _store.Set(Key(setting), value);

    public object? GetBoxed(Setting setting) => _store.GetBoxed(Key(setting), setting.ValueType, DefaultOf(setting));

    public void SetBoxed(Setting setting, object? value) => _store.SetBoxed(Key(setting), setting.ValueType, Coerce(value, setting.ValueType));

    private static object? Coerce(object? value, Type type)
    {
        if (value is null || type.IsInstanceOfType(value))
            return value;
        if (type.IsEnum)
            return Enum.ToObject(type, value);
        return value is IConvertible ? Convert.ChangeType(value, type, System.Globalization.CultureInfo.InvariantCulture) : value;
    }

    private string Key(Setting s) => $"{Prefix}.{s.Key}";

    private static object? DefaultOf(Setting s) => s.GetType().GetProperty(nameof(Setting<int>.Default))!.GetValue(s);
}

public sealed class SettingsStore
{
    private readonly JsonObject _values;
    private readonly string? _path;
    private readonly object _lock = new();

    public SettingsStore(string? path = null)
    {
        _path = path;
        _values = Load(path);
    }

    public event Action<string>? Changed;

    public SettingsScope Scope(string prefix) => new(this, prefix);

    public T Get<T>(string key, T fallback) => GetBoxed(key, typeof(T), fallback) is T v ? v : fallback;

    public void Set<T>(string key, T value) => SetBoxed(key, typeof(T), value);

    internal object? GetBoxed(string key, Type type, object? fallback)
    {
        lock (_lock)
        {
            if (!_values.TryGetPropertyValue(key, out var node) || node is null)
                return fallback;
            try
            {
                return node.Deserialize(type) ?? fallback;
            }
            catch (Exception e) when (e is JsonException or NotSupportedException or InvalidOperationException)
            {
                return fallback;
            }
        }
    }

    internal void SetBoxed(string key, Type type, object? value)
    {
        lock (_lock)
            _values[key] = JsonSerializer.SerializeToNode(value, type);
        Changed?.Invoke(key);
    }

    public void Save()
    {
        if (_path is null)
            return;
        string json;
        lock (_lock)
            json = _values.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path))!);
        File.WriteAllText(_path, json);
    }

    private static JsonObject Load(string? path)
    {
        if (path is null || !File.Exists(path))
            return new JsonObject();
        try
        {
            return JsonNode.Parse(File.ReadAllText(path)) as JsonObject ?? new JsonObject();
        }
        catch (JsonException)
        {
            return new JsonObject();
        }
    }
}
