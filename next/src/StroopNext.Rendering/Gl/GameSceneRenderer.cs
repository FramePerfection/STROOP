using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using StroopNext.Graphics;
using StroopNext.Map3D;

namespace StroopNext.Rendering.Gl;

internal sealed class GameSceneRenderer : IDisposable
{
    private const int InstanceStride = 64;

    private readonly int _program;
    private readonly Uniforms _u;
    private readonly int _skyProgram, _skyVao;
    private readonly int _skyUseTexture, _skyColor, _skyOrigin;

    private readonly int _levelVao, _levelVbo;
    private readonly int _arenaVao, _arenaVbo;
    private readonly int _streamVao, _streamVbo;
    private int _streamCapacity;
    private GfxVertex[] _streamData = new GfxVertex[4096];
    private readonly int _instanceVbo;
    private int _arenaCapacity, _arenaUsed, _instanceCapacity;
    private int _levelVersion = -1, _levelVertexCount;
    private GfxMesh? _level;
    private readonly Dictionary<int, int> _arenaOffsets = new();

    private readonly Dictionary<int, int> _textures = new();
    private int _textureGeneration = -1;
    private int _skyTexture;
    private DecodedTexture? _skySource;

    private readonly Dictionary<(int MeshId, bool Mirrored, bool Recolor), Group> _groups = new();
    private readonly List<Group> _groupList = new();
    private float[] _instanceData = new float[16 * 256];
    private readonly List<Draw> _draws = new();

    private sealed class Group
    {
        public GfxMesh Mesh = null!;
        public bool Mirrored;
        public bool Recolor;
        public readonly List<Matrix4x4> Matrices = new();
        public int FirstInstance;
        public int StreamOffset = -1;
    }

    private readonly record struct Draw(int Phase, int Texture, Group? Group, GfxBatch Batch);

    private IReadOnlyList<Vector3>? _levelCopies;
    private int LevelInstanceCount => 1 + (_levelCopies?.Count ?? 0);

    public GameSceneRenderer()
    {
        _program = GlUtil.Program(GameShaders.Vertex, GameShaders.Fragment);
        _u = new Uniforms(_program);
        _skyProgram = GlUtil.Program(GameShaders.SkyVertex, GameShaders.SkyFragment);
        _skyUseTexture = GL.GetUniformLocation(_skyProgram, "useTexture");
        _skyColor = GL.GetUniformLocation(_skyProgram, "color");
        _skyOrigin = GL.GetUniformLocation(_skyProgram, "windowOrigin");
        _skyVao = GL.GenVertexArray();

        _instanceVbo = GL.GenBuffer();
        (_levelVao, _levelVbo) = CreateMeshVao();
        (_arenaVao, _arenaVbo) = CreateMeshVao();
        (_streamVao, _streamVbo) = CreateMeshVao();
    }

    public int LastDrawCalls { get; private set; }
    public int ArenaVertices => _arenaUsed;

    private (int Vao, int Vbo) CreateMeshVao()
    {
        int vao = GL.GenVertexArray(), vbo = GL.GenBuffer();
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        int s = GfxVertex.Size;
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, s, 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, s, 12);
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, s, 20);
        GL.EnableVertexAttribArray(3);
        GL.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, true, s, 24);
        GL.EnableVertexAttribArray(4);
        GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Byte, false, s, 28);
        GL.EnableVertexAttribArray(5);
        GL.VertexAttribPointer(5, 3, VertexAttribPointerType.Byte, false, s, 32);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVbo);
        for (int i = 0; i < 4; i++)
        {
            GL.EnableVertexAttribArray(6 + i);
            GL.VertexAttribDivisor(6 + i, 1);
        }
        SetInstanceOffset(0);
        GL.BindVertexArray(0);
        return (vao, vbo);
    }

    private void SetInstanceOffset(int instance)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVbo);
        for (int i = 0; i < 4; i++)
            GL.VertexAttribPointer(6 + i, 4, VertexAttribPointerType.Float, false, InstanceStride, instance * InstanceStride + 16 * i);
    }

    public void Render(GameScene scene, GameVisualsOptions options, Camera3D camera, int width, int height,
        IReadOnlyList<Vector3>? marioCopies = null, IReadOnlyList<Vector3>? levelCopies = null, IReadOnlyList<(Vector3 Position, ushort Yaw)>? ghosts = null)
    {
        _levelCopies = levelCopies;
        LastDrawCalls = 0;
        SyncTextures(scene.Textures);
        if (options.Sky)
            DrawSky(scene, camera);
        if (!options.Level && !options.Objects)
            return;

        SyncLevel(scene);
        BuildGroups(scene, options, camera, height, marioCopies, ghosts);
        UploadInstances();
        UploadTransient();
        CollectDraws(options);

        var viewProj = camera.ViewProjection;
        var view = camera.View;
        bool viewMirrors = Det3(viewProj) > 0;
        GL.UseProgram(_program);
        _u.Reset();
        GlUtil.UniformMatrix4(_u.ViewProjection, viewProj);
        GlUtil.UniformMatrix3(_u.ViewRotation, view);
        GL.Uniform2(_u.FogPlanes, scene.PerspectivePlanes.X, scene.PerspectivePlanes.Y);
        GL.Uniform1(_u.FogDistanceOffset, Math.Max(0f, Vector3.Distance(camera.Position, camera.Target) - 1000f));
        GL.Uniform1(_u.MaxY, options.MaxY);
        GL.Uniform1(_u.Texture, 0);
        GlUtil.UniformMatrix3Identity(_u.UvTransform);

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);
        GL.Enable(EnableCap.PolygonOffsetFill);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.ActiveTexture(TextureUnit.Texture0);

        int phase = -1, boundTexture = int.MinValue, boundVao = -1, boundInstance = -1, boundUv = -1;
        bool? decal = null, mirrored = null, recolor = null;
        CullMode? cull = null;
        foreach (var d in _draws)
        {
            if (d.Phase != phase)
            {
                phase = d.Phase;
                bool translucent = phase == 2;
                if (translucent)
                    GL.Enable(EnableCap.Blend);
                else
                    GL.Disable(EnableCap.Blend);
                GL.DepthMask(!translucent);
                GL.Uniform1(_u.AlphaCutoff, phase == 1 ? 0.5f : translucent ? 1f / 255 : 0f);
                GL.Uniform1(_u.Opaque, translucent ? 0 : 1);
            }
            var m = d.Batch.Material;
            if (m.Texture != boundTexture)
            {
                boundTexture = m.Texture;
                int name = 0;
                bool has = m.Texture >= 0 && _textures.TryGetValue(m.Texture, out name);
                GL.Uniform1(_u.UseTexture, has ? 1 : 0);
                GL.BindTexture(TextureTarget.Texture2D, has ? name : 0);
            }
            if (decal != m.Decal)
            {
                decal = m.Decal;
                GL.Uniform1(_u.Decal, m.Decal ? 1 : 0);
                GL.PolygonOffset(m.Decal ? 1 : 2, m.Decal ? 1 : 2);
            }
            var wantCull = options.CullBackFaces ? m.Cull : CullMode.None;
            if (cull != wantCull)
            {
                cull = wantCull;
                if (wantCull == CullMode.None)
                    GL.Disable(EnableCap.CullFace);
                else
                {
                    GL.Enable(EnableCap.CullFace);
                    GL.CullFace(wantCull == CullMode.Back ? TriangleFace.Back : TriangleFace.Front);
                }
            }
            bool groupRecolor = d.Group?.Recolor ?? false;
            if (recolor != groupRecolor)
            {
                recolor = groupRecolor;
                GL.Uniform1(_u.RecolorRed, groupRecolor ? 1 : 0);
            }
            bool groupMirrored = d.Group?.Mirrored ?? false;
            if (mirrored != groupMirrored)
            {
                mirrored = groupMirrored;
                GL.FrontFace(groupMirrored != viewMirrors ? FrontFaceDirection.Cw : FrontFaceDirection.Ccw);
            }
            _u.SetTexGen(m.TexGen);
            _u.SetFog(options.Fog ? m.Fog : 0);
            int uv = m.UvAnimation >= 0 && m.UvAnimation < scene.UvAnimations.Count ? m.UvAnimation : -1;
            if (uv != boundUv)
            {
                boundUv = uv;
                if (uv < 0)
                    GlUtil.UniformMatrix3Identity(_u.UvTransform);
                else
                    GL.UniformMatrix3(_u.UvTransform, 1, false, scene.UvAnimations[uv].Transform);
            }

            if (d.Group is null)
            {
                if (boundVao != _levelVao)
                {
                    GL.BindVertexArray(boundVao = _levelVao);
                    boundInstance = -1;
                }
                if (boundInstance != 0)
                    SetInstanceOffset(boundInstance = 0);
                GL.DrawArraysInstanced(PrimitiveType.Triangles, d.Batch.First, d.Batch.Count, LevelInstanceCount);
            }
            else
            {
                bool streamed = d.Group.StreamOffset >= 0;
                int vao = streamed ? _streamVao : _arenaVao;
                if (boundVao != vao)
                {
                    GL.BindVertexArray(boundVao = vao);
                    boundInstance = -1;
                }
                if (boundInstance != d.Group.FirstInstance)
                    SetInstanceOffset(boundInstance = d.Group.FirstInstance);
                int first = streamed ? d.Group.StreamOffset : _arenaOffsets[d.Group.Mesh.Id];
                GL.DrawArraysInstanced(PrimitiveType.Triangles, first + d.Batch.First, d.Batch.Count, d.Group.Matrices.Count);
            }
            LastDrawCalls++;
        }

        GL.BindVertexArray(0);
        GL.Disable(EnableCap.PolygonOffsetFill);
        GL.Disable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Ccw);
        GL.DepthMask(true);
        GL.Disable(EnableCap.Blend);
    }

    private void SyncLevel(GameScene scene)
    {
        if (scene.LevelVersion == _levelVersion)
            return;
        _levelVersion = scene.LevelVersion;
        _level = scene.Level;
        _levelVertexCount = _level.Vertices.Length;
        GL.BindBuffer(BufferTarget.ArrayBuffer, _levelVbo);
        GlUtil.BufferData(BufferTarget.ArrayBuffer, _level.Vertices, BufferUsageHint.StaticDraw);
        _arenaOffsets.Clear();
        _arenaUsed = 0;
    }

    private void BuildGroups(GameScene scene, GameVisualsOptions options, Camera3D camera, int height, IReadOnlyList<Vector3>? marioCopies,
        IReadOnlyList<(Vector3 Position, ushort Yaw)>? ghosts = null)
    {
        foreach (var g in _groupList)
            g.Matrices.Clear();
        _groupList.Clear();
        if (!options.Objects && !options.Level)
            return;

        var view = camera.View;
        var billboard = new Matrix4x4(view.M11, view.M21, view.M31, 0, view.M12, view.M22, view.M32, 0, view.M13, view.M23, view.M33, 0, 0, 0, 0, 1);
        var marioPosition = scene.MarioModelPosition;
        float marioScale = 1;
        if (options.KeepMarioVisible)
        {
            float distance = Math.Max(1, Vector3.Distance(camera.Position, marioPosition));
            float pixels = camera.OrthographicWidth is float orthoWidth
                ? 160 * camera.Width / Math.Max(1, orthoWidth)
                : (float)(160 * height / (2 * Math.Tan(camera.FieldOfView / 2) * distance));
            marioScale = Math.Max(1, 40 / Math.Max(pixels, 1e-3f));
        }
        var aroundMario = marioScale == 1 ? Matrix4x4.Identity
            : Matrix4x4.CreateTranslation(-marioPosition) * Matrix4x4.CreateScale(marioScale) * Matrix4x4.CreateTranslation(marioPosition);

        foreach (var instance in scene.Instances)
        {
            if (instance.Level ? !options.Level : !options.Objects)
                continue;
            if (instance.Level)
            {
                Add(instance.Mesh, instance.Matrix, false);
                if (_levelCopies is not null)
                    foreach (var offset in _levelCopies)
                        Add(instance.Mesh, instance.Matrix * Matrix4x4.CreateTranslation(offset), false);
                continue;
            }
            var matrix = instance.Billboard
                ? instance.Matrix * Matrix4x4.CreateScale(instance.AnchorScale) * billboard * Matrix4x4.CreateTranslation(instance.Anchor)
                : instance.Matrix;
            if (instance.IsMario)
                matrix *= aroundMario;
            Add(instance.Mesh, matrix, instance.Ghost && instance.Cap);
            if (instance.IsMario && marioCopies is not null)
                foreach (var copy in marioCopies)
                    Add(instance.Mesh, matrix * Matrix4x4.CreateTranslation(copy - marioPosition), instance.Cap);
            if (instance.IsMario && ghosts is not null)
                foreach (var (position, yaw) in ghosts)
                {
                    float turn = (float)StroopNext.Game.Angle.ToRadians((ushort)(yaw - scene.MarioModelYaw));
                    Add(instance.Mesh, matrix * Matrix4x4.CreateTranslation(-marioPosition) * Matrix4x4.CreateRotationY(turn) * Matrix4x4.CreateTranslation(position), instance.Cap);
                }
        }

        void Add(GfxMesh mesh, Matrix4x4 matrix, bool recolor)
        {
            bool mirrored = Det3(matrix) < 0;
            var key = (mesh.Id, mirrored, recolor);
            if (!_groups.TryGetValue(key, out var group))
                _groups[key] = group = new Group { Mesh = mesh, Mirrored = mirrored, Recolor = recolor };
            group.Mesh = mesh;
            if (group.Matrices.Count == 0)
                _groupList.Add(group);
            group.Matrices.Add(matrix);
        }

        if (_groups.Count > 4 * Math.Max(64, _groupList.Count))
            foreach (var stale in _groups.Where(kv => kv.Value.Matrices.Count == 0).Select(kv => kv.Key).ToList())
                _groups.Remove(stale);

        foreach (var g in _groupList)
            if (!g.Mesh.Transient)
                EnsureInArena(g.Mesh);
    }

    private void UploadTransient()
    {
        int total = 0;
        foreach (var g in _groupList)
        {
            g.StreamOffset = -1;
            if (!g.Mesh.Transient)
                continue;
            int length = g.Mesh.Vertices.Length;
            if (_streamData.Length < total + length)
                Array.Resize(ref _streamData, Math.Max(total + length, _streamData.Length * 2));
            g.Mesh.Vertices.CopyTo(_streamData, total);
            g.StreamOffset = total;
            total += length;
        }
        if (total == 0)
            return;
        GL.BindBuffer(BufferTarget.ArrayBuffer, _streamVbo);
        int bytes = total * GfxVertex.Size;
        if (bytes > _streamCapacity)
            _streamCapacity = Math.Max(bytes, _streamCapacity * 2);
        GL.BufferData(BufferTarget.ArrayBuffer, _streamCapacity, IntPtr.Zero, BufferUsageHint.StreamDraw);
        GlUtil.BufferSubData(BufferTarget.ArrayBuffer, 0, _streamData.AsSpan(0, total));
    }

    private static float Det3(in Matrix4x4 m) =>
        m.M11 * (m.M22 * m.M33 - m.M23 * m.M32) - m.M12 * (m.M21 * m.M33 - m.M23 * m.M31) + m.M13 * (m.M21 * m.M32 - m.M22 * m.M31);

    private void EnsureInArena(GfxMesh mesh)
    {
        if (_arenaOffsets.ContainsKey(mesh.Id))
            return;
        int needed = mesh.Vertices.Length;
        if (_arenaUsed + needed > _arenaCapacity)
        {
            var live = _groupList.Select(g => g.Mesh).Where(m => !m.Transient).Append(mesh).DistinctBy(m => m.Id).ToList();
            int liveVertices = live.Sum(m => m.Vertices.Length);
            _arenaCapacity = Math.Max(1 << 16, Math.Max(liveVertices * 2, _arenaCapacity));
            GL.BindBuffer(BufferTarget.ArrayBuffer, _arenaVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _arenaCapacity * GfxVertex.Size, IntPtr.Zero, BufferUsageHint.StaticDraw);
            _arenaOffsets.Clear();
            _arenaUsed = 0;
            foreach (var m in live)
                Append(m);
            return;
        }
        Append(mesh);
    }

    private void Append(GfxMesh mesh)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _arenaVbo);
        GlUtil.BufferSubData(BufferTarget.ArrayBuffer, _arenaUsed * GfxVertex.Size, mesh.Vertices);
        _arenaOffsets[mesh.Id] = _arenaUsed;
        _arenaUsed += mesh.Vertices.Length;
    }

    private void UploadInstances()
    {
        int count = LevelInstanceCount + _groupList.Sum(g => g.Matrices.Count);
        if (_instanceData.Length < count * 16)
            Array.Resize(ref _instanceData, Math.Max(count * 16, _instanceData.Length * 2));
        var identity = Matrix4x4.Identity;
        MemoryMarshal.Write(MemoryMarshal.AsBytes(_instanceData.AsSpan(0, 16)), in identity);
        int at = 1;
        if (_levelCopies is not null)
            foreach (var offset in _levelCopies)
            {
                var copy = Matrix4x4.CreateTranslation(offset);
                MemoryMarshal.Write(MemoryMarshal.AsBytes(_instanceData.AsSpan(at++ * 16, 16)), in copy);
            }
        foreach (var g in _groupList)
        {
            g.FirstInstance = at;
            foreach (var m in g.Matrices)
            {
                var copy = m;
                MemoryMarshal.Write(MemoryMarshal.AsBytes(_instanceData.AsSpan(at * 16, 16)), in copy);
                at++;
            }
        }
        GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVbo);
        int bytes = count * InstanceStride;
        if (bytes > _instanceCapacity)
        {
            _instanceCapacity = Math.Max(bytes, _instanceCapacity * 2);
            GL.BufferData(BufferTarget.ArrayBuffer, _instanceCapacity, IntPtr.Zero, BufferUsageHint.StreamDraw);
        }
        GlUtil.BufferSubData(BufferTarget.ArrayBuffer, 0, _instanceData.AsSpan(0, count * 16));
    }

    private void CollectDraws(GameVisualsOptions options)
    {
        _draws.Clear();
        if (options.Level && _level is not null)
            foreach (var b in _level.Batches)
                _draws.Add(new Draw(b.Material.Phase, b.Material.Texture, null, b));
        foreach (var g in _groupList)
            foreach (var b in g.Mesh.Batches)
                _draws.Add(new Draw(b.Material.Phase, b.Material.Texture, g, b));
        _draws.Sort((a, b) => a.Phase != b.Phase ? a.Phase.CompareTo(b.Phase) : a.Texture.CompareTo(b.Texture));
    }

    private void SyncTextures(TextureLibrary library)
    {
        if (library.Generation != _textureGeneration)
        {
            foreach (int name in _textures.Values)
                GL.DeleteTexture(name);
            _textures.Clear();
            _textureGeneration = library.Generation;
        }
        var all = library.All;
        for (int id = 0; id < all.Count; id++)
            if (!_textures.ContainsKey(id))
                _textures[id] = Upload(all[id]);
    }

    private static int Upload(DecodedTexture t)
    {
        int name = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, name);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, t.Width, t.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, t.Rgba);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)Wrap(t.WrapS));
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)Wrap(t.WrapT));
        return name;
    }

    private static TextureWrapMode Wrap(TextureWrap w) => w switch
    {
        TextureWrap.MirroredRepeat => TextureWrapMode.MirroredRepeat,
        TextureWrap.Clamp => TextureWrapMode.ClampToEdge,
        _ => TextureWrapMode.Repeat,
    };

    private void DrawSky(GameScene scene, Camera3D camera)
    {
        if (scene.Skybox is null && scene.BackgroundColor is null)
            return;
        if (!ReferenceEquals(scene.Skybox, _skySource))
        {
            if (_skyTexture != 0)
                GL.DeleteTexture(_skyTexture);
            _skyTexture = 0;
            _skySource = scene.Skybox;
            if (scene.Skybox is { } sky)
            {
                _skyTexture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, _skyTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, sky.Width, sky.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, sky.Rgba);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }
        }
        var forward = camera.Forward;
        double yaw = (Math.Atan2(forward.X, forward.Z) * 180 / Math.PI + 360) % 360;
        double pitch = Math.Atan2(forward.Y, Math.Sqrt(forward.X * forward.X + forward.Z * forward.Z)) * 180 / Math.PI;
        double x = Math.Round(320 * yaw / 90) % 1280;
        double y = Math.Clamp(Math.Round(4 * pitch) + 600, 240, 960);

        GL.UseProgram(_skyProgram);
        GL.Uniform1(_skyUseTexture, _skyTexture != 0 ? 1 : 0);
        var color = scene.BackgroundColor ?? Vector3.Zero;
        GL.Uniform3(_skyColor, color.X, color.Y, color.Z);
        GL.Uniform2(_skyOrigin, (float)(1280 - x), (float)y);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _skyTexture);
        GL.Disable(EnableCap.DepthTest);
        GL.DepthMask(false);
        GL.Disable(EnableCap.Blend);
        GL.BindVertexArray(_skyVao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        GL.BindVertexArray(0);
        GL.DepthMask(true);
        GL.Enable(EnableCap.DepthTest);
        LastDrawCalls++;
    }

    public void Dispose()
    {
        foreach (int name in _textures.Values)
            GL.DeleteTexture(name);
        if (_skyTexture != 0)
            GL.DeleteTexture(_skyTexture);
        GL.DeleteBuffer(_levelVbo);
        GL.DeleteBuffer(_arenaVbo);
        GL.DeleteBuffer(_streamVbo);
        GL.DeleteVertexArray(_streamVao);
        GL.DeleteBuffer(_instanceVbo);
        GL.DeleteVertexArray(_levelVao);
        GL.DeleteVertexArray(_arenaVao);
        GL.DeleteVertexArray(_skyVao);
        GL.DeleteProgram(_program);
        GL.DeleteProgram(_skyProgram);
    }

    private sealed class Uniforms(int program)
    {
        public readonly int ViewProjection = GL.GetUniformLocation(program, "viewProjection");
        public readonly int ViewRotation = GL.GetUniformLocation(program, "viewRotation");
        public readonly int UvTransform = GL.GetUniformLocation(program, "uvTransform");
        public readonly int TexGen = GL.GetUniformLocation(program, "texGen");
        public readonly int FogEnabled = GL.GetUniformLocation(program, "fogEnabled");
        public readonly int FogFactors = GL.GetUniformLocation(program, "fogFactors");
        public readonly int FogPlanes = GL.GetUniformLocation(program, "fogPlanes");
        public readonly int FogDistanceOffset = GL.GetUniformLocation(program, "fogDistanceOffset");
        public readonly int FogColor = GL.GetUniformLocation(program, "fogColor");
        public readonly int Texture = GL.GetUniformLocation(program, "tex");
        public readonly int UseTexture = GL.GetUniformLocation(program, "useTexture");
        public readonly int Decal = GL.GetUniformLocation(program, "decal");
        public readonly int AlphaCutoff = GL.GetUniformLocation(program, "alphaCutoff");
        public readonly int Opaque = GL.GetUniformLocation(program, "opaque");
        public readonly int MaxY = GL.GetUniformLocation(program, "maxY");
        public readonly int RecolorRed = GL.GetUniformLocation(program, "recolorRed");

        private int _texGen = -1;
        private ulong? _fog;

        public void Reset()
        {
            _texGen = -1;
            _fog = null;
        }

        public void SetTexGen(int value)
        {
            if (value == _texGen)
                return;
            _texGen = value;
            GL.Uniform1(TexGen, value);
        }

        public void SetFog(ulong fog)
        {
            if (fog == _fog)
                return;
            _fog = fog;
            GL.Uniform1(FogEnabled, fog != 0 ? 1 : 0);
            if (fog == 0)
                return;
            uint color = (uint)(fog >> 32);
            GL.Uniform3(FogColor, (color >> 24) / 255f, ((color >> 16) & 0xFF) / 255f, ((color >> 8) & 0xFF) / 255f);
            GL.Uniform2(FogFactors, (float)(short)(fog >> 16), (float)(short)fog);
        }
    }
}

internal static class GlUtil
{
    public static int Program(string vertex, string fragment)
    {
        int vs = Compile(ShaderType.VertexShader, vertex), fs = Compile(ShaderType.FragmentShader, fragment);
        int program = GL.CreateProgram();
        GL.AttachShader(program, vs);
        GL.AttachShader(program, fs);
        GL.LinkProgram(program);
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int linked);
        if (linked == 0)
            throw new InvalidOperationException("Shader link failed: " + GL.GetProgramInfoLog(program));
        GL.DeleteShader(vs);
        GL.DeleteShader(fs);
        return program;
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

    public static unsafe void UniformMatrix4(int location, Matrix4x4 m) => GL.UniformMatrix4(location, 1, false, &m.M11);

    public static unsafe void UniformMatrix3(int location, Matrix4x4 m)
    {
        float* r = stackalloc float[] { m.M11, m.M12, m.M13, m.M21, m.M22, m.M23, m.M31, m.M32, m.M33 };
        GL.UniformMatrix3(location, 1, false, r);
    }

    public static unsafe void UniformMatrix3Identity(int location)
    {
        float* r = stackalloc float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
        GL.UniformMatrix3(location, 1, false, r);
    }

    public static unsafe void BufferData<T>(BufferTarget target, ReadOnlySpan<T> data, BufferUsageHint usage) where T : unmanaged
    {
        fixed (T* p = data)
            GL.BufferData(target, data.Length * sizeof(T), (IntPtr)p, usage);
    }

    public static unsafe void BufferSubData<T>(BufferTarget target, int offsetBytes, ReadOnlySpan<T> data) where T : unmanaged
    {
        if (data.IsEmpty)
            return;
        fixed (T* p = data)
            GL.BufferSubData(target, offsetBytes, data.Length * sizeof(T), (IntPtr)p);
    }
}

internal static class GameShaders
{
    public const string Vertex = """
        #version 330 core
        layout (location = 0) in vec3 vs_position;
        layout (location = 1) in vec2 vs_texCoord;
        layout (location = 2) in vec4 vs_color;
        layout (location = 3) in vec4 vs_ambient;
        layout (location = 4) in vec3 vs_normal;
        layout (location = 5) in vec3 vs_lightDir;
        layout (location = 6) in vec4 vs_model0;
        layout (location = 7) in vec4 vs_model1;
        layout (location = 8) in vec4 vs_model2;
        layout (location = 9) in vec4 vs_model3;

        uniform mat4 viewProjection;
        uniform mat3 viewRotation;
        uniform mat3 uvTransform;
        uniform int texGen;
        uniform int fogEnabled;
        uniform vec2 fogFactors;
        uniform vec2 fogPlanes;
        uniform float fogDistanceOffset;

        out vec2 fs_texCoord;
        out vec4 fs_color;
        out float fs_worldY;
        out float fs_fog;

        void main()
        {
            mat4 model = mat4(vs_model0, vs_model1, vs_model2, vs_model3);
            vec4 world = model * vec4(vs_position, 1.0);
            gl_Position = viewProjection * world;
            vec2 uv = vs_texCoord;
            if (texGen != 0)
            {
                vec3 n = viewRotation * (mat3(model) * vs_normal);
                vec2 d = length(n) > 0.0 ? n.xy / length(n) : vec2(0.0);
                uv *= texGen == 2 ? acos(clamp(-d, -1.0, 1.0)) / 3.14159265 : (d + 1.0) * 0.5;
            }
            fs_texCoord = (uvTransform * vec3(uv, 1.0)).xy;
            fs_worldY = world.y;

            fs_fog = 0.0;
            if (fogEnabled != 0)
            {
                float n = fogPlanes.x, f = fogPlanes.y, d = max(gl_Position.w - fogDistanceOffset, n);
                float z = (f + n) / (f - n) - 2.0 * f * n / ((f - n) * d);
                fs_fog = clamp((z * fogFactors.x + fogFactors.y) / 256.0, 0.0, 1.0);
            }

            vec4 color = vs_color;
            if (vs_ambient.a > 0.5)
            {
                vec3 n = viewRotation * (mat3(model) * vs_normal);
                float nLength = length(n), lLength = length(vs_lightDir);
                float k = nLength > 0.0 && lLength > 0.0 ? max(0.0, dot(n / nLength, vs_lightDir / lLength)) : 0.0;
                color.rgb = min(vec3(1.0), vs_ambient.rgb + vs_color.rgb * k);
            }
            fs_color = color;
        }
        """;

    public const string Fragment = """
        #version 330 core
        in vec2 fs_texCoord;
        in vec4 fs_color;
        in float fs_worldY;
        in float fs_fog;

        uniform sampler2D tex;
        uniform bool useTexture;
        uniform bool decal;
        uniform float alphaCutoff;
        uniform bool opaque;
        uniform float maxY;
        uniform vec3 fogColor;
        uniform bool recolorRed;

        layout(location = 0) out vec4 color;

        void main()
        {
            if (fs_worldY > maxY)
                discard;
            vec4 c = fs_color;
            if (useTexture)
            {
                vec4 t = texture(tex, fs_texCoord);
                c = decal ? vec4(mix(fs_color.rgb, t.rgb, t.a), fs_color.a) : t * fs_color;
            }
            if (c.a < alphaCutoff)
                discard;
            if (recolorRed && c.r > 0.15 && c.r > 1.5 * c.g && c.r > 1.5 * c.b)
                c.rg = c.gr;
            c.rgb = mix(c.rgb, fogColor, fs_fog);
            if (opaque)
                c.a = 1.0;
            color = c;
        }
        """;

    public const string SkyVertex = """
        #version 330 core
        out vec2 fs_screen;
        void main()
        {
            vec2 ndc = vec2((gl_VertexID & 1) * 4.0 - 1.0, (gl_VertexID >> 1) * 4.0 - 1.0);
            gl_Position = vec4(ndc, 1.0, 1.0);
            fs_screen = vec2(ndc.x * 0.5 + 0.5, 0.5 - ndc.y * 0.5);
        }
        """;

    public const string SkyFragment = """
        #version 330 core
        in vec2 fs_screen;
        uniform sampler2D sky;
        uniform bool useTexture;
        uniform vec3 color;
        uniform vec2 windowOrigin;
        layout(location = 0) out vec4 outColor;
        void main()
        {
            if (!useTexture)
            {
                outColor = vec4(color, 1.0);
                return;
            }
            float x = windowOrigin.x + fs_screen.x * 320.0;
            float y = windowOrigin.y - fs_screen.y * 240.0;
            outColor = vec4(texture(sky, vec2(x / 1600.0, (960.0 - y) / 960.0)).rgb, 1.0);
        }
        """;
}
