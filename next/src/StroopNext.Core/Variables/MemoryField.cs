using StroopNext.Memory;
using StroopNext.Session;

namespace StroopNext.Variables;

public sealed record Primitive<T>(int Size, Func<RamImage, uint, T> Read, Action<RamPatch, uint, T> Write) where T : struct;

public static class Primitives
{
    public static readonly Primitive<float> F32 = new(4, (r, a) => r.F32(a), (p, a, v) => p.WriteF32(a, v));
    public static readonly Primitive<uint> U32 = new(4, (r, a) => r.U32(a), (p, a, v) => p.WriteU32(a, v));
    public static readonly Primitive<int> S32 = new(4, (r, a) => r.S32(a), (p, a, v) => p.WriteS32(a, v));
    public static readonly Primitive<int> U16 = new(2, (r, a) => r.U16(a), (p, a, v) => p.WriteU16(a, (ushort)v));
    public static readonly Primitive<int> S16 = new(2, (r, a) => r.S16(a), (p, a, v) => p.WriteS16(a, (short)v));
    public static readonly Primitive<int> U8 = new(1, (r, a) => r.U8(a), (p, a, v) => p.WriteU8(a, (byte)v));
    public static readonly Primitive<int> S8 = new(1, (r, a) => r.S8(a), (p, a, v) => p.WriteS8(a, (sbyte)v));
}

public delegate uint? BaseResolver(GameFrame frame);

public static class Bases
{
    public static readonly BaseResolver Mario = f => f.Layout?.MarioStruct;
    public static readonly BaseResolver Camera = f => f.Layout?.Camera;

    public static BaseResolver Versioned(Game.Versioned<uint> address) => f => f.Layout is { } l ? address.For(l.Version) : null;

    public static BaseResolver Global(Func<Game.Sm64Layout, uint?> select) => f => f.Layout is { } l ? select(l) : null;

    public static BaseResolver Pointer(BaseResolver @base, uint offset) => f =>
        @base(f) is uint b && f.Ram.U32(b + offset) is var p && f.Ram.IsPointer(p) ? p : null;
}

public sealed record MemoryField<T>(BaseResolver Base, uint Offset, Primitive<T> Primitive) where T : struct
{
    public uint? AddressIn(GameFrame frame) => Base(frame) is uint b ? b + Offset : null;

    public T? Read(GameFrame frame) => AddressIn(frame) is uint a && frame.Ram.IsValid(a, Primitive.Size) ? Primitive.Read(frame.Ram, a) : null;

    public void Write(GameFrame frame, T value, RamPatch patch)
    {
        if (AddressIn(frame) is uint a)
            Primitive.Write(patch, a, value);
    }

    public Variable<T> ToVariable(string id, string label, string group, ValueFormat<T> format, bool writable = true, string? description = null)
        => new(id, label, group, format, Read, writable ? Write : null, description);
}
