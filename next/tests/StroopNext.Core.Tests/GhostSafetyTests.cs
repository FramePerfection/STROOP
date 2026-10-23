using StroopNext.Ghosts;
using StroopNext.Memory;

namespace StroopNext.Core.Tests;

public class GhostSafetyTests
{
    private const uint Base = 0x80400000, Hook = 0x8027B188, Timer = 0x8032D5D4, CurGraphNodeObject = 0x8032DF00, BodyState = 0x8033B3B4;
    private const uint MarioObjectPointer = 0x80361158, MarioObject = 0x8034BFA8, Root = 0x800F0860, Child = 0x800F0878;
    private const uint ObjParent = 0x8038BD88, OtherObject = 0x8034C210;
    private const uint Slot0 = Base + 0x7FF8, Slot1 = Slot0 - 0x68, Node0 = Slot0 - 0x64, Node1 = Slot1 - 0x64;

    private static TestRam Booted()
    {
        var ram = TestRam.Us().Do(p => p
            .WriteU32(0x80000000, 0x3C1A8000)
            .WriteU32(MarioObjectPointer, MarioObject).WriteU16(MarioObject + 2, 0x0021).WriteU32(MarioObject + 0x14, Root)
            .WriteU32(ObjParent + 0x10, MarioObject)
            .WriteU32(MarioObject + 4, MarioObject).WriteU32(MarioObject + 8, MarioObject).WriteU32(MarioObject + 0xC, ObjParent)
            .WriteU16(Root, 0x0028).WriteU32(Root + 4, Root).WriteU32(Root + 8, Root).WriteU32(Root + 0x10, Child)
            .WriteU16(Child, 0x001C).WriteU32(Child + 4, Child).WriteU32(Child + 8, Child).WriteU32(Child + 0xC, Root)
            .WriteU32(Hook, 0x10000001));
        ram.Image();
        return ram;
    }

    private static Session.GameFrame F(TestRam ram) => Session.GameFrame.Create(new RamImage(ram.Bytes));
    private static void W(TestRam ram, Action<RamPatch> write) { var p = new RamPatch(); write(p); p.ApplyTo(ram.Bytes); }
    private static uint U32(TestRam ram, uint address) => new RamImage(ram.Bytes).U32(address);

    private static TestRam Installed(out InGameGhosts hack)
    {
        var ram = Booted();
        hack = new InGameGhosts();
        hack.Install(F(ram), out _)!.ApplyTo(ram.Bytes);
        return ram;
    }

    private static void LiveGhost0(TestRam ram) => W(ram, p => p
        .WriteU32(Slot0, Node0).WriteU32(Node0 + 0xC, ObjParent)
        .WriteU32(MarioObject + 8, Node0).WriteU32(Node0 + 4, MarioObject)
        .WriteU32(Node0 + 8, MarioObject).WriteU32(MarioObject + 4, Node0));

    private static void RunGuard(TestRam ram) => MiniMips.Run(ram.Bytes, Base + 0x8000, stop: Base + 0x8800);

    [Fact]
    public void The_game_still_calls_base_8000_which_is_now_the_guard()
    {
        var ram = Installed(out var hack);
        Assert.Equal(0x0C102000u, U32(ram, Hook));
        Assert.True(hack.Guard().Length <= 0x200);
    }

    [Fact]
    public void The_guard_keeps_live_ghosts()
    {
        var ram = Installed(out _);
        LiveGhost0(ram);
        RunGuard(ram);
        Assert.Equal(Node0, U32(ram, Slot0));
    }

    [Fact]
    public void After_a_level_change_the_guard_forgets_the_old_ghost_nodes()
    {
        var ram = Installed(out _);
        LiveGhost0(ram);
        W(ram, p => p.WriteU32(Slot1, Node1).WriteU32(Node1 + 0xC, ObjParent).WriteU32(Node1 + 4, OtherObject).WriteU32(Node1 + 8, OtherObject));
        RunGuard(ram);
        Assert.Equal(Node0, U32(ram, Slot0));
        Assert.Equal(0u, U32(ram, Slot1));

        W(ram, p => p.WriteU32(ObjParent + 0x10, 0));
        RunGuard(ram);
        Assert.Equal(0u, U32(ram, Slot0));
    }

    [Fact]
    public void A_broken_object_list_changes_nothing_and_the_guard_still_ends()
    {
        var ram = Installed(out _);
        LiveGhost0(ram);
        W(ram, p => p.WriteU32(MarioObject + 8, OtherObject).WriteU32(OtherObject + 8, OtherObject));
        RunGuard(ram);
        Assert.Equal(Node0, U32(ram, Slot0));
    }

    [Fact]
    public void A_slot_pointing_outside_the_hack_area_is_forgotten()
    {
        var ram = Installed(out _);
        W(ram, p => p.WriteU32(Slot0, OtherObject));
        RunGuard(ram);
        Assert.Equal(0u, U32(ram, Slot0));
    }

    [Fact]
    public void In_and_out_swap_a_ghosts_body_state_only_while_it_is_drawn()
    {
        var ram = Installed(out _);
        var (inCode, outCode) = (Base + 0x8600, Base + 0x8700);
        uint[] real = [0x11111111, 0x22222222, 0x33333333, 0x44444444, 0x55555555];
        uint[] ghost = [0xAAAAAAAA, 0xBBBBBBBB, 0xCCCCCCCC, 0xDDDDDDDD, 0xEEEEEEEE];
        W(ram, p =>
        {
            p.WriteU32(Timer, 1000).WriteU8(Node0 + 0x60, 1);
            for (uint k = 0; k < 5; k++)
            {
                p.WriteU32(BodyState + k * 4, real[k]);
                p.WriteU32(Base + 0x60000 + (1000 & 0x7F) * 0x20 + k * 4, ghost[k]);
            }
        });
        uint[] Body() => Enumerable.Range(0, 5).Select(k => U32(ram, BodyState + (uint)k * 4)).ToArray();
        uint[] A0(uint a0) { var r = new uint[32]; r[4] = a0; return r; }

        W(ram, p => p.WriteU32(CurGraphNodeObject, MarioObject));
        MiniMips.Run(ram.Bytes, inCode, registers: A0(1));
        Assert.Equal(real, Body());

        W(ram, p => p.WriteU32(CurGraphNodeObject, Node0));
        MiniMips.Run(ram.Bytes, inCode, registers: A0(0));
        Assert.Equal(real, Body());
        MiniMips.Run(ram.Bytes, inCode, registers: A0(1));
        Assert.Equal(ghost, Body());
        MiniMips.Run(ram.Bytes, outCode, registers: A0(1));
        Assert.Equal(real, Body());

        W(ram, p => p.WriteU32(BodyState, 0x99999999));
        MiniMips.Run(ram.Bytes, outCode, registers: A0(1));
        Assert.Equal(0x99999999u, U32(ram, BodyState));
    }

    [Fact]
    public void A_level_change_rebuilds_marios_model_and_step_two_is_done_again()
    {
        var ram = Installed(out var hack);
        var before = InGameGhostsTests.GameMemory(Booted().Bytes);
        W(ram, p => p.WriteU16(MarioObject + 2, 0x0061));
        hack.ColoredCaps(F(ram))!.ApplyTo(ram.Bytes);
        Assert.Equal(Base + 0x7140, U32(ram, Root + 0x10));
        Assert.Null(hack.ColoredCaps(F(ram)));

        const uint Root2 = 0x800F2000, Child2 = 0x800F2018;
        W(ram, p =>
        {
            p.WriteU16(MarioObject + 2, 0x0021).WriteU32(MarioObject + 0x14, Root2);
            p.WriteU16(Root2, 0x0028).WriteU32(Root2 + 4, Root2).WriteU32(Root2 + 8, Root2).WriteU32(Root2 + 0x10, Child2);
            p.WriteU16(Child2, 0x001C).WriteU32(Child2 + 4, Child2).WriteU32(Child2 + 8, Child2).WriteU32(Child2 + 0xC, Root2);
            for (uint a = Root; a < Child + 0x20; a += 4)
                p.WriteU32(a, 0xABABABAB);
        });
        Assert.False(hack.StepTwoPending(F(ram)));
        W(ram, p => p.WriteU16(MarioObject + 2, 0x0061));
        Assert.True(hack.StepTwoPending(F(ram)));
        hack.ColoredCaps(F(ram))!.ApplyTo(ram.Bytes);
        Assert.Equal(Base + 0x7140, U32(ram, Root2 + 0x10));
        Assert.Equal(Base + 0x7160, U32(ram, Child2 + 8));

        new InGameGhosts().Restore(F(ram))!.ApplyTo(ram.Bytes);
        Assert.Equal(Child2, U32(ram, Root2 + 0x10));
        Assert.Equal(Child2, U32(ram, Child2 + 4));
        Assert.Equal(Child2, U32(ram, Child2 + 8));
        for (uint a = Root; a < Child + 0x20; a += 4)
            Assert.Equal(0xABABABABu, U32(ram, a));
        Assert.Equal(0x10000001u, U32(ram, Hook));
        Assert.False(hack.CanRestore(F(ram)));
    }

    [Fact]
    public void Removal_waits_for_every_ghost_slot()
    {
        var ram = Installed(out var hack);
        W(ram, p => p.WriteU32(Slot0 - 5 * 0x68, Node0 - 5 * 0x68));
        Assert.False(hack.ReadyToRestore(F(ram)));
        W(ram, p => p.WriteU32(Slot0 - 5 * 0x68, 0));
        Assert.True(hack.ReadyToRestore(F(ram)));
    }

    [Fact]
    public void Show_remove_show_again_leaves_the_game_as_it_was()
    {
        var ram = Booted();
        var original = InGameGhostsTests.GameMemory(ram.Bytes);
        for (int round = 0; round < 3; round++)
        {
            var hack = new InGameGhosts();
            Assert.Equal(InGameGhostState.Disabled, hack.State(F(ram)));
            hack.Install(F(ram), out _)!.ApplyTo(ram.Bytes);
            W(ram, p => p.WriteU16(MarioObject + 2, 0x0061));
            hack.ColoredCaps(F(ram))!.ApplyTo(ram.Bytes);
            hack.RequestDisable().ApplyTo(ram.Bytes);
            Assert.Equal(InGameGhostState.Disabling, hack.State(F(ram)));
            Assert.True(hack.ReadyToRestore(F(ram)));
            hack.Restore(F(ram))!.ApplyTo(ram.Bytes);
            W(ram, p => p.WriteU16(MarioObject + 2, 0x0021));
            Assert.Equal(original, InGameGhostsTests.GameMemory(ram.Bytes));
        }
    }
}
