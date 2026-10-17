using System.Buffers.Binary;

namespace StroopNext.Game;

public sealed class Mips(uint origin)
{
    public const int R0 = 0, AT = 1, V0 = 2, A0 = 4, A1 = 5, T0 = 8, T1 = 9, T2 = 10, T3 = 11, T4 = 12, T5 = 13, T6 = 14, T7 = 15, T8 = 24, T9 = 25, RA = 31;

    private readonly List<uint> _words = new();
    private readonly Dictionary<string, int> _labels = new();
    private readonly List<(int At, string Label)> _fixups = new();

    public uint Origin { get; } = origin;
    public uint Here => Origin + (uint)(_words.Count * 4);

    public static ushort Hi(uint address) => (ushort)((address + 0x8000) >> 16);
    public static short Lo(uint address) => (short)(address & 0xFFFF);

    private Mips I(int op, int rs, int rt, int imm) { _words.Add((uint)(op << 26 | rs << 21 | rt << 16 | (imm & 0xFFFF))); return this; }
    private Mips R(int rs, int rt, int rd, int sa, int funct) { _words.Add((uint)(rs << 21 | rt << 16 | rd << 11 | sa << 6 | funct)); return this; }

    public Mips Label(string name) { _labels[name] = _words.Count; return this; }
    public Mips Lui(int rt, ushort imm) => I(0x0F, 0, rt, imm);
    public Mips Ori(int rt, int rs, ushort imm) => I(0x0D, rs, rt, imm);
    public Mips Addiu(int rt, int rs, short imm) => I(0x09, rs, rt, imm);
    public Mips Andi(int rt, int rs, ushort imm) => I(0x0C, rs, rt, imm);
    public Mips Sltiu(int rt, int rs, short imm) => I(0x0B, rs, rt, imm);
    public Mips Lw(int rt, short offset, int rs) => I(0x23, rs, rt, offset);
    public Mips Lbu(int rt, short offset, int rs) => I(0x24, rs, rt, offset);
    public Mips Sw(int rt, short offset, int rs) => I(0x2B, rs, rt, offset);
    public Mips Sb(int rt, short offset, int rs) => I(0x28, rs, rt, offset);
    public Mips Sll(int rd, int rt, int sa) => R(0, rt, rd, sa, 0x00);
    public Mips Addu(int rd, int rs, int rt) => R(rs, rt, rd, 0, 0x21);
    public Mips Subu(int rd, int rs, int rt) => R(rs, rt, rd, 0, 0x23);
    public Mips Or(int rd, int rs, int rt) => R(rs, rt, rd, 0, 0x25);
    public Mips Sltu(int rd, int rs, int rt) => R(rs, rt, rd, 0, 0x2B);
    public Mips Jr(int rs) => R(rs, 0, 0, 0, 0x08);
    public Mips Nop() => R(0, 0, 0, 0, 0);

    public Mips J(uint target) { _words.Add(0x08000000 | ((target & 0x0FFFFFFF) >> 2)); return this; }

    public Mips Beq(int rs, int rt, string label) { _fixups.Add((_words.Count, label)); return I(0x04, rs, rt, 0); }
    public Mips Bne(int rs, int rt, string label) { _fixups.Add((_words.Count, label)); return I(0x05, rs, rt, 0); }

    public byte[] Assemble()
    {
        foreach (var (at, label) in _fixups)
        {
            int offset = _labels[label] - (at + 1);
            _words[at] = (_words[at] & 0xFFFF0000) | (uint)(offset & 0xFFFF);
        }
        var bytes = new byte[_words.Count * 4];
        for (int i = 0; i < _words.Count; i++)
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(i * 4), _words[i]);
        return bytes;
    }
}
