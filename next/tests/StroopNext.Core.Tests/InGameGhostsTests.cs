using System.Buffers.Binary;
using System.Numerics;
using StroopNext.Game;
using StroopNext.Ghosts;
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

        var loop = (byte[])InGameGhosts.HackFor(GameVersion.US).Entries.Single(e => e.Address == 0x80408000).Bytes.Clone();
        InGameGhosts.FixCleanUp(loop);
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
    public void A_hack_that_uses_the_expansion_pak_gets_its_ghosts_elsewhere()
    {
        // PSM and B3313 keep the US hook bytes but fill 0x80400000.. with their own data: the ghosts move to free RAM.
        var ram = Booted();
        W(ram, p =>
        {
            for (uint a = Base; a < Base + 0x400; a += 4)
                p.WriteU32(a, 0x27BDFFE8);
        });
        Assert.Null(InGameGhosts.Unavailable(F(ram)));
        ushort hi = InGameGhosts.BaseFor(F(ram))!.Value;
        Assert.NotEqual(InGameGhosts.DefaultBaseHi, hi);
        var ghosts = new InGameGhosts(hi);
        ghosts.Install(F(ram), out _)!.ApplyTo(ram.Bytes);
        Assert.Equal(hi, InGameGhosts.InstalledBaseHi(new RamImage(ram.Bytes)));
        Assert.Equal(InGameGhostState.Installed, ghosts.State(F(ram)));
        Assert.Equal(0x27BDFFE8u, new RamImage(ram.Bytes).U32(Base + 0x3FC));   // the hack's data is untouched

        var full = Booted();
        W(full, p =>
        {
            for (uint a = Base; a < Base + 0x400000; a += 0x80)
                p.WriteU32(a, 0x27BDFFE8);
        });
        Assert.Contains("no room", InGameGhosts.Unavailable(F(full)));
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
        var feed = ghosts.Feed(F(ram), [(ghost, Argb8Color.Rgb(0, 255, 0))], transparent: true);
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
        Assert.Equal(InGameGhosts.Lights(Argb8Color .Rgb(0, 255, 0)), Enumerable.Range(0, 24).Select(i => image.U8(Base + 0x8320 + (uint)i)).ToArray());

        Assert.DoesNotContain(feed.Writes, w => w.Address == 0x80000061);

        var later = new Ghost { PlaybackBaseFrame = 5000 };
        later.Frames[0] = new GhostFrame(Vector3.One, 0, 0, 0, 0, 0);
        ghosts.Feed(F(ram), [(later, Argb8Color.Rgb(0, 0, 255))], true).ApplyTo(ram.Bytes);
        Assert.Equal(30000f, new RamImage(ram.Bytes).F32(slot));
    }

    [Fact]
    public void Transparency_goes_to_existing_ghost_nodes()
    {
        var ram = Booted();
        var ghosts = new InGameGhosts();
        ghosts.Install(F(ram), out _)!.ApplyTo(ram.Bytes);
        W(ram, p => p.WriteU32(Base + 0x7FF8, Base + 0x7FF8 - 0x64));
        var feed = ghosts.Feed(F(ram), [(new Ghost(), Argb8Color.Rgb(1, 1, 1))], transparent: true);
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
        Assert.True(ghosts.Feed(F(ram), [(new Ghost(), Argb8Color.Rgb(1, 1, 1))], true).IsEmpty);
        Assert.False(ghosts.ReadyToRestore(F(ram)));
        W(ram, p => p.WriteU32(Base + 0x7FF8, 0));
        Assert.True(ghosts.ReadyToRestore(F(ram)));
    }

    [Fact]
    public void A_looping_ghost_replays_from_its_start_after_its_last_frame()
    {
        var ghost = new Ghost { PlaybackBaseFrame = 99, Loop = true };
        for (uint i = 0; i < 10; i++)
            ghost.Frames[i] = new GhostFrame(new Vector3(i, 0, 0), 0, 0, 0, 0, 0);
        Assert.Null(ghost.FrameAt(99));
        Assert.Equal(0, ghost.FrameAt(100)!.Value.Position.X);
        Assert.Equal(9, ghost.FrameAt(109)!.Value.Position.X);
        Assert.Equal(0, ghost.FrameAt(110)!.Value.Position.X);
        Assert.Equal(3, ghost.FrameAt(123)!.Value.Position.X);
        ghost.Loop = false;
        Assert.Null(ghost.FrameAt(110));
    }

    // Runs the loop's clean-up (count 0, then a drop from 12 to 1) on a tiny MIPS interpreter: every node goes.
    [Theory]
    [InlineData(0x78u, 0)]
    [InlineData(0x1A4u, 1)]
    public void The_loop_clean_up_removes_every_leftover_ghost(uint start, int keep)
    {
        byte[] loop = (byte[])InGameGhosts.HackFor(GameVersion.US).Entries.First(e => e.Address == 0x80408000).Bytes.Clone();
        InGameGhosts.FixCleanUp(loop);
        var ram = new Dictionary<uint, uint>();
        const uint first = Base + 0x7FF8;
        for (uint i = 0; i < 12; i++)
            ram[first - i * 0x68] = Base + 0x7000 + i * 0x10;
        ram[Base + 0x7FFC] = (uint)keep;
        var removed = new List<uint>();
        var r = new uint[32];
        r[17] = first - (uint)keep * 0x68; // s1 at the first leftover, as the loop leaves it
        uint pc = start;
        uint U(uint a) => ram.GetValueOrDefault(a);
        for (int steps = 0; steps < 500 && pc < 0x1C4; steps++)
        {
            uint w = BinaryPrimitives.ReadUInt32BigEndian(loop.AsSpan((int)pc));
            uint op = w >> 26, rs = (w >> 21) & 31, rt = (w >> 16) & 31;
            short imm = (short)w;
            uint next = pc + 4, target = pc + 4 + (uint)(imm << 2);
            void Delay() => Exec(BinaryPrimitives.ReadUInt32BigEndian(loop.AsSpan((int)pc + 4)));
            void Exec(uint d)
            {
                uint dop = d >> 26, drs = (d >> 21) & 31, drt = (d >> 16) & 31;
                short dimm = (short)d;
                if (d == 0) return;
                if (dop == 0x09) r[drt] = r[drs] + (uint)dimm;
                else if (dop == 0x0D) r[drt] = r[drs] | (ushort)dimm;
                else if (dop == 0x0F) r[drt] = (uint)(ushort)dimm << 16;
                else if (dop == 0x23) r[drt] = U(r[drs] + (uint)dimm);
                else if (dop == 0x2B) ram[r[drs] + (uint)dimm] = r[drt];
                else if (dop == 0x20) r[drt] = (uint)(sbyte)(U((r[drs] + (uint)dimm) & ~3u) >> 0);
                else if (dop == 0x00 && (d & 0x3F) == 0x25) r[(d >> 11) & 31] = r[drs] | r[drt];
                else throw new InvalidOperationException($"opcode {d:X8} at {pc:X}");
                r[0] = 0;
            }
            switch (op)
            {
                case 0x04: Delay(); next = r[rs] == r[rt] ? target : pc + 8; break;
                case 0x05: Delay(); next = r[rs] != r[rt] ? target : pc + 8; break;
                case 0x14: next = r[rs] == r[rt] ? (Delay2(), target).target : pc + 8; break;
                case 0x03: Delay(); removed.Add(r[4]); next = pc + 8; break;
                default: Exec(w); break;
            }
            int Delay2() { Delay(); return 0; }
            pc = next;
        }
        Assert.Equal(12 - keep, removed.Distinct().Count());
        for (uint i = (uint)keep; i < 12; i++)
            Assert.Equal(0u, U(first - i * 0x68));
    }

    [Fact]
    public void Removing_packs_ghosts_left_after_an_empty_slot_by_the_legacy_clean_up()
    {
        var ram = Booted();
        var ghosts = new InGameGhosts();
        ghosts.Install(F(ram), out _)!.ApplyTo(ram.Bytes);
        var legacy = InGameGhosts.HackFor(GameVersion.US).Entries.First(e => e.Address == 0x80408000).Bytes;
        W(ram, p =>
        {
            p.WriteBytes(Base + 0x8800, legacy);
            for (uint i = 2; i < 5; i++)
                p.WriteU32(Base + 0x7FF8 - i * 0x68, Base + 0x7F94 - i * 0x68);
        });
        var unstick = ghosts.Unstick(F(ram));
        Assert.NotNull(unstick);
        unstick!.ApplyTo(ram.Bytes);
        var image = new RamImage(ram.Bytes);
        for (uint i = 0; i < 3; i++)
            Assert.Equal(Base + 0x7F94 - (i + 2) * 0x68, image.U32(Base + 0x7FF8 - i * 0x68));
        Assert.Equal(0u, image.U32(Base + 0x7FF8 - 3 * 0x68));
        Assert.Equal(0x36318060u, image.U32(Base + 0x8800 + 0x4C));
        Assert.Null(ghosts.Unstick(F(ram)));
    }
}
