using System.Numerics;
using SkiaSharp;
using StroopNext.Graphics;
using StroopNext.Map;
using StroopNext.Map3D;

namespace StroopNext.Rendering.Skia;

internal sealed class SkiaGameRenderer : IDisposable
{
    private static readonly Vector3 Light = Vector3.Normalize(new Vector3(0.35f, 1f, 0.25f));

    private static readonly Matrix4x4 TopDownBillboard = Matrix4x4.CreateRotationX(-MathF.PI / 2);

    private readonly Dictionary<DecodedTexture, SKShader> _shaders = new(ReferenceEqualityComparer.Instance);
    private readonly SKPaint _paint = new() { IsAntialias = false, Color = SKColors.White };
    private readonly SKPaint _shadowPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly List<Run> _levelRuns = new();
    private GameScene? _levelScene;
    private int _levelVersion = -1;
    private float _levelMaxY = float.NaN;

    private readonly List<Tri> _tris = new();
    private readonly List<(float Y, GfxInstance Instance, Matrix4x4 Matrix)> _instances = new();

    private readonly record struct Run(SKVertices Vertices, SKShader? Shader);

    private struct Tri
    {
        public float Order;
        public int Texture;
        public SKPoint P0, P1, P2, T0, T1, T2;
        public SKColor C0, C1, C2;
    }

    public void DrawLevel(SKCanvas canvas, GameScene scene, float maxY, MapView view)
    {
        if (!ReferenceEquals(scene, _levelScene) || scene.LevelVersion != _levelVersion || !maxY.Equals(_levelMaxY))
            PrepareLevel(scene, maxY);
        canvas.Save();
        canvas.Concat(WorldMatrix(view));
        foreach (var run in _levelRuns)
            Draw(canvas, run.Vertices, run.Shader);
        canvas.Restore();
    }

    public void DrawLive(SKCanvas canvas, GameScene scene, GameVisualsOptions options, bool shadows, MapView view)
    {
        if (scene.LevelVersion != _levelVersion || !ReferenceEquals(scene, _levelScene))
            ResetTextures(scene);
        canvas.Save();
        canvas.Concat(WorldMatrix(view));
        if (shadows)
            DrawShadows(canvas, scene, options.MaxY);
        if (options.Objects)
            DrawObjects(canvas, scene, options, view);
        canvas.Restore();
    }

    private static SKMatrix WorldMatrix(MapView view)
    {
        var o = view.WorldToScreen(view.Center);
        var ex = view.WorldToScreen(view.Center + Vector2.UnitX) - o;
        var ez = view.WorldToScreen(view.Center + Vector2.UnitY) - o;
        var local = new SKMatrix(ex.X, ez.X, o.X, ex.Y, ez.Y, o.Y, 0, 0, 1);
        return local.PreConcat(SKMatrix.CreateTranslation(-view.Center.X, -view.Center.Y));
    }

    private void PrepareLevel(GameScene scene, float maxY)
    {
        if (!ReferenceEquals(scene, _levelScene) || scene.LevelVersion != _levelVersion)
            ResetTextures(scene);
        _levelMaxY = maxY;
        DisposeRuns(_levelRuns);
        _tris.Clear();
        AddMesh(scene, scene.Level, Matrix4x4.Identity, maxY, _tris);
        _tris.Sort((a, b) => a.Order.CompareTo(b.Order));
        BuildRuns(scene, _tris, _levelRuns);
        _tris.Clear();
    }

    private void ResetTextures(GameScene scene)
    {
        foreach (var shader in _shaders.Values)
            shader.Dispose();
        _shaders.Clear();
        DisposeRuns(_levelRuns);
        _levelScene = scene;
        _levelVersion = scene.LevelVersion;
        _levelMaxY = float.NaN;
    }

    private void DrawObjects(SKCanvas canvas, GameScene scene, GameVisualsOptions options, MapView view)
    {
        var mario = scene.MarioModelPosition;
        float marioScale = options.KeepMarioVisible ? Math.Max(1f, 30f / (150f * (float)view.Scale)) : 1f;
        var aroundMario = Matrix4x4.CreateTranslation(-mario) * Matrix4x4.CreateScale(marioScale) * Matrix4x4.CreateTranslation(mario);

        _instances.Clear();
        float slack = 100 + 1500 * (float)view.Scale;
        foreach (var instance in scene.Instances)
        {
            if (instance.Level)
                continue;
            var origin = instance.Billboard ? instance.Anchor : instance.Matrix.Translation;
            var onScreen = view.WorldToScreen(new Vector2(origin.X, origin.Z));
            if (onScreen.X < -slack || onScreen.Y < -slack || onScreen.X > view.Width + slack || onScreen.Y > view.Height + slack)
                continue;
            var matrix = instance.Billboard
                ? instance.Matrix * Matrix4x4.CreateScale(instance.AnchorScale) * TopDownBillboard * Matrix4x4.CreateTranslation(instance.Anchor)
                : instance.Matrix;
            if (instance.IsMario)
                matrix *= aroundMario;
            _instances.Add((matrix.M42, instance, matrix));
        }
        _instances.Sort((a, b) => (a.Instance.IsMario, a.Y).CompareTo((b.Instance.IsMario, b.Y)));

        var runs = new List<Run>();
        foreach (var (_, instance, matrix) in _instances)
        {
            _tris.Clear();
            AddMesh(scene, instance.Mesh, matrix, options.MaxY, _tris);
            if (_tris.Count == 0)
                continue;
            _tris.Sort((a, b) => a.Order.CompareTo(b.Order));
            BuildRuns(scene, _tris, runs);
            foreach (var run in runs)
                Draw(canvas, run.Vertices, run.Shader);
            DisposeRuns(runs);
        }
        _tris.Clear();
    }

    private void DrawShadows(SKCanvas canvas, GameScene scene, float maxY)
    {
        foreach (var s in scene.Shadows)
        {
            if (s.Position.Y > maxY)
                continue;
            _shadowPaint.Color = new SKColor(0, 0, 0, (byte)Math.Clamp(s.Opacity * 160, 0, 255));
            float r = s.Size / 2;
            if (s.Square)
                canvas.DrawRect(s.Position.X - r, s.Position.Z - r, 2 * r, 2 * r, _shadowPaint);
            else
                canvas.DrawCircle(s.Position.X, s.Position.Z, r, _shadowPaint);
        }
    }

    private void Draw(SKCanvas canvas, SKVertices vertices, SKShader? shader)
    {
        _paint.Shader = shader;
        canvas.DrawVertices(vertices, SKBlendMode.Modulate, _paint);
        _paint.Shader = null;
    }

    private static void AddMesh(GameScene scene, GfxMesh mesh, in Matrix4x4 matrix, float maxY, List<Tri> output)
    {
        var vertices = mesh.Vertices;
        bool mirrored = matrix.GetDeterminant() < 0;
        bool identity = matrix.IsIdentity;
        foreach (var batch in mesh.Batches)
        {
            var material = batch.Material;
            var texture = material.Texture >= 0 && material.TexGen == 0 ? scene.Textures.Find(material.Texture) : null;
            float w = texture?.Width ?? 1, h = texture?.Height ?? 1;
            float bias = (material.Phase == 2 ? 0.2f : 0) + (material.Decal ? 0.1f : 0);
            int end = Math.Min(vertices.Length, batch.First + batch.Count);
            for (int i = batch.First; i + 2 < end; i += 3)
            {
                ref readonly var a = ref vertices[i];
                ref readonly var b = ref vertices[i + 1];
                ref readonly var c = ref vertices[i + 2];
                Vector3 pa = identity ? a.Position : Vector3.Transform(a.Position, matrix);
                Vector3 pb = identity ? b.Position : Vector3.Transform(b.Position, matrix);
                Vector3 pc = identity ? c.Position : Vector3.Transform(c.Position, matrix);
                if (Math.Min(pa.Y, Math.Min(pb.Y, pc.Y)) > maxY)
                    continue;
                float up = (pb.Z - pa.Z) * (pc.X - pa.X) - (pb.X - pa.X) * (pc.Z - pa.Z);
                if (mirrored)
                    up = -up;
                if (MathF.Abs(up) < 1e-3f || !float.IsFinite(up))
                    continue;
                if ((up < 0 && material.Cull == CullMode.Back) || (up > 0 && material.Cull == CullMode.Front))
                    continue;
                output.Add(new Tri
                {
                    Order = (pa.Y + pb.Y + pc.Y) / 3 + bias,
                    Texture = texture is null ? -1 : material.Texture,
                    P0 = new SKPoint(pa.X, pa.Z), P1 = new SKPoint(pb.X, pb.Z), P2 = new SKPoint(pc.X, pc.Z),
                    T0 = new SKPoint(a.Uv.X * w, a.Uv.Y * h), T1 = new SKPoint(b.Uv.X * w, b.Uv.Y * h), T2 = new SKPoint(c.Uv.X * w, c.Uv.Y * h),
                    C0 = Shade(a, matrix, identity), C1 = Shade(b, matrix, identity), C2 = Shade(c, matrix, identity),
                });
            }
        }
    }

    private static SKColor Shade(in GfxVertex v, in Matrix4x4 matrix, bool identity)
    {
        uint c = v.Color;
        float r = c & 0xFF, g = (c >> 8) & 0xFF, b = (c >> 16) & 0xFF;
        byte alpha = (byte)(c >> 24);
        if ((v.Ambient >> 24) > 127)
        {
            var n = new Vector3((sbyte)(v.Normal & 0xFF), (sbyte)((v.Normal >> 8) & 0xFF), (sbyte)((v.Normal >> 16) & 0xFF));
            if (!identity)
                n = Vector3.TransformNormal(n, matrix);
            float length = n.Length();
            float k = length > 0 ? Math.Max(0, Vector3.Dot(n / length, Light)) : 0;
            r = Math.Min(255, (v.Ambient & 0xFF) + r * k);
            g = Math.Min(255, ((v.Ambient >> 8) & 0xFF) + g * k);
            b = Math.Min(255, ((v.Ambient >> 16) & 0xFF) + b * k);
        }
        return new SKColor((byte)r, (byte)g, (byte)b, alpha);
    }

    private void BuildRuns(GameScene scene, List<Tri> tris, List<Run> output)
    {
        int start = 0;
        while (start < tris.Count)
        {
            int texture = tris[start].Texture, end = start + 1;
            while (end < tris.Count && tris[end].Texture == texture)
                end++;
            int count = end - start;
            var positions = new SKPoint[count * 3];
            var colors = new SKColor[count * 3];
            var texs = texture >= 0 ? new SKPoint[count * 3] : null;
            for (int i = 0; i < count; i++)
            {
                var t = tris[start + i];
                positions[3 * i] = t.P0; positions[3 * i + 1] = t.P1; positions[3 * i + 2] = t.P2;
                colors[3 * i] = t.C0; colors[3 * i + 1] = t.C1; colors[3 * i + 2] = t.C2;
                if (texs is not null)
                {
                    texs[3 * i] = t.T0; texs[3 * i + 1] = t.T1; texs[3 * i + 2] = t.T2;
                }
            }
            output.Add(new Run(SKVertices.CreateCopy(SKVertexMode.Triangles, positions, texs, colors), Shader(scene, texture)));
            start = end;
        }
    }

    private SKShader? Shader(GameScene scene, int id)
    {
        if (scene.Textures.Find(id) is not { } texture)
            return null;
        if (_shaders.TryGetValue(texture, out var shader))
            return shader;
        var info = new SKImageInfo(texture.Width, texture.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        using var image = SKImage.FromPixelCopy(info, texture.Rgba);
        if (image is null)
            return null;
        shader = image.ToShader(Mode(texture.WrapS), Mode(texture.WrapT), new SKSamplingOptions(SKFilterMode.Linear));
        _shaders[texture] = shader;
        return shader;
    }

    private static SKShaderTileMode Mode(TextureWrap wrap) => wrap switch
    {
        TextureWrap.Clamp => SKShaderTileMode.Clamp,
        TextureWrap.MirroredRepeat => SKShaderTileMode.Mirror,
        _ => SKShaderTileMode.Repeat,
    };

    private static void DisposeRuns(List<Run> runs)
    {
        foreach (var run in runs)
            run.Vertices.Dispose();
        runs.Clear();
    }

    public void Dispose()
    {
        DisposeRuns(_levelRuns);
        foreach (var shader in _shaders.Values)
            shader.Dispose();
        _shaders.Clear();
        _paint.Dispose();
        _shadowPaint.Dispose();
    }
}
