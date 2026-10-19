using System.Text;
using StroopNext.Memory;
using StroopNext.Session;

namespace StroopNext.Game;

/// <summary>A warp the game itself can take: level, area and a warp node of that area.</summary>
public readonly record struct WarpTarget(int Level, int Area, int Node, string Label);

// Levels read from the running game instead of the original list, for ROM hacks: their levels, areas and warp nodes
// differ, and a warp to a node an area does not have crashes the level script.
//  - the current area's warp nodes (gCurrentArea->warpNodes) list where its warps lead: those destinations are valid
//    by construction, the game takes them itself; the area's own nodes with a spawn object are valid arrivals in it;
//  - the course names come from the game's own text (seg2_course_name_table, found in segment 2 by its shape: 25+
//    segmented pointers to texts, the first starting with "1"), so a hack shows its own names.
public static class HackLevels
{
    private const uint AreaWarpNodes = 0x14;
    private const int MaxNodes = 256;

    public static IReadOnlyList<WarpTarget> Targets(GameFrame frame)
    {
        if (frame.Version is not { } version || Sm64Addresses.CurrentAreaPointer.For(version) is not uint areaPointer
            || frame.LevelIndex is not short level || frame.AreaIndex is not int area)
            return [];
        var ram = frame.Ram;
        uint current = ram.U32(areaPointer);
        if (!ram.IsPointer(current))
            return [];
        var names = CourseNames(frame);
        var targets = new List<WarpTarget>();
        var seen = new HashSet<(int, int, int)>();
        var arrivals = new List<int>();
        uint node = ram.U32(current + AreaWarpNodes);
        for (int i = 0; i < MaxNodes && ram.IsPointer(node); i++, node = ram.U32(node + 8))
        {
            int id = ram.U8(node), destLevel = ram.U8(node + 1), destArea = ram.U8(node + 2), destNode = ram.U8(node + 3);
            if (id < 0xF0 && ram.IsPointer(ram.U32(node + 4)))
                arrivals.Add(id);
            if (destLevel is > 0 and < 64 && destArea is > 0 and < 8 && seen.Add((destLevel, destArea, destNode)))
                targets.Add(new WarpTarget(destLevel, destArea, destNode,
                    $"{LevelName(frame, names, destLevel)} · area {destArea} (node 0x{destNode:X2})"));
        }
        foreach (int id in arrivals.Distinct().OrderBy(n => n))
            targets.Add(new WarpTarget(level, area, id, $"This area · node 0x{id:X2}"));
        return targets;
    }

    public static string LevelName(GameFrame frame, IReadOnlyList<string>? names, int level)
    {
        if (frame.Version is { } version && Sm64Addresses.LevelToCourseNum.For(version) is uint table && level is >= 1 and <= 38
            && names is not null && frame.Ram.S8(table + (uint)(level - 1)) is var course && course >= 1 && course <= names.Count
            && names[course - 1].Trim().TrimStart("0123456789".ToCharArray()).Trim() is { Length: > 0 } name)
            return $"{name} ({level})";
        return $"Level {level}";
    }

    public static IReadOnlyList<string>? CourseNames(GameFrame frame)
    {
        if (frame.Version is not { } version || Sm64Addresses.SegmentTable.For(version) is not uint segments)
            return null;
        var ram = frame.Ram;
        uint segment2 = (ram.U32(segments + 8) & 0x1FFFFFFF) | 0x80000000;
        for (uint offset = 0; offset < 0x40000; offset += 4)
        {
            uint table = segment2 + offset;
            if (!ram.IsValid(table, 4 * 25))
                break;
            if ((ram.U32(table) >> 24) != 2)
                continue;
            var names = new List<string>();
            for (uint i = 0; i < 30; i++)
            {
                uint pointer = ram.U32(table + 4 * i);
                if (pointer >> 24 != 2 || Decode(ram, segment2 + (pointer & 0xFFFFFF)) is not { } text)
                    break;
                names.Add(text);
            }
            if (names.Count >= 25 && names[0].TrimStart().StartsWith('1') && names.Count(n => n.Contains('·')) <= 2)
                return names;
        }
        return null;
    }

    private static string? Decode(RamImage ram, uint address)
    {
        var text = new StringBuilder();
        for (uint k = 0; k < 40 && ram.IsValid(address + k); k++)
        {
            byte b = ram.U8(address + k);
            if (b == 0xFF)
                return text.ToString();
            text.Append(b switch
            {
                <= 0x09 => (char)('0' + b),
                >= 0x0A and <= 0x23 => (char)('A' + b - 0x0A),
                >= 0x24 and <= 0x3D => (char)('a' + b - 0x24),
                0x9E => ' ', 0x9F => '-', 0x3E => '\'', 0x3F => '.', 0x6F => ',', 0xE1 => '(', 0xE3 => ')',
                0xE4 => '+', 0xE5 => '&', 0xE6 => ':', 0xF2 => '!', 0xF4 => '?', 0xD0 => '/', 0x54 => '"',
                _ => '·',
            });
        }
        return null;
    }
}
