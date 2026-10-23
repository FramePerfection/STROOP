using System.Numerics;

namespace StroopNext.Map3D;

public readonly record struct Hit3D(Primitive3D Primitive, float Distance, Vector3 Point);

public static class Picking
{
    public static Hit3D? Pick(Scene3D scene, Camera3D camera, Vector2 screen, Func<Primitive3D, bool>? filter = null, float pointTolerancePx = 6)
    {
        var ray = camera.ScreenRay(screen);
        Hit3D? best = null;
        Hit3D? bestPoint = null;
        float bestPointPx = pointTolerancePx;

        foreach (var p in scene.All)
        {
            if (filter is not null && !filter(p))
                continue;
            switch (p)
            {
                case Point3D point:
                    if (camera.IsInsideSlab(point.Position) && camera.Project(point.Position) is { } at && Vector2.Distance(at, screen) <= bestPointPx)
                    {
                        bestPointPx = Vector2.Distance(at, screen);
                        bestPoint = new Hit3D(p, Vector3.Distance(camera.Position, point.Position), point.Position);
                    }
                    continue;
                case Triangle3D t when RayTriangle(ray, t.A, t.B, t.C, out float tt):
                    Consider(p, tt);
                    break;
                case Cylinder3D c when RayCylinder(ray, c.Base, c.Radius, c.Height, out float tc):
                    Consider(p, tc);
                    break;
            }
        }
        return bestPoint ?? best;

        void Consider(Primitive3D p, float t)
        {
            if (!camera.IsInsideSlab(ray.At(t)))
                return;
            if (best is null || t < best.Value.Distance)
                best = new Hit3D(p, t, ray.At(t));
        }
    }

    public static (Game.Surface Surface, Vector3 Point)? PickSurface(Game.SurfaceTable surfaces, Ray ray)
    {
        (Game.Surface, Vector3)? best = null;
        float bestT = float.MaxValue;
        foreach (var list in new[] { surfaces.Static, surfaces.Dynamic })
            foreach (var s in list)
                if (RayTriangle(ray, s.V1, s.V2, s.V3, out float t) && t < bestT)
                {
                    bestT = t;
                    best = (s, ray.Origin + ray.Direction * t);
                }
        return best;
    }

    public static Vector3? TeleportTarget(Game.SurfaceTable surfaces, Ray ray)
    {
        if (PickSurface(surfaces, ray) is not var (surface, point))
            return null;
        var probe = point - ray.Direction * 2;
        float startY = surface.Class == Game.SurfaceClass.Floor ? point.Y + 10 : probe.Y;
        return Game.GameCollision.FindFloor(surfaces, probe.X, startY, probe.Z) is { } floor
            ? new Vector3(probe.X, floor.Height, probe.Z)
            : surface.Class == Game.SurfaceClass.Floor ? point : null;
    }

    public static bool RayTriangle(Ray ray, Vector3 a, Vector3 b, Vector3 c, out float t)
    {
        t = 0;
        var e1 = b - a;
        var e2 = c - a;
        var p = Vector3.Cross(ray.Direction, e2);
        float det = Vector3.Dot(e1, p);
        if (MathF.Abs(det) < 1e-8f)
            return false;
        float inv = 1 / det;
        var s = ray.Origin - a;
        float u = Vector3.Dot(s, p) * inv;
        if (u < 0 || u > 1)
            return false;
        var q = Vector3.Cross(s, e1);
        float v = Vector3.Dot(ray.Direction, q) * inv;
        if (v < 0 || u + v > 1)
            return false;
        t = Vector3.Dot(e2, q) * inv;
        return t > 0;
    }

    public static bool RayCylinder(Ray ray, Vector3 baseCenter, float radius, float height, out float t)
    {
        t = float.MaxValue;
        var o = ray.Origin - baseCenter;
        var d = ray.Direction;

        float a = d.X * d.X + d.Z * d.Z;
        if (a > 1e-12f)
        {
            float b = 2 * (o.X * d.X + o.Z * d.Z);
            float c = o.X * o.X + o.Z * o.Z - radius * radius;
            float disc = b * b - 4 * a * c;
            if (disc >= 0)
            {
                float sq = MathF.Sqrt(disc);
                foreach (float root in new[] { (-b - sq) / (2 * a), (-b + sq) / (2 * a) })
                {
                    float y = o.Y + root * d.Y;
                    if (root > 0 && y >= 0 && y <= height && root < t)
                        t = root;
                }
            }
        }
        if (MathF.Abs(d.Y) > 1e-12f)
        {
            foreach (float capY in new[] { 0f, height })
            {
                float root = (capY - o.Y) / d.Y;
                if (root <= 0 || root >= t)
                    continue;
                float x = o.X + root * d.X, z = o.Z + root * d.Z;
                if (x * x + z * z <= radius * radius)
                    t = root;
            }
        }
        return t < float.MaxValue;
    }
}
