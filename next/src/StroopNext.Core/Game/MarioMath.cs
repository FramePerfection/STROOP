namespace StroopNext.Game;

public static class MarioMath
{
    public static double HeightFromVSpeed(double vSpeed)
    {
        if (vSpeed <= 0)
            return 0;
        double frames = Math.Ceiling(vSpeed / 4);
        double last = vSpeed - (frames - 1) * 4;
        return frames * (vSpeed + last) / 2;
    }

    public static double VSpeedForHeight(double height)
    {
        if (height <= 0)
            return 0;
        double frames = Math.Ceiling((-2 + Math.Sqrt(4 + 8 * height)) / 4);
        return (height + 2 * frames * (frames - 1)) / frames;
    }

    public static double DoubleJumpVSpeed(double hSpeed) => hSpeed / 4 + 52;

    public static double HSpeedForDoubleJumpVSpeed(double vSpeed) => (vSpeed - 52) * 4;

    public static double FlyingEnergy(double hSpeed, double y) => hSpeed * hSpeed + 4 / Math.PI * y;
}
