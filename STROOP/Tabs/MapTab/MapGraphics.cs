using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Drawing;
using OpenTK.GLControl;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using STROOP.Controls;
using STROOP.Core;
using STROOP.Extensions;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Tabs.MapTab.Views;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab
{
    public partial class MapGraphics
    {
        public enum DrawLayers
        {
            FillBuffers,
            FillBuffersRedirect,
            BakeText,
            Background,
            Geometry,
            Transparency,
            GeometryOverlay,
            Objects,
            Overlay,
        }

        public enum ViewMode
        {
            TopDown,
            Orthogonal,
            ThreeDimensional
        }

        static Vector3 ProjectOnLineSegment(Vector3 p, Vector3 A, Vector3 B)
        {
            Vector3 d = B - A;
            float distThing = Vector3.Dot(p - A, d) / Vector3.Dot(d, d);
            return A + d * Math.Max(0, Math.Min(1, distThing));
        }

        public bool HoverTopDown(Vector3 position, float radius) =>
            (position.Xz - mapCursorPosition.Xz).LengthSquared < radius * radius;

        public bool HoverOrthogonal(Vector3 position, float radius)
        {
            var projectedPos = Vector3.TransformPosition(position, ViewMatrix);
            projectedPos.X = (1 + projectedPos.X) * glControl.Width / 2;
            projectedPos.Y = (1 - projectedPos.Y) * glControl.Height / 2;
            return (projectedPos.Xy - mousePosition2D).LengthSquared < (radius * radius);
        }

        public readonly List<Action>[] drawLayers;

        public Renderers.RendererCollection rendererCollection { get; private set; }
        public Renderers.SpriteRenderer objectRenderer => rendererCollection.objectRenderer;
        public Renderers.TriangleRenderer triangleRenderer => rendererCollection.triangleRenderer;
        public Renderers.TriangleRenderer triangleOverlayRenderer => rendererCollection.triangleOverlayRenderer;
        public Renderers.LineRenderer lineRenderer => rendererCollection.lineRenderer;
        public Renderers.ShapeRenderer circleRenderer => rendererCollection.circleRenderer;
        public Renderers.GeometryRenderer cylinderRenderer => rendererCollection.cylinderRenderer;
        public Renderers.GeometryRenderer sphereRenderer => rendererCollection.sphereRenderer;
        public Renderers.TextRenderer textRenderer => rendererCollection.textRenderer;
        public Renderers.TransparencyRenderer transparencyRenderer;
        public Vector2 pixelsPerUnit { get; private set; }

        public Models.TriangleDataModel hoverTriangle;

        public int emptyVAO { get; private set; }

        int mainFrameBuffer, mainColorBuffer, mainDepthBuffer;

        // Only used when rendering into a foreign (shared) context - see OnPaint. This FBO lives in
        // THIS control's own context and wraps the shared color texture so we can blit it to our window.
        int presentFrameBuffer;

        public class CachedCollisionStructure
        {
            readonly TriangleClassification filter;

            public CachedCollisionStructure(TriangleClassification filter)
            {
                this.filter = filter;
                triangles = null;
                lastUpdate = ulong.MaxValue;
            }

            DataUtil.CollisionStructure triangles;
            ulong lastUpdate;

            public DataUtil.CollisionStructure GetTriangles()
            {
                uint globalTimer = Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress);
                if (triangles == null || lastUpdate != globalTimer)
                {
                    triangles = new DataUtil.CollisionStructure(filter);
                    lastUpdate = globalTimer;
                }

                return triangles;
            }
        }

        public readonly CachedCollisionStructure floors = new CachedCollisionStructure(TriangleClassification.Floor);
        public readonly CachedCollisionStructure ceilings = new CachedCollisionStructure(TriangleClassification.Ceiling);

        private enum MapScale
        {
            CourseDefault,
            MaxCourseSize,
            Custom
        };

        private enum MapCenter
        {
            BestFit,
            Origin,
            Mario,
            Custom
        };

        private enum MapAngle
        {
            Angle0,
            Angle16384,
            Angle32768,
            Angle49152,
            Mario,
            Camera,
            Centripetal,
            Custom
        };

        private MapScale MapViewScale;
        private MapCenter MapViewCenter;
        private MapAngle MapViewAngle;
        private bool MapViewScaleWasCourseDefault = true;

        private static readonly float DEFAULT_MAP_VIEW_SCALE_VALUE = 1;
        private static readonly float DEFAULT_MAP_VIEW_ANGLE_VALUE = 32768;

        public float MapViewScaleValue = DEFAULT_MAP_VIEW_SCALE_VALUE;
        public float MapViewAngleValue = DEFAULT_MAP_VIEW_ANGLE_VALUE;

        public bool MapViewEnablePuView = false;
        public bool MapViewScaleIconSizes = false;
        public bool MapViewCenterChangeByPixels = true;

        public readonly GLControl glControl;
        public readonly MapTab mapTab;

        public ViewMode viewMode = ViewMode.TopDown;

        public ViewBase currentView => viewMode switch
        {
            ViewMode.TopDown => viewTopDown,
            ViewMode.Orthogonal => viewOrthogonal,
            ViewMode.ThreeDimensional => view3D,
        };

        public readonly ViewTopDown viewTopDown = new();
        public readonly ViewOrthogonal viewOrthogonal = new();
        public readonly View3D view3D = new();

        public float MapViewRadius => (float)MoreMath.GetHypotenuse(glControl.Width / 2, glControl.Height / 2) / MapViewScaleValue;

        public bool drawCylinderOutlines = false;

        public float MapViewXMin
        {
            get => currentView.position.X - MapViewRadius * glControl.AspectRatio;
        }

        public float MapViewXMax
        {
            get => currentView.position.X + MapViewRadius * glControl.AspectRatio;
        }

        public float MapViewZMin
        {
            get => currentView.position.Z - MapViewRadius;
        }

        public float MapViewZMax
        {
            get => currentView.position.Z + MapViewRadius;
        }

        public static readonly int MAX_COURSE_SIZE_X_MIN = -8191;
        public static readonly int MAX_COURSE_SIZE_X_MAX = 8192;
        public static readonly int MAX_COURSE_SIZE_Z_MIN = -8191;
        public static readonly int MAX_COURSE_SIZE_Z_MAX = 8192;

        public static readonly RectangleF MAX_COURSE_SIZE =
            new RectangleF(
                MAX_COURSE_SIZE_X_MIN,
                MAX_COURSE_SIZE_Z_MIN,
                MAX_COURSE_SIZE_X_MAX - MAX_COURSE_SIZE_X_MIN,
                MAX_COURSE_SIZE_Z_MAX - MAX_COURSE_SIZE_Z_MIN);

        public Vector2 mousePosition2D
        {
            get
            {
                var a = glControl.PointToClient(Cursor.Position);
                return new Vector2(a.X, a.Y);
            }
        }

        public Vector3 mapCursorPosition;
        public bool cursorOnMap = false;
        Vector3 normalAtCursor;
        public float cursorViewPlaneDist = 1000;
        public bool fixCursorPlane => viewMode == ViewMode.ThreeDimensional && keyboardControls.IsShiftDown();

        public float nearClip { get; private set; }
        public float farClip { get; private set; }
        public (Vector3 normal, float d) worldspaceNearPlane { get; private set; }
        public (Vector3 normal, float d) worldspaceFarPlane { get; private set; }
        public (Vector3 normal, float d) orthographicZero { get; private set; }

        bool[] mouseDown = new bool[3];
        public bool IsMouseDown(int button) => mouseDown[button];

        public readonly KeyboardControls keyboardControls;

        Func<IGraphicsContext> getContext;

        /// <summary>
        /// The OpenGL context that hosts all resources necessary to render a complete map image,
        /// and is capable of rendering to the main window's Map tab.
        /// <para>
        /// Popout windows' <see cref="MapPopout.graphics"/> instances will share with this context,
        /// but blit to their own framebuffer before presenting.
        /// </para>
        /// </summary>
        IGraphicsContext hostGlContext => getContext != null ? getContext() : glControl.Context;

        public MapGraphics(MapTab mapTab, GLControl glControl, Func<IGraphicsContext> getContext = null)
        {
            this.mapTab = mapTab;
            this.glControl = glControl;
            this.getContext = getContext;

            glControl.MouseDown += (_, _) => glControl.Focus();
            keyboardControls = new(glControl);
            drawLayers = new List<Action>[Enum.GetNames(typeof(DrawLayers)).Length];
            for (int i = 0; i < drawLayers.Length; i++)
                drawLayers[i] = new List<Action>();
        }

        public Matrix4 ViewMatrix { get; private set; } = Matrix4.Identity;
        public Matrix4 BillboardMatrix { get; private set; } = Matrix4.Identity;
        public Vector3 cameraPosition { get; private set; }

        List<Models.TriangleDataModel> levelTrianglesFor3DMap;

        Control previouslyActiveControl;

        Control GetActiveLeafControl(ContainerControl root)
        {
            while (root.ActiveControl is ContainerControl ctrl && ctrl.ActiveControl != null)
                root = ctrl;
            return root.ActiveControl;
        }


        List<Action> glInits = new List<Action>();
        bool doingGLInit = false;

        void PerformGLInit()
        {
            doingGLInit = true;
            using (new AccessScope<MapTab>(mapTab))
                foreach (var c in glInits)
                    c();
            glInits.Clear();
            doingGLInit = false;
        }

        public void DoGLInit(Action action)
        {
            if (doingGLInit)
                action();
            else
                glInits.Add(action);
        }

        public void Load(Func<Renderers.RendererCollection> getRenderers)
        {
            glControl.Paint += (sender, e) => OnPaint();

            glControl.MouseDown += OnMouseDown;
            glControl.MouseUp += OnMouseUp;
            glControl.MouseMove += OnMouseMove;
            glControl.MouseWheel += OnScroll;
            glControl.MouseEnter += (_, __) =>
            {
                var form = glControl.FindForm();
                previouslyActiveControl = GetActiveLeafControl(form);
            };
            glControl.MouseLeave += (_, __) =>
            {
                var form = glControl.FindForm();
                var activeControl = GetActiveLeafControl(form);
                if (activeControl == glControl)
                    form.ActiveControl = previouslyActiveControl;
            };
            glControl.Resize += (_, __) =>
            {
                if (glControl.Width * glControl.Height > 0)
                    using (new AccessScope<MapTab>(mapTab))
                    {
                        // These surfaces must live in the host context, recreate them there.
                        hostGlContext.MakeCurrent();
                        DeleteMainSurfaces();
                        transparencyRenderer.SetDimensions(glControl.Width, glControl.Height);
                        InitMainSurfaces();
                        InitOrUpdatePresentFrameBuffer();
                    }
            };

            glInits.Add(() =>
            {
                emptyVAO = GL.GenVertexArray();
                GL.ClearColor(Color.FromKnownColor(KnownColor.Control));
                GL.Enable(EnableCap.Texture2D);
                GL.Enable(EnableCap.Blend);
                GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.OneMinusDstAlpha, BlendingFactorDest.One);
                GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

                InitMainSurfaces();
                InitOrUpdatePresentFrameBuffer();
            });

            rendererCollection = getRenderers();
            transparencyRenderer = new Renderers.TransparencyRenderer(16, () => mainDepthBuffer, glControl.Width, glControl.Height);
            transparencyRenderer.transparents.Add(objectRenderer.transparent);
            transparencyRenderer.transparents.Add(triangleRenderer.transparent);
            transparencyRenderer.transparents.Add(circleRenderer.transparent);
            transparencyRenderer.transparents.Add(cylinderRenderer);
            transparencyRenderer.transparents.Add(sphereRenderer);
        }

        void DeleteMainSurfaces()
        {
            GL.DeleteTextures(2, new int[] { mainColorBuffer, mainDepthBuffer });
            GL.DeleteFramebuffer(mainFrameBuffer);
        }

        void InitMainSurfaces()
        {
            mainFrameBuffer = GL.GenFramebuffer();
            mainColorBuffer = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, mainColorBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, glControl.Width, glControl.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            mainDepthBuffer = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, mainDepthBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, glControl.Width, glControl.Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, mainFrameBuffer);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, mainColorBuffer, 0);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, mainDepthBuffer, 0);
            GL.DrawBuffers(1, new[] { DrawBuffersEnum.ColorAttachment0 });

            FramebufferErrorCode error;
            if ((error = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer)) != FramebufferErrorCode.FramebufferComplete)
                throw null;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        void InitOrUpdatePresentFrameBuffer()
        {
            // Only needed for Map popouts
            if (getContext == null) return;

            glControl.MakeCurrent();
            if (presentFrameBuffer == 0)
                presentFrameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, presentFrameBuffer);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, mainColorBuffer, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void CleanUp()
        {
            if (getContext != null)
            {
                // The presentFrameBuffer is created specifically and exclusively in the "popout" context of the glControl this instance shall render to.
                // Temporarily switch contexts to free the name of the framebuffer as early as possible.
                glControl.Context!.MakeCurrent();
                GL.DeleteFramebuffer(presentFrameBuffer);
                getContext().MakeCurrent();
            }

            transparencyRenderer.CleanUp();
            DeleteMainSurfaces();
        }

        private void OnPaint()
        {
            hostGlContext.MakeCurrent();

            PerformGLInit();

            if (Config.Stream == null || rendererCollection == null)
                return;

            using (new AccessScope<StroopMainForm>((StroopMainForm)mapTab.FindForm()))
            using (new AccessScope<MapTab>(mapTab))
            {
                Cursor cursor = mapTab.HasMouseListeners ? Cursors.Cross : Cursors.Hand;
                if (glControl.Cursor != cursor)
                    glControl.Cursor = cursor;

                UpdateMapView();

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, mainFrameBuffer);
                GL.ClearColor(0, 0, 0.5f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                for (int i = 0; i < drawLayers.Length; i++)
                    drawLayers[i].Clear();

                foreach (var renderer in rendererCollection.renderers)
                    renderer.SetDrawCalls(this);
                transparencyRenderer.SetDrawCalls(this);
                mapTab.DrawOn2DControl(this);
                drawLayers[(int)DrawLayers.Objects].Insert(0, rendererCollection.UpdateObjectMap);

                if (levelTrianglesFor3DMap == null || mapTab.NeedsGeometryRefresh())
                    levelTrianglesFor3DMap = TriangleUtilities.GetLevelTriangles();

                if (currentView == view3D)
                {
                    GL.ClearDepth(1);
                    GL.Clear(ClearBufferMask.DepthBufferBit);

                    if (view3D.display3DLevelGeometry)
                        drawLayers[(int)DrawLayers.FillBuffers].Insert(0, () =>
                        {
                            foreach (var t in levelTrianglesFor3DMap)
                            {
                                var color = t.Classification == TriangleClassification.Wall ? new Vector3(0.4f, 0.66f, 0.4f) : (t.Classification == TriangleClassification.Floor ? new Vector3(0.4f, 0.4f, 0.8f) : new Vector3(0.8f, 0.4f, 0.4f));
                                triangleRenderer.Add(t.p1, t.p2, t.p3, false, new Vector4(color, 1), new Vector4(color * 0.5f, 1), new Vector4(color * 0.25f, 1), new Vector4(0.2f, 0.2f, 0.2f, 1), new Vector3(1.5f), false);
                            }
                        });
                }

                foreach (var layer in drawLayers)
                    foreach (var action in layer)
                        action.Invoke();

                if (getContext == null)
                {
                    // Main map: render context == our own context. Blit our FBO to our window directly.
                    GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                    GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, mainFrameBuffer);
                    GL.BlitFramebuffer(0, 0, glControl.Width, glControl.Height, 0, 0, glControl.Width, glControl.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
                    glControl.SwapBuffers();
                }
                else
                {
                    // Popout: We rendered in the host context. Present into OUR own context/window
                    // by blitting the shared color texture (valid via GLControl.SharedContext) through a
                    // present-FBO that lives in our context. This is the fix for issue #39: the old code
                    // blitted into the main window and swapped our never-rendered buffer.
                    GL.Flush();
                    glControl.MakeCurrent();
                    GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, presentFrameBuffer);
                    GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                    GL.BlitFramebuffer(0, 0, glControl.Width, glControl.Height, 0, 0, glControl.Width, glControl.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
                    glControl.SwapBuffers();
                }
            }
        }

        private void UpdateMapView()
        {
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            UpdateAngle();
            UpdateScale();
            UpdateCenter();
            var scale = 2 * MapViewScaleValue / glControl.Height;
            Matrix4 swapYZ = new Matrix4(
                1, 0, 0, 0,
                0, 0, 1, 0,
                0, 1, 0, 0,
                0, 0, 0, 1
            );


            float zFar = viewMode == ViewMode.TopDown || float.IsNaN(viewOrthogonal.orthoRelativeFarPlane) ? 100000 : viewOrthogonal.orthoRelativeFarPlane;
            float zNear = viewMode == ViewMode.TopDown || float.IsNaN(viewOrthogonal.orthoRelativeNearPlane) ? -100000 : viewOrthogonal.orthoRelativeNearPlane;
            zFar = Math.Max(zNear + 0.0001f, zFar);
            Matrix4 othoDepth = Matrix4.CreateOrthographic(2, 2, zNear, zFar);

            switch (viewMode)
            {
                case ViewMode.TopDown:
                    BillboardMatrix = swapYZ;
                    ViewMatrix = Matrix4.CreateTranslation(new Vector3(-currentView.position.X, 0, -currentView.position.Z))
                                 * swapYZ
                                 * Matrix4.CreateRotationZ((float)(Math.PI + MoreMath.AngleUnitsToRadians(MapViewAngleValue)))
                                 * Matrix4.CreateScale(scale / glControl.AspectRatio, -scale, 1)
                                 * othoDepth;
                    break;

                case ViewMode.Orthogonal:
                    float cool = (float)MoreMath.AngleUnitsToRadians(MapViewAngleValue);
                    BillboardMatrix = Matrix4.CreateRotationY(cool);
                    float d = -Vector3.Dot(-BillboardMatrix.Row2.Xyz, viewOrthogonal.focusPositionAngle.position);
                    orthographicZero = (-BillboardMatrix.Row2.Xyz, d);
                    worldspaceNearPlane = (-BillboardMatrix.Row2.Xyz, d + viewOrthogonal.orthoRelativeNearPlane);
                    worldspaceFarPlane = (BillboardMatrix.Row2.Xyz, d - viewOrthogonal.orthoRelativeFarPlane);
                    ViewMatrix =
                        Matrix4.CreateTranslation(-viewOrthogonal.focusPositionAngle.position)
                        * Matrix4.CreateRotationY(-cool)
                        * Matrix4.CreateTranslation(-viewOrthogonal.orthoOffset.X, -viewOrthogonal.orthoOffset.Y, 0)
                        * Matrix4.CreateScale(scale / glControl.AspectRatio, scale, 1)
                        * othoDepth;
                    break;

                case ViewMode.ThreeDimensional:
                    Vector3 target = view3D.focusPositionAngle.position;
                    Vector3 viewDirection = currentView.ComputeViewDirection();
                    if (float.IsNaN(viewDirection.X))
                        viewDirection = new Vector3(0, 0, 1);

                    switch (view3D.camera3DMode)
                    {
                        case View3D.Camera3DMode.InGame:
                            view3D.position = new Vector3(Models.DataModels.Camera.X, Models.DataModels.Camera.Y, Models.DataModels.Camera.Z);
                            view3D.yaw = (float)MoreMath.AngleUnitsToRadians(Models.DataModels.Camera.FacingYaw);
                            view3D.pitch = (float)MoreMath.AngleUnitsToRadians(-Models.DataModels.Camera.FacingPitch);
                            target = currentView.position + viewDirection;
                            break;
                        case View3D.Camera3DMode.FocusOnPositionAngle:
                            currentView.position = target - viewDirection / (float)Math.Exp(-view3D.camera3DDistanceController * 0.1f);
                            break;
                        case View3D.Camera3DMode.Free:
                            target = currentView.position + viewDirection * (mapCursorPosition - currentView.position).Length;
                            break;
                    }

                    nearClip = Math.Max(1, Math.Min(50, (target - currentView.position).Length / 100));
                    farClip = nearClip * 5000;

                    ViewMatrix = Matrix4.LookAt(currentView.position, target, new Vector3(0, 1, 0));
                    var mat = Matrix4.Invert(ViewMatrix);
                    mat.Row3 = new Vector4(0, 0, 0, 1);
                    BillboardMatrix = mat;
                    ViewMatrix *= Matrix4.CreatePerspectiveFieldOfView(1, glControl.Width / (float)glControl.Height, nearClip, farClip);

                    break;
            }

            pixelsPerUnit = new Vector2(scale * glControl.Height, scale * glControl.Height) * 0.5f;
            UpdateCursor();
        }

        bool FindClosestIntersection(Vector3 rayOrigin, Vector3 rayDirection, out Vector3 intersection, out Models.TriangleDataModel triangle)
        {
            intersection = default(Vector3);
            triangle = default(Models.TriangleDataModel);
            Vector3 viewDirection = Vector3.Normalize(rayDirection);
            float closestDistance = float.PositiveInfinity, newDistance;
            foreach (var t in levelTrianglesFor3DMap)
                if (t.Intersect(rayOrigin, viewDirection, out Vector3 newIntersection, out Vector3 newNormal)
                    && (newDistance = (newIntersection - currentView.position).LengthSquared) < closestDistance)
                {
                    closestDistance = newDistance;
                    intersection = newIntersection;
                    triangle = t;
                }

            return (closestDistance < float.PositiveInfinity);
        }

        public void UpdateCursor()
        {
            if (viewMode != ViewMode.ThreeDimensional)
            {
                var e = glControl.PointToClient(Cursor.Position);
                mapCursorPosition = Vector3.TransformPosition(new Vector3(2.0f * e.X / glControl.Width - 1, 1 - 2.0f * e.Y / glControl.Height, 0), Matrix4.Invert(ViewMatrix));
            }
            else
            {
                if (levelTrianglesFor3DMap != null)
                {
                    var glControlCursorPos = glControl.PointToClient(Cursor.Position);
                    float tan = 2 * (float)Math.Tan(.5f);
                    var dx = tan * (glControlCursorPos.X - glControl.Width / 2.0f) / glControl.Height;
                    var dy = -tan * (glControlCursorPos.Y - glControl.Height / 2.0f) / glControl.Height;
                    var dir = BillboardMatrix.Row0.Xyz * dx + BillboardMatrix.Row1.Xyz * dy - BillboardMatrix.Row2.Xyz;
                    if (float.IsNaN(dir.X)) dir = new Vector3(0, 0, 1);

                    if (!fixCursorPlane
                        && (cursorOnMap = FindClosestIntersection(currentView.position + dir, dir, out Vector3 closestIntersection, out hoverTriangle)))
                    {
                        normalAtCursor = new Vector3(hoverTriangle.NormX, hoverTriangle.NormY, hoverTriangle.NormZ);
                        mapCursorPosition = closestIntersection;
                        cursorViewPlaneDist = Vector3.Dot(mapCursorPosition - currentView.position, -BillboardMatrix.Row2.Xyz);
                    }
                    else
                        mapCursorPosition = currentView.position + dir * cursorViewPlaneDist;
                }
            }
        }

        public bool Hover3D(Vector3 position, float radius)
        {
            var lineEnd = cursorOnMap
                ? mapCursorPosition
                : currentView.position + Vector3.Normalize(mapCursorPosition - currentView.position) * 10000;
            return ((ProjectOnLineSegment(position, currentView.position, lineEnd) - position).Length < radius);
        }

        private int _dragStartMouseX = 0;
        private int _dragStartMouseY = 0;
        private Vector3 _translateStartCenter = new Vector3(0);
        private Vector2 _translateStartOrthoOffset = new Vector2(2);
        private Vector3 _rotatePivot = new Vector3(0);
        private Vector3 _rotateDiff = new Vector3(0);

        private float _rotateStartAngle = 0;
        private float _dragStartYaw, _dragStartPitch;

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    mouseDown[0] = true;
                    _rotateStartAngle = MapViewAngleValue;
                    _dragStartMouseX = e.X;
                    _dragStartMouseY = e.Y;
                    _translateStartCenter = currentView.position;
                    _translateStartOrthoOffset = viewOrthogonal.orthoOffset;
                    _dragStartYaw = currentView.yaw;
                    _dragStartPitch = currentView.pitch;
                    _rotatePivot = mapCursorPosition;
                    Matrix4 viewOrientation = currentView.ComputeViewOrientation();
                    _rotateDiff = Vector3.TransformPosition(currentView.position - mapCursorPosition, Matrix4.Invert(viewOrientation));

                    currentView.movementSpeed = (mapCursorPosition - currentView.position).Length * 0.5f;
                    break;
                case MouseButtons.Right:
                    mouseDown[1] = true;
                    break;
                case MouseButtons.Middle:
                    mouseDown[2] = true;
                    _dragStartMouseX = e.X;
                    _dragStartMouseY = e.Y;
                    _dragStartYaw = currentView.yaw;
                    _dragStartPitch = currentView.pitch;
                    break;
            }

            using (new AccessScope<MapTab>(mapTab))
            {
                mapTab.UpdateHover();
                foreach (var data in mapTab.hoverData)
                    if (e.Button == MouseButtons.Left)
                        data.LeftClick(mapCursorPosition);
                    else if (e.Button == MouseButtons.Right)
                        data.RightClick(mapCursorPosition);
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    mouseDown[0] = false;
                    break;
                case MouseButtons.Right:
                    mouseDown[1] = false;
                    break;
                case MouseButtons.Middle:
                    mouseDown[2] = false;
                    break;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < mouseDown.Length; i++)
                mouseDown[i] &= MouseUtility.IsMouseDown(i);

            if (viewMode != ViewMode.ThreeDimensional)
                mapCursorPosition = Vector3.TransformPosition(new Vector3(2.0f * e.X / glControl.Width - 1, 1 - 2.0f * e.Y / glControl.Height, 0), Matrix4.Invert(ViewMatrix));

            using (new AccessScope<MapTab>(mapTab))
            {
                foreach (var hover in mapTab.hoverData)
                    if (mouseDown[0])
                    {
                        if (keyboardControls.IsCtrlDown())
                        {
                            if (hover.CanDrag().HasFlag(DragMask.Angle))
                            {
                                hover.SetLookAt(mapCursorPosition);
                                return;
                            }
                        }
                        else if (hover.CanDrag() != DragMask.None)
                        {
                            hover.DragTo(mapCursorPosition, viewMode != ViewMode.TopDown);
                            return;
                        }
                    }
            }

            if (mouseDown[2])
            {
                if (viewMode == ViewMode.ThreeDimensional)
                {
                    int pixelDiffX = e.X - _dragStartMouseX;
                    int pixelDiffY = e.Y - _dragStartMouseY;
                    float mul = 10.0f / (float)Math.Log((currentView.position - _rotatePivot).Length);
                    float diffX = pixelDiffX / (float)glControl.Width * 2 * mul;
                    float diffY = pixelDiffY / (float)glControl.Height * 2 * mul;
                    if (float.IsNaN(diffX) || float.IsNaN(diffY))
                        throw null;
                    if (view3D.camera3DMode == View3D.Camera3DMode.Free)
                    {
                        view3D.yaw = _dragStartYaw - diffX;
                        view3D.pitch = Math.Max(-(float)Math.PI * 0.499f, Math.Min((float)Math.PI * 0.499f, _dragStartPitch + diffY));
                    }
                    else if (view3D.camera3DMode == View3D.Camera3DMode.FocusOnPositionAngle)
                    {
                        view3D.yaw = _dragStartYaw + diffX;
                        view3D.pitch = Math.Max(-(float)Math.PI * 0.499f, Math.Min((float)Math.PI * 0.499f, _dragStartPitch - diffY));
                    }
                }
            }

            if (mouseDown[0])
            {
                if (!keyboardControls.IsCtrlDown())
                {
                    int pixelDiffX = e.X - _dragStartMouseX;
                    int pixelDiffY = e.Y - _dragStartMouseY;
                    pixelDiffX = mapTab.MaybeReverse(pixelDiffX);
                    pixelDiffY = mapTab.MaybeReverse(pixelDiffY);
                    float unitDiffX = pixelDiffX / MapViewScaleValue;
                    float unitDiffY = pixelDiffY / MapViewScaleValue;
                    switch (viewMode)
                    {
                        case ViewMode.TopDown:
                            {
                                (float rotatedX, float rotatedY) = ((float, float))
                                    MoreMath.RotatePointAboutPointAnAngularDistance(
                                        unitDiffX, unitDiffY, 0, 0, MapViewAngleValue);
                                currentView.position.X = _translateStartCenter.X - rotatedX;
                                currentView.position.Z = _translateStartCenter.Z - rotatedY;
                                SetCustomCenter($"{currentView.position.X}; {currentView.position.Y}; {currentView.position.Z}");
                                break;
                            }
                        case ViewMode.Orthogonal:
                            {
                                viewOrthogonal.orthoOffset = _translateStartOrthoOffset + new Vector2(-unitDiffX, unitDiffY);
                                break;
                            }
                        case ViewMode.ThreeDimensional:
                            if (view3D.camera3DMode != View3D.Camera3DMode.InGame)
                            {
                                float mul = 10.0f / (float)Math.Log((currentView.position - _rotatePivot).Length);
                                float diffX = pixelDiffX / (float)glControl.Width * 2 * mul;
                                float diffY = pixelDiffY / (float)glControl.Height * 2 * mul;
                                if (float.IsNaN(diffX) || float.IsNaN(diffY))
                                    throw null;
                                view3D.yaw = _dragStartYaw + diffX;
                                view3D.pitch = Math.Max(-(float)Math.PI * 0.499f, Math.Min((float)Math.PI * 0.499f, _dragStartPitch - diffY));

                                if (view3D.camera3DMode == View3D.Camera3DMode.Free)
                                {
                                    var dir = Vector3.TransformPosition(_rotateDiff, currentView.ComputeViewOrientation());
                                    currentView.position = _rotatePivot + dir;
                                }
                            }

                            break;
                    }
                }
                else
                {
                    switch (viewMode)
                    {
                        case ViewMode.TopDown:
                            {
                                double oldAngle = Math.Atan2(glControl.Height / 2 - _dragStartMouseY, _dragStartMouseX - glControl.Width / 2);
                                double thingAngle = Math.Atan2(glControl.Height / 2 - e.Y, e.X - glControl.Width / 2);
                                float angleToMouse = (float)MoreMath.RadiansToAngleUnits(thingAngle - oldAngle) * mapTab.MaybeReverse(-1);
                                MapViewAngleValue = _rotateStartAngle + angleToMouse;
                                SetCustomAngle(MapViewAngleValue);
                                break;
                            }
                        case ViewMode.Orthogonal:
                            {
                                float newAngle = _rotateStartAngle - (e.X - _dragStartMouseX) * 128;
                                newAngle %= 0x10000;
                                if (newAngle < 0) newAngle += 0x10000;
                                int increment = 0x2000;
                                int snapMargin = Math.Min(0x800, increment / 2);
                                for (var snapValue = 0; snapValue <= 0x10000; snapValue += increment)
                                    if (Math.Abs(newAngle - snapValue) < snapMargin)
                                        newAngle = snapValue;
                                MapViewAngleValue = newAngle;
                                SetCustomAngle(MapViewAngleValue);
                                break;
                            }
                        case ViewMode.ThreeDimensional:
                            {
                                view3D.camera3DMode = View3D.Camera3DMode.Free;
                                float dx = -(float)(e.X - _dragStartMouseX) / glControl.Height * currentView.movementSpeed;
                                float dy = (float)(e.Y - _dragStartMouseY) / glControl.Height * currentView.movementSpeed;
                                currentView.position = _translateStartCenter + BillboardMatrix.Row0.Xyz * dx + BillboardMatrix.Row1.Xyz * dy;
                                break;
                            }
                    }
                }
            }
        }

        private void OnScroll(object sender, MouseEventArgs e)
        {
            int delta = e.Delta > 0 ? 1 : -1;
            if (viewMode == ViewMode.ThreeDimensional)
            {
                if (view3D.camera3DMode == View3D.Camera3DMode.FocusOnPositionAngle)
                    view3D.camera3DDistanceController = Math.Max(0.0f, Math.Min(100, view3D.camera3DDistanceController - delta));
                else if (view3D.camera3DMode == View3D.Camera3DMode.Free)
                {
                    var diff = mapCursorPosition - currentView.position;
                    if (Vector3.Dot(diff, normalAtCursor) < 0)
                        currentView.movementSpeed = diff.Length * 0.5f;
                    currentView.position += Vector3.Normalize(mapCursorPosition - currentView.position) * delta * currentView.movementSpeed / 5;
                }
            }
            else
                ChangeScale2(delta, SpecialConfig.Map2DScrollSpeed);

            UpdateCursor();
        }

        public void UpdateFlyingControls(double frameTime)
        {
            Vector3 forwards = -BillboardMatrix.Row2.Xyz;
            Vector3 up = BillboardMatrix.Row1.Xyz;
            Vector3 right = BillboardMatrix.Row0.Xyz;
            Vector3 relativeMovement = Vector3.Zero;
            if (keyboardControls.IsDown(Keys.W))
                relativeMovement.Z += 1;
            if (keyboardControls.IsDown(Keys.S))
                relativeMovement.Z -= 1;
            if (keyboardControls.IsDown(Keys.D))
                relativeMovement.X += 1;
            if (keyboardControls.IsDown(Keys.A))
                relativeMovement.X -= 1;
            if (keyboardControls.IsDown(Keys.E))
                relativeMovement.Y += 1;
            if (keyboardControls.IsDown(Keys.Q))
                relativeMovement.Y -= 1;
            if (relativeMovement != Vector3.Zero)
            {
                relativeMovement.Normalize();
                float movement = (float)frameTime * (keyboardControls.IsShiftDown() ? 100 : 2000);
                currentView.position += (right * relativeMovement.X + up * relativeMovement.Y + forwards * relativeMovement.Z) * movement;
            }
        }
    }
}
