using StroopNext.Game;
using StroopNext.Memory;

namespace StroopNext.Core.Tests;

public class MemoryTests
{
    [Fact]
    public void Word_swapped_layout_reverses_each_aligned_word()
    {
        var bytes = new byte[16];
        new RamPatch().WriteU32(0x80000004, 0x11223344).ApplyTo(bytes);
        Assert.Equal(new byte[] { 0x44, 0x33, 0x22, 0x11 }, bytes[4..8]);
    }

    [Fact]
    public void Reads_return_big_endian_meaning_for_every_size_and_alignment()
    {
        var bytes = new byte[16];
        new RamPatch().WriteBytes(0x80000000, [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08]).ApplyTo(bytes);
        var ram = new RamImage(bytes);

        Assert.Equal(0x01, ram.U8(0x80000000));
        Assert.Equal(0x04, ram.U8(0x80000003));
        Assert.Equal(0x0102, ram.U16(0x80000000));
        Assert.Equal(0x0304, ram.U16(0x80000002));
        Assert.Equal(0x0203, ram.U16(0x80000001));
        Assert.Equal(0x01020304u, ram.U32(0x80000000));
        Assert.Equal(0x03040506u, ram.U32(0x80000002));
        Assert.Equal(0x0102030405060708ul, (ulong)BitConverter.DoubleToInt64Bits(ram.F64(0x80000000)));
    }

    [Fact]
    public void Floats_and_signed_values_round_trip()
    {
        var bytes = new byte[16];
        new RamPatch().WriteF32(0x80000000, -818.66f).WriteS16(0x80000004, -2).WriteS8(0x80000007, -3).ApplyTo(bytes);
        var ram = new RamImage(bytes);
        Assert.Equal(-818.66f, ram.F32(0x80000000));
        Assert.Equal(-2, ram.S16(0x80000004));
        Assert.Equal(-3, ram.S8(0x80000007));
    }

    [Fact]
    public void Out_of_range_reads_return_zero_and_pointers_are_validated()
    {
        var ram = new RamImage(new byte[16]);
        Assert.Equal(0u, ram.U32(0x80000010));
        Assert.Equal(0, ram.U8(0x80FFFFFF));
        Assert.True(ram.IsPointer(0x8000000C));
        Assert.False(ram.IsPointer(0x8000000D + 4));
        Assert.False(ram.IsPointer(0x0000000C));
    }

    [Fact]
    public void Patch_runs_merge_contiguous_bytes_and_last_write_wins()
    {
        var patch = new RamPatch().WriteU32(0x80000000, 0xAABBCCDD).WriteU32(0x80000004, 1).WriteU8(0x80000000, 0x11);
        var runs = patch.ToSwappedRuns();
        Assert.Single(runs);
        Assert.Equal(0, runs[0].Offset);
        Assert.Equal(new byte[] { 0xDD, 0xCC, 0xBB, 0x11, 1, 0, 0, 0 }, runs[0].Bytes);
    }

    [Fact]
    public void Version_is_detected_and_unknown_roms_are_not_guessed()
    {
        Assert.Equal(GameVersion.US, GameVersionDetector.Detect(TestRam.Us().Image()));
        Assert.Null(GameVersionDetector.Detect(new TestRam().Image()));
        var jp = new TestRam().Do(p => p.WriteU32(GameVersionDetector.TellAddress, 0x27BD0020));
        Assert.Equal(GameVersion.JP, GameVersionDetector.Detect(jp.Image()));
    }

    [Fact]
    public void Missing_versioned_addresses_stay_unknown()
    {
        Assert.Null(Sm64Layout.For(GameVersion.EU).MarioObjectPointer);
        Assert.Equal(0x8033B170u, Sm64Layout.For(GameVersion.US).MarioStruct);
    }
}
