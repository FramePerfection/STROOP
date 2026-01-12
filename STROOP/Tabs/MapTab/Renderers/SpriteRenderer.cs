using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Mathematics;
using STROOP.Core;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab.Renderers
{
    public class SpriteRenderer : InstanceRenderer<SpriteRenderer.InstanceData>
    {
        class TransparentSpriteRenderer : SpriteRenderer, TransparencyRenderer.Transparent
        {
            SpriteRenderer parent;

            protected override int GetShader() => GraphicsUtil.GetShaderProgram("Resources/Shaders/Sprites.vert.glsl", "Resources/Shaders/DepthMask.frag.glsl");

            public TransparentSpriteRenderer(SpriteRenderer parent, int maxExpectedInstances) : base(maxExpectedInstances)
            {
                this.parent = parent;
            }

            public void DrawMask(TransparencyRenderer renderer)
            {
                if (instances.Count == 0)
                    return;
                BeginDraw(renderer.graphics, false);
                renderer.SetUniforms(shader);
                GL.Disable(EnableCap.CullFace);
                GL.DrawArraysInstanced(PrimitiveType.TriangleStrip, 0, 4, instances.Count);
                GL.BindVertexArray(0);
            }

            public void DrawTransparent(TransparencyRenderer renderer)
            {
                if (instances.Count == 0)
                    return;
                GL.UseProgram(parent.shader);
                GL.BindVertexArray(vertexArray);
                Matrix4 mat = ignoreView ? Matrix4.Identity : renderer.graphics.ViewMatrix;
                GL.UniformMatrix4(parent.uniform_viewProjection, false, ref mat);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2DArray, parent.texture);
                GL.Uniform1(uniform_sampler, (int)0);
                GL.Disable(EnableCap.CullFace);
                GL.DrawArraysInstanced(PrimitiveType.TriangleStrip, 0, 4, instances.Count);
                GL.BindVertexArray(0);
            }

            public void Prepare(TransparencyRenderer renderer)
            {
                if (instances.Count == 0)
                    return;
                UpdateBuffer(instances.Count);
            }
        }

        public struct InstanceData
        {
            public const int Size = sizeof(float) * 4 * 4 + 5 * sizeof(float);
            public Matrix4 transform;
            public Vector4 color;
            public float textureIndex;
        }


        public static void GenSpriteVAO(int vertexArray, int instanceBuffer)
        {
            GL.BindVertexArray(vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceBuffer);
            for (int i = 0; i <= 5; i++)
            {
                GL.EnableVertexAttribArray(i);
                GL.VertexAttribDivisor(i, 1);
            }

            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, InstanceData.Size, sizeof(float) * 0);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, InstanceData.Size, sizeof(float) * 4);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, InstanceData.Size, sizeof(float) * 8);
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, InstanceData.Size, sizeof(float) * 12);
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, InstanceData.Size, sizeof(float) * 16);
            GL.VertexAttribPointer(5, 1, VertexAttribPointerType.Float, false, InstanceData.Size, sizeof(float) * 20);

            GL.BindVertexArray(0);
        }

        int uniform_sampler;

        readonly MapGraphics.DrawLayers layer;
        public int texture;
        TransparentSpriteRenderer transparentRenderer;
        public TransparencyRenderer.Transparent transparent => transparentRenderer;

        protected virtual int GetShader() => GraphicsUtil.GetShaderProgram("Resources/Shaders/Sprites.vert.glsl", "Resources/Shaders/Texture.frag.glsl");

        private SpriteRenderer(int maxExpectedInstances)
        {
            AccessScope<MapTab>.content.graphics.DoGLInit(() =>
            {
                shader = GetShader();
                Init(maxExpectedInstances);
                uniform_sampler = GL.GetUniformLocation(shader, "sampler");

                GenSpriteVAO(vertexArray, instanceBuffer);
            });
        }

        public SpriteRenderer(MapGraphics.DrawLayers layer, int maxExpectedInstances = 128) : this(maxExpectedInstances)
        {
            this.layer = layer;
            transparentRenderer = new TransparentSpriteRenderer(this, maxExpectedInstances);
        }

        public void AddInstance(Matrix4 transform, int textureIndex, Vector4 color)
        {
            instances.Add(new InstanceData { transform = transform, textureIndex = textureIndex, color = color });
        }

        public void AddTransparentInstance(Matrix4 transform, int textureIndex, Vector4 color) =>
            transparentRenderer.AddInstance(transform, textureIndex, color);

        public override void SetDrawCalls(MapGraphics graphics)
        {
            instances.Clear();
            transparentRenderer.instances.Clear();
            graphics.drawLayers[(int)layer].Add(() =>
            {
                if (instances.Count == 0)
                    return;

                BeginDraw(graphics);

                for (int i = 0; i <= 5; i++)
                {
                    GL.EnableVertexAttribArray(i);
                    GL.VertexAttribDivisor(i, 1);
                }

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2DArray, texture);
                GL.Uniform1(uniform_sampler, (int)0);
                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.DepthTest);

                GL.DrawArraysInstanced(PrimitiveType.TriangleStrip, 0, 4, instances.Count);
                GL.BindVertexArray(0);
            });
        }
    }
}
