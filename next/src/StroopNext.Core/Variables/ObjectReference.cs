using System.Globalization;
using StroopNext.Session;

namespace StroopNext.Variables;

public readonly record struct ObjectReference(uint Address, int? Slot = null, string? Name = null)
{
    public static ObjectReference Resolve(GameFrame frame, uint address)
        => frame.Objects.Find(address) is { } o ? new ObjectReference(address, o.SlotNumber, o.Name) : new ObjectReference(address);

    public uint? AddressIn(GameFrame frame)
    {
        if (Slot is not int slot || Address != 0)
            return Address;
        return frame.Objects.All.FirstOrDefault(o => o.SlotNumber == slot)?.Address;
    }

    public static Variable<ObjectReference> Variable(string id, string label, string group, BaseResolver @base, uint offset, string? description = null)
    {
        var field = new MemoryField<uint>(@base, offset, Primitives.U32);
        return new Variable<ObjectReference>(id, label, group, ObjectReferenceFormat.Instance,
            f => field.Read(f) is uint address ? Resolve(f, address) : null,
            (f, v, p) =>
            {
                if (v.AddressIn(f) is uint address)
                    field.Write(f, address, p);
            },
            description ?? "Type \"slot N\", \"none\" or a 0x… address.");
    }
}

public sealed class ObjectReferenceFormat : ValueFormat<ObjectReference>
{
    public static readonly ObjectReferenceFormat Instance = new();

    public override string Format(ObjectReference value, DisplayOptions options)
    {
        if (value.Address == 0)
            return "none";
        if (value.Slot is int slot)
            return string.Create(CultureInfo.InvariantCulture, $"slot {slot} · {value.Name}");
        return $"0x{value.Address:X8}";
    }

    public override bool TryParse(string text, DisplayOptions options, out ObjectReference value)
    {
        string t = text.Trim();
        value = default;
        if (t.Length == 0 || t.Equals("none", StringComparison.OrdinalIgnoreCase) || t.Equals("no object", StringComparison.OrdinalIgnoreCase) || t == "0")
            return true;
        if (t.StartsWith("slot", StringComparison.OrdinalIgnoreCase))
        {
            string number = t[4..].Trim();
            int dot = number.IndexOf('·');
            if (dot >= 0)
                number = number[..dot].Trim();
            if (int.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out int slot) && slot >= 1)
            {
                value = new ObjectReference(0, slot);
                return true;
            }
            return false;
        }
        if (Formats.Hex.TryParse(t, options, out uint address))
        {
            value = new ObjectReference(address);
            return true;
        }
        return false;
    }

    public override double? Delta(ObjectReference current, ObjectReference previous) => null;
}
