using System.Numerics;
using StroopNext.Game;
using StroopNext.Map;

namespace StroopNext.Core.Tests;

public class AirMovementTests
{
    private static readonly AirState Start = new(Vector3.Zero, new Vector3(0, 30, 20), 20, 0);

    [Fact]
    public void Neutral_stick_drags_speed_by_0_35_and_applies_gravity_after_moving()
    {
        var s = AirMovement.Step(Start, RelativeDirection.Center);
        Assert.Equal(19.65f, s.HSpeed, 4);
        Assert.Equal(19.65f, s.Position.Z, 3);
        Assert.Equal(0, s.Position.X, 4);
        Assert.Equal(30, s.Position.Y, 4);
        Assert.Equal(26, s.Velocity.Y, 4);
    }

    [Fact]
    public void Forward_adds_1_5_and_sideways_pushes_10_across()
    {
        Assert.Equal(19.65f + 1.5f, AirMovement.Step(Start, RelativeDirection.Forward).HSpeed, 4);
        var left = AirMovement.Step(Start, RelativeDirection.Left);
        Assert.Equal(19.65f, left.HSpeed, 4);
        Assert.Equal(10, MathF.Abs(left.Position.X), 3);
    }

    [Fact]
    public void Gravity_caps_at_minus_75()
        => Assert.Equal(-75, AirMovement.Step(Start with { Velocity = new Vector3(0, -74, 0) }, RelativeDirection.Center).Velocity.Y);

    [Fact]
    public void Partial_frames_move_by_the_quarter_steps_taken()
    {
        var two = AirMovement.Repeat(Start, RelativeDirection.Center, 2);
        Assert.Equal(19.65f / 2, two.Position.Z, 3);
        var six = AirMovement.Repeat(Start, RelativeDirection.Center, 6);
        Assert.Equal(19.65f + 19.3f / 2, six.Position.Z, 3);
    }

    [Fact]
    public void Iwerlipses_grow_with_the_quarter_steps_and_match_the_stick_reach()
    {
        var ellipses = Iwerlipse.From(Start, 4);
        Assert.Equal(4, ellipses.Count);
        Assert.Equal(10, ellipses[3].SideRadius, 3);
        Assert.Equal(1.5f, ellipses[3].ForwardRadius, 3);
        Assert.True(ellipses[0].SideRadius < ellipses[3].SideRadius);
        Assert.All(ellipses[3].Outline(), p => Assert.True(Vector2.Distance(p, new Vector2(0, 19.65f)) <= 10.01f));
    }
}
