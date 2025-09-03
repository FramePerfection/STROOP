using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab.Renderers
{
    public class ShapeRenderer : InstanceRenderer<ShapeRenderer.InstanceData>
    {
        class TransparentShapeRenderer : ShapeRenderer, TransparencyRenderer.Transparent
        {
            ShapeRenderer parent;

            protected override int GetShader() => GraphicsUtil.GetShaderProgram("Resources/Shaders/Circles.vert.glsl", "Resources/Shaders/DepthMask.frag.glsl");
            public TransparentShapeRenderer(ShapeRenderer parent, int maxExpectedInstances) : base(maxExpectedInstances)
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
            public Matrix4 transform;
            public Vector4 color;
            public Vector4 outlineColor;
            public float shape;
        }

        public enum Shapes
        {
            Circle = 0,
            Quad = 1
        }

        public readonly MapGraphics.DrawLayers layer = MapGraphics.DrawLayers.Overlay;

        TransparentShapeRenderer transparentRenderer;
        public TransparencyRenderer.Transparent transparent => transparentRenderer;

        protected virtual int GetShader() => GraphicsUtil.GetShaderProgram("Resources/Shaders/Circles.vert.glsl", "Resources/Shaders/Circles.frag.glsl");

        protected ShapeRenderer(int numExpectedInstances = 512)
        {
            AccessScope<MapTab>.content.graphics.DoGLInit(() =>
            {
                shader = GetShader();
                Init(numExpectedInstances);

                GL.BindVertexArray(vertexArray);
                GL.BindBuffer(BufferTarget.ArrayBuffer, instanceBuffer);
                for (int i = 0; i <= 5; i++)
                {
                    GL.EnableVertexAttribArray(i);
                    GL.VertexAttribDivisor(i, 1);
                    GL.VertexAttribPointer(i, 4, VertexAttribPointerType.Float, false, instanceSize, sizeof(float) * i * 4);
                }
                GL.EnableVertexAttribArray(6);
                GL.VertexAttribDivisor(6, 1);
                GL.VertexAttribPointer(6, 1, VertexAttribPointerType.Float, false, instanceSize, sizeof(float) * 6 * 4);
                GL.BindVertexArray(0);
            });
        }

        public ShapeRenderer(MapGraphics.DrawLayers layer, int maxExpectedInstances = 128) : this(maxExpectedInstances)
        {
            this.layer = layer;
            transparentRenderer = new TransparentShapeRenderer(this, maxExpectedInstances);
        }

        public void AddInstance(bool sortTransparent, Matrix4 transform, float outlineWidth, Vector4 color, Vector4 outlineColor, Shapes shape = Shapes.Circle)
        {
            (sortTransparent ? transparentRenderer.instances : instances).Add(new InstanceData
            {
                transform = transform,
                color = color,
                outlineColor = new Vector4(outlineColor.X, outlineColor.Y, outlineColor.Z, outlineWidth),
                shape = (float)(int)shape
            });
        }

        public override void SetDrawCalls(MapGraphics graphics)
        {
            instances.Clear();
            transparentRenderer.instances.Clear();
            graphics.drawLayers[(int)layer].Add(() =>
           {
               if (instances.Count == 0)
                   return;

               BeginDraw(graphics);
               GL.Disable(EnableCap.CullFace);
               GL.Disable(EnableCap.DepthTest);

               GL.DrawArraysInstanced(PrimitiveType.TriangleStrip, 0, 4, instances.Count);
               GL.BindVertexArray(0);
           });
        }
    }
}
