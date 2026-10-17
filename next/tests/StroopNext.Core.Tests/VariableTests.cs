using System.Numerics;
using StroopNext.Memory;
using StroopNext.Variables;
using O = StroopNext.Game.MarioOffsets;

namespace StroopNext.Core.Tests;

public class VariableTests
{
    private static Variable V(string id) => MarioVariables.All.Single(v => v.Id == id);

    [Fact]
    public void Variable_ids_are_unique_and_grouped()
    {
        var ids = MarioVariables.All.Select(v => v.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
        Assert.All(MarioVariables.All, v => Assert.Contains(v.Group, MarioVariables.Groups));
    }

    [Fact]
    public void Memory_variable_reads_formats_and_writes()
    {
        var ram = TestRam.Us().Do(p => p.WriteF32(TestRam.UsMario + O.X, 12.5f));
        var frame = ram.Frame();
        var x = V("mario.x");
        Assert.Equal("12.5", x.Format(frame, DisplayOptions.Default));

        var patch = new RamPatch();
        Assert.True(x.TryWrite(frame, "-100.25", DisplayOptions.Default, patch, out _));
        patch.ApplyTo(ram.Bytes);
        Assert.Equal(-100.25f, new RamImage(ram.Bytes).F32(TestRam.UsMario + O.X));
    }

    [Fact]
    public void Invalid_input_and_read_only_variables_report_errors()
    {
        var frame = TestRam.Us().Frame();
        Assert.False(V("mario.x").TryWrite(frame, "abc", DisplayOptions.Default, new RamPatch(), out string? error));
        Assert.Contains("not a valid value", error);
        Assert.False(V("mario.ceilingY").TryWrite(frame, "1", DisplayOptions.Default, new RamPatch(), out error));
        Assert.Contains("read-only", error);
    }

    [Fact]
    public void Unknown_game_shows_not_available()
    {
        var frame = new TestRam().Frame();
        Assert.All(MarioVariables.All, v => Assert.Equal(Variable.NotAvailable, v.Format(frame, DisplayOptions.Default)));
    }

    [Fact]
    public void De_facto_speed_uses_floor_normal_only_when_grounded()
    {
        const uint tri = 0x80200000;
        var ram = TestRam.Us().Do(p => p
            .WriteF32(TestRam.UsMario + O.HSpeed, 40).WriteU32(TestRam.UsMario + O.FloorTri, tri).WriteF32(tri + 0x20, 0.5f)
            .WriteF32(TestRam.UsMario + O.Y, 100).WriteF32(TestRam.UsMario + O.FloorY, 100));
        Assert.Equal(20f, MarioVariables.DeFactoSpeed(ram.Frame()));

        ram.Patch.WriteF32(TestRam.UsMario + O.Y, 150);
        Assert.Equal(40f, MarioVariables.DeFactoSpeed(ram.Frame()));
    }

    [Theory]
    [InlineData("16384", 16384)]
    [InlineData("0x4000", 16384)]
    [InlineData("90°", 16384)]
    [InlineData("90 deg", 16384)]
    [InlineData("16384  (90°)", 16384)]
    [InlineData("-1", 65535)]
    public void Angles_parse_units_hex_and_degrees(string text, int expected)
    {
        Assert.True(Formats.Angle.TryParse(text, DisplayOptions.Default, out int value));
        Assert.Equal(expected, value);
    }

    [Fact]
    public void Angles_format_according_to_display_options()
    {
        Assert.Equal("16384  (90°)", Formats.Angle.Format(16384, DisplayOptions.Default));
        Assert.Equal("90°", Formats.Angle.Format(16384, DisplayOptions.Default with { Angles = AngleDisplay.Degrees }));
        Assert.Equal("-16384", Formats.SignedAngle.Format(0xC000, DisplayOptions.Default with { Angles = AngleDisplay.Units }));
    }

    [Fact]
    public void Deltas_compare_with_previous_game_frame_and_wrap_angles()
    {
        var before = TestRam.Us().Do(p => p.WriteF32(TestRam.UsMario + O.X, 10).WriteU16(TestRam.UsMario + O.FacingYaw, 0xFFFF)).Frame();
        var now = TestRam.Us().Do(p => p.WriteF32(TestRam.UsMario + O.X, 13.5f).WriteU16(TestRam.UsMario + O.FacingYaw, 1)).Frame(before);

        Assert.Equal("3.5", V("mario.x").FormatDelta(now, DisplayOptions.Default));
        Assert.Equal("+2", V("mario.facingYaw").FormatDelta(now, DisplayOptions.Default));
        Assert.Null(V("mario.x").FormatDelta(before, DisplayOptions.Default));
    }

    [Fact]
    public void Action_format_shows_names_and_accepts_names_or_hex()
    {
        var action = (Variable<uint>)V("mario.action");
        Assert.Equal("standing  (0x0C400201)", action.ValueFormat.Format(0x0C400201, DisplayOptions.Default));
        Assert.True(action.ValueFormat.TryParse("walking/running", DisplayOptions.Default, out uint walking));
        Assert.Equal(0x04000440u, walking);
        Assert.True(action.ValueFormat.TryParse("standing  (0x0C400201)", DisplayOptions.Default, out uint standing));
        Assert.Equal(0x0C400201u, standing);
    }

    [Fact]
    public void Computed_writers_express_intent()
    {
        var ram = TestRam.Us().Do(p => p
            .WriteF32(TestRam.UsMario + O.FloorY, 200).WriteF32(TestRam.UsMario + O.SlideVelX, 3).WriteF32(TestRam.UsMario + O.SlideVelZ, 4)
            .WriteU32(TestRam.UsMario + O.Flags, 0x11));
        var frame = ram.Frame();
        var patch = new RamPatch();
        Assert.True(V("mario.heightAboveFloor").TryWrite(frame, "50", DisplayOptions.Default, patch, out _));
        Assert.True(V("mario.slidingSpeed").TryWrite(frame, "10", DisplayOptions.Default, patch, out _));
        Assert.True(V("mario.hatOnHead").TryWrite(frame, "false", DisplayOptions.Default, patch, out _));
        patch.ApplyTo(ram.Bytes);

        var m = GameFrameOf(ram.Bytes).Mario!;
        Assert.Equal(250f, m.Position.Y);
        Assert.Equal(6f, m.SlideVelX, 4);
        Assert.Equal(8f, m.SlideVelZ, 4);
        Assert.Equal(0x01u, m.Flags);
    }

    [Fact]
    public void Relative_move_follows_facing_yaw()
    {
        var ram = TestRam.Us().Do(p => p.WriteU16(TestRam.UsMario + O.FacingYaw, 0x4000));
        var patch = MarioCommands.MoveRelative(ram.Frame(), forward: 10, right: 5, up: 1);
        patch.ApplyTo(ram.Bytes);
        var p = GameFrameOf(ram.Bytes).Mario!.Position;
        Assert.Equal(10, p.X, 3);
        Assert.Equal(1, p.Y, 3);
        Assert.Equal(5, p.Z, 3);
    }

    private static Session.GameFrame GameFrameOf(byte[] bytes) => Session.GameFrame.Create(new RamImage(bytes));
}
