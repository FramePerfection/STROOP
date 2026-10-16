using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using OpenTK.Mathematics;
using STROOP.Core;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab.Renderers
{
    public class TriangleRenderer : Renderer
    {
        class TransparentTriangleRenderer : TriangleRenderer, TransparencyRenderer.Transparent
        {
            TriangleRenderer parent;

            protected override int GetShader() => GraphicsUtil.GetShaderProgram("Resources/Shaders/Triangles.vert.glsl", "Resources/Shaders/DepthMask.frag.glsl", "Resources/Shaders/Triangles.geom.glsl");

            public TransparentTriangleRenderer(TriangleRenderer parent) : base()
            {
                this.parent = parent;
            }

            public void DrawMask(TransparencyRenderer renderer)
            {
                if (vertices.Count == 0)
                    return;
                GL.UseProgram(shader);
                renderer.SetUniforms(shader);
                DrawTriangles(renderer.graphics, shader);
            }

            public void DrawTransparent(TransparencyRenderer renderer)
            {
                if (vertices.Count == 0)
                    return;
                DrawTriangles(renderer.graphics, parent.shader);
            }

            public void Prepare(TransparencyRenderer renderer)
            {
                if (vertices.Count == 0)
                    return;
                WriteDataToBuffer();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct TriangleVertex
        {
            public const int Size = sizeof(float) * 4 + sizeof(float) * 4 + sizeof(float) * 4 + sizeof(float) * 3;
            internal Vector3 position;
            internal float showUnitSquares;
            internal Vector4 color;
            internal Vector4 outlineColor;
            internal Vector3 outlineThickness;
        }

        int shader;
        int buffer;
        int vao;

        int bufferSize = 0;

        readonly List<TriangleVertex> vertices = new List<TriangleVertex>();

        int uniform_viewProjection, uniform_pixelsPerUnit, uniform_unitShift;

        protected virtual int GetShader() => GraphicsUtil.GetShaderProgram(
            "Resources/Shaders/Triangles.vert.glsl",
            "Resources/Shaders/Triangles.frag.glsl",
            "Resources/Shaders/Triangles.geom.glsl");

        TransparentTriangleRenderer transparentRenderer;
        public TransparencyRenderer.Transparent transparent => transparentRenderer;
        public MapGraphics.DrawLayers drawlayer = MapGraphics.DrawLayers.Geometry;

        protected TriangleRenderer()
        {
        }

        public TriangleRenderer(int maxExpectedTriangles)
        {
            InitInternal(maxExpectedTriangles);
            transparentRenderer = new TransparentTriangleRenderer(this);
            transparentRenderer.InitInternal(maxExpectedTriangles);
        }

        void InitInternal(int maxExpectedTriangles)
        {
            int expectedSize = maxExpectedTriangles * TriangleVertex.Size * 3;
            AccessScope<MapTab>.content.graphics.DoGLInit(() =>
            {
                vao = GL.GenVertexArray();
                buffer = GL.GenBuffer();

                GL.BindVertexArray(vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)expectedSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.EnableVertexAttribArray(2);
                GL.EnableVertexAttribArray(3);
                GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, TriangleVertex.Size, 0);
                GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, TriangleVertex.Size, sizeof(float) * 4);
                GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, TriangleVertex.Size, sizeof(float) * 8);
                GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, TriangleVertex.Size, sizeof(float) * 12);

                GL.BindVertexArray(0);

                shader = GetShader();
                uniform_viewProjection = GL.GetUniformLocation(shader, "viewProjection");
                uniform_pixelsPerUnit = GL.GetUniformLocation(shader, "pixelsPerUnit");
                uniform_unitShift = GL.GetUniformLocation(shader, "unitShift");
            });

            bufferSize = expectedSize;
        }

        public override void SetDrawCalls(MapGraphics graphics)
        {
            vertices.Clear();
            transparentRenderer.vertices.Clear();
            graphics.drawLayers[(int)drawlayer].Add(() =>
            {
                if (vertices.Count == 0)
                    return;

                WriteDataToBuffer();
                if (graphics.viewMode == MapGraphics.ViewMode.ThreeDimensional)
                {
                    GL.Enable(EnableCap.DepthTest);
                    GL.DepthFunc(DepthFunction.Lequal);
                }

                DrawTriangles(graphics, shader);
            });
        }

        protected void DrawTriangles(MapGraphics graphics, int program)
        {
            Vector2 pixelsPerUnit = graphics.pixelsPerUnit;
            GL.UseProgram(program);
            GL.BindVertexArray(vao);
            var mat = graphics.ViewMatrix;
            GL.UniformMatrix4(GL.GetUniformLocation(program, "viewProjection"), false, ref mat);
            GL.Uniform2(uniform_pixelsPerUnit, ref pixelsPerUnit);
            GL.Uniform1(uniform_unitShift, (uint)Structs.Configurations.SpecialConfig.ExtBoundariesShift);

            GL.Uniform2(GL.GetUniformLocation(program, "gridOffset"), new Vector2(0));

            GL.Disable(EnableCap.CullFace);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Count);
            GL.BindVertexArray(0);
        }

        public void Add(Vector3 v1, Vector3 v2, Vector3 v3, bool showTriUnits, Vector4 color, Vector4 outlineColor, float outlineThickness, bool transparent) =>
            Add(v1, v2, v3, showTriUnits, color, outlineColor, new Vector3(outlineThickness), transparent);

        public void Add(Vector3 v1, Vector3 v2, Vector3 v3, bool showTriUnits, Vector4 color, Vector4 outlineColor, Vector3 outlineThickness, bool transparent) =>
            Add(v1, v2, v3, showTriUnits, color, color, color, outlineColor, outlineThickness, transparent);

        public void Add(Vector3 v1, Vector3 v2, Vector3 v3, bool showTriUnits, Vector4 color1, Vector4 color2, Vector4 color3, Vector4 outlineColor, Vector3 outlineThickness, bool transparent)
        {
            var target = transparent ? transparentRenderer.vertices : vertices;
            float showUnitSquares = showTriUnits ? 1.0f : 0.0f;
            target.Add(new TriangleVertex { position = v1, color = color1, outlineColor = outlineColor, showUnitSquares = showUnitSquares, outlineThickness = outlineThickness });
            target.Add(new TriangleVertex { position = v2, color = color2, outlineColor = outlineColor, showUnitSquares = showUnitSquares, outlineThickness = outlineThickness });
            target.Add(new TriangleVertex { position = v3, color = color3, outlineColor = outlineColor, showUnitSquares = showUnitSquares, outlineThickness = outlineThickness });
        }

        void WriteDataToBuffer()
        {
            var dataSize = vertices.Count * TriangleVertex.Size;
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
            if (dataSize > bufferSize)
            {
                bufferSize = Math.Max(dataSize, bufferSize * 2);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)bufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }

            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, dataSize, ref CollectionsMarshal.AsSpan(vertices)[0]);
        }
    }
}
