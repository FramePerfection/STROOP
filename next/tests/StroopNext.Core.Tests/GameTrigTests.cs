using StroopNext.Game;
using StroopNext.Session;
using StroopNext.Variables;
using O = StroopNext.Game.MarioOffsets;

namespace StroopNext.Core.Tests;

public class GameTrigTests
{
    private const uint ArctanTable = 0x8038B000;

    private static TestRam WithArctanTable(TestRam ram)
    {
        for (int i = 0; i <= 1024; i++)
        {
            double angle = Math.Atan2(i / 1024.0, 1.0) * Angle.UnitsPerTurn / (2 * Math.PI);
            ram.Patch.WriteU16(ArctanTable + (uint)(2 * i), (ushort)Math.Round(angle));
        }
        return ram;
    }

    [Theory]
    [InlineData(0, 0f, 1f)]
    [InlineData(16384, 1f, 0f)]
    [InlineData(32768, 0f, -1f)]
    [InlineData(49152, -1f, 0f)]
    public void In_game_sin_cos_are_exact_at_the_cardinal_angles(int angle, float sin, float cos)
    {
        Assert.Equal(sin, GameTrig.InGameSine(angle), 4);
        Assert.Equal(cos, GameTrig.InGameCosine(angle), 4);
    }

    [Fact]
    public void In_game_angles_truncate_the_low_four_bits()
    {
        Assert.Equal(GameTrig.InGameSine(0x1230), GameTrig.InGameSine(0x123F));
        Assert.Equal((ushort)0x1230, GameTrig.Truncate(0x123F));
    }

    [Fact]
    public void In_game_atan_reads_the_table_and_matches_the_cardinal_directions()
    {
        var frame = WithArctanTable(TestRam.Us()).Frame();
        Assert.Equal((ushort)0x0000, GameTrig.InGameAngleTo(frame, 0, 0, 0, 10)!.Value);
        Assert.Equal((ushort)0x4000, GameTrig.InGameAngleTo(frame, 0, 0, 10, 0)!.Value);
        Assert.Equal((ushort)0x8000, GameTrig.InGameAngleTo(frame, 0, 0, 0, -10)!.Value);
        Assert.Equal((ushort)0xC000, GameTrig.InGameAngleTo(frame, 0, 0, -10, 0)!.Value);
    }

    [Fact]
    public void Speed_multiplier_follows_the_friction_and_stick_formula()
    {
        var floor = (new System.Numerics.Vector3(-100, 0, -100), new System.Numerics.Vector3(100, 0, 200), new System.Numerics.Vector3(200, 0, -100), new System.Numerics.Vector3(0, 1, 0), 0f);
        const uint surfacePool = 0x80200000;
        var ram = GameModelTests.WithSurfaces(TestRam.Us(), 1, floor).Do(p => p
            .WriteU32(TestRam.UsMario + O.FloorTri, surfacePool)
            .WriteF32(TestRam.UsMario + O.IntendedMag, 0));
        Assert.Equal(0.92f, MarioVariables.SpeedMultiplier(ram.Frame())!.Value, 4);
    }

    [Fact]
    public void Speed_multiplier_is_not_available_without_a_floor()
        => Assert.Null(MarioVariables.SpeedMultiplier(TestRam.Us().Frame()));

    [Fact]
    public void Rotation_displacement_is_zero_without_a_platform()
    {
        var frame = TestRam.Us().Frame();
        Assert.Equal((0f, 0f, 0f), MarioVariables.RotationDisplacement(frame));
    }
}
