using System.Globalization;
using StroopNext.Game;

namespace StroopNext.Variables;

public enum AngleDisplay { Units, Degrees, UnitsAndDegrees }

public sealed record DisplayOptions
{
    public static readonly DisplayOptions Default = new();

    public AngleDisplay Angles { get; init; } = AngleDisplay.UnitsAndDegrees;

    public int? Decimals { get; init; }
}

public abstract class ValueFormat<T> where T : struct
{
    public abstract string Format(T value, DisplayOptions options);
    public abstract bool TryParse(string text, DisplayOptions options, out T value);

    public virtual double? ToNumber(T value) => null;

    public virtual double? Delta(T current, T previous) => ToNumber(current) - ToNumber(previous);

    public virtual string FormatDelta(double delta, DisplayOptions options) => FormatNumber(delta, options.Decimals);

    protected static CultureInfo Inv => CultureInfo.InvariantCulture;

    internal static string FormatNumber(double value, int? decimals)
        => decimals is int d ? value.ToString("F" + d, Inv) : value.ToString("R", Inv);

    protected static bool TryParseDouble(string text, out double value)
    {
        text = text.Trim();
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && long.TryParse(text.AsSpan(2), NumberStyles.HexNumber, Inv, out long hex))
        {
            value = hex;
            return true;
        }
        return double.TryParse(text, NumberStyles.Float, Inv, out value);
    }
}

public static class Formats
{
    public static readonly ValueFormat<float> Float = new FloatFormat();
    public static readonly ValueFormat<int> Integer = new IntegerFormat();
    public static readonly ValueFormat<uint> Hex = new HexFormat(8);
    public static readonly ValueFormat<uint> Pointer = new PointerFormat();
    public static readonly ValueFormat<bool> Boolean = new BooleanFormat();
    public static readonly ValueFormat<int> Angle = new AngleFormat(signed: false);
    public static readonly ValueFormat<int> SignedAngle = new AngleFormat(signed: true);

    private sealed class FloatFormat : ValueFormat<float>
    {
        public override string Format(float value, DisplayOptions o)
            => o.Decimals is int d ? value.ToString("F" + d, Inv) : value.ToString(Inv);

        public override bool TryParse(string text, DisplayOptions o, out float value)
        {
            bool ok = TryParseDouble(text, out double d);
            value = (float)d;
            return ok;
        }

        public override double? ToNumber(float value) => value;
    }

    private sealed class IntegerFormat : ValueFormat<int>
    {
        public override string Format(int value, DisplayOptions o) => value.ToString(Inv);

        public override bool TryParse(string text, DisplayOptions o, out int value)
        {
            bool ok = TryParseDouble(text, out double d) && d == Math.Floor(d) && d is >= int.MinValue and <= uint.MaxValue;
            value = ok ? unchecked((int)(long)d) : 0;
            return ok;
        }

        public override double? ToNumber(int value) => value;
        public override string FormatDelta(double delta, DisplayOptions o) => delta.ToString("+0;-0;0", Inv);
    }

    private class HexFormat(int digits) : ValueFormat<uint>
    {
        public override string Format(uint value, DisplayOptions o) => "0x" + value.ToString("X" + digits, Inv);

        public override bool TryParse(string text, DisplayOptions o, out uint value)
        {
            text = text.Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                text = text[2..];
            return uint.TryParse(text, NumberStyles.HexNumber, Inv, out value);
        }

        public override double? Delta(uint current, uint previous) => null;
    }

    private sealed class PointerFormat() : HexFormat(8)
    {
        public override string Format(uint value, DisplayOptions o) => value == 0 ? "null" : base.Format(value, o);
    }

    private sealed class BooleanFormat : ValueFormat<bool>
    {
        public override string Format(bool value, DisplayOptions o) => value ? "true" : "false";

        public override bool TryParse(string text, DisplayOptions o, out bool value)
        {
            switch (text.Trim().ToLowerInvariant())
            {
                case "true" or "1" or "yes" or "on":
                    value = true;
                    return true;
                case "false" or "0" or "no" or "off":
                    value = false;
                    return true;
                default:
                    value = false;
                    return false;
            }
        }

        public override double? ToNumber(bool value) => value ? 1 : 0;
    }

    private sealed class AngleFormat(bool signed) : ValueFormat<int>
    {
        public override string Format(int value, DisplayOptions o)
        {
            int units = signed ? (short)value : (ushort)value;
            string degrees = Game.Angle.ToDegrees(units).ToString("0.###", Inv) + "°";
            return o.Angles switch
            {
                AngleDisplay.Units => units.ToString(Inv),
                AngleDisplay.Degrees => degrees,
                _ => $"{units.ToString(Inv)}  ({degrees})",
            };
        }

        public override bool TryParse(string text, DisplayOptions o, out int value)
        {
            text = text.Trim();
            int paren = text.IndexOf('(');
            if (paren > 0)
                text = text[..paren].Trim();
            bool isDegrees = text.EndsWith('°') || text.EndsWith("deg", StringComparison.OrdinalIgnoreCase);
            text = text.TrimEnd('°').Trim();
            if (text.EndsWith("deg", StringComparison.OrdinalIgnoreCase))
                text = text[..^3];
            if (!TryParseDouble(text, out double number))
            {
                value = 0;
                return false;
            }
            double units = isDegrees ? number * Game.Angle.UnitsPerTurn / 360.0 : number;
            value = signed ? (short)(int)Math.Round(units) : (ushort)(long)Math.Round(units);
            return true;
        }

        public override double? ToNumber(int value) => signed ? (short)value : (ushort)value;

        public override double? Delta(int current, int previous) => Game.Angle.Difference(current, previous);

        public override string FormatDelta(double delta, DisplayOptions o) => delta.ToString("+0;-0;0", Inv);
    }
}

public sealed class ActionFormat(ActionTable table) : ValueFormat<uint>
{
    public override string Format(uint value, DisplayOptions o)
        => table.NameOf(value) is { } name ? $"{name}  (0x{value:X8})" : $"0x{value:X8}";

    public override bool TryParse(string text, DisplayOptions o, out uint value)
    {
        string trimmed = text.Trim();
        int paren = trimmed.IndexOf("(0x", StringComparison.OrdinalIgnoreCase);
        if (paren >= 0)
            trimmed = trimmed[(paren + 1)..].TrimEnd(')');
        if (Formats.Hex.TryParse(trimmed, o, out value))
            return true;
        foreach (var (action, name) in table.All)
            if (string.Equals(name, trimmed, StringComparison.OrdinalIgnoreCase))
            {
                value = action;
                return true;
            }
        return false;
    }

    public override double? Delta(uint current, uint previous) => null;
}
