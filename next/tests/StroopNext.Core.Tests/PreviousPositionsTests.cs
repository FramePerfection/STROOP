using System.Numerics;
using StroopNext.Map;
using StroopNext.Memory;

namespace StroopNext.Core.Tests;

public class PreviousPositionsTests
{
    private const uint Timer = 0x8032D5D4;

    private static void WriteFrame(RamPatch p, uint gt, int quarterSteps, Func<int, Vector3> position)
    {
        uint frameBase = PreviousPositions.BufferAddress + (gt & 0x7F) * PreviousPositions.RecordsPerFrame * PreviousPositions.RecordSize;
        int records = PreviousPositions.FrameStages + quarterSteps * 4;
        for (int i = 0; i < records; i++)
        {
            uint a = frameBase + (uint)i * PreviousPositions.RecordSize;
            var v = position(i);
            p.WriteF32(a, v.X).WriteF32(a + 4, v.Y).WriteF32(a + 8, v.Z).WriteU16(a + 0xC, (ushort)gt).WriteU16(a + 0xE, (ushort)(i * 100));
        }
    }

    [Fact]
    public void Reads_every_stage_of_the_recorded_frames_oldest_first()
    {
        var ram = TestRam.Us().Do(p =>
        {
            p.WriteU32(Timer, 1000);
            WriteFrame(p, 998, 4, i => new Vector3(i, 0, 0));
            WriteFrame(p, 999, 4, i => new Vector3(100 + i, 0, 0));
        });
        var points = PreviousPositions.Read(ram.Frame(), 2, skipIdentical: false);
        Assert.Equal(2 * PreviousPositions.RecordsPerFrame, points.Count);
        Assert.Equal(998u, points[0].GlobalTimer);
        Assert.Equal(0, points[0].Stage);
        Assert.Null(points[0].QuarterStep);
        Assert.Equal(3, points[PreviousPositions.RecordsPerFrame - 1].QuarterStep);
        Assert.Equal(10, points[PreviousPositions.RecordsPerFrame - 1].Stage);
        Assert.Equal(999u, points[^1].GlobalTimer);
    }

    [Fact]
    public void A_frame_that_stopped_early_is_flagged_with_its_quarter_steps()
    {
        var ram = TestRam.Us().Do(p =>
        {
            p.WriteU32(Timer, 1000);
            WriteFrame(p, 999, 2, i => new Vector3(i, 0, 0));
        });
        var points = PreviousPositions.Read(ram.Frame(), 1, skipIdentical: false);
        Assert.Equal(PreviousPositions.FrameStages + 2 * 4, points.Count);
        Assert.Equal(2, points[^1].EarlyQuarterSteps);
    }

    [Fact]
    public void Identical_consecutive_points_are_skipped_on_request()
    {
        var ram = TestRam.Us().Do(p =>
        {
            p.WriteU32(Timer, 1000);
            WriteFrame(p, 999, 4, _ => new Vector3(5, 5, 5));
        });
        Assert.Equal(PreviousPositions.RecordsPerFrame, PreviousPositions.Read(ram.Frame(), 1, skipIdentical: true).Count);
    }

    [Fact]
    public void Without_the_patched_rom_nothing_is_read()
    {
        var frame = TestRam.Us().Do(p => p.WriteU32(Timer, 1000)).Frame();
        Assert.Empty(PreviousPositions.Read(frame, 16, skipIdentical: true));
    }
}
