﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Drawing;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab
{
    public class MapGraphics
    {
        public enum DrawLayers
        {
            FillBuffers,
            FillBuffersRedirect,
            Background,
            Geometry,
            Transparency,
            GeometryOverlay,
            Objects,
            Overlay,
        }

        public readonly List<Action>[] drawLayers;

        bool needsRecreateObjectMipmaps = false;
        List<Renderers.Renderer> renderers = new List<Renderers.Renderer>();
        public Renderers.SpriteRenderer objectRenderer;
        public Renderers.TriangleRenderer triangleRenderer, triangleOverlayRenderer;
        public Renderers.LineRenderer lineRenderer;
        public Renderers.ShapeRenderer circleRenderer;
        public Renderers.GeometryRenderer cylinderRenderer, sphereRenderer;
        public Renderers.TransparencyRenderer transparencyRenderer;
        public Vector2 pixelsPerUnit { get; private set; }

        public Models.TriangleDataModel hoverTriangle;

        int mainFrameBuffer, mainColorBuffer, mainDepthBuffer;

        public class CachedCollisionStructure
        {
            readonly Func<Models.TriangleDataModel, bool> filter;
            public CachedCollisionStructure(Func<Models.TriangleDataModel, bool> filter)
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
                    triangles = new DataUtil.CollisionStructure(
                        TriangleUtilities.GetLevelTriangles().Where(filter).ToList(),
                        TriangleUtilities.GetObjectTriangles().Where(filter).ToList()
                        );
                    lastUpdate = globalTimer;
                }
                return triangles;
            }
        }

        public readonly CachedCollisionStructure floors = new CachedCollisionStructure(_ => _.Classification == TriangleClassification.Floor);
        public readonly CachedCollisionStructure ceilings = new CachedCollisionStructure(_ => _.Classification == TriangleClassification.Ceiling);


        const int OBJECTS_TEXTURE_SIZE = 256;
        const int OBJECTS_TEXTURE_LAYERS = 256;

        int objectLayer = 0;
        Dictionary<Image, int> knownIcons = new Dictionary<Image, int>();
        public int GetObjectTextureLayer(Image image)
        {
            if (knownIcons.TryGetValue(image, out var known))
                return known;
            var imageData = GraphicsUtil.GetPixelData(image, OBJECTS_TEXTURE_SIZE, OBJECTS_TEXTURE_SIZE);
            var result = objectLayer++;
            GL.BindTexture(TextureTarget.Texture2DArray, objectRenderer.texture);
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, result, OBJECTS_TEXTURE_SIZE, OBJECTS_TEXTURE_SIZE, 1, PixelFormat.Rgba, PixelType.UnsignedByte, imageData);
            GL.BindTexture(TextureTarget.Texture2DArray, 0);
            needsRecreateObjectMipmaps = true;
            knownIcons[image] = result;
            return result;
        }

        private enum MapScale { CourseDefault, MaxCourseSize, Custom };
        private enum MapCenter { BestFit, Origin, Mario, Custom };
        private enum MapAngle { Angle0, Angle16384, Angle32768, Angle49152, Mario, Camera, Centripetal, Custom };

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
        public readonly MapView view;

        public float MapViewRadius => (float)MoreMath.GetHypotenuse(glControl.Width / 2, glControl.Height / 2) / MapViewScaleValue;
        public float MapViewXMin { get => view.position.X - MapViewRadius * glControl.AspectRatio; }
        public float MapViewXMax { get => view.position.X + MapViewRadius * glControl.AspectRatio; }
        public float MapViewZMin { get => view.position.Z - MapViewRadius; }
        public float MapViewZMax { get => view.position.Z + MapViewRadius; }

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
        public bool fixCursorPlane => view.mode == MapView.ViewMode.ThreeDimensional && KeyboardUtilities.IsShiftHeld();

        public (Vector3 normal, float d) worldspaceNearPlane { get; private set; }
        public (Vector3 normal, float d) worldspaceFarPlane { get; private set; }
        public (Vector3 normal, float d) orthographicZero { get; private set; }

        bool[] mouseDown = new bool[3];
        public bool IsMouseDown(int button) => mouseDown[button];


        public MapGraphics(MapTab mapTab, GLControl glControl)
        {
            this.mapTab = mapTab;
            this.glControl = glControl;
            view = new MapView();
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

        public void Load()
        {
            glControl.MakeCurrent();
            glControl.Context.LoadAll();

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
                DeleteMainSurfaces();
                InitMainSurfaces();
            };

            GL.ClearColor(Color.FromKnownColor(KnownColor.Control));
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.OneMinusDstAlpha, BlendingFactorDest.One);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            InitMainSurfaces();

            CreateObjectsRenderer();
            renderers.Add(triangleRenderer = new Renderers.TriangleRenderer(0x100000));
            renderers.Add(triangleOverlayRenderer = new Renderers.TriangleRenderer(0x100000) { drawlayer = DrawLayers.GeometryOverlay });
            renderers.Add(lineRenderer = new Renderers.LineRenderer());
            renderers.Add(circleRenderer = new Renderers.ShapeRenderer(DrawLayers.Overlay));
            renderers.Add(cylinderRenderer = new Renderers.GeometryRenderer(Renderers.GeometryRenderer.GeometryData.Cylinder()));
            renderers.Add(sphereRenderer = new Renderers.GeometryRenderer(Renderers.GeometryRenderer.GeometryData.Sphere(128, 64)));

            transparencyRenderer = new Renderers.TransparencyRenderer(16, () => mainDepthBuffer, glControl.Width, glControl.Height);
            transparencyRenderer.transparents.Add(objectRenderer.transparent);
            transparencyRenderer.transparents.Add(triangleRenderer.transparent);
            transparencyRenderer.transparents.Add(circleRenderer.transparent);
            transparencyRenderer.transparents.Add(cylinderRenderer);
            transparencyRenderer.transparents.Add(sphereRenderer);
            renderers.Add(transparencyRenderer);
        }

        void DeleteMainSurfaces()
        {
            GL.DeleteTextures(2, new int[] { mainColorBuffer, mainDepthBuffer });
            GL.DeleteFramebuffer(mainFrameBuffer);
            transparencyRenderer.SetDimensions(glControl.Width, glControl.Height);
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

        void CreateObjectsRenderer()
        {
            renderers.Add(objectRenderer = new Renderers.SpriteRenderer(DrawLayers.Objects, 256));
            objectRenderer.texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, objectRenderer.texture);

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.LinearMipmapNearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            GL.TexImage3D(TextureTarget.Texture2DArray,
                    0,
                    PixelInternalFormat.Rgba8,
                    OBJECTS_TEXTURE_SIZE,
                    OBJECTS_TEXTURE_SIZE,
                    OBJECTS_TEXTURE_LAYERS,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    IntPtr.Zero);
        }

        private void OnPaint()
        {
            if (Config.Stream == null)
                return;

            using (new AccessScope<StroopMainForm>((StroopMainForm)mapTab.FindForm()))
            using (new AccessScope<MapTab>(mapTab))
            {
                Cursor cursor = mapTab.HasMouseListeners ? Cursors.Cross : Cursors.Hand;
                if (glControl.Cursor != cursor)
                    glControl.Cursor = cursor;

                glControl.MakeCurrent();
                UpdateMapView();

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, mainFrameBuffer);
                GL.ClearColor(0, 0, 0.5f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                for (int i = 0; i < drawLayers.Length; i++)
                    drawLayers[i].Clear();

                foreach (var renderer in renderers)
                    renderer.SetDrawCalls(this);
                mapTab.flowLayoutPanelMapTrackers.DrawOn2DControl(this);
                drawLayers[(int)DrawLayers.Objects].Insert(0, () =>
                {
                    if (needsRecreateObjectMipmaps)
                    {
                        GL.BindTexture(TextureTarget.Texture2DArray, objectRenderer.texture);
                        GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);
                        needsRecreateObjectMipmaps = false;
                    }
                });

                if (levelTrianglesFor3DMap == null || mapTab.NeedsGeometryRefresh())
                    levelTrianglesFor3DMap = TriangleUtilities.GetLevelTriangles();

                if (view.mode == MapView.ViewMode.ThreeDimensional)
                {
                    GL.ClearDepth(1);
                    GL.Clear(ClearBufferMask.DepthBufferBit);

                    if (view.display3DLevelGeometry)
                        drawLayers[(int)DrawLayers.FillBuffers].Insert(0, () =>
                        {
                            foreach (var t in levelTrianglesFor3DMap)
                            {
                                var color = t.Classification == TriangleClassification.Wall ? new Vector3(0.4f, 0.66f, 0.4f) :
                                    (t.Classification == TriangleClassification.Floor ? new Vector3(0.4f, 0.4f, 0.8f) : new Vector3(0.8f, 0.4f, 0.4f));
                                triangleRenderer.Add(t.p1, t.p2, t.p3, false, new Vector4(color, 1), new Vector4(color * 0.5f, 1), new Vector4(color * 0.25f, 1), new Vector4(0.2f, 0.2f, 0.2f, 1), new Vector3(1.5f), false);
                            }
                        });
                }

                foreach (var layer in drawLayers)
                    foreach (var action in layer)
                        action.Invoke();

                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, mainFrameBuffer);
                GL.BlitFramebuffer(0, 0, glControl.Width, glControl.Height, 0, 0, glControl.Width, glControl.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);

                glControl.SwapBuffers();
            }
        }

        private void UpdateMapView()
        {
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            UpdateAngle();
            UpdateScale();
            UpdateCenter();
            var scale = 2 * MapViewScaleValue / glControl.Width;
            Matrix4 swapYZ = new Matrix4(
                1, 0, 0, 0,
                0, 0, 1, 0,
                0, 1, 0, 0,
                0, 0, 0, 1
                );


            float zFar = view.mode == MapView.ViewMode.TopDown || float.IsNaN(view.orthoRelativeFarPlane) ? 100000 : view.orthoRelativeFarPlane;
            float zNear = view.mode == MapView.ViewMode.TopDown || float.IsNaN(view.orthoRelativeNearPlane) ? -100000 : view.orthoRelativeNearPlane;
            zFar = Math.Max(zNear + 0.0001f, zFar);
            Matrix4 othoDepth = Matrix4.CreateOrthographic(2, 2, zNear, zFar);

            switch (view.mode)
            {
                case MapView.ViewMode.TopDown:
                    BillboardMatrix = swapYZ;
                    ViewMatrix = Matrix4.CreateTranslation(new Vector3(-view.position.X, 0, -view.position.Z))
                        * swapYZ
                        * Matrix4.CreateRotationZ((float)(Math.PI + MoreMath.AngleUnitsToRadians(MapViewAngleValue)))
                        * Matrix4.CreateScale(scale / glControl.AspectRatio, -scale, 1)
                        * othoDepth;
                    break;

                case MapView.ViewMode.Orthogonal:
                    float cool = (float)MoreMath.AngleUnitsToRadians(MapViewAngleValue);
                    BillboardMatrix = Matrix4.CreateRotationY(cool);
                    float d = -Vector3.Dot(-BillboardMatrix.Row2.Xyz, view.focusPositionAngle.position);
                    orthographicZero = (-BillboardMatrix.Row2.Xyz, d);
                    worldspaceNearPlane = (-BillboardMatrix.Row2.Xyz, d + view.orthoRelativeNearPlane);
                    worldspaceFarPlane = (BillboardMatrix.Row2.Xyz, d - view.orthoRelativeFarPlane);
                    ViewMatrix =
                        Matrix4.CreateTranslation(-view.focusPositionAngle.position)
                        * Matrix4.CreateRotationY(-cool)
                        * Matrix4.CreateTranslation(-view.orthoOffset.X, -view.orthoOffset.Y, 0)
                        * Matrix4.CreateScale(scale / glControl.AspectRatio, scale, 1)
                        * othoDepth;
                    break;

                case MapView.ViewMode.ThreeDimensional:
                    Vector3 target = view.focusPositionAngle.position;
                    Vector3 viewDirection = view.ComputeViewDirection();
                    if (float.IsNaN(viewDirection.X))
                        viewDirection = new Vector3(0, 0, 1);

                    switch (view.camera3DMode)
                    {
                        case MapView.Camera3DMode.InGame:
                            view.position = new Vector3(Models.DataModels.Camera.X, Models.DataModels.Camera.Y, Models.DataModels.Camera.Z);
                            view.yaw = (float)MoreMath.AngleUnitsToRadians(Models.DataModels.Camera.FacingYaw);
                            view.pitch = (float)MoreMath.AngleUnitsToRadians(-Models.DataModels.Camera.FacingPitch);
                            target = view.position + viewDirection;
                            break;
                        case MapView.Camera3DMode.FocusOnPositionAngle:
                            view.position = target - viewDirection / (float)Math.Exp(-view.camera3DDistanceController * 0.1f);
                            break;
                        case MapView.Camera3DMode.Free:
                            target = view.position + viewDirection * (mapCursorPosition - view.position).Length;
                            break;
                    }

                    float nearClip = Math.Max(1, Math.Min(50, (target - view.position).Length / 100));

                    ViewMatrix = Matrix4.LookAt(view.position, target, new Vector3(0, 1, 0));
                    var mat = Matrix4.Invert(ViewMatrix);
                    mat.Row3 = new Vector4(0, 0, 0, 1);
                    BillboardMatrix = mat;
                    ViewMatrix *= Matrix4.CreatePerspectiveFieldOfView(1, glControl.Width / (float)glControl.Height, nearClip, nearClip * 5000);
                    Update3DCursor();

                    break;
            }

            pixelsPerUnit = new Vector2(scale * glControl.Height, scale * glControl.Height) * 0.5f;
        }

        bool FindClosestIntersection(Vector3 rayOrigin, Vector3 rayDirection, out Vector3 intersection, out Models.TriangleDataModel triangle)
        {
            intersection = default(Vector3);
            triangle = default(Models.TriangleDataModel);
            Vector3 viewDirection = Vector3.Normalize(rayDirection);
            float closestDistance = float.PositiveInfinity, newDistance;
            foreach (var t in levelTrianglesFor3DMap)
                if (t.Intersect(rayOrigin, viewDirection, out Vector3 newIntersection, out Vector3 newNormal)
                    && (newDistance = (newIntersection - view.position).LengthSquared) < closestDistance)
                {
                    closestDistance = newDistance;
                    intersection = newIntersection;
                    triangle = t;
                }

            return (closestDistance < float.PositiveInfinity);
        }

        public void Update3DCursor()
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
                    && (cursorOnMap = FindClosestIntersection(view.position + dir, dir, out Vector3 closestIntersection, out hoverTriangle)))
                {
                    normalAtCursor = new Vector3(hoverTriangle.NormX, hoverTriangle.NormY, hoverTriangle.NormZ);
                    mapCursorPosition = closestIntersection;
                    cursorViewPlaneDist = Vector3.Dot(mapCursorPosition - view.position, -BillboardMatrix.Row2.Xyz);
                }
                else
                    mapCursorPosition = view.position + dir * cursorViewPlaneDist;
            }
        }

        private void UpdateScale()
        {
            if (mapTab.radioButtonMapControllersScaleCourseDefault.Checked)
                MapViewScale = MapScale.CourseDefault;
            else if (mapTab.radioButtonMapControllersScaleMaxCourseSize.Checked)
                MapViewScale = MapScale.MaxCourseSize;
            else
                MapViewScale = MapScale.Custom;

            if (MapViewScale == MapScale.CourseDefault) MapViewScaleWasCourseDefault = true;
            if (MapViewScale == MapScale.MaxCourseSize) MapViewScaleWasCourseDefault = false;

            switch (MapViewScale)
            {
                case MapScale.CourseDefault:
                case MapScale.MaxCourseSize:
                    RectangleF rectangle = MapViewScale == MapScale.CourseDefault ?
                        mapTab.GetMapLayout().Coordinates : MAX_COURSE_SIZE;
                    List<(float, float)> coordinates = new List<(float, float)>()
                    {
                        (rectangle.Left, rectangle.Top),
                        (rectangle.Right, rectangle.Top),
                        (rectangle.Left, rectangle.Bottom),
                        (rectangle.Right, rectangle.Bottom),
                    };
                    List<(float, float)> rotatedCoordinates = coordinates.ConvertAll(coord =>
                    {
                        (float x, float z) = coord;
                        (double rotatedX, double rotatedZ) = MoreMath.RotatePointAboutPointAnAngularDistance(
                            x, z, 0, 0, 32768 - MapViewAngleValue);
                        return ((float)rotatedX, (float)rotatedZ);
                    });
                    float rotatedXMax = rotatedCoordinates.Max(coord => coord.Item1);
                    float rotatedXMin = rotatedCoordinates.Min(coord => coord.Item1);
                    float rotatedZMax = rotatedCoordinates.Max(coord => coord.Item2);
                    float rotatedZMin = rotatedCoordinates.Min(coord => coord.Item2);
                    float rotatedWidth = rotatedXMax - rotatedXMin;
                    float rotatedHeight = rotatedZMax - rotatedZMin;
                    MapViewScaleValue = Math.Min(
                        glControl.Width / rotatedWidth, glControl.Height / rotatedHeight);
                    break;
                case MapScale.Custom:
                    MapViewScaleValue = ParsingUtilities.ParseFloatNullable(
                        mapTab.textBoxMapControllersScaleCustom.LastSubmittedText)
                        ?? DEFAULT_MAP_VIEW_SCALE_VALUE;
                    break;
            }

            if (MapViewScale != MapScale.Custom)
            {
                mapTab.textBoxMapControllersScaleCustom.SubmitTextLoosely(MapViewScaleValue.ToString());
            }
        }

        private void UpdateCenter()
        {
            if (view.mode == MapView.ViewMode.ThreeDimensional)
                return;

            if (mapTab.radioButtonMapControllersCenterBestFit.Checked)
                MapViewCenter = MapCenter.BestFit;
            else if (mapTab.radioButtonMapControllersCenterOrigin.Checked)
                MapViewCenter = MapCenter.Origin;
            else if (mapTab.radioButtonMapControllersCenterMario.Checked)
                MapViewCenter = MapCenter.Mario;
            else
                MapViewCenter = MapCenter.Custom;

            switch (MapViewCenter)
            {
                case MapCenter.BestFit:
                    RectangleF rectangle = MapViewScaleWasCourseDefault ?
                        mapTab.GetMapLayout().Coordinates : MAX_COURSE_SIZE;
                    view.position.X = rectangle.X + rectangle.Width / 2;
                    view.position.Z = rectangle.Y + rectangle.Height / 2;
                    break;
                case MapCenter.Origin:
                    view.position = new Vector3(0.5f);
                    break;
                case MapCenter.Mario:
                    view.position = new Vector3(Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset),
                        Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset),
                        Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset));
                    break;
                case MapCenter.Custom:
                    PositionAngle posAngle = PositionAngle.FromString(
                        mapTab.textBoxMapControllersCenterCustom.LastSubmittedText);
                    if (posAngle != null)
                    {
                        view.position = posAngle.position;
                        break;
                    }
                    List<string> stringValues = ParsingUtilities.ParseStringList(
                        mapTab.textBoxMapControllersCenterCustom.LastSubmittedText, replaceComma: false);
                    if (stringValues.Count >= 3)
                    {
                        view.position.X = ParsingUtilities.ParseFloatNullable(stringValues[0]) ?? 0;
                        view.position.Y = ParsingUtilities.ParseFloatNullable(stringValues[1]) ?? 0;
                        view.position.Z = ParsingUtilities.ParseFloatNullable(stringValues[2]) ?? 0;
                    }
                    else if (stringValues.Count >= 2)
                    {
                        view.position.X = ParsingUtilities.ParseFloatNullable(stringValues[0]) ?? 0;
                        view.position.Z = ParsingUtilities.ParseFloatNullable(stringValues[1]) ?? 0;
                    }
                    else if (stringValues.Count == 1)
                    {
                        view.position = new Vector3(ParsingUtilities.ParseFloatNullable(stringValues[0]) ?? 0);
                    }
                    else
                        view.position = new Vector3();
                    break;
            }

            if (MapViewCenter != MapCenter.Custom)
            {
                mapTab.textBoxMapControllersCenterCustom.SubmitTextLoosely($"{view.position.X}; {view.position.Y}; {view.position.Z}");
            }
        }

        private void UpdateAngle()
        {
            if (mapTab.radioButtonMapControllersAngle0.Checked)
                MapViewAngle = MapAngle.Angle0;
            else if (mapTab.radioButtonMapControllersAngle16384.Checked)
                MapViewAngle = MapAngle.Angle16384;
            else if (mapTab.radioButtonMapControllersAngle32768.Checked)
                MapViewAngle = MapAngle.Angle32768;
            else if (mapTab.radioButtonMapControllersAngle49152.Checked)
                MapViewAngle = MapAngle.Angle49152;
            else if (mapTab.radioButtonMapControllersAngleMario.Checked)
                MapViewAngle = MapAngle.Mario;
            else if (mapTab.radioButtonMapControllersAngleCamera.Checked)
                MapViewAngle = MapAngle.Camera;
            else if (mapTab.radioButtonMapControllersAngleCentripetal.Checked)
                MapViewAngle = MapAngle.Centripetal;
            else
                MapViewAngle = MapAngle.Custom;

            switch (MapViewAngle)
            {
                case MapAngle.Angle0:
                    MapViewAngleValue = 0;
                    break;
                case MapAngle.Angle16384:
                    MapViewAngleValue = 16384;
                    break;
                case MapAngle.Angle32768:
                    MapViewAngleValue = 32768;
                    break;
                case MapAngle.Angle49152:
                    MapViewAngleValue = 49152;
                    break;
                case MapAngle.Mario:
                    MapViewAngleValue = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                    break;
                case MapAngle.Camera:
                    MapViewAngleValue = Config.Stream.GetUInt16(CameraConfig.StructAddress + CameraConfig.FacingYawOffset);
                    break;
                case MapAngle.Centripetal:
                    MapViewAngleValue = (float)MoreMath.ReverseAngle(
                        Config.Stream.GetUInt16(CameraConfig.StructAddress + CameraConfig.CentripetalAngleOffset));
                    break;
                case MapAngle.Custom:
                    PositionAngle posAngle = PositionAngle.FromString(
                        mapTab.textBoxMapControllersAngleCustom.LastSubmittedText);
                    if (posAngle != null)
                    {
                        MapViewAngleValue = (float)posAngle.Angle;
                        break;
                    }
                    MapViewAngleValue = ParsingUtilities.ParseFloatNullable(
                        mapTab.textBoxMapControllersAngleCustom.LastSubmittedText)
                        ?? DEFAULT_MAP_VIEW_ANGLE_VALUE;
                    break;
            }

            if (MapViewAngle != MapAngle.Custom)
            {
                mapTab.textBoxMapControllersAngleCustom.SubmitTextLoosely(MapViewAngleValue.ToString());
            }
        }

        public void ChangeScale(int sign, object value)
        {
            float? parsed = ParsingUtilities.ParseFloatNullable(value);
            if (!parsed.HasValue) return;
            mapTab.radioButtonMapControllersScaleCustom.Checked = true;
            float newScaleValue = MapViewScaleValue + sign * parsed.Value;
            mapTab.textBoxMapControllersScaleCustom.SubmitText(newScaleValue.ToString());
        }

        public void ChangeScale2(int power, object value)
        {
            float? parsed = ParsingUtilities.ParseFloatNullable(value);
            if (!parsed.HasValue) return;
            mapTab.radioButtonMapControllersScaleCustom.Checked = true;
            float newScaleValue = MapViewScaleValue * (float)Math.Pow(parsed.Value, power);
            mapTab.textBoxMapControllersScaleCustom.SubmitText(newScaleValue.ToString());
        }

        public void ChangeCenter(int xSign, int zSign, object value)
        {
            float? parsed = ParsingUtilities.ParseFloatNullable(value);
            if (!parsed.HasValue) return;
            mapTab.radioButtonMapControllersCenterCustom.Checked = true;
            float xOffset = xSign * parsed.Value;
            float zOffset = zSign * parsed.Value;
            (float xOffsetRotated, float zOffsetRotated) = ((float, float))MoreMath.RotatePointAboutPointAnAngularDistance(
                xOffset, zOffset, 0, 0, MapViewAngleValue);
            float multiplier = MapViewCenterChangeByPixels ? 1 / MapViewScaleValue : 1;
            float newCenterXValue = view.position.X + xOffsetRotated * multiplier;
            float newCenterZValue = view.position.Z + zOffsetRotated * multiplier;
            mapTab.textBoxMapControllersCenterCustom.SubmitText($"{newCenterXValue}; {view.position.Y}; {newCenterZValue}");
        }

        public void ChangeAngle(int sign, object value)
        {
            float? parsed = ParsingUtilities.ParseFloatNullable(value);
            if (!parsed.HasValue) return;
            mapTab.radioButtonMapControllersAngleCustom.Checked = true;
            float newAngleValue = MapViewAngleValue + sign * parsed.Value;
            newAngleValue = (float)MoreMath.NormalizeAngleDouble(newAngleValue);
            mapTab.textBoxMapControllersAngleCustom.SubmitText(newAngleValue.ToString());
        }

        public void SetCustomScale(object value)
        {
            mapTab.radioButtonMapControllersScaleCustom.Checked = true;
            mapTab.textBoxMapControllersScaleCustom.SubmitText(value.ToString());
        }

        public void SetCustomCenter(object value)
        {
            mapTab.radioButtonMapControllersCenterCustom.Checked = true;
            mapTab.textBoxMapControllersCenterCustom.SubmitText(value.ToString());
        }

        public void SetCustomAngle(object value)
        {
            mapTab.radioButtonMapControllersAngleCustom.Checked = true;
            mapTab.textBoxMapControllersAngleCustom.SubmitText(value.ToString());
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
                    _translateStartCenter = view.position;
                    _translateStartOrthoOffset = view.orthoOffset;
                    _dragStartYaw = view.yaw;
                    _dragStartPitch = view.pitch;
                    _rotatePivot = mapCursorPosition;
                    Matrix4 viewOrientation = view.ComputeViewOrientation();
                    _rotateDiff = Vector3.TransformPosition(view.position - mapCursorPosition, Matrix4.Invert(viewOrientation));

                    view.movementSpeed = (mapCursorPosition - view.position).Length * 0.5f;
                    break;
                case MouseButtons.Right:
                    mouseDown[1] = true;
                    break;
                case MouseButtons.Middle:
                    mouseDown[2] = true;
                    _dragStartMouseX = e.X;
                    _dragStartMouseY = e.Y;
                    _dragStartYaw = view.yaw;
                    _dragStartPitch = view.pitch;
                    break;
            }

            using (new AccessScope<MapTab>(mapTab))
            {
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

            if (view.mode != MapView.ViewMode.ThreeDimensional)
                mapCursorPosition = Vector3.TransformPosition(new Vector3(2.0f * e.X / glControl.Width - 1, 1 - 2.0f * e.Y / glControl.Height, 0), Matrix4.Invert(ViewMatrix));

            using (new AccessScope<MapTab>(mapTab))
            {
                foreach (var hover in mapTab.hoverData)
                    if (mouseDown[0] && hover.CanDrag())
                    {
                        hover.DragTo(mapCursorPosition);
                        return;
                    }
            }

            if (mouseDown[2])
            {
                if (view.mode == MapView.ViewMode.ThreeDimensional)
                {
                    int pixelDiffX = e.X - _dragStartMouseX;
                    int pixelDiffY = e.Y - _dragStartMouseY;
                    float mul = 10.0f / (float)Math.Log((view.position - _rotatePivot).Length);
                    float diffX = pixelDiffX / (float)glControl.Width * 2 * mul;
                    float diffY = pixelDiffY / (float)glControl.Height * 2 * mul;
                    if (float.IsNaN(diffX) || float.IsNaN(diffY))
                        throw null;
                    if (view.camera3DMode == MapView.Camera3DMode.Free)
                    {
                        view.yaw = _dragStartYaw - diffX;
                        view.pitch = Math.Max(-(float)Math.PI * 0.499f, Math.Min((float)Math.PI * 0.499f, _dragStartPitch + diffY));
                    }
                    else if (view.camera3DMode == MapView.Camera3DMode.FocusOnPositionAngle)
                    {
                        view.yaw = _dragStartYaw + diffX;
                        view.pitch = Math.Max(-(float)Math.PI * 0.499f, Math.Min((float)Math.PI * 0.499f, _dragStartPitch - diffY));
                    }
                }
            }

            if (mouseDown[0])
            {
                if (!KeyboardUtilities.IsCtrlHeld())
                {
                    int pixelDiffX = e.X - _dragStartMouseX;
                    int pixelDiffY = e.Y - _dragStartMouseY;
                    pixelDiffX = mapTab.MaybeReverse(pixelDiffX);
                    pixelDiffY = mapTab.MaybeReverse(pixelDiffY);
                    float unitDiffX = pixelDiffX / MapViewScaleValue;
                    float unitDiffY = pixelDiffY / MapViewScaleValue;
                    switch (view.mode)
                    {
                        case MapView.ViewMode.TopDown:
                            {
                                (float rotatedX, float rotatedY) = ((float, float))
                                    MoreMath.RotatePointAboutPointAnAngularDistance(
                                        unitDiffX, unitDiffY, 0, 0, MapViewAngleValue);
                                float newCenterX = _translateStartCenter.X - rotatedX;
                                float newCenterZ = _translateStartCenter.Z - rotatedY;
                                SetCustomCenter($"{newCenterX}; {view.position.Y}; {newCenterZ}");
                                break;
                            }
                        case MapView.ViewMode.Orthogonal:
                            {
                                view.orthoOffset = _translateStartOrthoOffset + new Vector2(-unitDiffX, unitDiffY);
                                break;
                            }
                        case MapView.ViewMode.ThreeDimensional:
                            if (view.camera3DMode != MapView.Camera3DMode.InGame)
                            {
                                float mul = 10.0f / (float)Math.Log((view.position - _rotatePivot).Length);
                                float diffX = pixelDiffX / (float)glControl.Width * 2 * mul;
                                float diffY = pixelDiffY / (float)glControl.Height * 2 * mul;
                                if (float.IsNaN(diffX) || float.IsNaN(diffY))
                                    throw null;
                                view.yaw = _dragStartYaw + diffX;
                                view.pitch = Math.Max(-(float)Math.PI * 0.499f, Math.Min((float)Math.PI * 0.499f, _dragStartPitch - diffY));

                                if (view.camera3DMode == MapView.Camera3DMode.Free)
                                {
                                    var dir = Vector3.TransformPosition(_rotateDiff, view.ComputeViewOrientation());
                                    view.position = _rotatePivot + dir;
                                }
                            }
                            break;
                    }
                }
                else
                {
                    switch (view.mode)
                    {
                        case MapView.ViewMode.TopDown:
                            {
                                double oldAngle = Math.Atan2(glControl.Height / 2 - _dragStartMouseY, _dragStartMouseX - glControl.Width / 2);
                                double thingAngle = Math.Atan2(glControl.Height / 2 - e.Y, e.X - glControl.Width / 2);
                                float angleToMouse = (float)MoreMath.RadiansToAngleUnits(thingAngle - oldAngle) * mapTab.MaybeReverse(-1);
                                float newAngle = _rotateStartAngle + angleToMouse;
                                SetCustomAngle(newAngle);
                                break;
                            }
                        case MapView.ViewMode.Orthogonal:
                            {
                                float newAngle = _rotateStartAngle - (e.X - _dragStartMouseX) * 128;
                                newAngle %= 0x10000;
                                if (newAngle < 0) newAngle += 0x10000;
                                int increment = 0x2000;
                                int snapMargin = Math.Min(0x800, increment / 2);
                                for (var snapValue = 0; snapValue <= 0x10000; snapValue += increment)
                                    if (Math.Abs(newAngle - snapValue) < snapMargin)
                                        newAngle = snapValue;
                                SetCustomAngle(newAngle);
                                break;
                            }
                        case MapView.ViewMode.ThreeDimensional:
                            {
                                view.camera3DMode = MapView.Camera3DMode.Free;
                                float dx = -(float)(e.X - _dragStartMouseX) / glControl.Height * view.movementSpeed;
                                float dy = (float)(e.Y - _dragStartMouseY) / glControl.Height * view.movementSpeed;
                                view.position = _translateStartCenter + BillboardMatrix.Row0.Xyz * dx + BillboardMatrix.Row1.Xyz * dy;
                                break;
                            }
                    }
                }
            }
        }

        private void OnScroll(object sender, MouseEventArgs e)
        {
            int delta = e.Delta > 0 ? 1 : -1;
            if (view.mode == MapView.ViewMode.ThreeDimensional)
            {
                if (view.camera3DMode == MapView.Camera3DMode.FocusOnPositionAngle)
                    view.camera3DDistanceController = Math.Max(0.0f, Math.Min(100, view.camera3DDistanceController - delta));
                else if (view.camera3DMode == MapView.Camera3DMode.Free)
                {
                    var diff = mapCursorPosition - view.position;
                    if (Vector3.Dot(diff, normalAtCursor) < 0)
                        view.movementSpeed = diff.Length * 0.5f;
                    view.position += Vector3.Normalize(mapCursorPosition - view.position) * delta * view.movementSpeed / 5;
                }
                Update3DCursor();
            }
            else
                ChangeScale2(delta, SpecialConfig.Map2DScrollSpeed);
        }

        public void UpdateFlyingControls(double frameTime)
        {
            Vector3 forwards = -BillboardMatrix.Row2.Xyz;
            Vector3 up = BillboardMatrix.Row1.Xyz;
            Vector3 right = BillboardMatrix.Row0.Xyz;
            Vector3 relativeMovement = Vector3.Zero;
            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.W))
                relativeMovement.Z += 1;
            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.S))
                relativeMovement.Z -= 1;
            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.D))
                relativeMovement.X += 1;
            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.A))
                relativeMovement.X -= 1;
            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.E))
                relativeMovement.Y += 1;
            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.Q))
                relativeMovement.Y -= 1;
            if (relativeMovement != Vector3.Zero)
            {
                relativeMovement.Normalize();
                float movement = (float)frameTime * (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) ? 100 : 2000);
                view.position += (right * relativeMovement.X + up * relativeMovement.Y + forwards * relativeMovement.Z) * movement;
            }
        }
    }
}
