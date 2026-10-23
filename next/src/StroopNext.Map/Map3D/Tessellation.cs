using StroopNext.Graphics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace StroopNext.Map3D;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct MeshVertex(Vector3 position, Vector3 normal, uint color, Vector2 bary)
{
    public const int Size = 36;
    public Vector3 Position = position;
    public Vector3 Normal = normal;
    public uint Color = color;
    public Vector2 Bary = bary;
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct LineVertex(Vector3 position, uint color)
{
    public const int Size = 16;
    public Vector3 Position = position;
    public uint Color = color;
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct PointVertex(Vector3 position, uint color, float size)
{
    public const int Size = 20;
    public Vector3 Position = position;
    public uint Color = color;
    public float SizePx = size;
}

public sealed class MeshBuffers
{
    private static readonly Vector2 NoEdges = new(1f / 3, 1f / 3);
    private static readonly Vector2[] Corners = [new(1, 0), new(0, 1), new(0, 0)];

    public VertexList<MeshVertex> Opaque { get; } = new();
    public VertexList<MeshVertex> Transparent { get; } = new();
    public VertexList<LineVertex> Lines { get; } = new();
    public VertexList<PointVertex> Points { get; } = new();

    public void Clear()
    {
        Opaque.Clear();
        Transparent.Clear();
        Lines.Clear();
        Points.Clear();
    }

    public static uint Pack(Argb8Color  c) => c.R | ((uint)c.G << 8) | ((uint)c.B << 16) | ((uint)c.A << 24);

    public void AddAll(IEnumerable<Primitive3D> primitives)
    {
        foreach (var p in primitives)
            Add(p);
    }

    public void Add(Primitive3D primitive)
    {
        switch (primitive)
        {
            case Triangle3D t:
                Triangle(t.A, t.B, t.C, t.Color, t.Edges);
                break;
            case Cylinder3D c:
                Cylinder(c);
                break;
            case Line3D l:
                uint lc = Pack(l.Color);
                for (int i = 0; i + 1 < l.Points.Length; i++)
                {
                    Lines.Add(new LineVertex(l.Points[i], lc));
                    Lines.Add(new LineVertex(l.Points[i + 1], lc));
                }
                break;
            case Arrow3D a:
                Arrow(a);
                break;
            case Point3D p:
                Points.Add(new PointVertex(p.Position, Pack(p.Color), p.SizePx));
                break;
        }
    }

    private VertexList<MeshVertex> PassFor(Argb8Color  c) => c.A < 255 ? Transparent : Opaque;

    private void Triangle(Vector3 a, Vector3 b, Vector3 c, Argb8Color color, bool edges)
    {
        var normal = Vector3.Cross(b - a, c - a);
        normal = normal.LengthSquared() > 0 ? Vector3.Normalize(normal) : Vector3.UnitY;
        var pass = PassFor(color);
        uint packed = Pack(color);
        pass.Add(new MeshVertex(a, normal, packed, edges ? Corners[0] : NoEdges));
        pass.Add(new MeshVertex(b, normal, packed, edges ? Corners[1] : NoEdges));
        pass.Add(new MeshVertex(c, normal, packed, edges ? Corners[2] : NoEdges));
    }

    private void Cylinder(Cylinder3D c)
    {
        var pass = PassFor(c.Color);
        uint color = Pack(c.Color);
        uint ring = Pack(c.Color.Lerp(new Argb8Color(255, 0, 0, 0), 0.35) with { A = 255 });
        int n = Math.Max(6, c.Segments);
        var top = c.Base + new Vector3(0, c.Height, 0);
        Vector3 Rim(int i, Vector3 center)
        {
            double a = i * Math.PI * 2 / n;
            return center + new Vector3((float)Math.Sin(a) * c.Radius, 0, (float)Math.Cos(a) * c.Radius);
        }
        for (int i = 0; i < n; i++)
        {
            Vector3 b0 = Rim(i, c.Base), b1 = Rim(i + 1, c.Base), t0 = Rim(i, top), t1 = Rim(i + 1, top);
            var n0 = Vector3.Normalize(new Vector3(b0.X - c.Base.X, 0, b0.Z - c.Base.Z));
            var n1 = Vector3.Normalize(new Vector3(b1.X - c.Base.X, 0, b1.Z - c.Base.Z));
            pass.Add(new MeshVertex(b0, n0, color, NoEdges));
            pass.Add(new MeshVertex(b1, n1, color, NoEdges));
            pass.Add(new MeshVertex(t1, n1, color, NoEdges));
            pass.Add(new MeshVertex(b0, n0, color, NoEdges));
            pass.Add(new MeshVertex(t1, n1, color, NoEdges));
            pass.Add(new MeshVertex(t0, n0, color, NoEdges));
            pass.Add(new MeshVertex(top, Vector3.UnitY, color, NoEdges));
            pass.Add(new MeshVertex(t0, Vector3.UnitY, color, NoEdges));
            pass.Add(new MeshVertex(t1, Vector3.UnitY, color, NoEdges));
            pass.Add(new MeshVertex(c.Base, -Vector3.UnitY, color, NoEdges));
            pass.Add(new MeshVertex(b1, -Vector3.UnitY, color, NoEdges));
            pass.Add(new MeshVertex(b0, -Vector3.UnitY, color, NoEdges));
            Lines.Add(new LineVertex(b0, ring));
            Lines.Add(new LineVertex(b1, ring));
            Lines.Add(new LineVertex(t0, ring));
            Lines.Add(new LineVertex(t1, ring));
        }
    }

    private void Arrow(Arrow3D a)
    {
        var dir = new Vector3((float)Math.Sin(Game.Angle.ToRadians(a.Yaw)), 0, (float)Math.Cos(Game.Angle.ToRadians(a.Yaw)));
        var side = new Vector3(dir.Z, 0, -dir.X);
        float head = Math.Min(a.Length * 0.35f, a.Width * 3);
        var shaftEnd = a.Origin + dir * (a.Length - head);
        var tip = a.Origin + dir * a.Length;
        var w = side * (a.Width / 2);
        var pass = PassFor(a.Color);
        uint color = Pack(a.Color);
        void Tri(Vector3 p, Vector3 q, Vector3 r)
        {
            pass.Add(new MeshVertex(p, Vector3.UnitY, color, NoEdges));
            pass.Add(new MeshVertex(q, Vector3.UnitY, color, NoEdges));
            pass.Add(new MeshVertex(r, Vector3.UnitY, color, NoEdges));
        }
        Tri(a.Origin - w, a.Origin + w, shaftEnd + w);
        Tri(a.Origin - w, shaftEnd + w, shaftEnd - w);
        Tri(shaftEnd - w * 2.5f, shaftEnd + w * 2.5f, tip);
    }
}
