using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Input;
using OpenTK.GLControl;
using OpenTK.Mathematics;
using STROOP.Utilities;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace STROOP.Controls
{
    public class ModelGraphics
    {
        volatile float _cameraAngle = 0;
        volatile float _cameraRadius = 0;
        volatile float _cameraHeight = 0;

        Vector3 _cameraPosition;
        Vector3 _cameraLook;
        float _cameraManualAngleLat;
        float _cameraManualAngleLong;

        Vector3 _modelCenter;
        float _modelRadius;
        float _zoom = 1.0f;
        float _pov = 90f; // Calculated from Zoom

        public RectangleF MapView;
        public GLControl Control;
        Timer _timer;
        float _speedMul;

        public bool ManualMode = false;

        public ModelGraphics(GLControl control)
        {
            Control = control;
            _timer = new Timer();
            _timer.Interval = 1000 / 60;
            _timer.Tick += _timer_Tick;

            control.MouseWheel += HandleMouseWheel;
            EventHandler disposeHandler = null;
            disposeHandler = (_, _) =>
            {
                control.Disposed -= disposeHandler;
                control.MouseWheel -= HandleMouseWheel;
            };
            control.Disposed += disposeHandler;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (!ManualMode)
            {
                float speed = 0.01f;
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    speed = 0.0f;
                else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    speed = 0.03f;
                else if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                    speed = 0.003f;

                _cameraAngle += speed;
            }

            CameraFly();
        }

        public void Load()
        {
            Control.MakeCurrent();

            Control.Paint += OnPaint;
            Control.Resize += OnResize;
            Control.MouseDown += OnMouseDown;
            Control.MouseMove += OnMouseMove;
            Control.MouseUp += OnMouseUp;

            GL.ClearColor(Color.FromKnownColor(KnownColor.Control));
            GL.Enable(EnableCap.DepthTest);

            _timer.Enabled = true;

            SetupViewport();
        }

        bool _mouseDown = false;

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            _mouseDown = true;
            _pMouseCoords = new Vector2(e.X, e.Y);
            _mouseCoords = new Vector2(e.X, e.Y);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            _mouseCoords = new Vector2(e.X, e.Y);
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
            => _mouseDown = false;

        Vector2 _pMouseCoords, _mouseCoords;
        float? _pMouseScroll = null;

        public void CameraFly()
        {
            // Calculate key speed multiplier
            _speedMul = 1f;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                _speedMul = 0.0f;
            else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                _speedMul = 3.0f;
            else if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                _speedMul = 0.3f;

            // Handle mouse
            if (_mouseDown)
            {
                // Reset previous coordinates so no movement occurs during the initial press

                // Calcualte mouse delta
                Vector2 delta = _mouseCoords - _pMouseCoords;
                _pMouseCoords = _mouseCoords;

                // Add speed multiplier
                delta *= _speedMul * 0.009f;
                delta *= _pov / 90;

                // Trackball (add mouse deltas to angle)
                _cameraManualAngleLat += delta.X;
                _cameraManualAngleLong += -delta.Y;

                if (_cameraManualAngleLong > Math.PI / 2 - 0.001f)
                {
                    _cameraManualAngleLong = (float)(Math.PI / 2) - 0.001f;
                }
                else if (_cameraManualAngleLong < -Math.PI / 2 + 0.001f)
                {
                    _cameraManualAngleLong = (float)(-Math.PI / 2) + 0.001f;
                }

                ManualMode = true;
            }

            Vector3 relDeltaPos = new Vector3(0, 0, 0);
            float posSpeed = _speedMul * _modelRadius * 0.01f; // Move at a rate relative to the model size

            // Handle Positional Movement 
            if (Keyboard.IsKeyDown(Key.W) || Keyboard.IsKeyDown(Key.Up))
            {
                relDeltaPos.Z += posSpeed;
                ManualMode = true;
            }

            if (Keyboard.IsKeyDown(Key.A) || Keyboard.IsKeyDown(Key.Left))
            {
                relDeltaPos.X += posSpeed;
                ManualMode = true;
            }

            if (Keyboard.IsKeyDown(Key.S) || Keyboard.IsKeyDown(Key.Down))
            {
                relDeltaPos.Z += -posSpeed;
                ManualMode = true;
            }

            if (Keyboard.IsKeyDown(Key.D) || Keyboard.IsKeyDown(Key.Right))
            {
                relDeltaPos.X += -posSpeed;
                ManualMode = true;
            }

            if (Keyboard.IsKeyDown(Key.Q))
            {
                relDeltaPos.Y += -posSpeed;
                ManualMode = true;
            }

            if (Keyboard.IsKeyDown(Key.E))
            {
                relDeltaPos.Y += posSpeed;
                ManualMode = true;
            }

            // Update camera position
            // This requires converting the coordinate system from the camera coordinates 
            // to the world coordinates. The camera X unit is calculate from the 
            // cross product of the camera Y unit and the camera Z unit. The camera
            // Y unit is the world Y unit since the Y coordinate is always up.
            // The Z unit is the normalized camera look vector (to move towards the look),
            // Hence, move formard.
            _cameraPosition += Vector3.Cross(Vector3.UnitY, _cameraLook) * relDeltaPos.X
                               + Vector3.UnitY * relDeltaPos.Y
                               + _cameraLook * relDeltaPos.Z;
        }

        private void HandleMouseWheel(object sender, MouseEventArgs e)
        {
            float deltaScroll = e.Delta / (float)Mouse.MouseWheelDeltaForOneLine;
            _zoom += deltaScroll * 0.1f * _speedMul;
        }

        public void OnPaint(object sender, EventArgs e)
        {
            Control.MakeCurrent();

            // Set default background color (clear drawing area)
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.Black);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.DepthRange(0.0, 1.0f);

            SetupViewport();

            if (ManualMode)
            {
                // Convert the long. and lat. angles into a camera look vector
                _cameraLook.Y = (float)(Math.Sin(_cameraManualAngleLong));
                float yy = (float)Math.Sqrt(1 - _cameraLook.Y * _cameraLook.Y);
                _cameraLook.X = (float)Math.Cos(_cameraManualAngleLat) * yy;
                _cameraLook.Z = (float)Math.Sin(_cameraManualAngleLat) * yy;
            }
            else
            {
                // Rotate around model
                _cameraPosition = new Vector3((float)(_cameraRadius * Math.Cos(_cameraAngle)),
                    _cameraHeight, (float)(_cameraRadius * Math.Sin(_cameraAngle)));
                _cameraLook = (_modelCenter - _cameraPosition).Normalized();

                // Update the long. and lat. angles for switching to manual mode
                _cameraManualAngleLat = (float)Math.Atan2(_cameraLook.Z, _cameraLook.X);
                _cameraManualAngleLong = (float)Math.Asin(_cameraLook.Y);
            }

            _pov = (float)(90f + Math.Atan(_zoom) * 180f / Math.PI);
            SetLookAtCamera(_cameraPosition, _cameraPosition + _cameraLook);
            DrawModel();

            Control.SwapBuffers();
        }

        public void OnResize(object sender, EventArgs e)
        {
            Control.MakeCurrent();
            SetupViewport();
        }

        private void SetupViewport()
        {
            int w = Control.Width;
            int h = Control.Height;

            GL.Viewport(0, 0, w, h); // Use all of the glControl painting area\

            SetPerspectiveProjection(w, h, _pov);
        }

        public Color ColorFromTri(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            float normY = Vector3.Cross(v2 - v1, v3 - v1).Normalized().Y;
            // Floor
            if (normY > 0.01)
                return Color.LightBlue;
            // Ceiling
            else if (normY < -0.01)
                return Color.Pink;
            // Wall   
            else
                return Color.LightGreen;
        }

        private void DrawModel()
        {
            lock (_modelLock)
            {
                // Draw triangles
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.Begin(PrimitiveType.Triangles);
                for (int i = 0; i < _triangles.Length; i++)
                {
                    if (!_triangleSelected[i])
                        continue;

                    var t = _triangles[i];

                    GL.Color3(_triangleColors[i]);
                    GL.Vertex3(_vertices[t[0]]);
                    GL.Vertex3(_vertices[t[1]]);
                    GL.Vertex3(_vertices[t[2]]);
                }

                GL.End();

                // Draw lines
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                GL.LineWidth(3.0f);
                GL.Begin(PrimitiveType.Triangles);
                for (int i = 0; i < _triangles.Length; i++)
                {
                    var t = _triangles[i];

                    GL.Color3(Color.Blue);
                    GL.Vertex3(_vertices[t[0]]);
                    GL.Vertex3(_vertices[t[1]]);
                    GL.Vertex3(_vertices[t[2]]);
                }

                GL.End();

                // Draw vertices
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
                GL.PointSize(8.0f);
                GL.Color3(Color.Yellow);
                GL.Begin(PrimitiveType.Points);
                for (int i = 0; i < _vertices.Length; i++)
                {
                    var v = _vertices[i];

                    // Only show vertices that are selected
                    if (!_vertexSelected[i])
                        continue;

                    GL.Vertex3(v);
                }

                GL.End();
            }
        }

        Random rng = new Random();

        Vector3[] _vertices = new Vector3[0];
        Color[] _triangleColors = new Color[0];
        int[][] _triangles = new int[0][];
        bool[] _triangleSelected = new bool[0];
        bool[] _vertexSelected = new bool[0];
        object _modelLock = new object();

        public void ClearModel()
        {
            _vertices = new Vector3[0];
            _triangleColors = new Color[0];
            _triangles = new int[0][];
        }

        public void ChangeModel(List<short[]> vertices, List<int[]> triangles)
        {
            ManualMode = false;

            var maxRadius = vertices.Max(v => MoreMath.GetDistanceBetween(v[0], v[2], 0, 0));
            var maxHeight = vertices.Max(v => v[1]);
            var minHeight = vertices.Min(v => v[1]);

            _cameraHeight = maxHeight + (float)(Math.Sqrt(2) * maxRadius);
            _cameraRadius = (float)maxRadius * 2f;

            _modelCenter = new Vector3(0, (maxHeight + minHeight) / 2, 0);
            _modelRadius = vertices.Max(v => (new Vector3(v[0], v[1], v[2]) - _modelCenter).Length);

            _zoom = -0.57735026919f; // 60 degree FOV

            lock (_modelLock)
            {
                // Create vertice point vectors
                _vertices = new Vector3[vertices.Count];
                for (int i = 0; i < _vertices.Length; i++)
                {
                    _vertices[i] = new Vector3(vertices[i][0], vertices[i][1], vertices[i][2]);
                }

                // Create triangle
                _triangles = new int[triangles.Count][];
                _triangleColors = new Color[triangles.Count];
                for (int i = 0; i < _triangles.Length; i++)
                {
                    // Make sure vertices exist
                    _triangles[i] = triangles[i].Select(t => t >= _vertices.Length || t < 0 ? 0 : t).ToArray();
                    // Find triangle colors
                    var tri = _triangles[i];
                    _triangleColors[i] = ColorFromTri(_vertices[tri[0]], _vertices[tri[1]], _vertices[tri[2]]);
                }

                // Unselect all triangle and vertices
                _vertexSelected = new bool[_vertices.Length];
                _triangleSelected = new bool[_triangles.Length];
            }
        }

        public void ChangeVertexSelection(bool[] vertexSelected)
        {
            lock (_modelLock)
            {
                for (int i = 0; i < vertexSelected.Length && i < _vertexSelected.Length; i++)
                    _vertexSelected[i] = vertexSelected[i];
            }
        }

        public void ChangeTriangleSelection(bool[] triangleSelected)
        {
            lock (_modelLock)
            {
                for (int i = 0; i < triangleSelected.Length && i < _triangleSelected.Length; i++)
                    _triangleSelected[i] = triangleSelected[i];
            }
        }

        private void SetPerspectiveProjection(int width, int height, float FOV)
        {
            var projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI * (FOV / 180f), width / (float)height, 10f, 100000.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projectionMatrix);
        }

        private void SetLookAtCamera(Vector3 position, Vector3 target)
        {
            var modelViewMatrix = Matrix4.LookAt(position, target, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelViewMatrix);
        }
    }
}