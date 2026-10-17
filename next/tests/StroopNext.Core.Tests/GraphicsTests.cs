using System.Diagnostics;
using System.Numerics;
using StroopNext.Graphics;
using StroopNext.Memory;
using StroopNext.Session;
using Xunit.Abstractions;

namespace StroopNext.Core.Tests;

public class GraphicsTests(ITestOutputHelper output)
{
    private static GameFrame? LoadFrame(string relative)
    {
        string root = Environment.GetEnvironmentVariable("STROOP_TEST_SAVESTATES") ?? @"D:\mupen";
        string path = Path.Combine(root, relative);
        if (!File.Exists(path))
            return null;
        using var source = Savestate.Load(path);
        var bytes = new byte[source.Size];
        source.TryRead(bytes);
        return GameFrame.Create(new RamImage(bytes));
    }

    [Fact]
    public void Decomp_symbols_resolve_for_us_and_jp()
    {
        var us = Sm64Symbols.For(Game.GameVersion.US)!;
        Assert.Equal(0x8029DB48u, us.Get("geo_switch_anim_state"));
        Assert.Equal(0x80330F00u, us.Get("sSkyboxTextures"));
        Assert.True(us.Count > 1000);
        Assert.NotNull(Sm64Symbols.For(Game.GameVersion.JP));
        Assert.Null(Sm64Symbols.For(Game.GameVersion.EU));
        Assert.Equal(0u, us.Get("no_such_symbol"));
    }

    [Fact]
    public void Texture_formats_decode_to_rgba8()
    {
        var bytes = new byte[0x1000];
        new RamPatch().WriteU16(0x80000100, 0xF801).WriteU8(0x80000200, 0xF0).ApplyTo(bytes);
        var memory = new GfxMemory(new RamImage(bytes), new uint[32]);
        var red = TextureDecoder.Decode(memory, new TextureKey(0x80000100, TextureDecoder.FormatRgba, 2, 1, 1, uint.MaxValue, 0, 0, 0, 0));
        Assert.Equal(new byte[] { 255, 0, 0, 255 }, red);
        var ia = TextureDecoder.Decode(memory, new TextureKey(0x80000200, TextureDecoder.FormatIa, 1, 1, 1, uint.MaxValue, 0, 0, 0, 0));
        Assert.Equal(new byte[] { 255, 255, 255, 0 }, ia);
    }

    [Fact]
    public void Segmented_addresses_resolve_through_the_segment_table()
    {
        var segments = new uint[32];
        segments[7] = 0x00100000;
        var memory = new GfxMemory(new RamImage(new byte[0x200000]), segments);
        Assert.Equal(0x00100010u, memory.Phys(0x07000010));
        Assert.Equal(0x00000020u, memory.Phys(0x80000020));
        Assert.Equal(uint.MaxValue, memory.Phys(0x07FFFFF0));
    }

    [Theory]
    [InlineData(@"sm64-tas-cpu\MOVIE_0STAR\03_avant_pont.st")]
    [InlineData(@"sm64-tas-cpu\MOVIE_0STAR\05_hall.st")]
    public void Real_savestates_rebuild_the_game_scene(string savestate)
    {
        if (LoadFrame(savestate) is not { } frame)
            return;
        var builder = new GameSceneBuilder();
        var clock = Stopwatch.StartNew();
        Assert.True(builder.Update(frame));
        double first = clock.Elapsed.TotalMilliseconds;
        var scene = builder.Scene;

        Assert.True(scene.Level.TriangleCount > 500, $"level triangles: {scene.Level.TriangleCount}");
        Assert.True(scene.Textures.All.Count > 5, $"textures: {scene.Textures.All.Count}");
        Assert.Contains(scene.Instances, i => i.IsMario);
        Assert.True(scene.Instances.Count > 10);
        Assert.All(scene.Level.Vertices, v => Assert.True(float.IsFinite(v.Position.X) && float.IsFinite(v.Position.Y) && float.IsFinite(v.Position.Z)));

        var mario = frame.Mario!.Position;
        var marioInstance = scene.Instances.First(i => i.IsMario && !i.Billboard);
        Assert.True(Vector3.Distance(marioInstance.Matrix.Translation, mario) < 300, $"Mario part at {marioInstance.Matrix.Translation}, Mario at {mario}");

        int levelVersion = scene.LevelVersion, cached = builder.CachedMeshCount;
        clock.Restart();
        builder.Update(frame);
        double second = clock.Elapsed.TotalMilliseconds;
        Assert.Equal(levelVersion, scene.LevelVersion);
        Assert.Equal(cached, builder.CachedMeshCount);

        output.WriteLine($"{savestate}: level {scene.Level.TriangleCount} tris / {scene.Level.Batches.Length} batches, {scene.Textures.All.Count} textures, " +
                         $"{scene.Instances.Count} instances ({scene.Instances.Count(i => i.IsMario)} Mario), {scene.Shadows.Count} shadows, " +
                         $"sky {(scene.Skybox is null ? scene.BackgroundColor?.ToString() ?? "none" : "texture")}; first update {first:0.0} ms, steady update {second:0.00} ms");
    }
}
