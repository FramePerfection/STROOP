using StroopNext.Session;
using StroopNext.Settings;

namespace StroopNext.Map;

public sealed record MapContext(GameFrame Frame, MapView View, MapScene Scene, SettingsScope Settings, Selection Selection);

public interface IMapLayer
{
    string Id { get; }
    string Name { get; }
    bool VisibleByDefault { get; }
    IReadOnlyList<Setting> Settings { get; }

    void Build(MapContext context);
}

public abstract class MapLayer : IMapLayer
{
    private readonly List<Setting> _settings = new();

    public abstract string Id { get; }
    public abstract string Name { get; }
    public virtual bool VisibleByDefault => true;
    public IReadOnlyList<Setting> Settings => _settings;

    protected Setting<T> Declare<T>(Setting<T> setting)
    {
        _settings.Add(setting);
        return setting;
    }

    public abstract void Build(MapContext context);
}
