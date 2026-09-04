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
                if (triangles.Count == 0)
                    return;
                GL.UseProgram(shader);
                renderer.SetUniforms(shader);
                DrawTriangles(renderer.graphics, shader);
            }

            public void DrawTransparent(TransparencyRenderer renderer)
            {
                if (triangles.Count == 0)
                    return;
                DrawTriangles(renderer.graphics, parent.shader);
            }

            public void Prepare(TransparencyRenderer renderer)
            {
                if (triangles.Count == 0)
                    return;
                WriteDataToBuffer();
            }
        }

        struct TriangleVertex
        {
            public const int Size = sizeof(float) * 4 + sizeof(float) * 4 + sizeof(float) * 4 + sizeof(float) * 3;
            internal Vector3 position;
            internal float showUnitSquares;
            internal Vector4 color;
            internal Vector4 outlineColor;
            internal Vector3 outlineThickness;
        }

        struct Triangle
        {
            public Vector3[] positions;
            public Vector4[] colors;
            public bool showUnitSquares;
            public Vector4 outlineColor;
            public Vector3 outlineThickness;
        }

        int shader;
        int buffer;
        int vao;

        int bufferSize = 0;
        IntPtr dataPtr;

        List<Triangle> triangles = new List<Triangle>();

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

            dataPtr = Marshal.AllocHGlobal((IntPtr)(bufferSize = expectedSize));
        }

        public override void SetDrawCalls(MapGraphics graphics)
        {
            triangles.Clear();
            transparentRenderer.triangles.Clear();
            graphics.drawLayers[(int)drawlayer].Add(() =>
            {
                if (triangles.Count == 0)
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
            var error = GL.GetError();
            GL.UseProgram(program);
            GL.BindVertexArray(vao);
            var mat = graphics.ViewMatrix;
            GL.UniformMatrix4(GL.GetUniformLocation(program, "viewProjection"), false, ref mat);
            GL.Uniform2(uniform_pixelsPerUnit, ref pixelsPerUnit);
            GL.Uniform1(uniform_unitShift, (uint)Structs.Configurations.SpecialConfig.ExtBoundariesShift);

            GL.Uniform2(GL.GetUniformLocation(program, "gridOffset"), new Vector2(0));

            GL.Disable(EnableCap.CullFace);
            GL.DrawArrays(PrimitiveType.Triangles, 0, triangles.Count * 3);
            error = GL.GetError();
            GL.BindVertexArray(0);
        }

        public void Add(Vector3 v1, Vector3 v2, Vector3 v3, bool showTriUnits, Vector4 color, Vector4 outlineColor, float outlineThickness, bool transparent) =>
            Add(v1, v2, v3, showTriUnits, color, outlineColor, new Vector3(outlineThickness), transparent);

        public void Add(Vector3 v1, Vector3 v2, Vector3 v3, bool showTriUnits, Vector4 color, Vector4 outlineColor, Vector3 outlineThickness, bool transparent) =>
            Add(v1, v2, v3, showTriUnits, color, color, color, outlineColor, outlineThickness, transparent);

        public void Add(Vector3 v1, Vector3 v2, Vector3 v3, bool showTriUnits, Vector4 color1, Vector4 color2, Vector4 color3, Vector4 outlineColor, Vector3 outlineThickness, bool transparent)
        {
            (transparent ? transparentRenderer.triangles : triangles).Add(new Triangle
            {
                positions = new[] { v1, v2, v3 },
                colors = new[] { color1, color2, color3 },
                outlineColor = outlineColor,
                showUnitSquares = showTriUnits,
                outlineThickness = outlineThickness
            });
        }

        void WriteDataToBuffer()
        {
            var dataSize = triangles.Count * TriangleVertex.Size * 3;
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
            if (dataSize > bufferSize)
            {
                Marshal.FreeHGlobal(dataPtr);
                dataPtr = Marshal.AllocHGlobal((IntPtr)(bufferSize = Math.Max(dataSize, bufferSize * 2)));
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)bufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }

            IntPtr ptr = dataPtr;
            foreach (var instance in triangles)
            {
                for (int i = 0; i < 3; i++)
                {
                    Marshal.StructureToPtr(new TriangleVertex
                    {
                        position = instance.positions[i],
                        color = instance.colors[i],
                        outlineColor = instance.outlineColor,
                        showUnitSquares = instance.showUnitSquares ? 1.0f : 0.0f,
                        outlineThickness = instance.outlineThickness
                    }, ptr, false);
                    ptr = IntPtr.Add(ptr, TriangleVertex.Size);
                }
            }

            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(dataSize), dataPtr);
        }
    }
}
