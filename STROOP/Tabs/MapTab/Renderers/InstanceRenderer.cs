using OpenTK.Graphics.OpenGL;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using STROOP.Core;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab.Renderers
{
    public abstract class InstanceRenderer<InstanceData> : Renderer where InstanceData : struct
    {
        protected List<InstanceData> instances = new List<InstanceData>();
        protected readonly int instanceSize;
        protected int maxInstances { get; private set; }

        public int uniform_viewProjection { get; private set; }

        protected int shader;
        protected int instanceBuffer, vertexArray;
        public bool ignoreView = false;

        public InstanceRenderer()
        {
            instanceSize = Marshal.SizeOf(typeof(InstanceData));
            if (instanceSize != Unsafe.SizeOf<InstanceData>())
                throw new InvalidOperationException($"{typeof(InstanceData)} is not blittable");
        }

        void WriteDataToBuffer()
        {
            if (instances.Count > 0)
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, instanceSize * instances.Count, ref CollectionsMarshal.AsSpan(instances)[0]);
        }

        protected void Init(int maxExpectedInstances)
        {
            AccessScope<MapTab>.content.graphics.DoGLInit(() =>
            {
                uniform_viewProjection = GL.GetUniformLocation(shader, "viewProjection");
                instanceBuffer = GL.GenBuffer();
                vertexArray = GL.GenVertexArray();
                UpdateBuffer(maxExpectedInstances, false);
            });
        }

        protected void UpdateBuffer(int maxInstances, bool writeData = true)
        {
            int bufferSize = instanceSize * maxInstances;
            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceBuffer);

            if (maxInstances > this.maxInstances)
            {
                this.maxInstances = maxInstances;
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)bufferSize, IntPtr.Zero, BufferUsageHint.StreamDraw);
            }

            if (writeData)
                WriteDataToBuffer();
        }

        protected void BeginDraw(MapGraphics graphics, bool updateBuffer = true)
        {
            if (updateBuffer)
                UpdateBuffer(instances.Count);

            GL.UseProgram(shader);
            GL.BindVertexArray(vertexArray);
            Matrix4 mat = ignoreView ? Matrix4.Identity : graphics.ViewMatrix;
            GL.UniformMatrix4(uniform_viewProjection, false, ref mat);
        }
    }
}
