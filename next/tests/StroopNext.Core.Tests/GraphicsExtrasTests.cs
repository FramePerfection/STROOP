using System.Numerics;
using StroopNext.Game;
using StroopNext.Graphics;
using StroopNext.Map;
using StroopNext.Map3D;
using StroopNext.Memory;
using StroopNext.Session;
using StroopNext.Settings;
using O = StroopNext.Game.MarioOffsets;

namespace StroopNext.Core.Tests;

public class GraphicsExtrasTests
{
    private static GfxMemory MemoryWith(Action<RamPatch> write)
    {
        var bytes = new byte[0x10000];
        var patch = new RamPatch();
        write(patch);
        patch.ApplyTo(bytes);
        return new GfxMemory(new RamImage(bytes), new uint[32]);
    }

    [Fact]
    public void Mips_leaf_functions_run_on_the_snapshot()
    {
        var memory = MemoryWith(p => p
            .WriteU32(0x80001000, 0x24820005)
            .WriteU32(0x80001004, 0x24080007)
            .WriteU32(0x80001008, 0x14480002)
            .WriteU32(0x8000100C, 0x00000000)
            .WriteU32(0x80001010, 0x24020063)
            .WriteU32(0x80001014, 0x03E00008)
            .WriteU32(0x80001018, 0x00000000));
        Assert.Equal(6u, MipsLeaf.Call(memory, 0x80001000, 1));
        Assert.Equal(99u, MipsLeaf.Call(memory, 0x80001000, 2));
    }

    [Fact]
    public void Mips_leaf_refuses_unsupported_code_and_never_writes_ram()
    {
        var memory = MemoryWith(p => p
            .WriteU32(0x80002000, 0x0C000000)
            .WriteU32(0x80003000, 0xAC850000)
            .WriteU32(0x80003004, 0x8C820000)
            .WriteU32(0x80003008, 0x03E00008)
            .WriteU32(0x8000300C, 0x00000000));
        Assert.Null(MipsLeaf.Call(memory, 0x80002000));
        Assert.Equal(0x12345678u, MipsLeaf.Call(memory, 0x80003000, 0x80004000, 0x12345678));
        Assert.Equal(0u, memory.U32(0x80004000));
    }

    [Fact]
    public void Segmented_constants_are_found_in_game_code()
    {
        var memory = MemoryWith(p => p
            .WriteU32(0x80001000, 0x3C080700)
            .WriteU32(0x80001004, 0x25081234)
            .WriteU32(0x80001008, 0x3C090200)
            .WriteU32(0x8000100C, 0x35290010)
            .WriteU32(0x80001010, 0x03E00008)
            .WriteU32(0x80001014, 0x00000000));
        Assert.Equal([0x07001234u], MipsLeaf.FindSegmentedConstants(memory, 0x80001000, 0x07));
        Assert.Equal([0x02000010u], MipsLeaf.FindSegmentedConstants(memory, 0x80001000, 0x02));
    }

    [Fact]
    public void Prediction_follows_the_floor_at_de_facto_speed()
    {
        var floor = (new Vector3(-5000, 0, -5000), new Vector3(5000, 0, -5000), new Vector3(0, 0, 5000), new Vector3(0, 1, 0), 0f);
        var ram = GameModelTests.WithSurfaces(TestRam.Us(), 1, floor);
        ram.Patch.WriteF32(TestRam.UsMario + O.HSpeed, 40).WriteU16(TestRam.UsMario + O.FacingYaw, 0x4000)
            .WriteU32(TestRam.UsMario + O.FloorTri, 0x80200000);
        var predicted = MarioPrediction.Predict(ram.Frame(), 3, quarterSteps: false);
        Assert.Equal(3, predicted.Count);
        Assert.Equal(40, predicted[0].Position.X, 2);
        Assert.Equal(120, predicted[2].Position.X, 2);
        Assert.All(predicted, p => Assert.Equal(0, p.Position.Y, 3));
        Assert.Equal(12, MarioPrediction.Predict(ram.Frame(), 3, quarterSteps: true).Count);
    }

    [Fact]
    public void Prediction_falls_with_gravity_off_a_ledge()
    {
        var ram = TestRam.Us();
        ram.Patch.WriteF32(TestRam.UsMario + O.Y, 500).WriteF32(TestRam.UsMario + O.FloorY, -11000).WriteF32(TestRam.UsMario + O.HSpeed, 10);
        var predicted = MarioPrediction.Predict(ram.Frame(), 3, quarterSteps: false);
        Assert.True(predicted[2].Position.Y < predicted[0].Position.Y);
        Assert.All(predicted, p => Assert.False(p.OnGround));
    }

    [Theory]
    [InlineData(0f, 8)]
    [InlineData(-8192f, 0)]
    [InlineData(8191f, 15)]
    [InlineData(-1f, 7)]
    public void Collision_cells_cover_plus_minus_8192(float coordinate, int cell) => Assert.Equal(cell, MarioPrediction.Cell(coordinate));

    [Theory]
    [InlineData(0f, 0)]
    [InlineData(32767f, 0)]
    [InlineData(32768f, 1)]
    [InlineData(-32769f, -1)]
    [InlineData(65536f * 3, 3)]
    public void Parallel_universes_are_centred_on_the_origin(float coordinate, int pu) => Assert.Equal(pu, MarioPrediction.ParallelUniverse(coordinate));

    [Fact]
    public void Grounds_savestate_has_animated_water_and_the_layer_feeds_the_3d_scene()
    {
        string path = Path.Combine(Environment.GetEnvironmentVariable("STROOP_TEST_SAVESTATES") ?? @"D:\mupen", @"sm64-tas-cpu\MOVIE_0STAR\03_avant_pont.st");
        if (!File.Exists(path))
            return;
        using var source = Savestate.Load(path);
        var bytes = new byte[source.Size];
        source.TryRead(bytes);
        var frame = GameFrame.Create(new RamImage(bytes));

        var visuals = new GameVisualsLayer();
        var next = new NextPositionsLayer();
        var scene = new Scene3D();
        var store = new SettingsStore();
        var camera = new Camera3D { Width = 800, Height = 600 };
        visuals.Build3D(new Map3DContext(frame, camera, scene, store.Scope("game"), new Selection()));
        next.Build3D(new Map3DContext(frame, camera, scene, store.Scope("next"), new Selection()));

        Assert.NotNull(scene.GameVisuals);
        Assert.NotEmpty(scene.GameVisuals!.UvAnimations);
        Assert.Contains(scene.GameVisuals.Level.Batches, b => b.Material.UvAnimation >= 0);
        Assert.NotNull(scene.GameVisuals.Skybox);
        Assert.Equal(3, scene.MarioModelCopies.Count);
        Assert.Contains(scene.Items, p => p is Triangle3D { Edges: false });
    }
}
