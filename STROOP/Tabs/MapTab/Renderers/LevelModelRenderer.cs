using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using STROOP.Tabs.MapTab.DataUtil;

namespace STROOP.Tabs.MapTab.Renderers
{
    public class LevelModelRenderer
    {
        const int LAYER_ALPHA = 4, LAYER_TRANSPARENT = 5;

        const uint WaterColor = 0x66D57B3A;
        const int ShadowSegments = 16;

        int shader, vao, buffer, dynamicVao, dynamicBuffer;
        int bufferCapacity, uploadedLength;
        readonly List<int> textures = new List<int>();
        LevelModel uploaded;
        byte[] dynamicVertices = new byte[MeshStore.VertexSize * 1024];
        int dynamicLength;

        int uniform_viewRotation;
        int uniform_viewProjection, uniform_model, uniform_tex, uniform_useTexture, uniform_decal, uniform_alphaCutoff, uniform_opaque, uniform_maxY;

        int skyShader, skyVao, skyTexture;
        int uniform_sky_useTexture, uniform_sky_color, uniform_sky_windowOrigin;

        static int CreateVertexArray(out int vertexBuffer)
        {
            int vertexArray = GL.GenVertexArray();
            vertexBuffer = GL.GenBuffer();
            GL.BindVertexArray(vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            for (int i = 0; i < 6; i++)
                GL.EnableVertexAttribArray(i);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, MeshStore.VertexSize, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, MeshStore.VertexSize, 12);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, MeshStore.VertexSize, 20);
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, true, MeshStore.VertexSize, 24);
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Byte, false, MeshStore.VertexSize, 28);
            GL.VertexAttribPointer(5, 3, VertexAttribPointerType.Byte, false, MeshStore.VertexSize, 32);
            GL.BindVertexArray(0);
            return vertexArray;
        }

        void Init()
        {
            shader = GraphicsUtil.GetShaderProgram("Resources/Shaders/LevelModel.vert.glsl", "Resources/Shaders/LevelModel.frag.glsl");
            uniform_viewProjection = GL.GetUniformLocation(shader, "viewProjection");
            uniform_model = GL.GetUniformLocation(shader, "model");
            uniform_viewRotation = GL.GetUniformLocation(shader, "viewRotation");
            uniform_tex = GL.GetUniformLocation(shader, "tex");
            uniform_useTexture = GL.GetUniformLocation(shader, "useTexture");
            uniform_decal = GL.GetUniformLocation(shader, "decal");
            uniform_alphaCutoff = GL.GetUniformLocation(shader, "alphaCutoff");
            uniform_opaque = GL.GetUniformLocation(shader, "opaque");
            uniform_maxY = GL.GetUniformLocation(shader, "maxY");
            vao = CreateVertexArray(out buffer);
            dynamicVao = CreateVertexArray(out dynamicBuffer);

            skyShader = GraphicsUtil.GetShaderProgram("Resources/Shaders/Skybox.vert.glsl", "Resources/Shaders/Skybox.frag.glsl");
            uniform_sky_useTexture = GL.GetUniformLocation(skyShader, "useTexture");
            uniform_sky_color = GL.GetUniformLocation(skyShader, "color");
            uniform_sky_windowOrigin = GL.GetUniformLocation(skyShader, "windowOrigin");
            skyVao = GL.GenVertexArray();
        }

        void UploadSkybox(Texture sky)
        {
            if (skyTexture != 0)
                GL.DeleteTexture(skyTexture);
            skyTexture = 0;
            if (sky == null)
                return;
            skyTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, skyTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, sky.width, sky.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, sky.rgba);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        void DrawSky(MapGraphics graphics, LevelModel model)
        {
            if (skyTexture == 0 && !model.backgroundColor.HasValue)
                return;
            var forward = -graphics.BillboardMatrix.Row2.Xyz;
            double yaw = (Math.Atan2(forward.X, forward.Z) * 180 / Math.PI + 360) % 360;
            double pitch = Math.Atan2(forward.Y, Math.Sqrt(forward.X * forward.X + forward.Z * forward.Z)) * 180 / Math.PI;
            double x = Math.Round(320 * yaw / 90) % 1280;
            double y = Math.Min(960, Math.Max(240, Math.Round(4 * pitch) + 600));

            GL.UseProgram(skyShader);
            GL.Uniform1(uniform_sky_useTexture, skyTexture != 0 ? 1 : 0);
            var color = model.backgroundColor ?? Vector3.Zero;
            GL.Uniform3(uniform_sky_color, ref color);
            GL.Uniform2(uniform_sky_windowOrigin, (float)(1280 - x), (float)y);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, skyTexture);
            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false);
            GL.Disable(EnableCap.Blend);
            GL.BindVertexArray(skyVao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.BindVertexArray(0);
            GL.DepthMask(true);
        }

        static TextureWrapMode WrapMode(int wrap) => wrap switch
        {
            1 => TextureWrapMode.MirroredRepeat,
            2 => TextureWrapMode.ClampToEdge,
            _ => TextureWrapMode.Repeat,
        };

        void Upload(LevelModel model)
        {
            if (model != uploaded)
            {
                if (textures.Count > 0)
                    GL.DeleteTextures(textures.Count, textures.ToArray());
                textures.Clear();
                uploadedLength = 0;
                UploadSkybox(model.skybox);
                uploaded = model;
            }

            var modelTextures = model.textures.textures;
            for (int i = textures.Count; i < modelTextures.Count; i++)
            {
                var t = modelTextures[i];
                int texture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, t.width, t.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, t.rgba);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)WrapMode(t.wrapS));
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)WrapMode(t.wrapT));
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                textures.Add(texture);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);

            var store = model.store;
            if (store.length > uploadedLength)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
                if (store.length > bufferCapacity || uploadedLength == 0)
                {
                    bufferCapacity = store.data.Length;
                    GL.BufferData(BufferTarget.ArrayBuffer, bufferCapacity, store.data, BufferUsageHint.StaticDraw);
                }
                else
                    GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)uploadedLength, store.length - uploadedLength, ref store.data[uploadedLength]);

                uploadedLength = store.length;
            }
        }

        int UploadDynamic(IReadOnlyList<LevelModel.Shadow> shadows, IReadOnlyList<(int y, int xMin, int xMax, int zMin, int zMax)> waters)
        {
            dynamicLength = 0;
            if (shadows != null)
                foreach (var s in shadows)
                {
                    var c = s.position + new Vector3(0, 4, 0);
                    uint color = (uint)(255 * Math.Min(1, s.opacity)) << 24;
                    float r = s.size / 2;
                    if (s.square)
                    {
                        Quad(c.X - r, c.X + r, c.Z - r, c.Z + r, c.Y, color);
                        continue;
                    }

                    for (int i = 0; i < ShadowSegments; i++)
                    {
                        DynamicVertex(c.X, c.Y, c.Z, color);
                        DynamicVertex(c.X + r * CircleSin[i], c.Y, c.Z + r * CircleCos[i], color);
                        DynamicVertex(c.X + r * CircleSin[i + 1], c.Y, c.Z + r * CircleCos[i + 1], color);
                    }
                }

            if (waters != null)
                foreach (var w in waters)
                    Quad(w.xMin, w.xMax, w.zMin, w.zMax, w.y, WaterColor);

            if (dynamicLength > 0)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, dynamicBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, dynamicLength, dynamicVertices, BufferUsageHint.StreamDraw);
            }

            return dynamicLength / MeshStore.VertexSize;
        }

        static readonly float[] CircleSin = new float[ShadowSegments + 1], CircleCos = new float[ShadowSegments + 1];

        static LevelModelRenderer()
        {
            for (int i = 0; i <= ShadowSegments; i++)
            {
                CircleSin[i] = (float)Math.Sin(2 * Math.PI * i / ShadowSegments);
                CircleCos[i] = (float)Math.Cos(2 * Math.PI * i / ShadowSegments);
            }
        }

        void Quad(float xMin, float xMax, float zMin, float zMax, float y, uint color)
        {
            DynamicVertex(xMin, y, zMin, color);
            DynamicVertex(xMax, y, zMin, color);
            DynamicVertex(xMax, y, zMax, color);
            DynamicVertex(xMin, y, zMin, color);
            DynamicVertex(xMax, y, zMax, color);
            DynamicVertex(xMin, y, zMax, color);
        }

        void DynamicVertex(float x, float y, float z, uint color)
        {
            if (dynamicLength + MeshStore.VertexSize > dynamicVertices.Length)
                Array.Resize(ref dynamicVertices, dynamicVertices.Length * 2);
            var span = dynamicVertices.AsSpan(dynamicLength, MeshStore.VertexSize);
            BitConverter.TryWriteBytes(span, x);
            BitConverter.TryWriteBytes(span.Slice(4), y);
            BitConverter.TryWriteBytes(span.Slice(8), z);
            BitConverter.TryWriteBytes(span.Slice(12), 0f);
            BitConverter.TryWriteBytes(span.Slice(16), 0f);
            BitConverter.TryWriteBytes(span.Slice(20), color);
            span.Slice(24).Clear();
            dynamicLength += MeshStore.VertexSize;
        }

        static float Determinant3(in Matrix4 m) => new Matrix3(m).Determinant;

        FrontFaceDirection frontFace;
        bool viewMirrors;

        void SetModel(in Matrix4 model)
        {
            var m = model;
            GL.UniformMatrix4(uniform_model, false, ref m);
            var face = (Determinant3(model) < 0) != viewMirrors ? FrontFaceDirection.Cw : FrontFaceDirection.Ccw;
            if (face != frontFace)
                GL.FrontFace(frontFace = face);
        }

        int stateTransparent, stateCull, stateDecal, stateTexture, stateCutoffLayer;

        void ResetState() => stateTransparent = stateCull = stateDecal = stateTexture = stateCutoffLayer = int.MinValue;

        void DrawBatch(Batch batch, bool cull)
        {
            int transparent = batch.layer >= LAYER_TRANSPARENT ? 1 : 0;
            if (transparent != stateTransparent)
            {
                if (transparent == 1)
                    GL.Enable(EnableCap.Blend);
                else
                    GL.Disable(EnableCap.Blend);
                GL.DepthMask(transparent == 0);
                GL.Uniform1(uniform_opaque, 1 - transparent);
                stateTransparent = transparent;
            }

            int cullMode = cull ? (int)batch.cull : (int)CullMode.None;
            if (cullMode != stateCull)
            {
                if (cullMode != (int)CullMode.None)
                {
                    GL.Enable(EnableCap.CullFace);
                    GL.CullFace(cullMode == (int)CullMode.Back ? TriangleFace.Back : TriangleFace.Front);
                }
                else
                    GL.Disable(EnableCap.CullFace);
                stateCull = cullMode;
            }

            int decal = batch.decal ? 1 : 0;
            if (decal != stateDecal)
            {
                float offset = decal == 1 ? 1 : 2;
                GL.PolygonOffset(offset, offset);
                GL.Uniform1(uniform_decal, decal);
                stateDecal = decal;
            }

            int cutoffLayer = batch.layer == LAYER_ALPHA ? 1 : transparent == 1 ? 2 : 0;
            if (cutoffLayer != stateCutoffLayer)
            {
                GL.Uniform1(uniform_alphaCutoff, cutoffLayer == 1 ? 0.5f : cutoffLayer == 2 ? 1 / 255f : 0);
                stateCutoffLayer = cutoffLayer;
            }

            int texture = batch.texture >= 0 && batch.texture < textures.Count ? textures[batch.texture] : 0;
            if (texture != stateTexture)
            {
                GL.Uniform1(uniform_useTexture, texture != 0 ? 1 : 0);
                GL.BindTexture(TextureTarget.Texture2D, texture);
                stateTexture = texture;
            }

            GL.DrawArrays(PrimitiveType.Triangles, batch.first, batch.count);
        }

        static int Phase(Batch batch) => batch.layer >= LAYER_TRANSPARENT ? 2 : batch.layer == LAYER_ALPHA ? 1 : 0;

        public void Draw(MapGraphics graphics, LevelModel model, float maxY, bool cull, bool drawObjects,
            IReadOnlyList<(int y, int xMin, int xMax, int zMin, int zMax)> waters, bool drawSky)
        {
            if (shader == 0)
                Init();
            Upload(model);
            int dynamicVertexCount = UploadDynamic(drawObjects ? model.shadows : null, waters);

            bool blendWasEnabled = GL.IsEnabled(EnableCap.Blend);
            if (drawSky)
                DrawSky(graphics, model);

            GL.UseProgram(shader);
            ResetState();
            var viewProjection = graphics.ViewMatrix;
            GL.UniformMatrix4(uniform_viewProjection, false, ref viewProjection);
            var viewRotation = Matrix3.Transpose(new Matrix3(graphics.BillboardMatrix));
            GL.UniformMatrix3(uniform_viewRotation, false, ref viewRotation);
            GL.Uniform1(uniform_tex, 0);
            GL.Uniform1(uniform_maxY, maxY);
            GL.ActiveTexture(TextureUnit.Texture0);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            viewMirrors = Determinant3(graphics.ViewMatrix) > 0;
            frontFace = FrontFaceDirection.Ccw;
            GL.FrontFace(frontFace);
            GL.Enable(EnableCap.PolygonOffsetFill);

            var billboardRotation = graphics.BillboardMatrix;
            billboardRotation.Row3 = Vector4.UnitW;

            GL.BindVertexArray(vao);
            for (int phase = 0; phase < 3; phase++)
            {
                SetModel(Matrix4.Identity);
                foreach (var batch in model.level.batches)
                    if (Phase(batch) == phase)
                        DrawBatch(batch, cull);

                if (drawObjects)
                    foreach (var instance in model.instances)
                    {
                        bool modelSet = false;
                        foreach (var batch in instance.mesh.batches)
                        {
                            if (Phase(batch) != phase)
                                continue;
                            if (!modelSet)
                            {
                                SetModel(instance.billboard
                                    ? instance.matrix * Matrix4.CreateScale(instance.anchorScale) * billboardRotation * Matrix4.CreateTranslation(instance.anchor)
                                    : instance.matrix);
                                modelSet = true;
                            }

                            DrawBatch(batch, cull);
                        }
                    }
            }

            if (dynamicVertexCount > 0)
            {
                GL.BindVertexArray(dynamicVao);
                SetModel(Matrix4.Identity);
                DrawBatch(new Batch { texture = -1, layer = LAYER_TRANSPARENT, decal = true, first = 0, count = dynamicVertexCount }, false);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
            GL.DepthMask(true);
            GL.Disable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.Disable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(0, 0);
            if (blendWasEnabled)
                GL.Enable(EnableCap.Blend);
            else
                GL.Disable(EnableCap.Blend);
        }
    }
}
