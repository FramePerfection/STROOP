using StroopNext.Rendering.Skia;
using StroopNext.Rendering.Gl;
using StroopNext.Rendering.Diagnostics;
using System.Globalization;
using System.Numerics;
using OpenTK.GLControl;
using StroopNext.App.Modules;
using StroopNext.Map;
using StroopNext.Map3D;
using StroopNext.Session;
using StroopNext.Settings;
using StroopNext.Variables;

namespace StroopNext.App.Map3D;

internal sealed class Map3DView : UserControl
{
    private static readonly Setting<CameraMode3D> ModeSetting = new("mode", "Camera", CameraMode3D.FollowMario);
    private static readonly Setting<double> YawSetting = new("yaw", "Yaw", Math.PI);
    private static readonly Setting<double> PitchSetting = new("pitch", "Pitch", 0.9);
    private static readonly Setting<double> DistanceSetting = new("distance", "Distance", 1800);
    private static readonly Setting<bool> XRaySetting = new("xray", "X-ray", true);

    private enum Drag { None, Orbit, Pan }

    private readonly ModuleContext _context;
    private readonly SettingsScope _settings;
    private readonly Func<IEnumerable<(IMapLayer3D Layer, SettingsScope Scope)>> _layers;
    private readonly GLControl _gl;
    private readonly Camera3D _camera = new();
    private readonly Scene3D _scene = new();
    private readonly ToolTip _tip = new() { InitialDelay = 0, ReshowDelay = 0, UseAnimation = false, UseFading = false };
    private readonly ContextMenuStrip _menu = new();
    private readonly ComboBox _mode = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly CheckBox _xray = new() { Text = "X-ray", AutoSize = true, ForeColor = Color.Gainsboro, Margin = new Padding(10, 5, 3, 3) };
    private readonly Label _status = new() { AutoSize = true, ForeColor = Color.Gainsboro, Margin = new Padding(12, 6, 3, 3) };

    private GlMapRenderer? _renderer;
    private bool _glFailed;
    private Primitive3D? _hovered;
    private Vector2? _cursor;
    private Drag _drag;
    private Vector2 _dragStart, _dragLast;
    private bool _dragMoved;
    private Hit3D? _menuHit;

    public Map3DView(ModuleContext context, SettingsScope settings, Func<IEnumerable<(IMapLayer3D, SettingsScope)>> layers)
    {
        _context = context;
        _settings = settings;
        _layers = layers;
        Dock = DockStyle.Fill;

        _camera.Yaw = settings.Get(YawSetting);
        _camera.Pitch = settings.Get(PitchSetting);
        _camera.Distance = settings.Get(DistanceSetting);

        _gl = new GLControl { APIVersion = new Version(3, 3), Dock = DockStyle.Fill };
        _gl.Paint += (_, _) => Render();
        _gl.Resize += (_, _) => _gl.Invalidate();
        _gl.MouseDown += OnMouseDown;
        _gl.MouseMove += OnMouseMove;
        _gl.MouseUp += OnMouseUp;
        _gl.MouseWheel += OnWheel;
        _gl.DoubleClick += OnDoubleClick;
        _gl.MouseLeave += (_, _) => { _cursor = null; SetHovered(null); };

        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, BackColor = Color.FromArgb(0x26, 0x27, 0x30), Padding = new Padding(4, 2, 4, 2) };
        _mode.Items.AddRange(["Follow Mario", "Free", "Game camera"]);
        _mode.SelectedIndex = (int)settings.Get(ModeSetting);
        _mode.SelectedIndexChanged += (_, _) => SetMode((CameraMode3D)_mode.SelectedIndex);
        var reset = new Button { Text = "Reset view", AutoSize = true, FlatStyle = FlatStyle.Flat, ForeColor = Color.Gainsboro };
        reset.Click += (_, _) => ResetView();
        bar.Controls.Add(new Label { Text = "Camera", AutoSize = true, ForeColor = Color.Gainsboro, Margin = new Padding(3, 6, 3, 3) });
        _xray.Checked = settings.Get(XRaySetting);
        _xray.CheckedChanged += (_, _) =>
        {
            _settings.Set(XRaySetting, _xray.Checked);
            _gl.Invalidate();
        };
        _tip.SetToolTip(_xray, "Show Mario, objects and hitboxes through walls as faint ghosts");
        bar.Controls.AddRange([_mode, reset, _xray, _status]);
        _status.Text = "Left-drag: orbit · right-drag: pan · wheel: zoom · double-click: look at · right-click: teleport";

        BuildContextMenu();
        Controls.Add(_gl);
        Controls.Add(bar);
    }

    private CameraMode3D Mode => (CameraMode3D)_mode.SelectedIndex;

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
            _gl.Invalidate();
    }

    private void Rebuild(GameFrame frame)
    {
        _camera.Width = Math.Max(1, _gl.ClientSize.Width);
        _camera.Height = Math.Max(1, _gl.ClientSize.Height);
        _scene.Clear();
        _camera.Far = Camera3D.DefaultFar;
        foreach (var (layer, scope) in _layers())
            layer.Build3D(new Map3DContext(frame, _camera, _scene, scope, _context.Selection));
        if (_cursor is { } cursor && _drag == Drag.None)
            SetHovered(Picking.Pick(_scene, _camera, cursor)?.Primitive, cursor);
        _gl.Invalidate();
    }

    private void ApplyCamera(GameFrame frame)
    {
        if (_drag != Drag.None)
            return;
        switch (Mode)
        {
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
        }
    }

    private void Render()
    {
        if (_glFailed)
            return;
        try
        {
            _gl.MakeCurrent();
            _renderer ??= new GlMapRenderer();
            _renderer.XRay = _xray.Checked;
            _camera.Width = Math.Max(1, _gl.ClientSize.Width);
            _camera.Height = Math.Max(1, _gl.ClientSize.Height);
            _renderer.Render(_scene, _camera, _gl.ClientSize.Width, _gl.ClientSize.Height, _hovered);
            _gl.SwapBuffers();
        }
        catch (Exception e) when (e is InvalidOperationException or DllNotFoundException or EntryPointNotFoundException)
        {
            _glFailed = true;
            _context.ReportError($"3D map unavailable: {e.Message}");
        }
    }

    private void SetMode(CameraMode3D mode)
    {
        if (_mode.SelectedIndex != (int)mode)
            _mode.SelectedIndex = (int)mode;
        _settings.Set(ModeSetting, mode);
        if (mode != CameraMode3D.Game)
            _camera.DetachFromEye();
        Rebuild();
    }

    private void ResetView()
    {
        _camera.Yaw = YawSetting.Default;
        _camera.Pitch = PitchSetting.Default;
        _camera.Distance = DistanceSetting.Default;
        SaveCamera();
        SetMode(CameraMode3D.FollowMario);
    }

    private void SaveCamera()
    {
        _settings.Set(YawSetting, _camera.Yaw);
        _settings.Set(PitchSetting, _camera.Pitch);
        _settings.Set(DistanceSetting, _camera.Distance);
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        _gl.Focus();
        if (e.Button is not (MouseButtons.Left or MouseButtons.Right))
            return;
        _drag = e.Button == MouseButtons.Left ? Drag.Orbit : Drag.Pan;
        _dragStart = _dragLast = new Vector2(e.X, e.Y);
        _dragMoved = false;
        _gl.Capture = true;
        SetHovered(null);
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        var at = new Vector2(e.X, e.Y);
        _cursor = at;
        if (_drag != Drag.None)
        {
            if (Vector2.Distance(at, _dragStart) > 3)
                _dragMoved = true;
            if (_dragMoved)
            {
                var delta = at - _dragLast;
                if (_drag == Drag.Orbit)
                {
                    if (Mode == CameraMode3D.Game)
                        SetMode(CameraMode3D.Free);
                    _camera.Orbit(delta.X, delta.Y);
                }
                else
                {
                    if (Mode != CameraMode3D.Free)
                        SetMode(CameraMode3D.Free);
                    _camera.Pan(delta.X, delta.Y);
                }
                _gl.Invalidate();
            }
            _dragLast = at;
            return;
        }
        SetHovered(Picking.Pick(_scene, _camera, at)?.Primitive, at);
    }

    private void OnMouseUp(object? sender, MouseEventArgs e)
    {
        var drag = _drag;
        _drag = Drag.None;
        _gl.Capture = false;
        if (drag == Drag.None)
            return;
        if (_dragMoved)
        {
            SaveCamera();
            Rebuild();
            return;
        }

        var at = new Vector2(e.X, e.Y);
        var hit = Picking.Pick(_scene, _camera, at);
        if (e.Button == MouseButtons.Right)
        {
            _menuHit = hit;
            foreach (ToolStripItem item in _menu.Items)
                item.Enabled = hit is not null;
            _menu.Show(_gl, e.Location);
            return;
        }
        if (hit?.Primitive.Hit is ObjectHit objectHit)
        {
            if ((ModifierKeys & Keys.Control) != 0)
                _context.Selection.Toggle(objectHit.Object.Address);
            else
                _context.Selection.SelectOnly(objectHit.Object.Address);
            _context.ReportStatus($"Selected {objectHit.Title}.");
        }
        else if (hit is null)
            _context.Selection.Clear();
    }

    private void OnWheel(object? sender, MouseEventArgs e)
    {
        if (Mode == CameraMode3D.Game)
            SetMode(CameraMode3D.Free);
        _camera.Zoom(e.Delta > 0 ? 0.85 : 1 / 0.85);
        SaveCamera();
        _gl.Invalidate();
    }

    private void OnDoubleClick(object? sender, EventArgs e)
    {
        if (_cursor is not { } cursor || Picking.Pick(_scene, _camera, cursor) is not { } hit)
            return;
        SetMode(CameraMode3D.Free);
        _camera.Target = hit.Point;
        _gl.Invalidate();
    }

    private void SetHovered(Primitive3D? primitive, Vector2? at = null)
    {
        var hovered = primitive?.Hit is null ? null : primitive;
        if (ReferenceEquals(hovered, _hovered) || (hovered is not null && _hovered is not null && Equals(hovered.Hit, _hovered.Hit)))
        {
            _hovered = hovered;
            return;
        }
        _hovered = hovered;
        if (hovered?.Hit is { } hit && at is { } p)
            _tip.Show(hit.Title + Environment.NewLine + string.Join(Environment.NewLine, hit.Details), _gl, (int)p.X + 18, (int)p.Y + 18);
        else
            _tip.Hide(_gl);
        _gl.Invalidate();
    }

    private void BuildContextMenu()
    {
        _menu.Items.Add("Teleport Mario to this point", null, (_, _) =>
        {
            if (_menuHit is not { } hit || _context.CurrentFrame is not { } frame)
                return;
            var p = hit.Point;
            _context.Apply(MarioCommands.Teleport(frame, p.X, p.Y, p.Z), $"Mario teleported to ({p.X:0.#}, {p.Y:0.#}, {p.Z:0.#})");
        });
        _menu.Items.Add("Look at this point", null, (_, _) =>
        {
            if (_menuHit is not { } hit)
                return;
            SetMode(CameraMode3D.Free);
            _camera.Target = hit.Point;
            _gl.Invalidate();
        });
        _menu.Items.Add("Copy coordinates", null, (_, _) =>
        {
            if (_menuHit is { } hit)
                Clipboard.SetText(string.Create(CultureInfo.InvariantCulture, $"{hit.Point.X:0.###}, {hit.Point.Y:0.###}, {hit.Point.Z:0.###}"));
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_renderer is not null && !_gl.IsDisposed)
            {
                _gl.MakeCurrent();
                _renderer.Dispose();
            }
            _tip.Dispose();
            _menu.Dispose();
        }
        base.Dispose(disposing);
    }
}
