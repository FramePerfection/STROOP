using System.Numerics;
using StroopNext.Game;
using StroopNext.Map;
using StroopNext.Memory;

namespace StroopNext.Core.Tests;

public class InGameGhostsTests
{
    private const uint Base = 0x80400000, Hook = 0x8027B188, Timer = 0x8032D5D4;
    private const uint SegmentTable = 0x8033B400, Bank4 = 0x80100000;
    private const uint CapNode = Bank4 + 0x40;
    private const uint MarioObjectPointer = 0x80361158, MarioObject = 0x8034BFA8, Root = 0x800F0860, Child = 0x800F0878;

    internal static byte[] GameMemory(byte[] ram) => ram.AsSpan(0, 0x400000).ToArray();

    private static TestRam Booted(uint timer = 0)
    {
        var ram = TestRam.Us().Do(p => p
        .WriteU32(Timer, timer)
        .WriteU32(0x80000000, 0x3C1A8000)
        .WriteU32(SegmentTable + 0x10, Bank4 & 0x1FFFFFFF)
        .WriteU32(CapNode, 0x001B0000).WriteU32(CapNode + 0x14, 0x040119A0)
        .WriteU32(Bank4 + 0x100, 0x02580640)
        .WriteU32(Hook, 0x0C0DEAD0)
        .WriteU32(MarioObjectPointer, MarioObject).WriteU16(MarioObject + 2, 0x0021).WriteU32(MarioObject + 0x14, Root)
        .WriteU16(Root, 0x0028).WriteU32(Root + 4, Root).WriteU32(Root + 8, Root).WriteU32(Root + 0x10, Child)
        .WriteU16(Child, 0x001C).WriteU32(Child + 4, Child).WriteU32(Child + 8, Child).WriteU32(Child + 0xC, Root)
        .WriteU32(Base + 0x7FF8, 0x01010101));
        ram.Image();
        return ram;
    }

    private static Session.GameFrame F(TestRam ram) => Session.GameFrame.Create(new RamImage(ram.Bytes));

    private static void LoopRuns(TestRam ram) => W(ram, p => p.WriteU16(MarioObject + 2, 0x0061));

    private static void W(TestRam ram, Action<RamPatch> write)
    {
        var patch = new RamPatch();
        write(patch);
        patch.ApplyTo(ram.Bytes);
    }

    [Theory]
    [InlineData(GameVersion.US, 0x8027B188u, 0x80277988u)]
    [InlineData(GameVersion.JP, 0x8027ABD8u, 0x802773D8u)]
    public void Hack_files_parse_with_the_expected_entries(GameVersion version, uint hook, uint holp)
    {
        var entries = InGameGhosts.HackFor(version).Entries.ToDictionary(e => e.Address, e => e.Bytes.Length);
        Assert.Equal(4, entries[hook]);
        Assert.Equal(0x1E4, entries[0x80408000]);
        Assert.Equal(0xC8, entries[0x80408200]);
        Assert.Equal(0x24, entries[holp]);
        Assert.Equal(7, entries.Count);
    }

    [Fact]
    public void Install_then_restore_puts_every_byte_back()
    {
        var ram = Booted();
        var before = (byte[])ram.Bytes.Clone();
        var ghosts = new InGameGhosts();
        Assert.Equal(InGameGhostState.Disabled, ghosts.State(F(ram)));

        var install = ghosts.Install(F(ram), out string? error);
        Assert.Null(error);
        install!.ApplyTo(ram.Bytes);
        var image = new RamImage(ram.Bytes);
        Assert.Equal(0x0C102000u, image.U32(Hook));
        Assert.Equal(0u, image.U32(Base + 0x7FF8));
        Assert.Equal(InGameGhostState.Installed, ghosts.State(F(ram)));

        Assert.Equal(0x001B, image.U16(CapNode));
        Assert.False(ghosts.StepTwoPending(F(ram)));
        Assert.Null(ghosts.ColoredCaps(F(ram)));
        LoopRuns(ram);
        Assert.True(ghosts.StepTwoPending(F(ram)));
        ghosts.ColoredCaps(F(ram))!.ApplyTo(ram.Bytes);
        image = new RamImage(ram.Bytes);
        Assert.Equal(0x012A, image.U16(CapNode));
        Assert.Equal(Base + 0x8200, image.U32(CapNode + 0x14));
        Assert.Equal(0x02587FFFu, image.U32(Bank4 + 0x100));
        Assert.Null(ghosts.ColoredCaps(F(ram)));

        var loop = InGameGhosts.HackFor(GameVersion.US).Entries.Single(e => e.Address == 0x80408000).Bytes;
        byte[] LoopNow() => Enumerable.Range(0, loop.Length).Select(i => new RamImage(ram.Bytes).U8(Base + 0x8800 + (uint)i)).ToArray();
        Assert.Equal(loop, LoopNow());

        Assert.True(ghosts.CanRestore(F(ram)));
        new InGameGhosts().Restore(F(ram))!.ApplyTo(ram.Bytes);
        W(ram, p => p.WriteU16(MarioObject + 2, 0x0021));
        Assert.Equal(GameMemory(before), GameMemory(ram.Bytes));
        Assert.False(ghosts.CanRestore(F(ram)));
        Assert.Equal(InGameGhostState.Disabled, ghosts.State(F(ram)));
        Assert.Equal(loop, LoopNow());
    }

    [Fact]
    public void Another_base_relocates_the_code_and_its_luis()
    {
        var ram = Booted();
        var ghosts = new InGameGhosts(0x8060);
        ghosts.Install(F(ram), out _)!.ApplyTo(ram.Bytes);
        LoopRuns(ram);
        ghosts.ColoredCaps(F(ram))!.ApplyTo(ram.Bytes);
        var image = new RamImage(ram.Bytes);
        Assert.Equal(0x0C182000u, image.U32(Hook));
        Assert.Equal(0x8060, image.U16(0x80608800 + 0x44 + 2));
        Assert.Equal(0x8061, image.U16(0x80608800 + 0xF8 + 2));
        Assert.Equal(0x8070, image.U16(0x80608800 + 0x50 + 2));
        Assert.Equal(0x08182200u, image.U32(0x80608000 + (uint)new InGameGhosts(0x8060).Guard().Length - 8));
        Assert.Equal(0x80608200u, image.U32(CapNode + 0x14));
    }

    [Fact]
    public void Unavailable_without_expansion_pak_boot_or_a_supported_version()
    {
        Assert.Contains("not booted", InGameGhosts.Unavailable(TestRam.Us().Frame()));
        Assert.Null(InGameGhosts.Unavailable(Booted().Frame()));
        var booted = Booted();
        var small = new RamImage(booted.Bytes.AsSpan(0, N64.RamSize4MB).ToArray());
        Assert.Contains("Expansion Pak", InGameGhosts.Unavailable(Session.GameFrame.Create(small)));
        Assert.Null(new InGameGhosts().Install(Session.GameFrame.Create(small), out _));
    }

    [Fact]
    public void Feed_fills_the_ring_buffer_big_endian_and_hides_ghosts_without_a_frame_now()
    {
        var ram = Booted(timer: 1000);
        var ghosts = new InGameGhosts();
        ghosts.Install(F(ram), out _)!.ApplyTo(ram.Bytes);

        var ghost = new Ghost { PlaybackBaseFrame = 900 };
        for (uint key = 98; key <= 110; key++)
            ghost.Frames[key] = new GhostFrame(new Vector3(key, 2, 3), 0x2A, (short)key, 0x10, 0x4000 + key, 0x20);
        var feed = ghosts.Feed(F(ram), [(ghost, MapColor.Rgb(0, 255, 0))], transparent: true);
        feed.ApplyTo(ram.Bytes);
        var image = new RamImage(ram.Bytes);

        Assert.Equal(1, image.U8(Base + 0x7FFF));
        uint slot = Base + 0x9B00 + (1000 & 0x7F) * 0x20;
        Assert.Equal(100f, image.F32(slot));
        Assert.Equal(0x2Au, image.U32(slot + 0x0C));
        Assert.Equal(0x10, image.U16(slot + 0x10));
        Assert.Equal(0x4000 + 100, image.U16(slot + 0x12));
        Assert.Equal(0x20, image.U16(slot + 0x14));
        Assert.Equal(100, image.U16(slot + 0x16));
        Assert.Equal(0u, image.U32(slot + 0x18));
        Assert.Equal(InGameGhosts.Lights(MapColor.Rgb(0, 255, 0)), Enumerable.Range(0, 24).Select(i => image.U8(Base + 0x8320 + (uint)i)).ToArray());

        Assert.DoesNotContain(feed.Writes, w => w.Address == 0x80000061);

        var later = new Ghost { PlaybackBaseFrame = 5000 };
        later.Frames[0] = new GhostFrame(Vector3.One, 0, 0, 0, 0, 0);
        ghosts.Feed(F(ram), [(later, MapColor.Rgb(0, 0, 255))], true).ApplyTo(ram.Bytes);
        Assert.Equal(30000f, new RamImage(ram.Bytes).F32(slot));
    }

    [Fact]
    public void Transparency_goes_to_existing_ghost_nodes()
    {
        var ram = Booted();
        var ghosts = new InGameGhosts();
        ghosts.Install(F(ram), out _)!.ApplyTo(ram.Bytes);
        W(ram, p => p.WriteU32(Base + 0x7FF8, Base + 0x7FF8 - 0x64));
        var feed = ghosts.Feed(F(ram), [(new Ghost(), MapColor.Rgb(1, 1, 1))], transparent: true);
        Assert.Contains(feed.Writes, w => w.Address == Base + 0x7FF8 - 0x64 + 0x61 && w.Bytes[0] == 1);
        Assert.Equal(InGameGhostState.Running, ghosts.State(F(ram)));
    }

    [Fact]
    public void Disable_asks_the_game_first_and_feeding_stops_meanwhile()
    {
        var ram = Booted();
        var ghosts = new InGameGhosts();
        ghosts.Install(F(ram), out _)!.ApplyTo(ram.Bytes);
        W(ram, p => p.WriteU32(Base + 0x7FF8, Base + 0x7FF8 - 0x64));
        ghosts.RequestDisable().ApplyTo(ram.Bytes);
        W(ram, p => p.WriteU32(Base + 0x7FF8, Base + 0x7FF8 - 0x64));
        Assert.Equal(InGameGhostState.Disabling, ghosts.State(F(ram)));
        Assert.True(ghosts.Feed(F(ram), [(new Ghost(), MapColor.Rgb(1, 1, 1))], true).IsEmpty);
        Assert.False(ghosts.ReadyToRestore(F(ram)));
        W(ram, p => p.WriteU32(Base + 0x7FF8, 0));
        Assert.True(ghosts.ReadyToRestore(F(ram)));
    }
}
