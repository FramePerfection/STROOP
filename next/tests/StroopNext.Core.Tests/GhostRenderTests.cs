using System.Numerics;
using StroopNext.Graphics;
using StroopNext.Memory;
using StroopNext.Session;

namespace StroopNext.Core.Tests;

public class GhostRenderTests
{
    [Fact]
    public void A_ghost_drawn_again_after_another_animation_is_the_same()
    {
        string root = Environment.GetEnvironmentVariable("STROOP_TEST_SAVESTATES") ?? @"D:\mupen";
        string path = Path.Combine(root, @"sm64-tas-cpu\MOVIE_0STAR\03_avant_pont.st");
        if (!File.Exists(path))
            return;
        using var source = Savestate.Load(path);
        var bytes = new byte[source.Size];
        Assert.True(source.TryRead(bytes));
        var frame = GameFrame.Create(new RamImage(bytes));
        var builder = new GameSceneBuilder();
        Assert.True(builder.Update(frame));
        var scene = builder.Scene;
        var at = frame.Mario!.Position + new Vector3(300, 0, 0);

        List<Matrix4x4> Draw(short animation, short animationFrame)
        {
            int n0 = scene.Instances.Count;
            bool drawn = scene.AddVirtualGhost!(new VirtualGhost(at, 0, 0x4000, 0, animation, animationFrame));
            return drawn ? scene.Instances.Skip(n0).Where(i => i.Ghost).Select(i => i.Matrix).ToList() : [];
        }

        short known = Enumerable.Range(0, 210).Select(i => (short)i).First(i => Draw(i, 3).Count > 0);
        var first = Draw(known, 3);
        Assert.NotEmpty(first);
        var other = Draw(known, 12);
        Assert.Equal(first.Count, other.Count);
        Assert.NotEqual(first, other);
        Assert.Equal(first, Draw(known, 3));
    }

    [Fact]
    public void Every_Mario_animation_comes_from_the_rom_as_the_game_loads_it()
    {
        string root = Environment.GetEnvironmentVariable("STROOP_TEST_SAVESTATES") ?? @"D:\mupen";
        string st = Path.Combine(root, @"sm64-tas-cpu\MOVIE_0STAR\03_avant_pont.st");
        string rom = Path.Combine(root, @"mupen64-rr-lua\build\out\roms\Super Mario 64 (USA).z64");
        if (!File.Exists(st) || !File.Exists(rom))
            return;
        using var source = Savestate.Load(st);
        var bytes = new byte[source.Size];
        Assert.True(source.TryRead(bytes));
        var frame = GameFrame.Create(new RamImage(bytes));
        uint obj = frame.Mario!.MarioObject;
        short id = frame.Ram.S16(obj + 0x38);
        uint header = frame.Ram.U32(obj + 0x3C);

        MarioAnimations.LoadFromRom(rom);
        Assert.NotNull(MarioAnimations.RomLoaded);
        Assert.True(MarioAnimations.Count >= 209);
        var fromRom = MarioAnimations.Get(id)!;
        for (int i = 0; i < fromRom.Blob.Length; i++)
            if (i is < 0x0C or >= 0x14)
                Assert.Equal(frame.Ram.U8(header + (uint)i), fromRom.Blob[i]);
        Assert.Equal((frame.Ram.U32(header + 0x0C) & 0x1FFFFFFF) - (header & 0x1FFFFFFF), fromRom.ValuesOffset);

        var builder = new GameSceneBuilder();
        Assert.True(builder.Update(frame));
        int drawable = Enumerable.Range(0, 209).Count(a => builder.Scene.AddVirtualGhost!(
            new VirtualGhost(frame.Mario.Position, 0, 0, 0, (short)a, 1)));
        Assert.Equal(209, drawable);
    }
}
