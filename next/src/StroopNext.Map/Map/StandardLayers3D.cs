using System.Numerics;
using StroopNext.Game;
using StroopNext.Map3D;
using StroopNext.Settings;

namespace StroopNext.Map;


public sealed partial class GridLayer : IMapLayer3D
{
    public void Build3D(Map3DContext c)
    {
        float y = c.Frame.Mario?.FloorY ?? c.Camera.Target.Y;
        double extent = Math.Clamp(Vector3.Distance(c.Camera.Position, c.Camera.Target) * 2, 1000, 16384);
        double spacing = c.Settings.Get(Spacing);
        if (spacing <= 0)
        {
            spacing = 1;
            while (extent / spacing > 40)
                spacing *= 2;
        }
        if (extent / spacing > 400)
            return;
        var color = MapPalette.Grid.WithAlpha(0.35);
        double cx = Math.Round(c.Camera.Target.X / spacing) * spacing, cz = Math.Round(c.Camera.Target.Z / spacing) * spacing;
        for (double d = -extent; d <= extent; d += spacing)
        {
            c.Scene.Add(new Line3D([new((float)(cx + d), y, (float)(cz - extent)), new((float)(cx + d), y, (float)(cz + extent))], color));
            c.Scene.Add(new Line3D([new((float)(cx - extent), y, (float)(cz + d)), new((float)(cx + extent), y, (float)(cz + d))], color));
        }
    }
}

public abstract partial class HorizontalSurfaceLayer : IMapLayer3D
{
    public void Build3D(Map3DContext c)
    {
        var mario = c.Frame.Mario;
        int range = c.Settings.Get(HeightRange);
        bool objects = c.Settings.Get(IncludeObjects);
        bool filtered = range > 0 && mario is not null;
        var surfaces = c.Frame.Surfaces;

        bool Keep(Surface s) => s.Class == Class && !(filtered && (s.MaxY < mario!.Position.Y - range || s.MinY > mario.Position.Y + range));
        bool overlay = c.Scene.GameVisuals is not null && c.Scene.GameVisualsOptions.Level;
        Argb8Color Color(Surface s) => overlay ? ColorFor(s, null).WithAlpha(0.12) : ColorFor(s, null);

        if (c.Scene.BeginBackground(this, (surfaces.StaticVersion, range, filtered ? mario!.Position.Y : (float?)null, overlay)))
            foreach (var s in surfaces.Static)
                if (Keep(s))
                    c.Scene.AddBackground(new Triangle3D(s.V1, s.V2, s.V3, Color(s)) { Hit = SurfaceHit.Of(s) });
        if (objects)
            foreach (var s in surfaces.Dynamic)
                if (Keep(s))
                    c.Scene.Add(new Triangle3D(s.V1, s.V2, s.V3, Color(s)) { Hit = SurfaceHit.Of(s) });

        if (mario is not null && surfaces.Find(CurrentTriangle(mario)) is { } h && h.Class == Class)
        {
            var lift = h.Normal * 2;
            c.Scene.Add(new Triangle3D(h.V1 + lift, h.V2 + lift, h.V3 + lift, MapPalette.Highlight.WithAlpha(c.Scene.GameVisuals is null ? 0.45 : 0.22)) { Hit = SurfaceHit.Of(h) });
            c.Scene.Add(new Line3D([h.V1 + lift, h.V2 + lift, h.V3 + lift, h.V1 + lift], MapPalette.Highlight));
        }
    }
}

public sealed partial class WallsLayer : IMapLayer3D
{
    public void Build3D(Map3DContext c)
    {
        var mario = c.Frame.Mario;
        bool band = c.Settings.Get(ShowHitbox);
        float radius = (float)c.Settings.Get(HitboxRadius);
        double opacity = c.Settings.Get(Opacity);
        bool heightOnly = c.Settings.Get(AtMarioHeightOnly) && mario is not null;
        bool objects = c.Settings.Get(IncludeObjects);
        var surfaces = c.Frame.Surfaces;

        bool overlay = c.Scene.GameVisuals is not null && c.Scene.GameVisualsOptions.Level;
        bool rebuildLevel = c.Scene.BeginBackground(this, (surfaces.StaticVersion, band, radius, opacity, heightOnly ? mario!.Position.Y : (float?)null, overlay));
        for (int pass = 0; pass < (objects ? 2 : 1); pass++)
        {
            bool level = pass == 0;
            if (level && !rebuildLevel)
                continue;
            var list = level ? surfaces.Static : surfaces.Dynamic;
            Action<Primitive3D> add = level ? c.Scene.AddBackground : c.Scene.Add;
            foreach (var s in list)
            {
                if (s.Class != SurfaceClass.Wall)
                    continue;
                var color = s.IsStatic ? MapPalette.Wall : MapPalette.ObjectWall;
                var hit = SurfaceHit.Of(s);
                add(new Triangle3D(s.V1, s.V2, s.V3, overlay ? color.WithAlpha(0.15) : color) { Hit = hit });

                bool atHeight = !heightOnly || !(s.MinY - 5 > mario!.Position.Y + 160 || s.MaxY + 5 < mario.Position.Y);
                if (band && atHeight && Prism(s, radius) is { } prism)
                    foreach (var (a, b, d) in prism)
                        add(new Triangle3D(a, b, d, color.WithAlpha(opacity * 0.6), Edges: false));
            }
        }

        if (mario is not null && surfaces.Find(mario.WallTri) is { } w)
        {
            var lift = w.Normal * 2;
            c.Scene.Add(new Line3D([w.V1 + lift, w.V2 + lift, w.V3 + lift, w.V1 + lift], MapPalette.Highlight));
            c.Scene.Add(new Triangle3D(w.V1 + lift, w.V2 + lift, w.V3 + lift, MapPalette.Highlight.WithAlpha(c.Scene.GameVisuals is null ? 0.45 : 0.22)) { Hit = SurfaceHit.Of(w) });
        }
    }

    public static (Vector3, Vector3, Vector3)[]? Prism(Surface s, float radius)
    {
        float axisNormal = s.ProjectsOnX ? s.Normal.X : s.Normal.Z;
        if (MathF.Abs(axisNormal) < 1e-4f)
            return null;
        float d = radius / MathF.Abs(axisNormal);
        var o = s.ProjectsOnX ? new Vector3(d, 0, 0) : new Vector3(0, 0, d);
        Vector3 a1 = s.V1 + o, b1 = s.V2 + o, c1 = s.V3 + o, a2 = s.V1 - o, b2 = s.V2 - o, c2 = s.V3 - o;
        return
        [
            (a1, b1, c1), (a2, b2, c2),
            (a1, b1, b2), (a1, b2, a2),
            (b1, c1, c2), (b1, c2, b2),
            (c1, a1, a2), (c1, a2, c2),
        ];
    }
}

public sealed partial class ObjectsLayer : IMapLayer3D
{
    public void Build3D(Map3DContext c)
    {
        uint marioObject = c.Frame.Mario?.MarioObject ?? 0;
        bool hitboxes = c.Settings.Get(ShowHitboxes), hurtboxes = c.Settings.Get(ShowHurtboxes), facing = c.Settings.Get(ShowFacing);
        foreach (var o in c.Frame.Objects.All)
        {
            if (o.Address == marioObject)
                continue;
            bool selected = c.Selection.IsSelected(o.Address);
            var hit = new ObjectHit(o);
            var bottom = o.Position - new Vector3(0, o.HitboxDownOffset, 0);
            var color = selected ? MapPalette.Highlight : MapPalette.Object;

            if (hitboxes && o.HitboxRadius > 0 && o.HitboxHeight > 0)
                c.Scene.Add(new Cylinder3D(bottom, o.HitboxRadius, o.HitboxHeight, color.WithAlpha(selected ? 0.5 : 0.3)) { Hit = hit });
            if (hurtboxes && o.HurtboxRadius > 0 && o.HurtboxHeight > 0)
                c.Scene.Add(new Cylinder3D(bottom, o.HurtboxRadius, o.HurtboxHeight, MapPalette.Hurtbox.WithAlpha(0.25)) { Hit = hit });
            if (facing)
                c.Scene.Add(new Arrow3D(o.Position + new Vector3(0, 3, 0), o.FacingYaw, 70, 10, color));
            c.Scene.Add(new Point3D(o.Position, selected ? 10 : 7, color) { Hit = hit });
        }
    }
}

public sealed partial class MarioLayer : IMapLayer3D
{
    public void Build3D(Map3DContext c)
    {
        if (c.Frame.Mario is not { } m)
            return;
        var hit = new MarioHit(m, c.Frame.Services.Actions.NameOf(m.Action));
        var feet = m.Position + new Vector3(0, 3, 0);

        if (c.Settings.Get(ShowHitbox))
            c.Scene.Add(new Cylinder3D(m.Position, MarioState.HitboxRadius, MarioState.HitboxHeight, MapPalette.Mario.WithAlpha(0.45)) { Hit = hit });
        if (c.Settings.Get(ShowWallRadius))
            c.Scene.Add(new Cylinder3D(m.Position + new Vector3(0, 30, 0), MarioState.WallHitboxRadius, 30, MapPalette.Mario.WithAlpha(0.2)));
        if (c.Settings.Get(ShowMoving))
            c.Scene.Add(new Arrow3D(feet, m.SlideYaw, 110, 12, MapPalette.Moving));
        if (c.Settings.Get(ShowSliding) && m.SlidingSpeed > 0)
            c.Scene.Add(new Arrow3D(feet, m.SlidingYaw, 110, 12, MapPalette.Sliding));
        if (c.Settings.Get(ShowIntended) && m.IntendedMag > 0)
            c.Scene.Add(new Arrow3D(feet + new Vector3(0, 1, 0), m.IntendedYaw, Math.Max(30, 140 * m.IntendedMag / 32f), 12, MapPalette.Intended));
        if (c.Settings.Get(ShowFacing))
            c.Scene.Add(new Arrow3D(feet + new Vector3(0, 2, 0), m.FacingYaw, 140, 16, MapPalette.Mario));
        if (c.Settings.Get(ShowVelocity3D) && m.Velocity.LengthSquared() > 1e-4f)
            LineArrow(c, m.Position + new Vector3(0, 80, 0), m.Velocity, m.Velocity.Length() * 4, MapPalette.Sliding);
        c.Scene.Add(new Point3D(m.Position + new Vector3(0, MarioState.HitboxHeight, 0), 9, MapPalette.Mario) { Hit = hit });
    }
}

public sealed partial class MarioLayer
{
    public readonly Setting<bool> ShowVelocity3D = new("velocity3d", "Velocity vector, with Y speed (3D)", true);

    internal static void LineArrow(Map3DContext c, Vector3 origin, Vector3 direction, float length, Argb8Color color)
    {
        if (length <= 0 || direction.LengthSquared() < 1e-8f)
            return;
        var d = Vector3.Normalize(direction);
        var tip = origin + d * length;
        var side = Vector3.Cross(d, c.Camera.Position - tip);
        if (side.LengthSquared() < 1e-6f)
            side = Vector3.Cross(d, MathF.Abs(d.Y) < 0.9f ? Vector3.UnitY : Vector3.UnitX);
        side = Vector3.Normalize(side);
        float head = MathF.Min(40, length * 0.35f) * MathF.Sqrt(0.5f);
        c.Scene.Add(new Line3D([origin, tip], color));
        c.Scene.Add(new Line3D([tip - d * head + side * head, tip, tip - d * head - side * head], color));
    }
}

public sealed partial class PathLayer : IMapLayer3D
{
    public void Build3D(Map3DContext c)
    {
        var samples = _track.Snapshot();
        int count = Math.Min(samples.Length, Math.Max(2, c.Settings.Get(Frames)));
        if (count < 2)
            return;
        var recent = samples.AsSpan(samples.Length - count);
        var points = new Vector3[count];
        for (int i = 0; i < count; i++)
            points[i] = recent[i].Value + new Vector3(0, 1, 0);
        c.Scene.Add(new Line3D(points, MapPalette.Path));
        if (!c.Settings.Get(ShowPoints))
            return;
        for (int i = 0; i < count; i++)
        {
            float? moved = i == 0 ? null : Vector2.Distance(new(points[i].X, points[i].Z), new(points[i - 1].X, points[i - 1].Z));
            c.Scene.Add(new Point3D(points[i], 5, MapPalette.Path) { Hit = new PathSampleHit(recent[i].GlobalTimer, recent[i].Value, moved) });
        }
    }
}

public sealed partial class CameraLayer : IMapLayer3D
{
    public void Build3D(Map3DContext c)
    {
        if (c.Frame.Camera is not { } cam)
            return;
        c.Scene.Add(new Line3D([cam.Position, cam.Focus], MapPalette.Camera.WithAlpha(0.7)));
        c.Scene.Add(new Point3D(cam.Position, 11, MapPalette.Camera) { Hit = new CameraHit(cam) });

        var forward = cam.Focus - cam.Position;
        if (forward.LengthSquared() < 1e-6f)
            return;
        forward = Vector3.Normalize(forward);
        float depth = Math.Clamp(Vector3.Distance(c.Camera.Position, cam.Position) * 0.12f, 60, 1500);
        float halfH = depth * MathF.Tan(22.5f * MathF.PI / 180), halfW = halfH * 4 / 3;
        var right = Vector3.Cross(forward, Vector3.UnitY);
        right = right.LengthSquared() < 1e-6f ? Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitZ)) : Vector3.Normalize(right);
        var up = Vector3.Cross(right, forward);
        var center = cam.Position + forward * depth;
        Vector3 tl = center - right * halfW + up * halfH, tr = center + right * halfW + up * halfH, br = center + right * halfW - up * halfH, bl = center - right * halfW - up * halfH;
        var color = MapPalette.White.WithAlpha(0.85);
        c.Scene.Add(new Line3D([tl, tr, br, bl, tl], color));
        foreach (var corner in new[] { tl, tr, br, bl })
            c.Scene.Add(new Line3D([cam.Position, corner], color));
    }
}

public sealed partial class HolpLayer : IMapLayer3D
{
    public void Build3D(Map3DContext c)
    {
        if (c.Frame.Mario is not { } m || Variables.MarioVariables.HolpPosition(c.Frame) is not { } h)
            return;
        c.Scene.Add(new Line3D([m.Position + new Vector3(0, 60, 0), h], HolpColor.WithAlpha(0.6)));
        c.Scene.Add(new Point3D(h, 10, HolpColor) { Hit = new PointHit("HOLP", h) });
    }
}
