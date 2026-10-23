using System.Numerics;
using StroopNext.Game;
using StroopNext.Map3D;
using StroopNext.Settings;

namespace StroopNext.Map;


internal static class Shapes3D
{
    public static void Box(Scene3D scene, Vector3 min, Vector3 max, Argb8Color color)
    {
        Vector3 c000 = min, c111 = max;
        Vector3 c100 = new(max.X, min.Y, min.Z), c010 = new(min.X, max.Y, min.Z), c001 = new(min.X, min.Y, max.Z);
        Vector3 c110 = new(max.X, max.Y, min.Z), c101 = new(max.X, min.Y, max.Z), c011 = new(min.X, max.Y, max.Z);
        void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            scene.Add(new Triangle3D(a, b, c, color, Edges: false));
            scene.Add(new Triangle3D(a, c, d, color, Edges: false));
        }
        Quad(c000, c100, c110, c010);
        Quad(c001, c101, c111, c011);
        Quad(c000, c001, c011, c010);
        Quad(c100, c101, c111, c110);
        Quad(c000, c100, c101, c001);
        Quad(c010, c110, c111, c011);
        var edge = color with { A = 255 };
        scene.Add(new Line3D([c000, c100, c110, c010, c000, c001, c101, c111, c011, c001], edge));
        scene.Add(new Line3D([c100, c101], edge));
        scene.Add(new Line3D([c110, c111], edge));
        scene.Add(new Line3D([c010, c011], edge));
    }

    public static void ColumnGrid(Scene3D scene, double minX, double maxX, double minZ, double maxZ, double spacing, float[] levels, Argb8Color color)
    {
        int nx = (int)Math.Min(64, Math.Round((maxX - minX) / spacing) + 1), nz = (int)Math.Min(64, Math.Round((maxZ - minZ) / spacing) + 1);
        foreach (float y in levels)
        {
            for (int i = 0; i < nx; i++)
            {
                float x = (float)(minX + i * spacing);
                scene.Add(new Line3D([new(x, y, (float)minZ), new(x, y, (float)maxZ)], color));
            }
            for (int k = 0; k < nz; k++)
            {
                float z = (float)(minZ + k * spacing);
                scene.Add(new Line3D([new((float)minX, y, z), new((float)maxX, y, z)], color));
            }
        }
        float y0 = levels.Min(), y1 = levels.Max();
        for (int i = 0; i < nx; i++)
        for (int k = 0; k < nz; k++)
        {
            float x = (float)(minX + i * spacing), z = (float)(minZ + k * spacing);
            scene.Add(new Line3D([new(x, y0, z), new(x, y1, z)], color));
        }
    }

    public static bool Finite(Vector3 p) => float.IsFinite(p.X) && float.IsFinite(p.Y) && float.IsFinite(p.Z) && Math.Abs(p.X) < 1e7 && Math.Abs(p.Z) < 1e7;
}

public sealed class CellsLayer : MapLayer, IMapLayer3D
{
    public override string Id => "cells";
    public override string Name => "Collision cells";
    public override bool VisibleByDefault => false;

    public readonly Setting<bool> CurrentCell;

    public CellsLayer() => CurrentCell = Declare(new Setting<bool>("current", "Highlight Mario's cell", true));

    private static readonly Argb8Color GridColor = new(170, 0, 0, 0);
    private static readonly Argb8Color CellColor = Argb8Color.Rgb(255, 230, 40);

    public override void Build(MapContext c)
    {
        var (min, max) = c.View.VisibleBounds();
        if (1024 * c.View.Scale < 6)
            return;
        for (float x = MathF.Floor(min.X / 1024) * 1024; x <= max.X; x += 1024)
            c.Scene.Add(new MapPolyline([new(x, min.Y), new(x, max.Y)], GridColor, 1.5f));
        for (float z = MathF.Floor(min.Y / 1024) * 1024; z <= max.Y; z += 1024)
            c.Scene.Add(new MapPolyline([new(min.X, z), new(max.X, z)], GridColor, 1.5f));
        if (c.Settings.Get(CurrentCell) && c.Frame.Mario is { } m && Shapes3D.Finite(m.Position))
        {
            float x0 = (MarioPrediction.Cell(m.Position.X) - 8) * 1024, z0 = (MarioPrediction.Cell(m.Position.Z) - 8) * 1024;
            c.Scene.Add(new MapPolygon([new(x0, z0), new(x0 + 1024, z0), new(x0 + 1024, z0 + 1024), new(x0, z0 + 1024)], CellColor.WithAlpha(0.2), CellColor, 2));
        }
    }

    public void Build3D(Map3DContext c)
    {
        if (c.Frame.Mario is not { } m || !Shapes3D.Finite(m.Position))
            return;
        float y = m.Position.Y;
        double xMin = (Math.Floor(m.Position.X / 1024) - 3) * 1024, zMin = (Math.Floor(m.Position.Z / 1024) - 3) * 1024;
        Shapes3D.ColumnGrid(c.Scene, xMin, xMin + 7 * 1024, zMin, zMin + 7 * 1024, 1024, [y - 2000, y, y + 2000], GridColor);
        if (c.Settings.Get(CurrentCell))
        {
            float x0 = (MarioPrediction.Cell(m.Position.X) - 8) * 1024, z0 = (MarioPrediction.Cell(m.Position.Z) - 8) * 1024;
            Shapes3D.Box(c.Scene, new Vector3(x0, y - 500, z0), new Vector3(x0 + 1024, y + 500, z0 + 1024), CellColor.WithAlpha(0.15));
        }
    }
}

public sealed class ParallelUniversesLayer : MapLayer, IMapLayer3D
{
    public override string Id => "pu";
    public override string Name => "Parallel universes";
    public override bool VisibleByDefault => false;

    public readonly Setting<bool> LevelCopies;

    public ParallelUniversesLayer() =>
        LevelCopies = Declare(new Setting<bool>("copies", "Draw the level in neighbouring PUs (3D)", true, "The 3×3 PUs around the camera, drawn with GPU instancing."));

    private static readonly Argb8Color PuColor = new(255, 255, 51, 255);

    public override void Build(MapContext c)
    {
        var (min, max) = c.View.VisibleBounds();
        if (65536 * c.View.Scale < 6)
            return;
        var color = PuColor.WithAlpha(0.8);
        for (float x = MathF.Floor((min.X + 32768) / 65536) * 65536 - 32768; x <= max.X; x += 65536)
            c.Scene.Add(new MapPolyline([new(x, min.Y), new(x, max.Y)], color, 2));
        for (float z = MathF.Floor((min.Y + 32768) / 65536) * 65536 - 32768; z <= max.Y; z += 65536)
            c.Scene.Add(new MapPolyline([new(min.X, z), new(max.X, z)], color, 2));
        if (c.Frame.Mario is { } m && Shapes3D.Finite(m.Position))
        {
            var label = $"Mario PU ({MarioPrediction.ParallelUniverse(m.Position.X)}, {MarioPrediction.ParallelUniverse(m.Position.Z)})";
            c.Scene.Add(new MapLabel(new Vector2(m.Position.X, m.Position.Z), label, PuColor, 10, 22));
        }
    }

    public void Build3D(Map3DContext c)
    {
        if (c.Frame.Mario is not { } m || !Shapes3D.Finite(m.Position))
            return;
        float y = m.Position.Y;
        double cx = Math.Round(c.Camera.Target.X / 65536) * 65536, cz = Math.Round(c.Camera.Target.Z / 65536) * 65536;
        Shapes3D.ColumnGrid(c.Scene, cx - 3 * 65536 - 32768, cx + 3 * 65536 - 32768, cz - 3 * 65536 - 32768, cz + 3 * 65536 - 32768, 65536,
            [y - 2000, y, y + 2000], PuColor.WithAlpha(0.6));
        if (c.Settings.Get(LevelCopies) && c.Scene.GameVisuals is not null)
        {
            int ci = (int)Math.Round(c.Camera.Target.X / 65536), ck = (int)Math.Round(c.Camera.Target.Z / 65536);
            for (int i = ci - 1; i <= ci + 1; i++)
            for (int k = ck - 1; k <= ck + 1; k++)
                if (i != 0 || k != 0)
                    c.Scene.LevelCopies.Add(new Vector3(i * 65536f, 0, k * 65536f));
            float reach = Vector3.Distance(c.Camera.Position, new Vector3(ci * 65536f, 0, ck * 65536f)) + 1.5f * 65536 * MathF.Sqrt(2);
            c.Camera.Far = Math.Max(c.Camera.Far, reach * 1.05f);
        }

        var p = m.Position;
        c.Scene.Add(new Line3D([p, p + new Vector3(0, 20000, 0)], PuColor));
        float px = MarioPrediction.ParallelUniverse(p.X) * 65536f, pz = MarioPrediction.ParallelUniverse(p.Z) * 65536f;
        c.Scene.Add(new Line3D([new(px - 32768, y, pz - 32768), new(px + 32768, y, pz - 32768), new(px + 32768, y, pz + 32768), new(px - 32768, y, pz + 32768), new(px - 32768, y, pz - 32768)], PuColor));
    }
}

public sealed class CurrentUnitLayer : MapLayer, IMapLayer3D
{
    public override string Id => "unit";
    public override string Name => "Current unit";
    public override bool VisibleByDefault => false;

    private static readonly Argb8Color UnitColor = Argb8Color.Rgb(170, 60, 255);

    private static (float X0, float X1) Span1(float v)
    {
        float t = MathF.Truncate(v);
        return v >= 0 ? (t, t + 1) : (t - 1, t);
    }

    public override void Build(MapContext c)
    {
        if (c.Frame.Mario is not { } m || !Shapes3D.Finite(m.Position))
            return;
        var (x0, x1) = Span1(m.Position.X);
        var (z0, z1) = Span1(m.Position.Z);
        c.Scene.Add(new MapPolygon([new(x0, z0), new(x1, z0), new(x1, z1), new(x0, z1)], UnitColor.WithAlpha(0.45), UnitColor, 1.5f));
    }

    public void Build3D(Map3DContext c)
    {
        if (c.Frame.Mario is not { } m || !Shapes3D.Finite(m.Position))
            return;
        var (x0, x1) = Span1(m.Position.X);
        var (z0, z1) = Span1(m.Position.Z);
        float y = (MarioPrediction.FloorHeight(c.Frame.Surfaces, m.Position.X, m.Position.Y, m.Position.Z) ?? m.Position.Y) + 1;
        c.Scene.Add(new Triangle3D(new(x0, y, z0), new(x1, y, z0), new(x1, y, z1), UnitColor.WithAlpha(0.6), Edges: false));
        c.Scene.Add(new Triangle3D(new(x0, y, z0), new(x1, y, z1), new(x0, y, z1), UnitColor.WithAlpha(0.6), Edges: false));
    }
}

public sealed class FacingDividerLayer : MapLayer, IMapLayer3D
{
    public override string Id => "divider";
    public override string Name => "Facing divider";
    public override bool VisibleByDefault => false;

    public readonly Setting<int> Size;
    private static readonly Argb8Color DividerColor = Argb8Color.Rgb(230, 40, 40);

    public FacingDividerLayer() => Size = Declare(new Setting<int>("size", "Half length", 1000) { Range = (10, 20000, 50) });

    private static (Vector3 A, Vector3 B) Ends(MarioState m, float size)
    {
        var (lx, lz) = Angle.Direction(m.FacingYaw - 16384);
        var (rx, rz) = Angle.Direction(m.FacingYaw + 16384);
        return (m.Position + new Vector3((float)lx, 0, (float)lz) * size, m.Position + new Vector3((float)rx, 0, (float)rz) * size);
    }

    public override void Build(MapContext c)
    {
        if (c.Frame.Mario is not { } m)
            return;
        var (a, b) = Ends(m, c.Settings.Get(Size));
        c.Scene.Add(new MapPolyline([new(a.X, a.Z), new(b.X, b.Z)], DividerColor, 3));
    }

    public void Build3D(Map3DContext c)
    {
        if (c.Frame.Mario is not { } m)
            return;
        float size = c.Settings.Get(Size);
        var (a, b) = Ends(m, size);
        var up = new Vector3(0, size, 0);
        c.Scene.Add(new Triangle3D(a - up, b - up, b + up, DividerColor.WithAlpha(0.25), Edges: false));
        c.Scene.Add(new Triangle3D(a - up, b + up, a + up, DividerColor.WithAlpha(0.25), Edges: false));
        c.Scene.Add(new Line3D([a, b], DividerColor));
    }
}

public sealed class SectorLayer : MapLayer, IMapLayer3D
{
    public override string Id => "sector";
    public override string Name => "Facing sector";
    public override bool VisibleByDefault => false;

    public readonly Setting<int> Radius;
    public readonly Setting<int> HalfAngle;
    private static readonly Argb8Color SectorColor = Argb8Color.Rgb(255, 220, 40);

    public SectorLayer()
    {
        Radius = Declare(new Setting<int>("radius", "Radius", 1000) { Range = (10, 20000, 50) });
        HalfAngle = Declare(new Setting<int>("halfAngle", "Half angle (units)", 4096) { Range = (1, 32768, 256) });
    }

    private static List<Vector3> Arc(MarioState m, float radius, int halfAngle)
    {
        int span = Math.Min(2 * Math.Abs(halfAngle), 65536);
        int segments = Math.Max(2, (int)Math.Ceiling(256.0 * span / 65536));
        var points = new List<Vector3>(segments + 1);
        for (int i = 0; i <= segments; i++)
        {
            var (x, z) = Angle.Direction(m.FacingYaw - halfAngle + span * (double)i / segments);
            points.Add(m.Position + new Vector3((float)x, 0, (float)z) * radius);
        }
        return points;
    }

    public override void Build(MapContext c)
    {
        if (c.Frame.Mario is not { } m)
            return;
        var arc = Arc(m, c.Settings.Get(Radius), c.Settings.Get(HalfAngle));
        var polygon = new Vector2[arc.Count + 1];
        polygon[0] = new Vector2(m.Position.X, m.Position.Z);
        for (int i = 0; i < arc.Count; i++)
            polygon[i + 1] = new Vector2(arc[i].X, arc[i].Z);
        c.Scene.Add(new MapPolygon(polygon, SectorColor.WithAlpha(0.2), SectorColor, 1.5f));
    }

    public void Build3D(Map3DContext c)
    {
        if (c.Frame.Mario is not { } m)
            return;
        var arc = Arc(m, c.Settings.Get(Radius), c.Settings.Get(HalfAngle));
        for (int i = 0; i + 1 < arc.Count; i++)
            c.Scene.Add(new Triangle3D(m.Position, arc[i], arc[i + 1], SectorColor.WithAlpha(0.2), Edges: false));
        c.Scene.Add(new Line3D(arc.ToArray(), SectorColor));
        if (2 * Math.Abs(c.Settings.Get(HalfAngle)) < 65536)
            c.Scene.Add(new Line3D([arc[^1], m.Position, arc[0]], SectorColor));
    }
}

public sealed class NextPositionsLayer : MapLayer, IMapLayer3D
{
    public override string Id => "next";
    public override string Name => "Next positions";

    public readonly Setting<int> Frames;
    public readonly Setting<bool> QuarterSteps;
    public readonly Setting<bool> MarioModels;
    private static readonly Argb8Color PathColor = Argb8Color.Rgb(255, 235, 60);

    public NextPositionsLayer()
    {
        Frames = Declare(new Setting<int>("frames", "Frames ahead", 3) { Range = (1, 30, 1) });
        QuarterSteps = Declare(new Setting<bool>("quarter", "Quarter steps", false));
        MarioModels = Declare(new Setting<bool>("models", "Draw as Mario models (3D)", true));
    }

    public override void Build(MapContext c)
    {
        if (c.Frame.Mario is not { } m)
            return;
        var predicted = MarioPrediction.Predict(c.Frame, c.Settings.Get(Frames), c.Settings.Get(QuarterSteps));
        if (predicted.Count == 0)
            return;
        var points = new Vector2[predicted.Count + 1];
        points[0] = new Vector2(m.Position.X, m.Position.Z);
        for (int i = 0; i < predicted.Count; i++)
            points[i + 1] = new Vector2(predicted[i].Position.X, predicted[i].Position.Z);
        c.Scene.Add(new MapPolyline(points, PathColor, 2));
        foreach (var p in predicted)
            c.Scene.Add(new MapMarker(new Vector2(p.Position.X, p.Position.Z), p.FullFrame ? 7 : 4, MarkerShape.Circle, PathColor, MapPalette.Black, m.FacingYaw));
    }

    public void Build3D(Map3DContext c)
    {
        if (c.Frame.Mario is not { } m)
            return;
        var predicted = MarioPrediction.Predict(c.Frame, c.Settings.Get(Frames), c.Settings.Get(QuarterSteps));
        if (predicted.Count == 0)
            return;
        var line = new Vector3[predicted.Count + 1];
        line[0] = m.Position + new Vector3(0, 2, 0);
        for (int i = 0; i < predicted.Count; i++)
            line[i + 1] = predicted[i].Position + new Vector3(0, 2, 0);
        c.Scene.Add(new Line3D(line, PathColor));

        bool models = c.Settings.Get(MarioModels) && c.Scene.GameVisuals is not null && c.Scene.GameVisualsOptions.Objects;
        foreach (var p in predicted)
        {
            if (models && p.FullFrame)
                c.Scene.MarioModelCopies.Add(p.Position);
            else
                c.Scene.Add(new Point3D(p.Position + new Vector3(0, 2, 0), p.FullFrame ? 8 : 5, PathColor));
        }
    }
}
