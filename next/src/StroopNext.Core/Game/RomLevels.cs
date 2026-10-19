using System.Buffers.Binary;
using StroopNext.Memory;
using StroopNext.Session;

namespace StroopNext.Game;

/// <summary>An area of a level read from the ROM, with the warp nodes Mario can arrive at.</summary>
public sealed record RomArea(int Index, IReadOnlyList<int> Arrivals)
{
    /// <summary>The usual entry (0x0A) when the area has it, else its first arrival.</summary>
    public int Entry => Arrivals.Contains(0x0A) ? 0x0A : Arrivals[0];
}

// Every level of the running game, read from its ROM (for ROM hacks, whose levels, areas and warp nodes are their own):
//  - the level table is in segment 0x15 (resident in RAM): JUMP_IF(level == N, script) commands, the script being
//    EXECUTE(segment, rom start, rom end, entry), where the level's script lives in the ROM;
//  - each level script is walked through its LOAD_RAW segments and JUMP_LINKs: AREA ... END_AREA blocks with their
//    WARP_NODEs and OBJECTs;
//  - an arrival is a warp node with an object of a warp behaviour (sWarpBhvSpawnTable, read from RAM) carrying the
//    node id in its second parameter byte: load_obj_warp_nodes() links only those, and a warp to a node with no
//    object crashes the level script.
// Compressed segments (MIO0) are not read: their areas are skipped. Checked on SM64 US, Power Star Madness and B3313.
public static class RomLevels
{
    private const int MaxCommands = 4000, MaxDepth = 12, WarpBehaviors = 20;

    public static IReadOnlyDictionary<int, IReadOnlyList<RomArea>>? Scan(byte[] rom, GameFrame frame)
    {
        if (frame.Version is not { } version || Sm64Addresses.SegmentTable.For(version) is not uint segments
            || Sm64Addresses.WarpBhvSpawnTable.For(version) is not uint warpTable)
            return null;
        var ram = frame.Ram;
        byte[] z64 = ToBigEndian(rom);
        var warpBehaviors = Enumerable.Range(0, WarpBehaviors).Select(i => ram.U32(warpTable + 4 * (uint)i)).ToHashSet();
        uint segment15 = (ram.U32(segments + 4 * 0x15) & 0x1FFFFFFF) | 0x80000000;
        var levels = new SortedDictionary<int, IReadOnlyList<RomArea>>();
        for (uint offset = 0; offset < 0x4000 && ram.IsValid(segment15 + offset, 12); offset += 4)
        {
            uint a = segment15 + offset;
            if (ram.U8(a) != 0x0C || ram.U8(a + 1) != 0x0C || ram.U8(a + 2) != 2)
                continue;
            uint level = ram.U32(a + 4), target = ram.U32(a + 8);
            if (target >> 24 != 0x15 || level is < 1 or > 63 || levels.ContainsKey((int)level))
                continue;
            uint execute = segment15 + (target & 0xFFFFFF);
            if (!ram.IsValid(execute, 16) || ram.U8(execute) != 0x00 || ram.U8(execute + 1) != 0x10)
                continue;
            var areas = Walk(z64, ram.U8(execute + 3), ram.U32(execute + 4), ram.U32(execute + 8), ram.U32(execute + 12), warpBehaviors);
            if (areas.Count > 0)
                levels[(int)level] = areas;
        }
        return levels;
    }

    private static List<RomArea> Walk(byte[] rom, int segment, uint start, uint end, uint entry, HashSet<uint> warpBehaviors)
    {
        var segments = new Dictionary<int, (uint Start, uint End)> { [segment] = (start, end) };
        var nodes = new SortedDictionary<int, HashSet<int>>();
        var objects = new Dictionary<int, HashSet<int>>();
        var seen = new HashSet<(int, uint)>();
        int? area = null;

        void Run(int seg, uint offset, int depth)
        {
            if (depth > MaxDepth || !segments.TryGetValue(seg, out var range))
                return;
            for (int n = 0; n < MaxCommands; n++)
            {
                uint a = range.Start + offset;
                if (!seen.Add((seg, offset)) || offset >= range.End - range.Start || a + 2 > rom.Length)
                    return;
                byte command = rom[a], length = rom[a + 1];
                if (length == 0 || a + length > rom.Length)
                    return;
                switch (command)
                {
                    case 0x17:   // LOAD_RAW(segment, rom start, rom end)
                        segments[rom[a + 3]] = (U32(rom, a + 4), U32(rom, a + 8));
                        break;
                    case 0x1F:   // AREA(index, geo)
                        area = rom[a + 2];
                        nodes.TryAdd(area.Value, []);
                        objects.TryAdd(area.Value, []);
                        break;
                    case 0x20:   // END_AREA
                        area = null;
                        break;
                    case 0x26 or 0x27 when area is int inArea:   // WARP_NODE / PAINTING_WARP_NODE(id, level, area, node)
                        nodes[inArea].Add(rom[a + 2]);
                        break;
                    case 0x24 when area is int inArea && length >= 0x18:   // OBJECT(..., behParam, behavior)
                        if (warpBehaviors.Contains(U32(rom, a + 0x14)))
                            objects[inArea].Add(rom[a + 0x11]);
                        break;
                    case 0x05 or 0x06:   // JUMP / JUMP_LINK
                        uint target = U32(rom, a + 4);
                        Run((int)(target >> 24), target & 0xFFFFFF, depth + 1);
                        if (command == 0x05)
                            return;
                        break;
                    case 0x02 or 0x07:   // EXIT / RETURN
                        return;
                }
                offset += length;
            }
        }

        Run(segment, entry & 0xFFFFFF, 0);
        return nodes.Select(kv => new RomArea(kv.Key, kv.Value.Where(objects[kv.Key].Contains).Order().ToList()))
            .Where(a => a.Arrivals.Count > 0).ToList();
    }

    private static uint U32(byte[] rom, uint a) => BinaryPrimitives.ReadUInt32BigEndian(rom.AsSpan((int)a, 4));

    /// <summary>The ROM in big-endian (z64) order, whatever its dump format.</summary>
    public static byte[] ToBigEndian(byte[] rom)
    {
        if (rom.Length < 4 || rom[0] == 0x80)
            return rom;
        var z64 = (byte[])rom.Clone();
        if (rom[0] == 0x37)
            for (int i = 0; i + 1 < z64.Length; i += 2)
                (z64[i], z64[i + 1]) = (z64[i + 1], z64[i]);
        else if (rom[0] == 0x40)
            for (int i = 0; i + 3 < z64.Length; i += 4)
                (z64[i], z64[i + 1], z64[i + 2], z64[i + 3]) = (z64[i + 3], z64[i + 2], z64[i + 1], z64[i]);
        return z64;
    }
}
