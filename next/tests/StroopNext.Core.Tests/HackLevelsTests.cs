using StroopNext.Game;
using StroopNext.Memory;
using Xunit;

namespace StroopNext.Core.Tests;

public sealed class HackLevelsTests
{
    private const uint CurrentLevel = 0x8032DDF8, CurrentArea = 0x8032DDCC, Areas = 0x8033B8D0, SegmentTable = 0x8033B400;
    private const uint LevelToCourse = 0x8032DD98, Nodes = 0x80500000, Segment2 = 0x80100000;

    private static byte[] Text(string s)
    {
        var bytes = s.Select(c => c switch
        {
            >= '0' and <= '9' => (byte)(c - '0'),
            >= 'A' and <= 'Z' => (byte)(0x0A + c - 'A'),
            ' ' => (byte)0x9E,
            _ => (byte)0x9F,
        }).ToList();
        bytes.Add(0xFF);
        return [.. bytes];
    }

    // A hack in level 16 area 1, whose area has two warps: to level 9 area 1 node 0x0A (a node with a spawn object
    // here too) and to level 16 area 2 node 0x05 (no spawn object here); its course 1 is called "GREENS".
    private static TestRam Hack() => TestRam.Us().Do(p =>
    {
        uint area1 = Areas + Sm64Addresses.AreaStructSize;
        p.WriteS16(CurrentLevel, 16).WriteU32(CurrentArea, area1).WriteU32(area1 + 0x14, Nodes);
        p.WriteU8(Nodes, 0x0A).WriteU8(Nodes + 1, 9).WriteU8(Nodes + 2, 1).WriteU8(Nodes + 3, 0x0A)
            .WriteU32(Nodes + 4, 0x80500100).WriteU32(Nodes + 8, Nodes + 0x10);
        p.WriteU8(Nodes + 0x10, 0x20).WriteU8(Nodes + 0x11, 16).WriteU8(Nodes + 0x12, 2).WriteU8(Nodes + 0x13, 0x05)
            .WriteU32(Nodes + 0x14, 0).WriteU32(Nodes + 0x18, 0);
        p.WriteU32(SegmentTable + 8, Segment2 & 0x1FFFFFFF);
        uint strings = Segment2 + 0x1000;
        for (uint i = 0; i < 26; i++)
        {
            byte[] text = Text(i == 0 ? " 1 GREENS" : $"{i + 1} COURSE");
            p.WriteBytes(strings + 0x40 * i, text).WriteU32(Segment2 + 0x200 + 4 * i, 0x02000000 | (0x1000 + 0x40 * i));
        }
        p.WriteU8(LevelToCourse + 8, 1);
    });

    [Fact]
    public void Targets_are_the_area_warps_with_the_hacks_own_names()
    {
        var targets = HackLevels.Targets(Hack().Frame());
        Assert.Contains(new WarpTarget(9, 1, 0x0A, "GREENS (9) · area 1 (node 0x0A)"), targets);
        Assert.Contains(new WarpTarget(16, 2, 0x05, "Level 16 · area 2 (node 0x05)"), targets);
        Assert.Contains(new WarpTarget(16, 1, 0x0A, "This area · node 0x0A"), targets);
        Assert.DoesNotContain(targets, t => t.Label == "This area · node 0x20");   // no spawn object: a crash
    }

    [Fact]
    public void Course_names_come_from_the_games_text()
    {
        var names = HackLevels.CourseNames(Hack().Frame());
        Assert.NotNull(names);
        Assert.Equal(" 1 GREENS", names![0]);
        Assert.Equal(26, names.Count);
    }

    [Fact]
    public void No_area_no_targets()
        => Assert.Empty(HackLevels.Targets(TestRam.Us().Frame()));

    [Fact]
    public void A_raw_destination_goes_into_sWarpDest()
    {
        var frame = Hack().Frame();
        var plan = LevelWarp.Plan(frame, 9, 1, 0x0A, 1);
        Assert.Equal(3, plan.Count);
        var ram = new TestRam();
        foreach (var patch in plan)
            patch.ApplyTo(ram.Bytes);
        Assert.Equal(0x0109010Au, new RamImage(ram.Bytes).U32(0x8033B248));
    }
}
