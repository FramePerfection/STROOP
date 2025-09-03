using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab.Renderers
{
    public struct MeshInstanceData
    {
        public const int Size = sizeof(float) * 4 * 4 + sizeof(float) * 4;
        public Matrix4 transform;
        public Vector4 color;
    }


    public class GeometryRenderer : InstanceRenderer<MeshInstanceData>, TransparencyRenderer.Transparent
    {
        public class GeometryData
        {
            public Vector4[] vertices;
            public ushort[] indices;
            public ushort[] outlineIndices = new ushort[0];

            public static GeometryData Cylinder(uint resolution = 256)
            {
                const uint FIRST_LOWER_VERTEX = 2;
                uint FIRST_UPPER_VERTEX = 3 + resolution;
                GeometryData data = new GeometryData();
                data.vertices = new Vector4[2 * (2 + resolution)];
                data.vertices[0] = new Vector4(0, 0, 0, 0.5f);
                data.vertices[1] = new Vector4(0, 1, 0, 0.5f);
                for (int i = 0; i <= resolution; i++)
                {
                    var theta = Math.PI * 2.0f * i / resolution;
                    float x = (float)Math.Cos(theta), y = (float)Math.Sin(theta);
                    data.vertices[i + FIRST_LOWER_VERTEX] = new Vector4(x, 0, y, 1);
                    data.vertices[i + FIRST_UPPER_VERTEX] = new Vector4(x, 1, y, 1);
                }

                data.indices = new ushort[3 * 4 * resolution];
                data.outlineIndices = new ushort[resolution * 4 + 2];
                int k = 0;
                for (int i = 0; i < resolution; i++)
                {
                    int baseIndex = i * 3 * 4;
                    ushort a_low = (ushort)(i + FIRST_LOWER_VERTEX), b_low = (ushort)(i + FIRST_LOWER_VERTEX + 1);
                    ushort a_high = (ushort)(i + FIRST_UPPER_VERTEX), b_high = (ushort)(i + FIRST_UPPER_VERTEX + 1);

                    data.indices[baseIndex++] = 1;
                    data.indices[baseIndex++] = a_high;
                    data.indices[baseIndex++] = b_high;

                    data.indices[baseIndex++] = a_high;
                    data.indices[baseIndex++] = b_high;
                    data.indices[baseIndex++] = a_low;

                    data.indices[baseIndex++] = b_high;
                    data.indices[baseIndex++] = a_low;
                    data.indices[baseIndex++] = b_low;

                    data.indices[baseIndex++] = a_low;
                    data.indices[baseIndex++] = b_low;
                    data.indices[baseIndex++] = 0;

                    var outlineEndIndex = (i + 1) % resolution;
                    data.outlineIndices[k++] = (ushort)(2 + i);
                    data.outlineIndices[k++] = (ushort)(2 + outlineEndIndex);
                    data.outlineIndices[k++] = (ushort)(3 + resolution + i);
                    data.outlineIndices[k++] = (ushort)(3 + resolution + outlineEndIndex);
                }

                return data;
            }

            public static GeometryData Sphere(uint resolution, uint rings)
            {
                GeometryData data = new GeometryData();
                data.vertices = new Vector4[resolution * rings + 2];
                data.vertices[0] = new Vector4(0, 1, 0, 0.5f);
                data.vertices[1] = new Vector4(0, -1, 0, 0.5f);
                for (int i = 0; i < rings; i++)
                {
                    var theta = Math.PI * (i + 1) / (rings + 1);
                    float y = (float)Math.Cos(theta), xz = (float)Math.Sin(theta);
                    for (int k = 0; k < resolution; k++)
                    {
                        var phi = Math.PI * 2.0f * k / resolution;
                        float x = xz * (float)Math.Cos(phi), z = xz * (float)Math.Sin(phi);
                        data.vertices[2 + i * resolution + k] = new Vector4(x, y, z, 0.75f + (float)Math.Cos(y * Math.PI / 2) * 0.25f);
                    }
                }

                data.indices = new ushort[2 * 3 * resolution + (rings - 1) * 6 * resolution];
                uint baseIndex = 0;
                for (int k = 0; k < resolution; k++)
                {
                    var endVertex = ((k + 1) % resolution);

                    data.indices[baseIndex++] = 0; //Top pole
                    data.indices[baseIndex++] = (ushort)(2 + k);
                    data.indices[baseIndex++] = (ushort)(2 + endVertex);

                    for (int i = 1; i < rings; i++) //Rings
                    {
                        var baseVertexA = resolution * (i - 1) + 2;
                        var baseVertexB = resolution * i + 2;
                        data.indices[baseIndex++] = (ushort)(baseVertexA + k);
                        data.indices[baseIndex++] = (ushort)(baseVertexA + endVertex);
                        data.indices[baseIndex++] = (ushort)(baseVertexB + k);

                        data.indices[baseIndex++] = (ushort)(baseVertexA + endVertex);
                        data.indices[baseIndex++] = (ushort)(baseVertexB + k);
                        data.indices[baseIndex++] = (ushort)(baseVertexB + endVertex);
                    }

                    uint baseVertex = (rings - 1) * resolution;
                    data.indices[baseIndex++] = 1; //Bottom pole
                    data.indices[baseIndex++] = (ushort)(2 + baseVertex + k);
                    data.indices[baseIndex++] = (ushort)(2 + baseVertex + endVertex);
                }

                data.outlineIndices = new ushort[(resolution * 2) + (rings + 1) * 8];
                baseIndex = 0;
                for (int k = 0; k < resolution; k++)
                {
                    uint baseVertex = rings / 2 * resolution + 2;
                    data.outlineIndices[baseIndex++] = (ushort)(baseVertex + k);
                    data.outlineIndices[baseIndex++] = (ushort)(baseVertex + (k + 1) % resolution);
                }

                Func<float, int, uint> ses = (off, i) => i == -1 ? 0 : (uint)(resolution * (i + off) + 2);
                Func<float, int, uint> sas = (off, i) => i == rings ? 1 : (uint)(resolution * (i + off) + 2);
                for (int i = -1; i < rings; i++)
                {
                    data.outlineIndices[baseIndex++] = (ushort)(ses(0, i));
                    data.outlineIndices[baseIndex++] = (ushort)(sas(0, i + 1));
                    data.outlineIndices[baseIndex++] = (ushort)(ses(0.5f, i));
                    data.outlineIndices[baseIndex++] = (ushort)(sas(0.5f, i + 1));

                    data.outlineIndices[baseIndex++] = (ushort)(ses(0.25f, i));
                    data.outlineIndices[baseIndex++] = (ushort)(sas(0.25f, i + 1));
                    data.outlineIndices[baseIndex++] = (ushort)(ses(0.75f, i));
                    data.outlineIndices[baseIndex++] = (ushort)(sas(0.75f, i + 1));
                }

                return data;
            }
        }


        int maskShader;
        int meshVertices, meshIndices;
        GeometryData data;

        public GeometryRenderer(GeometryData data, int maxExpectedInstances = 256)
        {
            this.data = data;
            AccessScope<MapTab>.content.graphics.DoGLInit(() =>
            {
                maskShader = GraphicsUtil.GetShaderProgram("Resources/Shaders/Meshes.vert.glsl", "Resources/Shaders/DepthMask.frag.glsl");
                shader = GraphicsUtil.GetShaderProgram("Resources/Shaders/Meshes.vert.glsl", "Resources/Shaders/Meshes.frag.glsl");
                Init(maxExpectedInstances);

                GL.BindVertexArray(vertexArray);
                GL.BindBuffer(BufferTarget.ArrayBuffer, instanceBuffer);
                for (int i = 0; i <= 4; i++)
                {
                    GL.EnableVertexAttribArray(i);
                    GL.VertexAttribDivisor(i, 1);
                }
                GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, MeshInstanceData.Size, sizeof(float) * 0);
                GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, MeshInstanceData.Size, sizeof(float) * 4);
                GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, MeshInstanceData.Size, sizeof(float) * 8);
                GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, MeshInstanceData.Size, sizeof(float) * 12);
                GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, MeshInstanceData.Size, sizeof(float) * 16);

                meshVertices = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, meshVertices);
                GL.EnableVertexAttribArray(5);
                GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * 4 * data.vertices.Length), data.vertices, BufferUsageHint.StaticDraw);

                meshIndices = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, meshIndices);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(ushort) * data.indices.Length), data.indices, BufferUsageHint.StaticDraw);

                GL.BindVertexArray(0);
            });
        }

        public void Add(Matrix4 transform, Vector4 color) =>
            instances.Add(new MeshInstanceData() { transform = transform, color = color });

        public void DrawMask(TransparencyRenderer renderer)
        {
            GL.UseProgram(maskShader);
            GL.BindVertexArray(vertexArray);
            Matrix4 mat = renderer.graphics.ViewMatrix;
            GL.UniformMatrix4(GL.GetUniformLocation(maskShader, "viewProjection"), false, ref mat);
            renderer.SetUniforms(maskShader);
            DrawGeometry();
        }

        public void DrawTransparent(TransparencyRenderer renderer)
        {
            GL.UseProgram(shader);
            GL.BindVertexArray(vertexArray);
            Matrix4 mat = renderer.graphics.ViewMatrix;
            GL.UniformMatrix4(GL.GetUniformLocation(shader, "viewProjection"), false, ref mat);
            DrawGeometry();
        }

        public void Prepare(TransparencyRenderer renderer)
        {
            UpdateBuffer(instances.Count);
        }

        void DrawGeometry()
        {
            GL.BindVertexArray(vertexArray);
            GL.DrawElementsInstanced(PrimitiveType.Triangles, data.indices.Length, DrawElementsType.UnsignedShort, IntPtr.Zero, instances.Count);
        }

        public override void SetDrawCalls(MapGraphics graphics)
        {
            instances.Clear();
            if (graphics.view.drawCylinderOutlines)
                graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffersRedirect].Add(() =>
                {
                    foreach (var instance in instances)
                        for (int i = 0; i < data.outlineIndices.Length; i += 2)
                            graphics.lineRenderer.Add(
                                Vector3.TransformPosition(data.vertices[data.outlineIndices[i]].Xyz, instance.transform),
                                Vector3.TransformPosition(data.vertices[data.outlineIndices[i + 1]].Xyz, instance.transform),
                                new Vector4(0, 0, 0, 1),
                                2);
                });
        }
    }
}
