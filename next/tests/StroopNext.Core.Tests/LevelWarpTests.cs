using StroopNext.Game;
using StroopNext.Memory;

namespace StroopNext.Core.Tests;

public class LevelWarpTests
{
    private static TestRam Jp()
    {
        var ram = new TestRam();
        ram.Patch.WriteU32(GameVersionDetector.TellAddress, 0x27BD0020);
        return ram;
    }

    private static TestRam InLevel(TestRam ram, uint marioStruct, uint marioObject)
        => ram.Do(p => p.WriteU32(marioStruct + MarioOffsets.MarioObject, marioObject));

    private static GameLevel Bob => GameLevels.Find(9)!;

    [Fact]
    public void The_plan_writes_the_destination_last()
    {
        var frame = TestRam.Us().Frame();
        var plan = LevelWarp.Plan(frame, Bob, Bob.Areas[0], 3);
        Assert.Equal(3, plan.Count);

        var ram = TestRam.Us();
        plan[0].ApplyTo(ram.Bytes);
        plan[1].ApplyTo(ram.Bytes);
        var image = new RamImage(ram.Bytes);
        Assert.Equal(3, image.S16(Sm64Addresses.CurrentAct.US!.Value));
        Assert.Equal(1, image.U8(Sm64Addresses.WarpCheckpointActive.US!.Value));
        Assert.Equal(0, image.S16(Sm64Addresses.TransitionTimer.US!.Value));
        Assert.Equal(LevelWarp.PlayModeChangeLevel, image.S16(Sm64Addresses.PlayMode.US!.Value));
        Assert.Equal(0, image.U8(Sm64Addresses.WarpDestination.US!.Value));

        plan[2].ApplyTo(ram.Bytes);
        image = new RamImage(ram.Bytes);
        uint destination = Sm64Addresses.WarpDestination.US!.Value;
        Assert.Equal(LevelWarp.WarpTypeChangeLevel, image.U8(destination));
        Assert.Equal(9, image.U8(destination + 1));
        Assert.Equal(1, image.U8(destination + 2));
        Assert.Equal(0x0A, image.U8(destination + 3));
        Assert.Equal(0u, image.U32(destination + 4));
    }

    [Fact]
    public void The_destination_is_one_contiguous_write()
    {
        var plan = LevelWarp.Plan(TestRam.Us().Frame(), Bob, Bob.Areas[0], 1);
        Assert.Single(plan[2].ToSwappedRuns());
        Assert.Single(plan[1].ToSwappedRuns());
    }

    [Fact]
    public void The_progress_follows_the_warp_from_start_to_arrival()
    {
        var us = TestRam.Us();
        InLevel(us, TestRam.UsMario, 0x80361000);
        us.Do(p => p.WriteS16(Sm64Addresses.CurrentLevel.US!.Value, 6));
        var before = LevelWarp.Read(us.Frame());
        Assert.NotNull(before);
        Assert.False(before.Value.InFlight);
        Assert.True(before.Value.HasMario);

        foreach (var step in LevelWarp.Plan(us.Frame(), Bob, Bob.Areas[0], 1))
            step.ApplyTo(us.Bytes);
        Assert.True(LevelWarp.Read(us.Frame())!.Value.InFlight);
    }

    [Fact]
    public void Arrival_needs_the_level_the_area_and_no_warp_in_flight()
    {
        var progress = new WarpProgress(9, 1, 0, 0, true);
        Assert.True(progress.Arrived(9, 1));
        Assert.False(progress.Arrived(9, 2));
        Assert.False(progress.Arrived(5, 1));
        Assert.False((progress with { PlayMode = LevelWarp.PlayModeChangeLevel }).Arrived(9, 1));
        Assert.False((progress with { WarpType = 1 }).Arrived(9, 1));
        Assert.False((progress with { HasMario = false }).Arrived(9, 1));
        Assert.True((progress with { PlayMode = 3 }).Arrived(9, 1));
    }

    [Fact]
    public void An_unknown_game_cannot_change_level()
    {
        Assert.NotNull(LevelWarp.Unavailable(null));
        var unknown = new TestRam().Frame();
        Assert.Contains("not recognised", LevelWarp.Unavailable(unknown));
        Assert.Empty(LevelWarp.Plan(unknown, Bob, Bob.Areas[0], 1));
        Assert.Null(LevelWarp.Read(unknown));
    }

    [Fact]
    public void Versions_without_the_variables_are_refused()
    {
        var sh = new TestRam();
        sh.Patch.WriteU32(GameVersionDetector.TellAddress, 0x8F250004);
        var frame = sh.Frame();
        Assert.Equal(GameVersion.SH, frame.Version);
        Assert.Contains("US and JP", LevelWarp.Unavailable(frame));
        Assert.Empty(LevelWarp.Plan(frame, Bob, Bob.Areas[0], 1));
    }

    [Fact]
    public void No_level_running_is_refused()
    {
        Assert.Contains("No level", LevelWarp.Unavailable(TestRam.Us().Frame()));
        Assert.Null(LevelWarp.Unavailable(InLevel(TestRam.Us(), TestRam.UsMario, 0x80361000).Frame()));
    }

    [Fact]
    public void Japan_skips_the_unnamed_checkpoint_flag()
    {
        var frame = Jp().Frame();
        var plan = LevelWarp.Plan(frame, Bob, Bob.Areas[0], 1);
        Assert.Equal(3, plan.Count);
        var ram = Jp();
        foreach (var step in plan)
            step.ApplyTo(ram.Bytes);
        var image = new RamImage(ram.Bytes);
        Assert.Equal(LevelWarp.WarpTypeChangeLevel, image.U8(0x80339ED8));
        Assert.Equal(LevelWarp.PlayModeChangeLevel, image.S16(0x80339EC8));
        Assert.Equal(1, image.S16(0x8033A758));
    }

    [Fact]
    public void The_act_stays_within_one_to_six()
    {
        var frame = TestRam.Us().Frame();
        var ram = TestRam.Us();
        LevelWarp.Plan(frame, Bob, Bob.Areas[0], 99)[0].ApplyTo(ram.Bytes);
        Assert.Equal(6, new RamImage(ram.Bytes).S16(Sm64Addresses.CurrentAct.US!.Value));
    }

    [Fact]
    public void The_level_table_is_consistent()
    {
        Assert.Equal(GameLevels.All.Count, GameLevels.All.Select(l => l.Id).Distinct().Count());
        foreach (var level in GameLevels.All)
        {
            Assert.InRange(level.Id, 4, 36);
            Assert.NotEmpty(level.Areas);
            Assert.Equal(level.Areas.Count, level.Areas.Select(a => a.Index).Distinct().Count());
            Assert.All(level.Areas, a => Assert.InRange(a.Index, 1, 8));
            Assert.All(level.Areas, a => Assert.True(a.ArrivalNode < 0xF0, "credits nodes are not levels"));
        }
        Assert.NotNull(GameLevels.Find(16));
        Assert.Null(GameLevels.Find(25));
    }
}
