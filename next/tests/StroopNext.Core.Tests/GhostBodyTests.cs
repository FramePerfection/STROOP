using System.Numerics;
using StroopNext.Game;
using StroopNext.Map;
using StroopNext.Memory;

namespace StroopNext.Core.Tests;

public class GhostBodyTests
{
    private const uint Base = 0x80400000, Hook = 0x8027B188, Timer = 0x8032D5D4;
    private const uint MarioObjectPointer = 0x80361158, MarioObject = 0x8034BFA8, Root = 0x800F0860, Child = 0x800F0878;

    [Fact]
    public void Mips_encodes_like_the_original_hack()
    {
        var code = new Mips(0x80400000)
            .Lui(Mips.T0, 0x8036).Lw(Mips.T0, 0x1158, Mips.T0).Jr(Mips.RA).Or(Mips.V0, Mips.R0, Mips.R0)
            .Sll(Mips.T5, Mips.T5, 12).Addu(Mips.T5, Mips.T5, Mips.T6).Sb(Mips.T9, 0x130, Mips.T7)
            .Assemble();
        uint W(int i) => (uint)(code[i * 4] << 24 | code[i * 4 + 1] << 16 | code[i * 4 + 2] << 8 | code[i * 4 + 3]);
        Assert.Equal(0x3C088036u, W(0));
        Assert.Equal(0x8D081158u, W(1));
        Assert.Equal(0x03E00008u, W(2));
        Assert.Equal(0x00001025u, W(3));
        Assert.Equal(0x000D6B00u, W(4));
        Assert.Equal(0x01AE6821u, W(5));
        Assert.Equal(0xA1F90130u, W(6));
    }

    [Fact]
    public void Branches_land_on_their_labels_after_the_delay_slot()
    {
        var code = new Mips(0).Beq(Mips.T0, Mips.R0, "end").Addiu(Mips.AT, Mips.R0, 1).Addiu(Mips.AT, Mips.R0, 2).Label("end").Jr(Mips.RA).Assemble();
        Assert.Equal(0x11000002u, (uint)(code[0] << 24 | code[1] << 16 | code[2] << 8 | code[3]));
    }

    [Fact]
    public void The_switchers_only_touch_the_hack_area_and_the_body_state()
    {
        var (inCode, outCode) = new InGameGhosts().BodySwitchers(GameVersion.US);
        Assert.Equal(0, inCode.Length % 4);
        Assert.True(inCode.Length < 0x100 && outCode.Length < 0x100);
        Assert.Equal([0x03, 0xE0, 0x00, 0x08, 0x00, 0x00, 0x10, 0x25], inCode[^8..]);
        Assert.Equal([0x03, 0xE0, 0x00, 0x08, 0x00, 0x00, 0x10, 0x25], outCode[^8..]);
    }

    private static TestRam Booted()
    {
        var ram = TestRam.Us().Do(p => p
            .WriteU32(0x80000000, 0x3C1A8000)
            .WriteU32(MarioObjectPointer, MarioObject).WriteU16(MarioObject + 2, 0x0021).WriteU32(MarioObject + 0x14, Root)
            .WriteU16(Root, 0x0028).WriteU32(Root + 4, Root).WriteU32(Root + 8, Root).WriteU32(Root + 0x10, Child)
            .WriteU16(Child, 0x001C).WriteU32(Child + 4, Child).WriteU32(Child + 8, Child).WriteU32(Child + 0xC, Root)
            .WriteU32(Hook, 0x10000001));
        ram.Image();
        return ram;
    }

    private static Session.GameFrame F(TestRam ram) => Session.GameFrame.Create(new RamImage(ram.Bytes));

    [Fact]
    public void Step_two_puts_in_and_out_around_the_model_and_restore_undoes_it()
    {
        var ram = Booted();
        var before = (byte[])ram.Bytes.Clone();
        var hack = new InGameGhosts();
        hack.Install(F(ram), out _)!.ApplyTo(ram.Bytes);
        Assert.Equal(Child, new RamImage(ram.Bytes).U32(Root + 0x10));

        new RamPatch().WriteU16(MarioObject + 2, 0x0061).ApplyTo(ram.Bytes);
        hack.ColoredCaps(F(ram))!.ApplyTo(ram.Bytes);
        var image = new RamImage(ram.Bytes);
        uint inNode = Base + 0x7140, outNode = Base + 0x7160;
        Assert.Equal(inNode, image.U32(Root + 0x10));
        Assert.Equal(Child, image.U32(inNode + 8));
        Assert.Equal(outNode, image.U32(Child + 8));
        Assert.Equal(inNode, image.U32(outNode + 8));
        Assert.Equal(outNode, image.U32(inNode + 4));
        Assert.Equal(inNode, image.U32(Child + 4));
        Assert.Equal(Child, image.U32(outNode + 4));
        Assert.Equal(0x012A, image.U16(inNode));
        Assert.Equal(Base + 0x8600, image.U32(inNode + 0x14));
        Assert.Equal(Base + 0x8700, image.U32(outNode + 0x14));

        hack.Restore(F(ram))!.ApplyTo(ram.Bytes);
        new RamPatch().WriteU16(MarioObject + 2, 0x0021).ApplyTo(ram.Bytes);
        Assert.Equal(InGameGhostsTests.GameMemory(before), InGameGhostsTests.GameMemory(ram.Bytes));
    }

    private static Ghost WithBodies()
    {
        var ghost = new Ghost { Name = "b", PlaybackBaseFrame = 900 };
        for (uint k = 98; k <= 110; k++)
        {
            ghost.Frames[k] = new GhostFrame(new Vector3(k, 0, 0), 72, 0, 0, 0, 0);
            ghost.Bodies[k] = Enumerable.Range(0, InGameGhosts.BodyBytes).Select(i => (byte)(k + i)).ToArray();
        }
        return ghost;
    }

    [Fact]
    public void Bodies_round_trip_in_the_file_and_legacy_readers_stop_before_them()
    {
        using var stream = new MemoryStream();
        WithBodies().Write(stream);
        stream.Position = 0;
        var read = Ghost.Read(stream, "b");
        Assert.Equal(13, read.Bodies.Count);
        Assert.Equal(WithBodies().Bodies[100], read.Bodies[100]);
        Assert.Equal(13, read.Frames.Count);
    }

    [Fact]
    public void Feed_writes_each_ghosts_body_state_in_its_slot()
    {
        var ram = Booted();
        new RamPatch().WriteU32(Timer, 1000).ApplyTo(ram.Bytes);
        var hack = new InGameGhosts();
        hack.Install(F(ram), out _)!.ApplyTo(ram.Bytes);
        hack.Feed(F(ram), [(WithBodies(), MapColor.Rgb(0, 255, 0))], true).ApplyTo(ram.Bytes);
        var image = new RamImage(ram.Bytes);
        uint slot = Base + 0x60000 + (1000 & 0x7F) * 0x20;
        Assert.Equal(WithBodies().Bodies[100], Enumerable.Range(0, InGameGhosts.BodyBytes).Select(i => image.U8(slot + (uint)i)).ToArray());
    }
}
