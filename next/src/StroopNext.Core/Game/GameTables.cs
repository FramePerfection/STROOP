using System.Globalization;
using System.Xml.Linq;

namespace StroopNext.Game;

internal static class EmbeddedData
{
    public static string LoadText(string name)
    {
        using var stream = typeof(EmbeddedData).Assembly.GetManifestResourceStream($"StroopNext.Data.{name}")
                           ?? throw new InvalidOperationException($"Missing embedded resource {name}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static XDocument Load(string name)
    {
        using var stream = typeof(EmbeddedData).Assembly.GetManifestResourceStream($"StroopNext.Data.{name}")
                           ?? throw new InvalidOperationException($"Missing embedded resource {name}");
        return XDocument.Load(stream);
    }

    public static uint ParseHex(string? text)
        => text is not null && text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            && uint.TryParse(text.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint v) ? v : 0;
}

public sealed class ActionTable
{
    private static readonly Lazy<ActionTable> LazyDefault = new(() => FromXml(EmbeddedData.Load("MarioActions.xml")));
    public static ActionTable Default => LazyDefault.Value;

    private readonly Dictionary<uint, string> _names;
    private readonly Dictionary<uint, uint> _handsFree;

    public ActionTable(Dictionary<uint, string> names, Dictionary<uint, uint>? handsFree = null, uint defaultHandsFree = 0x0C400201)
    {
        _names = names;
        _handsFree = handsFree ?? new Dictionary<uint, uint>();
        DefaultHandsFree = defaultHandsFree;
    }

    public static ActionTable FromXml(XDocument doc)
    {
        var names = new Dictionary<uint, string>();
        var handsFree = new Dictionary<uint, uint>();
        foreach (var e in doc.Root!.Elements("Action"))
        {
            uint value = EmbeddedData.ParseHex((string?)e.Attribute("value"));
            names.TryAdd(value, (string?)e.Attribute("name") ?? "?");
            if ((string?)e.Attribute("handsfreeValue") is { } free)
                handsFree.TryAdd(value, EmbeddedData.ParseHex(free));
        }
        uint fallback = (string?)doc.Root.Element("Default")?.Attribute("handsfreeValue") is { } d ? EmbeddedData.ParseHex(d) : 0x0C400201;
        return new ActionTable(names, handsFree, fallback);
    }

    public uint DefaultHandsFree { get; }

    public string? NameOf(uint action) => _names.GetValueOrDefault(action);

    public uint HandsFreeFor(uint action) => _handsFree.TryGetValue(action, out uint free) ? free : DefaultHandsFree;

    public IEnumerable<KeyValuePair<uint, string>> All => _names;
}

public sealed class ObjectNames
{
    private static readonly Lazy<ObjectNames> LazyDefault = new(() => FromXml(EmbeddedData.Load("ObjectAssociations.xml")));
    public static ObjectNames Default => LazyDefault.Value;

    private readonly Dictionary<uint, string> _us;
    private readonly Dictionary<uint, string> _jp;
    private readonly Dictionary<uint, string> _usImages = new();
    private readonly Dictionary<uint, string> _jpImages = new();

    public ObjectNames(Dictionary<uint, string> usSegmentedNames, Dictionary<uint, string>? usSegmentedImages = null)
    {
        _us = usSegmentedNames;
        _jp = new Dictionary<uint, string>();
        foreach (var (address, name) in _us)
            _jp.TryAdd(AlignJP(address), name);
        foreach (var (address, image) in usSegmentedImages ?? [])
        {
            _usImages[address] = image;
            _jpImages.TryAdd(AlignJP(address), image);
        }
    }

    public static ObjectNames FromXml(XDocument doc)
    {
        var names = new Dictionary<uint, string>();
        var images = new Dictionary<uint, string>();
        var root = doc.Root!;
        void Image(XElement e, uint behavior)
        {
            if ((string?)e.Element("Image")?.Attribute("path") is { Length: > 0 } path)
                images.TryAdd(behavior, path);
        }
        if (root.Element("Mario") is { } mario)
        {
            uint behavior = EmbeddedData.ParseHex((string?)mario.Attribute("behaviorScriptAddress"));
            names[behavior] = "Mario";
            Image(mario, behavior);
        }
        foreach (var e in root.Elements("Object"))
        {
            uint behavior = EmbeddedData.ParseHex((string?)e.Attribute("behaviorScriptAddress"));
            names.TryAdd(behavior, (string?)e.Attribute("name") ?? "?");
            Image(e, behavior);
        }
        return new ObjectNames(names, images);
    }

    /// <summary>The legacy STROOP image file of an object (Resources/Object Images), or null.</summary>
    public string? ImageOf(uint segmentedBehavior, GameVersion version)
        => (version == GameVersion.JP ? _jpImages : _usImages).GetValueOrDefault(segmentedBehavior);

    public string Lookup(uint segmentedBehavior, GameVersion version)
    {
        if (segmentedBehavior == 0)
            return "Unknown";
        var table = version == GameVersion.JP ? _jp : _us;
        return table.TryGetValue(segmentedBehavior, out var name) ? name : $"Behavior {segmentedBehavior:X8}";
    }

    private static uint AlignJP(uint segmented) => segmented switch
    {
        >= 0x13002EA0 => segmented + 32,
        >= 0x13002C6C => segmented + 36,
        >= 0x13002998 => segmented + 24,
        _ => segmented,
    };
}
