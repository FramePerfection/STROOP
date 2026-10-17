using System.Globalization;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using StroopNext.Desktop.Modules;
using StroopNext.Map;
using StroopNext.Map3D;
using StroopNext.Rendering.Gl;
using StroopNext.Session;
using StroopNext.Settings;
using StroopNext.Variables;

namespace StroopNext.Desktop.Map;

internal sealed class Map3DView : UserControl
{
    private static readonly Setting<CameraMode3D> ModeSetting = new("mode", "Camera", CameraMode3D.FollowMario);
    private static readonly Setting<double> YawSetting = new("yaw", "Yaw", Math.PI);
    private static readonly Setting<double> PitchSetting = new("pitch", "Pitch", 0.9);
    private static readonly Setting<double> DistanceSetting = new("distance", "Distance", 1800);
    private static readonly Setting<bool> XRaySetting = new("xray", "X-ray", true);
    private static readonly Setting<SideAxis> SideAxisSetting = new("sideAxis", "Look", SideAxis.SideOfMario);
    private static readonly Setting<double> SideWidthSetting = new("sideWidth", "Side view width", 3000);
    private static readonly Setting<int> SideCutSetting = new("sideCut", "Cut ±", 0);

    private enum Drag { None, Orbit, Pan }

    private readonly ModuleContext _context;
    private readonly SettingsScope _settings;
    private readonly Func<IEnumerable<(IMapLayer3D Layer, SettingsScope Scope)>> _layers;
    private readonly GlSurface _gl = new();
    private readonly Camera3D _camera = new();
    private readonly Scene3D _scene = new();
    private readonly ComboBox _mode = new() { Width = 160, ItemsSource = new[] { "Follow Mario", "Free", "Game camera", "Side view", "Follow ghosts" } };
    private readonly ComboBox _sideAxis = new() { Width = 170, ItemsSource = new[] { "Side of Mario", "Behind Mario", "Along X", "Along Z (north)" } };
    private readonly NumericUpDown _sideCut = new() { Minimum = 0, Maximum = 20000, Increment = 50, FormatString = "0", Width = 120 };
    private readonly StackPanel _sideBar = new() { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(6, 0, 6, 4) };
    private readonly TextBlock _hint = new() { VerticalAlignment = VerticalAlignment.Center, Opacity = 0.65, TextTrimming = TextTrimming.CharacterEllipsis };

    private Vector3 _sideOffset;

    private bool _hasFreeTarget;
    private short? _level;
    private readonly CheckBox _xray = new() { Content = "X-ray" };
    private readonly Border _tip = new()
    {
        IsVisible = false,
        IsHitTestVisible = false,
        Background = new SolidColorBrush(Color.FromArgb(230, 20, 22, 30)),
        CornerRadius = new CornerRadius(4),
        Padding = new Thickness(8, 5),
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top,
    };
    private readonly TextBlock _tipText = new() { Foreground = Brushes.White, FontSize = 12 };

    private GlMapRenderer? _renderer;
    private bool _xrayEnabled;
    private Primitive3D? _hovered;
    private Vector2? _cursor;
    private Drag _drag;
    private Vector2 _dragStart, _dragLast;
    private bool _dragMoved;
    private Hit3D? _menuHit;

    public Action<ContextMenu, IMapHit?, Vector3>? ExtendMenu { get; init; }

    public Map3DView(ModuleContext context, SettingsScope settings, Func<IEnumerable<(IMapLayer3D, SettingsScope)>> layers)
    {
        _context = context;
        _settings = settings;
        _layers = layers;

        _camera.Yaw = settings.Get(YawSetting);
        _camera.Pitch = settings.Get(PitchSetting);
        _camera.Distance = settings.Get(DistanceSetting);

        _gl.Init = () => _renderer = new GlMapRenderer();
        _gl.RenderGl = (width, height) =>
        {
            if (_renderer is null)
                return;
            _renderer.XRay = _xrayEnabled;
            _camera.Width = width;
            _camera.Height = height;
            _renderer.Render(_scene, _camera, width, height, _hovered);
        };
        _gl.Deinit = () =>
        {
            _renderer?.Dispose();
            _renderer = null;
        };
        _gl.Failed += message => _context.ReportError(message);
        _gl.PointerPressed += OnPointerPressed;
        _gl.PointerMoved += OnPointerMoved;
        _gl.PointerReleased += OnPointerReleased;
        _gl.PointerWheelChanged += OnWheel;
        _gl.DoubleTapped += (_, e) => LookAt(Pixels(e.GetPosition(_gl)));
        _gl.PointerExited += (_, _) => { _cursor = null; SetHovered(null); };

        _mode.SelectedIndex = (int)settings.Get(ModeSetting);
        _mode.SelectionChanged += (_, _) =>
        {
            if (_mode.SelectedIndex >= 0)
                SetMode((CameraMode3D)_mode.SelectedIndex);
        };
        var reset = new Button { Content = "Reset view" };
        reset.Click += (_, _) => ResetView();
        _xray.IsChecked = _xrayEnabled = settings.Get(XRaySetting);
        ToolTip.SetTip(_xray, "Show Mario, objects and hitboxes through walls as faint ghosts");
        _xray.IsCheckedChanged += (_, _) =>
        {
            _xrayEnabled = _xray.IsChecked == true;
            _settings.Set(XRaySetting, _xrayEnabled);
            _gl.RequestNextFrameRendering();
        };

        var bar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(6, 4) };
        bar.Children.AddRange([
            new TextBlock { Text = "Camera", VerticalAlignment = VerticalAlignment.Center },
            _mode, reset, _xray, _hint,
        ]);

        _sideAxis.SelectedIndex = (int)settings.Get(SideAxisSetting);
        _sideAxis.SelectionChanged += (_, _) =>
        {
            if (_sideAxis.SelectedIndex < 0)
                return;
            _settings.Set(SideAxisSetting, (SideAxis)_sideAxis.SelectedIndex);
            RefreshSide();
        };
        _sideCut.Value = settings.Get(SideCutSetting);
        ToolTip.SetTip(_sideCut, "Cross-section: only draw what is within this distance of the plane through Mario (0 = everything)");
        _sideCut.ValueChanged += (_, _) =>
        {
            _settings.Set(SideCutSetting, (int)(_sideCut.Value ?? 0));
            RefreshSide();
        };
        _sideBar.Children.AddRange([
            new TextBlock { Text = "Look", VerticalAlignment = VerticalAlignment.Center },
            _sideAxis,
            new TextBlock { Text = "Cut ±", VerticalAlignment = VerticalAlignment.Center },
            _sideCut,
            new TextBlock { Text = "units (0 = no cut)", VerticalAlignment = VerticalAlignment.Center, Opacity = 0.65 },
        ]);
        var bars = new StackPanel();
        bars.Children.AddRange([bar, _sideBar]);
        UpdateModeUi();
        _tip.Child = _tipText;

        var surface = new Panel();
        surface.Children.Add(_gl);
        surface.Children.Add(_tip);
        var root = new DockPanel();
        DockPanel.SetDock(bars, Dock.Top);
        root.Children.Add(bars);
        root.Children.Add(surface);
        Content = root;
    }

    private CameraMode3D Mode => (CameraMode3D)Math.Max(0, _mode.SelectedIndex);

    internal Control Surface => _gl;
    internal string CameraState => $"mode {Mode} · yaw {_camera.Yaw:0.###} · pitch {_camera.Pitch:0.###} · distance {_camera.Distance:0} · target {_camera.Target}";

    private Vector2 Pixels(Point p) => new((float)(p.X * _gl.Scaling), (float)(p.Y * _gl.Scaling));

    public void OnFrame(GameFrame frame)
    {
        ApplyCamera(frame);
        Rebuild(frame);
    }

    public void Rebuild()
    {
        if (_context.CurrentFrame is { } frame)
            Rebuild(frame);
        else
            _gl.RequestNextFrameRendering();
    }

    private void Rebuild(GameFrame frame)
    {
        if (Mode == CameraMode3D.FollowGhosts)
            ApplyCamera(frame);
        lock (_gl.SyncRoot)
        {
            _camera.Width = Math.Max(1, (float)(_gl.Bounds.Width * _gl.Scaling));
            _camera.Height = Math.Max(1, (float)(_gl.Bounds.Height * _gl.Scaling));
            _scene.Clear();
            _camera.Far = Camera3D.DefaultFar;
            foreach (var (layer, scope) in _layers())
                layer.Build3D(new Map3DContext(frame, _camera, _scene, scope, _context.Selection));
            if (_cursor is { } cursor && _drag == Drag.None)
                SetHovered(Picking.Pick(_scene, _camera, cursor)?.Primitive, cursor);
        }
        _gl.RequestNextFrameRendering();
    }

    private void ApplyCamera(GameFrame frame)
    {
        if (_drag != Drag.None)
            return;
        lock (_gl.SyncRoot)
        {
            if (frame.LevelIndex != _level)
            {
                _level = frame.LevelIndex;
                _hasFreeTarget = false;
            }
            switch (Mode)
            {
                case CameraMode3D.Free when !_hasFreeTarget && frame.Mario is { } start:
                    _camera.Eye = null;
                    _camera.Target = start.Position + new Vector3(0, 80, 0);
                    _hasFreeTarget = true;
                    break;
                case CameraMode3D.FollowMario when frame.Mario is { } mario:
                    _camera.Eye = null;
                    _camera.Target = mario.Position + new Vector3(0, 80, 0);
                    _camera.FieldOfView = 60 * Math.PI / 180;
                    break;
                case CameraMode3D.Game when frame.Camera is { } game:
                    _camera.Eye = game.Position;
                    _camera.Target = game.Focus;
                    _camera.FieldOfView = 45 * Math.PI / 180;
                    break;
                case CameraMode3D.FollowGhosts when (GhostsCentre(frame) ?? frame.Mario?.Position) is { } centre:
                    _camera.Eye = null;
                    _camera.Target = centre + new Vector3(0, 80, 0);
                    _camera.FieldOfView = 60 * Math.PI / 180;
                    break;
                case CameraMode3D.Side when frame.Mario is { } m:
                    SideView.Apply(_camera, m.Position + new Vector3(0, 80, 0) + _sideOffset,
                        SideView.LookYaw(_settings.Get(SideAxisSetting), m.FacingYaw),
                        (float)_settings.Get(SideWidthSetting), _settings.Get(SideCutSetting));
                    break;
            }
        }
    }

    private Vector3? GhostsCentre(GameFrame frame)
    {
        var sum = Vector3.Zero;
        int n = 0;
        foreach (var (layer, _) in _layers())
            if (layer is StroopNext.Map.GhostsLayer ghosts)
                foreach (var g in ghosts.Ghosts)
                    if (g.Visible && !g.IsMarker && g.FrameNow(frame.GlobalTimer) is { } now)
                    {
                        sum += now.Position;
                        n++;
                    }
        return n == 0 ? null : sum / n;
    }

    private void RefreshSide()
    {
        if (Mode == CameraMode3D.Side && _context.CurrentFrame is { } frame)
        {
            ApplyCamera(frame);
            Rebuild(frame);
        }
    }

    private void UpdateModeUi()
    {
        bool side = Mode == CameraMode3D.Side;
        _sideBar.IsVisible = side;
        _hint.Text = side
            ? "Drag: pan · wheel: zoom · right-click: teleport, measure · the view follows Mario"
            : "Left-drag: orbit · right-drag: pan · wheel: zoom · double-click: look at · right-click: teleport";
    }

    private void SetMode(CameraMode3D mode)
    {
        if (_mode.SelectedIndex != (int)mode)
            _mode.SelectedIndex = (int)mode;
        _settings.Set(ModeSetting, mode);
        lock (_gl.SyncRoot)
        {
            if (mode != CameraMode3D.Side && _camera.OrthographicWidth is not null)
            {
                _camera.OrthographicWidth = null;
                _camera.SlabHalfDepth = null;
                _camera.Yaw = _settings.Get(YawSetting);
                _camera.Pitch = _settings.Get(PitchSetting);
                _camera.Distance = _settings.Get(DistanceSetting);
            }
            if (mode is not (CameraMode3D.Game or CameraMode3D.Side))
                _camera.DetachFromEye();
            if (mode == CameraMode3D.Free && _level is not null)
                _hasFreeTarget = true;
        }
        UpdateModeUi();
        if (mode == CameraMode3D.Side)
            RefreshSide();
        else
            Rebuild();
    }

    private void ResetView()
    {
        lock (_gl.SyncRoot)
        {
            _camera.Yaw = YawSetting.Default;
            _camera.Pitch = PitchSetting.Default;
            _camera.Distance = DistanceSetting.Default;
        }
        SaveCamera();
        _sideOffset = Vector3.Zero;
        if (Mode == CameraMode3D.Side)
        {
            _settings.Set(SideWidthSetting, SideWidthSetting.Default);
            RefreshSide();
        }
        else
            SetMode(CameraMode3D.FollowMario);
    }

    private void SaveCamera()
    {
        if (Mode == CameraMode3D.Side)
            return;
        _settings.Set(YawSetting, _camera.Yaw);
        _settings.Set(PitchSetting, _camera.Pitch);
        _settings.Set(DistanceSetting, _camera.Distance);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(_gl);
        if (!point.Properties.IsLeftButtonPressed && !point.Properties.IsRightButtonPressed)
            return;
        _drag = point.Properties.IsLeftButtonPressed ? Drag.Orbit : Drag.Pan;
        _dragStart = _dragLast = Pixels(point.Position);
        _dragMoved = false;
        e.Pointer.Capture(_gl);
        SetHovered(null);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var at = Pixels(e.GetPosition(_gl));
        _cursor = at;
        if (_drag != Drag.None)
        {
            if (Vector2.Distance(at, _dragStart) > 3)
                _dragMoved = true;
            if (_dragMoved)
            {
                var delta = at - _dragLast;
                if (Mode == CameraMode3D.Side)
                {
                    lock (_gl.SyncRoot)
                    {
                        var before = _camera.Target;
                        _camera.Pan(delta.X, delta.Y);
                        _sideOffset += _camera.Target - before;
                    }
                }
                else if (_drag == Drag.Orbit)
                {
                    if (Mode == CameraMode3D.Game)
                        SetMode(CameraMode3D.Free);
                    lock (_gl.SyncRoot)
                        _camera.Orbit(delta.X, delta.Y);
                }
                else
                {
                    if (Mode != CameraMode3D.Free)
                        SetMode(CameraMode3D.Free);
                    lock (_gl.SyncRoot)
                        _camera.Pan(delta.X, delta.Y);
                }
                _gl.RequestNextFrameRendering();
            }
            _dragLast = at;
            return;
        }
        lock (_gl.SyncRoot)
            SetHovered(Picking.Pick(_scene, _camera, at)?.Primitive, at);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var drag = _drag;
        _drag = Drag.None;
        e.Pointer.Capture(null);
        if (drag == Drag.None)
            return;
        if (_dragMoved)
        {
            SaveCamera();
            Rebuild();
            return;
        }

        var at = Pixels(e.GetPosition(_gl));
        Hit3D? hit;
        lock (_gl.SyncRoot)
            hit = Picking.Pick(_scene, _camera, at);
        if (e.InitialPressMouseButton == MouseButton.Right)
        {
            _menuHit = hit;
            Ray ray;
            lock (_gl.SyncRoot)
                ray = _camera.ScreenRay(at);
            _menuGround = _context.CurrentFrame is { } current ? Picking.TeleportTarget(current.Surfaces, ray) : null;
            _menuPlane = null;
            if (_menuHit is null && _menuGround is null && _context.CurrentFrame?.Mario?.Position is { } mario
                && MathF.Abs(ray.Direction.Y) > 1e-4f && (mario.Y - ray.Origin.Y) / ray.Direction.Y is var t && t > 0)
                _menuPlane = ray.Origin + ray.Direction * t;
            ShowMenu();
            return;
        }
        if (hit?.Primitive.Hit is ObjectHit objectHit)
        {
            if ((e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Meta)) != 0)
                _context.Selection.Toggle(objectHit.Object.Address);
            else
                _context.Selection.SelectOnly(objectHit.Object.Address);
            _context.ReportStatus($"Selected {objectHit.Title}.");
        }
        else if (hit is null)
            _context.Selection.Clear();
    }

    private void OnWheel(object? sender, PointerWheelEventArgs e)
    {
        if (Mode == CameraMode3D.Side)
        {
            double width = Math.Clamp(_settings.Get(SideWidthSetting) * (e.Delta.Y > 0 ? 0.85 : 1 / 0.85), 20, 60000);
            _settings.Set(SideWidthSetting, width);
            lock (_gl.SyncRoot)
                _camera.OrthographicWidth = (float)width;
            _gl.RequestNextFrameRendering();
            e.Handled = true;
            return;
        }
        if (Mode == CameraMode3D.Game)
            SetMode(CameraMode3D.Free);
        lock (_gl.SyncRoot)
            _camera.Zoom(e.Delta.Y > 0 ? 0.85 : 1 / 0.85);
        SaveCamera();
        _gl.RequestNextFrameRendering();
        e.Handled = true;
    }

    private void LookAt(Vector2 at)
    {
        Hit3D? hit;
        lock (_gl.SyncRoot)
            hit = Picking.Pick(_scene, _camera, at);
        if (hit is not { } target)
            return;
        SetMode(CameraMode3D.Free);
        lock (_gl.SyncRoot)
            _camera.Target = target.Point;
        _gl.RequestNextFrameRendering();
    }

    private void SetHovered(Primitive3D? primitive, Vector2? at = null)
    {
        var hovered = primitive?.Hit is null ? null : primitive;
        bool same = ReferenceEquals(hovered, _hovered) || (hovered is not null && _hovered is not null && Equals(hovered.Hit, _hovered.Hit));
        _hovered = hovered;
        if (hovered?.Hit is { } hit && at is { } p)
        {
            if (!same)
                _tipText.Text = hit.Title + Environment.NewLine + string.Join(Environment.NewLine, hit.Details);
            _tip.Margin = new Thickness(p.X / _gl.Scaling + 18, p.Y / _gl.Scaling + 18, 0, 0);
            _tip.IsVisible = true;
        }
        else
            _tip.IsVisible = false;
        if (!same)
            _gl.RequestNextFrameRendering();
    }

    private Vector3? _menuGround;
    private Vector3? _menuPlane;

    private Vector3? MenuPoint => _menuHit?.Point ?? _menuGround ?? _menuPlane;

    private void ShowMenu()
    {
        bool enabled = MenuPoint is not null;
        var menu = new ContextMenu();
        var teleport = new MenuItem { Header = "Teleport Mario to this point", IsEnabled = (_menuGround ?? _menuHit?.Point) is not null };
        teleport.Click += (_, _) =>
        {
            if ((_menuGround ?? _menuHit?.Point) is not { } p || _context.CurrentFrame is not { } frame)
                return;
            if (frame.Mario is null)
            {
                _context.ReportError("No Mario to teleport (not in a level).");
                return;
            }
            _context.Apply(MarioCommands.Teleport(frame, p.X, p.Y, p.Z), $"Mario teleported to ({p.X:0.#}, {p.Y:0.#}, {p.Z:0.#})");
        };
        var look = new MenuItem { Header = "Look at this point", IsEnabled = enabled };
        look.Click += (_, _) =>
        {
            if (MenuPoint is not { } point)
                return;
            SetMode(CameraMode3D.Free);
            lock (_gl.SyncRoot)
                _camera.Target = point;
            _gl.RequestNextFrameRendering();
        };
        var copy = new MenuItem { Header = "Copy coordinates", IsEnabled = enabled };
        copy.Click += async (_, _) =>
        {
            if (MenuPoint is { } point && TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
                await clipboard.SetTextAsync(string.Create(CultureInfo.InvariantCulture, $"{point.X:0.###}, {point.Y:0.###}, {point.Z:0.###}"));
        };
        menu.Items.Add(teleport);
        menu.Items.Add(look);
        menu.Items.Add(copy);
        if (MenuPoint is { } menuPoint)
            ExtendMenu?.Invoke(menu, _menuHit?.Primitive.Hit, _menuGround ?? menuPoint);
        menu.Open(_gl);
    }
}
