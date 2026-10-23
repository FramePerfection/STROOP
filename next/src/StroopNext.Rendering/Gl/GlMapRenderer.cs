using System.Numerics;
using OpenTK.Graphics.OpenGL4;
using StroopNext.Map3D;

namespace StroopNext.Rendering.Gl;

internal sealed class GlMapRenderer : IDisposable
{
    public static readonly Vector3 ClearColor = new(0x1B / 255f, 0x1C / 255f, 0x24 / 255f);
    private static readonly Argb8Color HoverColor = new(110, 255, 255, 255);

    private readonly ShaderProgram _mesh;
    private readonly ShaderProgram _lines;
    private readonly ShaderProgram _points;

    private readonly MeshBuffers _backgroundBuffers = new();
    private readonly MeshBuffers _itemBuffers = new();
    private readonly MeshBuffers _hoverBuffers = new();
    private readonly GpuStreams _background;
    private readonly GpuStreams _items;
    private readonly GpuStreams _hover;
    private long _uploadedVersion = -1;

    public GlMapRenderer()
    {
        _mesh = new ShaderProgram(Shaders.MeshVertex, Shaders.MeshFragment);
        _lines = new ShaderProgram(Shaders.LineVertex, Shaders.ColorFragment);
        _points = new ShaderProgram(Shaders.PointVertex, Shaders.PointFragment);
        _background = new GpuStreams();
        _items = new GpuStreams();
        _hover = new GpuStreams();
        _game = new GameSceneRenderer();
    }

    private readonly GameSceneRenderer _game;

    public int GameDrawCalls => _game.LastDrawCalls;

    public bool XRay { get; set; } = true;

    public int BackgroundUploads { get; private set; }

    public void Render(Scene3D scene, Camera3D camera, int width, int height, Primitive3D? hovered)
    {
        if (scene.BackgroundVersion != _uploadedVersion)
        {
            _backgroundBuffers.Clear();
            _backgroundBuffers.AddAll(scene.Background);
            _background.Upload(_backgroundBuffers);
            _uploadedVersion = scene.BackgroundVersion;
            BackgroundUploads++;
        }
        _itemBuffers.Clear();
        _itemBuffers.AddAll(scene.Items);
        _items.Upload(_itemBuffers);
        _hoverBuffers.Clear();
        if (hovered is not null)
            _hoverBuffers.Add(Recolor(hovered));
        _hover.Upload(_hoverBuffers);

        GL.Viewport(0, 0, width, height);
        GL.ClearColor(ClearColor.X, ClearColor.Y, ClearColor.Z, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        if (scene.GameVisuals is { } game)
            _game.Render(game, scene.GameVisualsOptions, camera, width, height, scene.MarioModelCopies, scene.LevelCopies, scene.MarioModelGhosts);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);
        GL.Disable(EnableCap.CullFace);
        GL.Enable(EnableCap.ProgramPointSize);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        var viewProj = camera.ViewProjection;
        var light = Vector3.Normalize(new Vector3(0.35f, 1f, 0.55f));
        float focus = Vector3.Distance(camera.Position, camera.Target);
        var normal = new FrameUniforms(camera.Position, focus * 1.5f, focus * 6f, 1f);
        var ghost = normal with { AlphaScale = 0.28f };

        GL.Disable(EnableCap.Blend);
        GL.DepthMask(true);
        _mesh.Use(viewProj, light, normal);
        _background.Opaque.Draw(PrimitiveType.Triangles);
        _items.Opaque.Draw(PrimitiveType.Triangles);

        GL.Enable(EnableCap.Blend);
        _lines.Use(viewProj, light, normal);
        _background.Lines.Draw(PrimitiveType.Lines);
        _items.Lines.Draw(PrimitiveType.Lines);

        GL.DepthMask(false);
        _mesh.Use(viewProj, light, normal);
        _background.Transparent.Draw(PrimitiveType.Triangles);
        _items.Transparent.Draw(PrimitiveType.Triangles);

        _points.Use(viewProj, light, normal);
        _background.Points.Draw(PrimitiveType.Points);
        _items.Points.Draw(PrimitiveType.Points);

        if (XRay)
        {
            GL.DepthFunc(DepthFunction.Greater);
            _mesh.Use(viewProj, light, ghost);
            _items.Opaque.Draw(PrimitiveType.Triangles);
            _items.Transparent.Draw(PrimitiveType.Triangles);
            _points.Use(viewProj, light, ghost);
            _items.Points.Draw(PrimitiveType.Points);
            GL.DepthFunc(DepthFunction.Lequal);
        }

        GL.Disable(EnableCap.DepthTest);
        _mesh.Use(viewProj, light, normal);
        _hover.Opaque.Draw(PrimitiveType.Triangles);
        _hover.Transparent.Draw(PrimitiveType.Triangles);
        _lines.Use(viewProj, light, normal);
        _hover.Lines.Draw(PrimitiveType.Lines);
        _points.Use(viewProj, light, normal);
        _hover.Points.Draw(PrimitiveType.Points);
        GL.DepthMask(true);
    }

    private static Primitive3D Recolor(Primitive3D p) => p switch
    {
        Triangle3D t => t with { Color = HoverColor },
        Cylinder3D c => c with { Color = HoverColor },
        Point3D d => d with { Color = new Argb8Color(255, 255, 255, 255), SizePx = d.SizePx + 4 },
        _ => p,
    };

    public static byte[] ReadPixels(int width, int height)
    {
        var bottomUp = new byte[width * height * 4];
        GL.ReadPixels(0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, bottomUp);
        var topDown = new byte[bottomUp.Length];
        int stride = width * 4;
        for (int y = 0; y < height; y++)
            System.Buffer.BlockCopy(bottomUp, (height - 1 - y) * stride, topDown, y * stride, stride);
        return topDown;
    }

    public void Dispose()
    {
        _background.Dispose();
        _items.Dispose();
        _hover.Dispose();
        _mesh.Dispose();
        _lines.Dispose();
        _points.Dispose();
        _game.Dispose();
    }
}

internal sealed class GpuStreams : IDisposable
{
    public GpuStream Opaque { get; } = new(VertexLayout.Mesh);
    public GpuStream Transparent { get; } = new(VertexLayout.Mesh);
    public GpuStream Lines { get; } = new(VertexLayout.Line);
    public GpuStream Points { get; } = new(VertexLayout.Point);

    public void Upload(MeshBuffers buffers)
    {
        Opaque.Upload(buffers.Opaque.Span, MeshVertex.Size);
        Transparent.Upload(buffers.Transparent.Span, MeshVertex.Size);
        Lines.Upload(buffers.Lines.Span, LineVertex.Size);
        Points.Upload(buffers.Points.Span, PointVertex.Size);
    }

    public void Dispose()
    {
        Opaque.Dispose();
        Transparent.Dispose();
        Lines.Dispose();
        Points.Dispose();
    }
}

internal enum VertexLayout { Mesh, Line, Point }

internal sealed class GpuStream : IDisposable
{
    private readonly int _vao;
    private readonly int _vbo;
    private int _capacityBytes;

    public GpuStream(VertexLayout layout)
    {
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        switch (layout)
        {
            case VertexLayout.Mesh:
                Attribute(0, 3, VertexAttribPointerType.Float, false, MeshVertex.Size, 0);
                Attribute(1, 3, VertexAttribPointerType.Float, false, MeshVertex.Size, 12);
                Attribute(2, 4, VertexAttribPointerType.UnsignedByte, true, MeshVertex.Size, 24);
                Attribute(3, 2, VertexAttribPointerType.Float, false, MeshVertex.Size, 28);
                break;
            case VertexLayout.Line:
                Attribute(0, 3, VertexAttribPointerType.Float, false, LineVertex.Size, 0);
                Attribute(1, 4, VertexAttribPointerType.UnsignedByte, true, LineVertex.Size, 12);
                break;
            case VertexLayout.Point:
                Attribute(0, 3, VertexAttribPointerType.Float, false, PointVertex.Size, 0);
                Attribute(1, 4, VertexAttribPointerType.UnsignedByte, true, PointVertex.Size, 12);
                Attribute(2, 1, VertexAttribPointerType.Float, false, PointVertex.Size, 16);
                break;
        }
        GL.BindVertexArray(0);
    }

    public int Count { get; private set; }

    private static void Attribute(int index, int size, VertexAttribPointerType type, bool normalized, int stride, int offset)
    {
        GL.EnableVertexAttribArray(index);
        GL.VertexAttribPointer(index, size, type, normalized, stride, offset);
    }

    public unsafe void Upload<T>(ReadOnlySpan<T> vertices, int vertexSize) where T : unmanaged
    {
        Count = vertices.Length;
        if (Count == 0)
            return;
        int bytes = Count * vertexSize;
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        if (bytes > _capacityBytes)
        {
            _capacityBytes = Math.Max(bytes, _capacityBytes * 3 / 2);
            GL.BufferData(BufferTarget.ArrayBuffer, _capacityBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }
        fixed (T* data = vertices)
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, bytes, (IntPtr)data);
    }

    public void Draw(PrimitiveType type)
    {
        if (Count == 0)
            return;
        GL.BindVertexArray(_vao);
        GL.DrawArrays(type, 0, Count);
    }

    public void Dispose()
    {
        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_vao);
    }
}

internal readonly record struct FrameUniforms(Vector3 Eye, float FogNear, float FogFar, float AlphaScale);

internal sealed class ShaderProgram : IDisposable
{
    private readonly int _program;
    private readonly int _viewProj;
    private readonly int _light;
    private readonly int _eye, _fogNear, _fogFar, _fogColor, _alphaScale;

    public ShaderProgram(string vertexSource, string fragmentSource)
    {
        int vs = Compile(ShaderType.VertexShader, vertexSource);
        int fs = Compile(ShaderType.FragmentShader, fragmentSource);
        _program = GL.CreateProgram();
        GL.AttachShader(_program, vs);
        GL.AttachShader(_program, fs);
        GL.LinkProgram(_program);
        GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out int linked);
        if (linked == 0)
            throw new InvalidOperationException("Shader link failed: " + GL.GetProgramInfoLog(_program));
        GL.DeleteShader(vs);
        GL.DeleteShader(fs);
        _viewProj = GL.GetUniformLocation(_program, "uViewProj");
        _light = GL.GetUniformLocation(_program, "uLightDir");
        _eye = GL.GetUniformLocation(_program, "uEye");
        _fogNear = GL.GetUniformLocation(_program, "uFogNear");
        _fogFar = GL.GetUniformLocation(_program, "uFogFar");
        _fogColor = GL.GetUniformLocation(_program, "uFogColor");
        _alphaScale = GL.GetUniformLocation(_program, "uAlphaScale");
    }

    public unsafe void Use(Matrix4x4 viewProj, Vector3 light, FrameUniforms frame)
    {
        GL.UseProgram(_program);
        GL.UniformMatrix4(_viewProj, 1, false, &viewProj.M11);
        if (_light >= 0)
            GL.Uniform3(_light, light.X, light.Y, light.Z);
        if (_eye >= 0)
            GL.Uniform3(_eye, frame.Eye.X, frame.Eye.Y, frame.Eye.Z);
        if (_fogNear >= 0)
            GL.Uniform1(_fogNear, frame.FogNear);
        if (_fogFar >= 0)
            GL.Uniform1(_fogFar, frame.FogFar);
        if (_fogColor >= 0)
            GL.Uniform3(_fogColor, GlMapRenderer.ClearColor.X, GlMapRenderer.ClearColor.Y, GlMapRenderer.ClearColor.Z);
        if (_alphaScale >= 0)
            GL.Uniform1(_alphaScale, frame.AlphaScale);
    }

    private static int Compile(ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int ok);
        if (ok == 0)
            throw new InvalidOperationException($"{type} compile failed: {GL.GetShaderInfoLog(shader)}");
        return shader;
    }

    public void Dispose() => GL.DeleteProgram(_program);
}

internal static class Shaders
{
    public const string MeshVertex = """
        #version 330 core
        layout(location = 0) in vec3 aPos;
        layout(location = 1) in vec3 aNormal;
        layout(location = 2) in vec4 aColor;
        layout(location = 3) in vec2 aBary;
        uniform mat4 uViewProj;
        out vec3 vNormal;
        out vec4 vColor;
        out vec3 vBary;
        out vec3 vWorld;
        void main()
        {
            gl_Position = uViewProj * vec4(aPos, 1.0);
            vNormal = aNormal;
            vColor = aColor;
            vBary = vec3(aBary, 1.0 - aBary.x - aBary.y);
            vWorld = aPos;
        }
        """;

    public const string MeshFragment = """
        #version 330 core
        in vec3 vNormal;
        in vec4 vColor;
        in vec3 vBary;
        in vec3 vWorld;
        uniform vec3 uLightDir;
        uniform vec3 uEye;
        uniform float uFogNear;
        uniform float uFogFar;
        uniform vec3 uFogColor;
        uniform float uAlphaScale;
        out vec4 fragColor;
        void main()
        {
            float light = 0.55 + 0.45 * abs(dot(normalize(vNormal), uLightDir));
            vec3 color = vColor.rgb * light;
            float d = min(min(vBary.x, vBary.y), vBary.z);
            float w = max(fwidth(d), 1e-5);
            float edge = 1.0 - smoothstep(0.0, 1.5 * w, d);
            color = mix(color, color * 0.4, edge);
            float fog = clamp((distance(vWorld, uEye) - uFogNear) / max(uFogFar - uFogNear, 1.0), 0.0, 0.8);
            fragColor = vec4(mix(color, uFogColor, fog), vColor.a * uAlphaScale);
        }
        """;

    public const string LineVertex = """
        #version 330 core
        layout(location = 0) in vec3 aPos;
        layout(location = 1) in vec4 aColor;
        uniform mat4 uViewProj;
        out vec4 vColor;
        out vec3 vWorld;
        void main()
        {
            gl_Position = uViewProj * vec4(aPos, 1.0);
            vColor = aColor;
            vWorld = aPos;
        }
        """;

    public const string ColorFragment = """
        #version 330 core
        in vec4 vColor;
        in vec3 vWorld;
        uniform vec3 uEye;
        uniform float uFogNear;
        uniform float uFogFar;
        uniform vec3 uFogColor;
        uniform float uAlphaScale;
        out vec4 fragColor;
        void main()
        {
            float fog = clamp((distance(vWorld, uEye) - uFogNear) / max(uFogFar - uFogNear, 1.0), 0.0, 0.8);
            fragColor = vec4(mix(vColor.rgb, uFogColor, fog), vColor.a * uAlphaScale);
        }
        """;

    public const string PointVertex = """
        #version 330 core
        layout(location = 0) in vec3 aPos;
        layout(location = 1) in vec4 aColor;
        layout(location = 2) in float aSize;
        uniform mat4 uViewProj;
        out vec4 vColor;
        void main()
        {
            gl_Position = uViewProj * vec4(aPos, 1.0);
            gl_PointSize = aSize;
            vColor = aColor;
        }
        """;

    public const string PointFragment = """
        #version 330 core
        in vec4 vColor;
        uniform float uAlphaScale;
        out vec4 fragColor;
        void main()
        {
            vec2 p = gl_PointCoord * 2.0 - 1.0;
            float r = dot(p, p);
            if (r > 1.0) discard;
            vec3 color = r > 0.55 ? vColor.rgb * 0.35 : vColor.rgb;
            fragColor = vec4(color, vColor.a * uAlphaScale);
        }
        """;
}
