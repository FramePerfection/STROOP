using System.Numerics;
using StroopNext.Game;

namespace StroopNext.Map;

public sealed class MapView
{
    public const double MinScale = 1.0 / 256;
    public const double MaxScale = 256;

    public Vector2 Center { get; set; }

    public double Scale { get; set; } = 0.05;

    public double Rotation { get; set; }

    public float Width { get; set; } = 1;
    public float Height { get; set; } = 1;

    public Vector2 WorldToScreen(Vector2 world)
    {
        double dx = world.X - Center.X, dz = world.Y - Center.Y;
        double c = Math.Cos(Rotation), s = Math.Sin(Rotation);
        double rx = dx * c - dz * s, rz = dx * s + dz * c;
        return new Vector2((float)(Width / 2 + rx * Scale), (float)(Height / 2 + rz * Scale));
    }

    public Vector2 ScreenToWorld(Vector2 screen)
    {
        double rx = (screen.X - Width / 2) / Scale, rz = (screen.Y - Height / 2) / Scale;
        double c = Math.Cos(Rotation), s = Math.Sin(Rotation);
        return new Vector2((float)(Center.X + rx * c + rz * s), (float)(Center.Y - rx * s + rz * c));
    }

    public void PointYawUp(double yawUnits) => Rotation = Angle.ToRadians(yawUnits) + Math.PI;

    public double ScreenAngleOfYaw(double yawUnits)
    {
        var (x, z) = Angle.Direction(yawUnits);
        double c = Math.Cos(Rotation), s = Math.Sin(Rotation);
        return Math.Atan2(x * s + z * c, x * c - z * s);
    }

    public void ZoomAt(Vector2 screenAnchor, double factor)
    {
        var before = ScreenToWorld(screenAnchor);
        Scale = Math.Clamp(Scale * factor, MinScale, MaxScale);
        var after = ScreenToWorld(screenAnchor);
        Center += before - after;
    }

    public void PanByScreen(Vector2 deltaPixels)
    {
        var a = ScreenToWorld(Vector2.Zero);
        var b = ScreenToWorld(deltaPixels);
        Center -= b - a;
    }

    public float CullMarginPx { get; set; }

    private (Vector2, double, double, float, float, float) _boundsKey;
    private (Vector2 Min, Vector2 Max) _bounds;

    public (Vector2 Min, Vector2 Max) VisibleBounds()
    {
        var key = (Center, Scale, Rotation, Width, Height, CullMarginPx);
        if (key == _boundsKey && _bounds != default)
            return _bounds;
        _boundsKey = key;
        return _bounds = ComputeBounds(CullMarginPx);
    }

    private (Vector2 Min, Vector2 Max) ComputeBounds(float margin)
    {
        Span<Vector2> corners =
        [
            ScreenToWorld(new(-margin, -margin)), ScreenToWorld(new(Width + margin, -margin)),
            ScreenToWorld(new(-margin, Height + margin)), ScreenToWorld(new(Width + margin, Height + margin)),
        ];
        var min = corners[0];
        var max = corners[0];
        foreach (var c in corners)
        {
            min = Vector2.Min(min, c);
            max = Vector2.Max(max, c);
        }
        return (min, max);
    }

    public bool IsVisible(Vector2 min, Vector2 max)
    {
        var (vMin, vMax) = VisibleBounds();
        return max.X >= vMin.X && min.X <= vMax.X && max.Y >= vMin.Y && min.Y <= vMax.Y;
    }
}
