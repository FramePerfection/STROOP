using StroopNext.Memory;

namespace StroopNext.Core.Tests;

internal static class MiniMips
{
    public const uint ReturnAddress = 0x80000180;

    public static uint[] Run(byte[] ram, uint pc, uint stop = ReturnAddress, uint[]? registers = null, int maxSteps = 200_000)
    {
        var r = registers ?? new uint[32];
        r[31] = ReturnAddress;
        var image = new RamImage(ram);
        uint next = pc + 4;
        for (int step = 0; step < maxSteps; step++)
        {
            if (pc == stop || pc == ReturnAddress)
                return r;
            uint i = image.U32(pc);
            uint after = next + 4;
            int op = (int)(i >> 26), rs = (int)(i >> 21) & 31, rt = (int)(i >> 16) & 31, rd = (int)(i >> 11) & 31, sa = (int)(i >> 6) & 31;
            uint imm = i & 0xFFFF, simm = (uint)(short)imm;
            uint branch = next + (simm << 2);
            void Set(int reg, uint value) { if (reg != 0) r[reg] = value; }
            switch (op)
            {
                case 0:
                    switch (i & 0x3F)
                    {
                        case 0x00: Set(rd, r[rt] << sa); break;
                        case 0x08: after = r[rs]; break;
                        case 0x21: Set(rd, r[rs] + r[rt]); break;
                        case 0x23: Set(rd, r[rs] - r[rt]); break;
                        case 0x25: Set(rd, r[rs] | r[rt]); break;
                        case 0x2B: Set(rd, r[rs] < r[rt] ? 1u : 0); break;
                        default: throw new InvalidOperationException($"special {i:X8} at {pc:X8}");
                    }
                    break;
                case 0x02: after = (next & 0xF0000000) | ((i & 0x03FFFFFF) << 2); break;
                case 0x04: if (r[rs] == r[rt]) after = branch; break;
                case 0x05: if (r[rs] != r[rt]) after = branch; break;
                case 0x09: Set(rt, r[rs] + simm); break;
                case 0x0B: Set(rt, r[rs] < simm ? 1u : 0); break;
                case 0x0C: Set(rt, r[rs] & imm); break;
                case 0x0D: Set(rt, r[rs] | imm); break;
                case 0x0F: Set(rt, imm << 16); break;
                case 0x23: Set(rt, Load(image, r[rs] + simm, 4)); break;
                case 0x24: Set(rt, Load(image, r[rs] + simm, 1)); break;
                case 0x28: new RamPatch().WriteU8(Check(image, r[rs] + simm, 1), (byte)r[rt]).ApplyTo(ram); break;
                case 0x2B: new RamPatch().WriteU32(Check(image, r[rs] + simm, 4), r[rt]).ApplyTo(ram); break;
                default: throw new InvalidOperationException($"opcode {i:X8} at {pc:X8}");
            }
            pc = next;
            next = after;
        }
        throw new InvalidOperationException("the code did not finish");
    }

    private static uint Load(RamImage image, uint address, int size)
        => size == 4 ? image.U32(Check(image, address, 4)) : image.U8(Check(image, address, 1));

    private static uint Check(RamImage image, uint address, int size)
    {
        if (address < 0x80000000 || !image.IsValid(address, size) || address % (uint)size != 0)
            throw new InvalidOperationException($"bad memory access at {address:X8}");
        return address;
    }
}
