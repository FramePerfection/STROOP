using System.Xml.Linq;

namespace StroopNext.Game;

public sealed class TriangleInfo
{
    private static readonly Lazy<TriangleInfo> LazyDefault = new(() => FromXml(EmbeddedData.Load("TriangleInfo.xml")));
    public static TriangleInfo Default => LazyDefault.Value;

    private readonly Dictionary<short, short> _slipperiness;

    public TriangleInfo(Dictionary<short, short> slipperiness) => _slipperiness = slipperiness;

    public static TriangleInfo FromXml(XDocument doc)
    {
        var slipperiness = new Dictionary<short, short>();
        foreach (var e in doc.Root!.Elements("Triangle"))
            if ((string?)e.Attribute("type") is { } type && (string?)e.Attribute("slipperiness") is { } slip)
                slipperiness[short.Parse(type, System.Globalization.CultureInfo.InvariantCulture)] = (short)EmbeddedData.ParseHex(slip);
        return new TriangleInfo(slipperiness);
    }

    public short? Slipperiness(short type) => _slipperiness.TryGetValue(type, out short s) ? s : null;

    public double FrictionMultiplier(short type) => Slipperiness(type) switch
    {
        0x13 => 0.98,
        0x14 => 0.96,
        0x00 or 0x15 => 0.92,
        _ => double.NaN,
    };
}
