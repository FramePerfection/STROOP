namespace StroopNext.Graphics;

public static class MipsLeaf
{
    private const uint ReturnAddress = 0xFFFFFFF0, Stack = 0x807FFFF0;
    private const int MaxSteps = 4096;

    public static uint? Call(GfxMemory memory, uint function, uint a0 = 0, uint a1 = 0, uint a2 = 0, uint a3 = 0)
    {
        if (function == 0)
            return null;
        var r = new uint[32];
        r[4] = a0;
        r[5] = a1;
        r[6] = a2;
        r[7] = a3;
        r[29] = Stack;
        r[31] = ReturnAddress;
        Dictionary<uint, byte>? stores = null;

        uint Load8(uint a) => stores is not null && stores.TryGetValue(a, out byte b) ? b : memory.U8(a);
        uint Load16(uint a) => Load8(a) << 8 | Load8(a + 1);
        uint Load32(uint a) => Load16(a) << 16 | Load16(a + 2);

        uint pc = function;
        uint? delayed = null;
        for (int step = 0; step < MaxSteps; step++)
        {
            if (pc == ReturnAddress)
                return r[2];
            if (!memory.IsPointer(pc))
                return null;

            uint i = memory.U32(pc);
            uint next = pc + 4;
            uint? target = null;
            bool skipDelaySlot = false;
            int op = (int)(i >> 26), rs = (int)(i >> 21) & 31, rt = (int)(i >> 16) & 31, rd = (int)(i >> 11) & 31, sa = (int)(i >> 6) & 31;
            uint imm = i & 0xFFFF, simm = (uint)(short)imm;
            uint branch = next + (simm << 2);
            uint value = 0;
            int dest = 0;

            switch (op)
            {
                case 0x00:
                    dest = rd;
                    switch (i & 63)
                    {
                        case 0x00: value = r[rt] << sa; break;
                        case 0x02: value = r[rt] >> sa; break;
                        case 0x03: value = (uint)((int)r[rt] >> sa); break;
                        case 0x04: value = r[rt] << (int)(r[rs] & 31); break;
                        case 0x06: value = r[rt] >> (int)(r[rs] & 31); break;
                        case 0x07: value = (uint)((int)r[rt] >> (int)(r[rs] & 31)); break;
                        case 0x08: target = r[rs]; dest = 0; break;
                        case 0x20: case 0x21: value = r[rs] + r[rt]; break;
                        case 0x22: case 0x23: value = r[rs] - r[rt]; break;
                        case 0x24: value = r[rs] & r[rt]; break;
                        case 0x25: value = r[rs] | r[rt]; break;
                        case 0x26: value = r[rs] ^ r[rt]; break;
                        case 0x27: value = ~(r[rs] | r[rt]); break;
                        case 0x2A: value = (int)r[rs] < (int)r[rt] ? 1u : 0; break;
                        case 0x2B: value = r[rs] < r[rt] ? 1u : 0; break;
                        default: return null;
                    }
                    break;
                case 0x01:
                    if (rt > 3)
                        return null;
                    bool taken = rt is 0 or 2 ? (int)r[rs] < 0 : (int)r[rs] >= 0;
                    if (taken)
                        target = branch;
                    else if (rt >= 2)
                        skipDelaySlot = true;
                    break;
                case 0x02:
                    target = (next & 0xF0000000) | ((i & 0x3FFFFFF) << 2);
                    break;
                case 0x04: case 0x05: case 0x06: case 0x07:
                case 0x14: case 0x15: case 0x16: case 0x17:
                    bool condition = (op & 3) switch
                    {
                        0 => r[rs] == r[rt],
                        1 => r[rs] != r[rt],
                        2 => (int)r[rs] <= 0,
                        _ => (int)r[rs] > 0,
                    };
                    if (condition)
                        target = branch;
                    else if (op >= 0x14)
                        skipDelaySlot = true;
                    break;
                case 0x08: case 0x09: dest = rt; value = r[rs] + simm; break;
                case 0x0A: dest = rt; value = (int)r[rs] < (int)simm ? 1u : 0; break;
                case 0x0B: dest = rt; value = r[rs] < simm ? 1u : 0; break;
                case 0x0C: dest = rt; value = r[rs] & imm; break;
                case 0x0D: dest = rt; value = r[rs] | imm; break;
                case 0x0E: dest = rt; value = r[rs] ^ imm; break;
                case 0x0F: dest = rt; value = imm << 16; break;
                case 0x20: dest = rt; value = (uint)(sbyte)Load8(r[rs] + simm); break;
                case 0x21: dest = rt; value = (uint)(short)Load16(r[rs] + simm); break;
                case 0x23: dest = rt; value = Load32(r[rs] + simm); break;
                case 0x24: dest = rt; value = Load8(r[rs] + simm); break;
                case 0x25: dest = rt; value = Load16(r[rs] + simm); break;
                case 0x28: case 0x29: case 0x2B:
                    int size = op == 0x28 ? 1 : op == 0x29 ? 2 : 4;
                    uint address = r[rs] + simm;
                    stores ??= new Dictionary<uint, byte>();
                    for (int b = 0; b < size; b++)
                        stores[address + (uint)b] = (byte)(r[rt] >> (8 * (size - 1 - b)));
                    break;
                default:
                    return null;
            }

            if (dest != 0)
                r[dest] = value;

            if (delayed.HasValue)
            {
                pc = delayed.Value;
                delayed = null;
                if (target.HasValue)
                    return null;
            }
            else if (target.HasValue)
            {
                delayed = target;
                pc = next;
            }
            else
                pc = skipDelaySlot ? next + 4 : next;
        }
        return null;
    }

    private const uint JrRa = 0x03E00008;

    private static int WrittenRegister(uint i)
    {
        int op = (int)(i >> 26);
        if (op == 0x00)
            return (int)(i >> 11) & 31;
        if (op == 0x03)
            return 31;
        if (op is >= 0x08 and <= 0x0F or >= 0x20 and <= 0x27)
            return (int)(i >> 16) & 31;
        return 0;
    }

    public static List<uint> FindSegmentedConstants(GfxMemory memory, uint function, int segment)
    {
        var found = new List<uint>();
        if (!memory.IsPointer(function))
            return found;
        var upper = new uint?[32];
        uint end = function + 4 * 512;
        for (uint pc = function; pc < end; pc += 4)
        {
            uint i = memory.U32(pc);
            int op = (int)(i >> 26), rs = (int)(i >> 21) & 31, rt = (int)(i >> 16) & 31;
            uint imm = i & 0xFFFF;
            if ((op == 0x09 || op == 0x0D) && upper[rs].HasValue)
            {
                uint value = op == 0x09 ? upper[rs]!.Value + (uint)(short)imm : upper[rs]!.Value | imm;
                if (found.Count == 0 || found[^1] != value)
                    found.Add(value);
            }
            upper[WrittenRegister(i)] = null;
            if (op == 0x0F)
                upper[rt] = (imm >> 8) == segment ? imm << 16 : null;
            else if (i == JrRa)
                end = pc + 8;
        }
        return found;
    }

    public static uint FindFirstHalfwordLoad(GfxMemory memory, uint function)
    {
        if (!memory.IsPointer(function))
            return 0;
        var upper = new uint?[32];
        uint end = function + 4 * 512;
        for (uint pc = function; pc < end; pc += 4)
        {
            uint i = memory.U32(pc);
            int op = (int)(i >> 26), rs = (int)(i >> 21) & 31, rt = (int)(i >> 16) & 31;
            uint imm = i & 0xFFFF;
            if ((op == 0x21 || op == 0x25) && upper[rs].HasValue)
                return upper[rs]!.Value + (uint)(short)imm;
            upper[WrittenRegister(i)] = null;
            if (op == 0x0F)
                upper[rt] = (imm >> 8) == 0x80 ? imm << 16 : null;
            else if (i == JrRa)
                end = pc + 8;
        }
        return 0;
    }
}
