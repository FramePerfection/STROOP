using System.Numerics;
using StroopNext.Session;

namespace StroopNext.Game;

public readonly record struct PredictedPosition(Vector3 Position, bool FullFrame, bool OnGround);

public static class MarioPrediction
{
    public static float? FloorHeight(SurfaceTable surfaces, float x, float y, float z) => GameCollision.FindFloor(surfaces, x, y, z)?.Height;

    public static List<PredictedPosition> Predict(GameFrame frame, int frames, bool quarterSteps)
    {
        var result = new List<PredictedPosition>();
        if (frame.Mario is not { } m || frames <= 0)
            return result;
        var surfaces = frame.Surfaces;
        var position = m.Position;
        bool onGround = m.Position.Y == m.FloorY;
        float normalY = onGround && surfaces.Find(m.FloorTri) is { } floorTri ? floorTri.Normal.Y : 1;
        Vector3 step;
        if (m.Velocity.X != 0 || m.Velocity.Z != 0)
            step = new Vector3(m.Velocity.X, 0, m.Velocity.Z) * (normalY / 4);
        else
        {
            var (dx, dz) = Angle.Direction(m.FacingYaw);
            step = new Vector3((float)dx, 0, (float)dz) * (m.HSpeed * normalY / 4);
        }
        float ySpeed = m.Velocity.Y;

        for (int i = 0; i < frames * 4; i++)
        {
            position += step;
            float? floor = FloorHeight(surfaces, position.X, position.Y, position.Z);
            if (onGround)
            {
                if (floor is float f && position.Y > f + 100)
                {
                    onGround = false;
                    ySpeed = 0;
                }
                else if (floor is float snap)
                    position.Y = snap;
            }
            else
            {
                position.Y += ySpeed / 4;
                if (floor is float f && position.Y <= f)
                {
                    position.Y = f;
                    onGround = true;
                }
            }
            bool fullFrame = i % 4 == 3;
            if (fullFrame && !onGround)
                ySpeed = Math.Max(ySpeed - 4, -75);
            if (fullFrame || quarterSteps)
                result.Add(new PredictedPosition(position, fullFrame, onGround));
        }
        return result;
    }

    public static int Cell(float coordinate) => (int)MathF.Floor((coordinate + 8192) / 1024);

    public static int ParallelUniverse(float coordinate) => (int)MathF.Floor((coordinate + 32768) / 65536);
}
