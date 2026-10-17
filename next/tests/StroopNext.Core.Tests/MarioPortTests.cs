using StroopNext.Game;
using StroopNext.Memory;
using StroopNext.Variables;
using O = StroopNext.Game.MarioOffsets;

namespace StroopNext.Core.Tests;

public class MarioPortTests
{
    private static Variable V(string id) => MarioVariables.All.Single(v => v.Id == id);

    private static string Show(TestRam ram, string id) => V(id).Format(ram.Frame(), DisplayOptions.Default);

    private static RamImage Apply(TestRam ram, RamPatch patch)
    {
        patch.ApplyTo(ram.Bytes);
        return new RamImage(ram.Bytes);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(4, 4)]
    [InlineData(10, 18)]
    [InlineData(42, 242)]
    public void Jump_height_matches_gravity_of_4_per_frame(double vSpeed, double height)
    {
        Assert.Equal(height, MarioMath.HeightFromVSpeed(vSpeed), 6);
        if (height > 0)
            Assert.Equal(height, MarioMath.HeightFromVSpeed(MarioMath.VSpeedForHeight(height)), 6);
    }

    [Fact]
    public void Trajectory_values_are_writable_through_the_speed()
    {
        var ram = TestRam.Us().Do(p => p.WriteF32(TestRam.UsMario + O.Y, 100).WriteF32(TestRam.UsMario + O.VelY, 42).WriteF32(TestRam.UsMario + O.HSpeed, 32));
        Assert.Equal("242", Show(ram, "mario.trajRemainingHeight"));
        Assert.Equal("342", Show(ram, "mario.trajPeakHeight"));
        Assert.Equal("60", Show(ram, "mario.djVSpeed"));

        var patch = new RamPatch();
        Assert.True(V("mario.djVSpeed").TryWrite(ram.Frame(), "62", DisplayOptions.Default, patch, out _));
        Assert.Equal(40f, Apply(ram, patch).F32(TestRam.UsMario + O.HSpeed));
    }

    [Fact]
    public void Cap_flags_are_separate_editable_bits()
    {
        var ram = TestRam.Us().Do(p => p.WriteU32(TestRam.UsMario + O.Flags, O.FlagCapOnHead | O.FlagWingCap));
        Assert.Equal("true", Show(ram, "mario.wingCap").ToLowerInvariant());
        Assert.Equal("false", Show(ram, "mario.metalCap").ToLowerInvariant());
        var patch = new RamPatch();
        Assert.True(V("mario.metalCap").TryWrite(ram.Frame(), "true", DisplayOptions.Default, patch, out _));
        Assert.Equal(O.FlagCapOnHead | O.FlagWingCap | O.FlagMetalCap, Apply(ram, patch).U32(TestRam.UsMario + O.Flags));
    }

    [Fact]
    public void Holp_commands_copy_only_the_chosen_axes()
    {
        var ram = TestRam.Us().Do(p => p
            .WriteF32(TestRam.UsMario + O.X, 1).WriteF32(TestRam.UsMario + O.Y, 2).WriteF32(TestRam.UsMario + O.Z, 3)
            .WriteF32(TestRam.UsMario + O.HolpX, 10).WriteF32(TestRam.UsMario + O.HolpY, 20).WriteF32(TestRam.UsMario + O.HolpZ, 30));
        var after = Apply(ram, MarioCommands.GoToHolp(ram.Frame(), Axes.Lateral));
        Assert.Equal(10f, after.F32(TestRam.UsMario + O.X));
        Assert.Equal(2f, after.F32(TestRam.UsMario + O.Y));
        Assert.Equal(30f, after.F32(TestRam.UsMario + O.Z));

        after = Apply(ram, MarioCommands.RetrieveHolp(ram.Frame(), Axes.Y));
        Assert.Equal(2f, after.F32(TestRam.UsMario + O.HolpY));
        Assert.Equal(10f, after.F32(TestRam.UsMario + O.HolpX));
    }

    [Fact]
    public void Holp_distance_and_angle_move_mario_around_the_holp()
    {
        var ram = TestRam.Us().Do(p => p.WriteF32(TestRam.UsMario + O.HolpX, 100).WriteF32(TestRam.UsMario + O.HolpZ, 0));
        Assert.Equal("100", Show(ram, "mario.holpHDistance"));
        var patch = new RamPatch();
        Assert.True(V("mario.holpHDistance").TryWrite(ram.Frame(), "40", DisplayOptions.Default, patch, out _));
        var after = Apply(ram, patch);
        Assert.Equal(60f, after.F32(TestRam.UsMario + O.X), 3);
        Assert.Equal(0f, after.F32(TestRam.UsMario + O.Z), 3);
    }

    [Fact]
    public void Slide_commands_keep_the_moving_yaw_in_sync()
    {
        var ram = TestRam.Us().Do(p => p.WriteF32(TestRam.UsMario + O.SlideVelX, 0).WriteF32(TestRam.UsMario + O.SlideVelZ, 10));
        var after = Apply(ram, MarioCommands.AddSlideVelocity(ram.Frame(), 10, -10));
        Assert.Equal(0x4000, after.U16(TestRam.UsMario + O.SlideYaw));

        ram = TestRam.Us().Do(p => p.WriteF32(TestRam.UsMario + O.SlideVelX, 0).WriteF32(TestRam.UsMario + O.SlideVelZ, 10));
        after = Apply(ram, MarioCommands.AdjustSlide(ram.Frame(), 5, 0x4000));
        Assert.Equal(15f, after.F32(TestRam.UsMario + O.SlideVelX), 3);
        Assert.Equal(0f, after.F32(TestRam.UsMario + O.SlideVelZ), 3);
    }

    [Fact]
    public void Hands_free_uses_the_action_table_and_needs_a_held_object()
    {
        var ram = TestRam.Us().Do(p => p.WriteU32(TestRam.UsMario + O.Action, 0x0C400201));
        Assert.True(MarioCommands.HandsFree(ram.Frame()).IsEmpty);
        ram.Patch.WriteU32(TestRam.UsMario + O.HeldObject, 0x80340000);
        var after = Apply(ram, MarioCommands.HandsFree(ram.Frame()));
        Assert.Equal(0x08000207u, after.U32(TestRam.UsMario + O.Action));
    }

    [Theory]
    [InlineData("none", 0u, null)]
    [InlineData("slot 12", 0u, 12)]
    [InlineData("slot 12 · Goomba", 0u, 12)]
    [InlineData("0x80341234", 0x80341234u, null)]
    public void Object_references_parse_slots_names_and_addresses(string text, uint address, int? slot)
    {
        Assert.True(ObjectReferenceFormat.Instance.TryParse(text, DisplayOptions.Default, out var value));
        Assert.Equal(address, value.Address);
        Assert.Equal(slot, value.Slot);
    }

    [Fact]
    public void Every_mario_variable_has_a_unique_id_and_a_known_group()
    {
        Assert.Equal(MarioVariables.All.Count, MarioVariables.All.Select(v => v.Id).Distinct().Count());
        Assert.All(MarioVariables.All, v => Assert.Contains(v.Group, MarioVariables.Groups));
        Assert.True(MarioVariables.All.Count >= 90);
    }
}
