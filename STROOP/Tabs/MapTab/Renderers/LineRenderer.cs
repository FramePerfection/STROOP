using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab.Renderers
{
    public class LineRenderer : InstanceRenderer<LineRenderer.InstanceData>
    {
        public struct InstanceData
        {
            public const int Size = sizeof(float) * 4 + sizeof(float) * 4;
            public Vector3 position;
            public float width;
            public Vector4 color;
        }

        int uniform_viewportSize;

        public LineRenderer(int numExpectedLines = 1024)
        {
            AccessScope<MapTab>.content.graphics.DoGLInit(() =>
            {
                shader = GraphicsUtil.GetShaderProgram("Resources/Shaders/Lines.vert.glsl", "Resources/Shaders/Lines.frag.glsl", "Resources/Shaders/Lines.geom.glsl");
                Init(numExpectedLines * 2);
                uniform_viewportSize = GL.GetUniformLocation(shader, "viewportSize");

                GL.BindVertexArray(vertexArray);
                GL.BindBuffer(BufferTarget.ArrayBuffer, instanceBuffer);
                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, InstanceData.Size, sizeof(float) * 0);
                GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, InstanceData.Size, sizeof(float) * 4);

                GL.BindVertexArray(0);

                UpdateBuffer(numExpectedLines, false);
            });
        }

        public void Add(Vector3 pos1, Vector3 pos2, Vector4 color, float thickness)
        {
            instances.Add(new InstanceData()
            {
                position = pos1,
                color = color,
                width = thickness
            });
            instances.Add(new InstanceData()
            {
                position = pos2,
                color = color,
                width = thickness
            });
        }

        public void AddArrow(float x, float y, float z, float size, float yaw, float _arrowHeadSideLength, Vector4 color, float thickness)
        {
            (float arrowHeadX, float arrowHeadZ) =
                ((float, float))MoreMath.AddVectorToPoint(size, yaw, x, z);

            (float pointSide1X, float pointSide1Z) =
                ((float, float))MoreMath.AddVectorToPoint(_arrowHeadSideLength, yaw + 32768 + 8192, arrowHeadX, arrowHeadZ);
            (float pointSide2X, float pointSide2Z) =
                ((float, float))MoreMath.AddVectorToPoint(_arrowHeadSideLength, yaw + 32768 - 8192, arrowHeadX, arrowHeadZ);

            Add(new Vector3(x, y, z), new Vector3(arrowHeadX, y, arrowHeadZ), color, thickness);
            Add(new Vector3(arrowHeadX, y, arrowHeadZ), new Vector3(pointSide1X, y, pointSide1Z), color, thickness);
            Add(new Vector3(arrowHeadX, y, arrowHeadZ), new Vector3(pointSide2X, y, pointSide2Z), color, thickness);
        }

        public override void SetDrawCalls(MapGraphics graphics)
        {
            instances.Clear();
            graphics.drawLayers[(int)MapGraphics.DrawLayers.Geometry].Add(() =>
            {
                if (instances.Count == 0)
                    return;
                UpdateBuffer(instances.Count);
                BeginDraw(graphics);
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Lequal);
                Vector2 viewportSize = new Vector2(graphics.glControl.Width, graphics.glControl.Height);
                GL.Uniform2(uniform_viewportSize, viewportSize);

                GL.DrawArrays(PrimitiveType.Lines, 0, instances.Count);
                GL.BindVertexArray(0);
            });
        }
    }
}
