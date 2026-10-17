using System.Numerics;

namespace StroopNext.Game;

public readonly record struct AirState(Vector3 Position, Vector3 Velocity, float HSpeed, ushort FacingYaw)
{
    public static AirState Of(MarioState m) => new(m.Position, m.Velocity, m.HSpeed, m.FacingYaw);
}

public enum RelativeDirection { Center, Forward, Backward, Left, Right }

public static class AirMovement
{
    public static AirState Step(AirState s, RelativeDirection direction, int quarterSteps = 4)
    {
        int dyaw = direction switch
        {
            RelativeDirection.Backward => 0x8000,
            RelativeDirection.Left => 0x4000,
            RelativeDirection.Right => 0xC000,
            _ => 0,
        };
        float magnitude = direction == RelativeDirection.Center ? 0 : 32;
        return Step(s, dyaw, magnitude, quarterSteps);
    }

    public static AirState Step(AirState s, int deltaIntendedFacing, float magnitude, int quarterSteps = 4)
    {
        float hSpeed = Approach(s.HSpeed, 0, 0.35f);
        float perp = 0;
        if (magnitude > 0)
        {
            hSpeed += magnitude / 32 * 1.5f * GameTrig.InGameCosine(deltaIntendedFacing);
            perp = GameTrig.InGameSine(deltaIntendedFacing) * (magnitude / 32) * 10;
        }
        if (hSpeed > 32)
            hSpeed -= 1;
        if (hSpeed < -16)
            hSpeed += 2;
        float vx = GameTrig.InGameSine(s.FacingYaw) * hSpeed + perp * GameTrig.InGameSine(s.FacingYaw + 0x4000);
        float vz = GameTrig.InGameCosine(s.FacingYaw) * hSpeed + perp * GameTrig.InGameCosine(s.FacingYaw + 0x4000);

        var velocity = new Vector3(vx, s.Velocity.Y, vz);
        var position = s.Position + velocity / 4 * quarterSteps;

        velocity.Y = MathF.Max(velocity.Y - 4, -75);
        return new AirState(position, velocity, hSpeed, s.FacingYaw);
    }

    public static AirState Repeat(AirState s, RelativeDirection direction, int quarterSteps)
    {
        for (int i = 0; i < quarterSteps / 4; i++)
            s = Step(s, direction);
        return quarterSteps % 4 == 0 ? s : Step(s, direction, quarterSteps % 4);
    }

    private static float Approach(float value, float target, float step)
        => value < target ? MathF.Min(target, value + step) : MathF.Max(target, value - step);
}
