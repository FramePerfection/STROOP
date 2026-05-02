using OpenTK.Graphics.OpenGL;
using OpenTK;
using System;
using System.Collections.Generic;
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
        protected IntPtr dataPtr { get; private set; }

        public int uniform_viewProjection { get; private set; }

        protected int shader;
        protected int instanceBuffer, vertexArray;
        public bool ignoreView = false;

        public InstanceRenderer()
        {
            instanceSize = Marshal.SizeOf(typeof(InstanceData));
        }

        protected void WriteDataToBuffer()
        {
            IntPtr ptr = dataPtr;
            foreach (var instance in instances)
            {
                Marshal.StructureToPtr(instance, ptr, false);
                ptr = IntPtr.Add(ptr, instanceSize);
            }
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
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataPtr);
                dataPtr = Marshal.AllocHGlobal(bufferSize);
                this.maxInstances = maxInstances;
                if (writeData)
                    WriteDataToBuffer();
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)bufferSize, writeData ? dataPtr : IntPtr.Zero, BufferUsageHint.StreamDraw);
            }
            else
            {
                WriteDataToBuffer();
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(instanceSize * instances.Count), dataPtr);
            }
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
