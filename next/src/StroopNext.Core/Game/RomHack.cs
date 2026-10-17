using System.Globalization;

namespace StroopNext.Game;

public sealed class RomHack
{
    public readonly record struct Entry(uint Address, byte[] Bytes);

    public RomHack(IReadOnlyList<Entry> entries) => Entries = entries;

    public IReadOnlyList<Entry> Entries { get; }

    public static RomHack Parse(string text)
    {
        string s = new(text.Where(c => !char.IsWhiteSpace(c)).ToArray());
        var entries = new List<Entry>();
        int colon = s.IndexOf(':');
        while (colon >= 8)
        {
            int next = s.IndexOf(':', colon + 1);
            int end = next < 0 ? s.Length : next - 8;
            if (uint.TryParse(s.AsSpan(colon - 8, 8), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint address)
                && end >= colon + 1 && (end - colon - 1) % 2 == 0
                && TryHex(s.AsSpan(colon + 1, end - colon - 1), out byte[] bytes))
                entries.Add(new Entry(address, bytes));
            if (next < 0)
                break;
            colon = next;
        }
        return new RomHack(entries);
    }

    private static bool TryHex(ReadOnlySpan<char> hex, out byte[] bytes)
    {
        bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
            if (!byte.TryParse(hex.Slice(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[i]))
                return false;
        return true;
    }
}
