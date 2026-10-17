using StroopNext.Rendering.Skia;
using StroopNext.Rendering.Gl;
using StroopNext.Rendering.Diagnostics;
using System.Globalization;
using System.Numerics;
using StroopNext.App.Controls;
using StroopNext.App.Map3D;
using StroopNext.App.Modules;
using StroopNext.Game;
using StroopNext.Map;
using StroopNext.Map3D;
using StroopNext.Session;
using StroopNext.Settings;
using StroopNext.Variables;

namespace StroopNext.App.Map;

internal sealed class MapModule : IStroopModule
{
    public string Id => "map";
    public string Title => "Map";
    public ModuleView CreateView(ModuleContext context) => new MapModuleView(context);
}

internal sealed class MapModuleView : ModuleView
{
    private static readonly Setting<double> ScaleSetting = new("scale", "Zoom", 0.08);
    private static readonly Setting<bool> LayerVisible = new("visible", "Visible", true);
    private static readonly Setting<bool> Show3D = new("3d", "3D view", false);

    private enum Drag { None, Pan, Mario }

    private readonly IReadOnlyList<IMapLayer> _layers;
    private readonly SampleTrack<Vector3> _track;
    private readonly Font _boldFont;
    private readonly SettingsScope _viewSettings;
    private readonly MapView _view = new();
    private readonly MapScene _scene = new();
    private readonly SkiaMapRenderer _renderer = new();
    private readonly SkiaCanvas _canvas = new() { Dock = DockStyle.Fill };
    private readonly CheckBox _follow = new() { Text = MapViewSettings.FollowMario.Label, AutoSize = true };
    private readonly ContextMenuStrip _menu = new();
    private readonly Panel _host = new() { Dock = DockStyle.Fill };
    private readonly RadioButton _radio2D = new() { Text = "2D (top-down)", AutoSize = true };
    private readonly RadioButton _radio3D = new() { Text = "3D", AutoSize = true };
    private Control? _options2D;
    private Map3DView? _view3D;

    private MapPrimitive? _hovered;
    private Vector2? _cursor;
    private Drag _drag;
    private Vector2 _dragStart, _dragLast;
    private bool _dragMoved;
    private Vector2 _menuWorld;

    public MapModuleView(ModuleContext context) : base(context)
    {
        _track = context.Session.AddTrack(StandardLayers.CreateMarioPathTrack());
        _layers = StandardLayers.Create(_track);
        _boldFont = new Font(Font, FontStyle.Bold);
        _viewSettings = context.Settings.Child("view");
        _view.Scale = _viewSettings.Get(ScaleSetting);

        _canvas.PaintSkia += OnPaint;
        _canvas.MouseWheel += OnWheel;
        _canvas.MouseDown += OnMouseDown;
        _canvas.MouseMove += OnMouseMove;
        _canvas.MouseUp += OnMouseUp;
        _canvas.MouseLeave += (_, _) => { _cursor = null; _hovered = null; _canvas.Invalidate(); };
        _canvas.DoubleClick += (_, _) => SetFollow(true);
        BuildContextMenu();

        var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel2, SplitterWidth = 6 };
        _host.Controls.Add(_canvas);
        split.Panel1.Controls.Add(_host);
        split.Panel2.Controls.Add(BuildSidePanel());
        SetProjection(_viewSettings.Get(Show3D));
        Controls.Add(split);
        split.HandleCreated += (_, _) => split.SplitterDistance = Math.Max(300, split.Width - 330);

        context.Selection.Changed += Rebuild;
    }

    private bool _is3D;
    private bool Is3D => _is3D && _view3D is not null;

    public override void OnFrame(GameFrame frame)
    {
        if (Is3D)
        {
            _view3D!.OnFrame(frame);
            return;
        }
        if (_drag == Drag.None)
            MapViewSettings.Apply(_view, frame, _follow.Checked, _viewSettings.Get(MapViewSettings.Orientation));
        Rebuild(frame);
    }

    private void Rebuild()
    {
        if (Is3D)
            _view3D!.Rebuild();
        else if (Context.CurrentFrame is { } frame)
            Rebuild(frame);
        else
            _canvas.Invalidate();
    }

    private void SetProjection(bool is3D)
    {
        _is3D = is3D;
        _viewSettings.Set(Show3D, is3D);
        if (is3D && _view3D is null)
        {
            _view3D = new Map3DView(Context, Context.Settings.Child("view3d"), VisibleLayers3D);
            _host.Controls.Add(_view3D);
        }
        _canvas.Visible = !is3D;
        if (_view3D is not null)
            _view3D.Visible = is3D;
        if (_options2D is not null)
            _options2D.Visible = !is3D;
        _radio2D.Checked = !is3D;
        _radio3D.Checked = is3D;
        Rebuild();
    }

    private IEnumerable<(IMapLayer3D, SettingsScope)> VisibleLayers3D()
    {
        foreach (var layer in _layers)
        {
            var scope = LayerScope(layer);
            if (layer is IMapLayer3D layer3D && scope.Get(VisibleSetting(layer)))
                yield return (layer3D, scope);
        }
    }

    private void Rebuild(GameFrame frame)
    {
        _view.Width = Math.Max(1, _canvas.Width);
        _view.Height = Math.Max(1, _canvas.Height);
        _view.CullMarginPx = 0.25f * Math.Max(_view.Width, _view.Height);
        _scene.Clear();
        foreach (var layer in _layers)
        {
            var scope = LayerScope(layer);
            if (scope.Get(VisibleSetting(layer)))
                layer.Build(new MapContext(frame, _view, _scene, scope, Context.Selection));
        }
        _hovered = _cursor is { } c && _drag == Drag.None ? _scene.HitTest(_view, c) : null;
        _canvas.Invalidate();
    }

    private SettingsScope LayerScope(IMapLayer layer) => Context.Settings.Child($"layer.{layer.Id}");
    private static Setting<bool> VisibleSetting(IMapLayer layer) => new(LayerVisible.Key, LayerVisible.Label, layer.VisibleByDefault);

    private void OnPaint(SkiaSharp.SKCanvas canvas, int width, int height)
    {
        _view.Width = width;
        _view.Height = height;
        if (Context.CurrentFrame is null)
        {
            canvas.Clear(SkiaMapRenderer.Background);
            using var paint = new SkiaSharp.SKPaint { Color = SkiaSharp.SKColors.Gray, IsAntialias = true };
            using var font = new SkiaSharp.SKFont(SkiaSharp.SKTypeface.Default, 16);
            canvas.DrawText("Connect to an emulator or open a savestate (toolbar above).", 24, 40, font, paint);
            return;
        }
        _renderer.Draw(canvas, _scene, _view, _hovered);
        if (_cursor is { } cursor)
        {
            _renderer.DrawCursorReadout(canvas, _view, _view.ScreenToWorld(cursor));
            if (_hovered?.Hit is { } hit && _drag == Drag.None)
                _renderer.DrawTooltip(canvas, _view, cursor, hit);
        }
    }

    private void OnWheel(object? sender, MouseEventArgs e)
    {
        double factor = e.Delta > 0 ? 1.25 : 0.8;
        var anchor = _follow.Checked ? new Vector2(_view.Width / 2, _view.Height / 2) : new Vector2(e.X, e.Y);
        _view.ZoomAt(anchor, factor);
        _viewSettings.Set(ScaleSetting, _view.Scale);
        Rebuild();
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        var at = new Vector2(e.X, e.Y);
        if (e.Button == MouseButtons.Right)
        {
            _menuWorld = _view.ScreenToWorld(at);
            _menu.Show(_canvas, e.Location);
            return;
        }
        if (e.Button != MouseButtons.Left)
            return;
        _drag = _scene.HitTest(_view, at)?.Hit is MarioHit ? Drag.Mario : Drag.Pan;
        _dragStart = _dragLast = at;
        _dragMoved = false;
        _canvas.Capture = true;
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        var at = new Vector2(e.X, e.Y);
        _cursor = at;
        if (_drag != Drag.None && Vector2.Distance(at, _dragStart) > 3)
            _dragMoved = true;

        switch (_drag)
        {
            case Drag.Pan when _dragMoved:
                if (_follow.Checked)
                    SetFollow(false);
                _view.PanByScreen(at - _dragLast);
                Rebuild();
                break;
            case Drag.Mario when _dragMoved && Context.CurrentFrame is { } frame:
                var world = _view.ScreenToWorld(at);
                Context.Apply(MarioCommands.Teleport(frame, world.X, null, world.Y), $"Mario dragged to ({world.X:0.#}, {world.Y:0.#})");
                break;
            case Drag.None:
                var hovered = _scene.HitTest(_view, at);
                _hovered = hovered;
                _canvas.Invalidate();
                break;
        }
        _dragLast = at;
    }

    private void OnMouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;
        _canvas.Capture = false;
        bool click = _drag == Drag.Pan && !_dragMoved;
        _drag = Drag.None;
        if (!click)
            return;

        var hit = _scene.HitTest(_view, new Vector2(e.X, e.Y))?.Hit;
        if (hit is ObjectHit objectHit)
        {
            if ((ModifierKeys & Keys.Control) != 0)
                Context.Selection.Toggle(objectHit.Object.Address);
            else
                Context.Selection.SelectOnly(objectHit.Object.Address);
            Context.ReportStatus($"Selected {objectHit.Title}.");
        }
        else if (hit is null)
            Context.Selection.Clear();
    }

    private void SetFollow(bool follow)
    {
        _follow.Checked = follow;
        Rebuild();
    }

    private void BuildContextMenu()
    {
        _menu.Items.Add("Teleport Mario here (keep Y)", null, (_, _) => Teleport(onFloor: false));
        _menu.Items.Add("Teleport Mario onto the floor here", null, (_, _) => Teleport(onFloor: true));
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add("Center map here", null, (_, _) =>
        {
            SetFollow(false);
            _view.Center = _menuWorld;
            Rebuild();
        });
        _menu.Items.Add("Copy coordinates", null, (_, _) =>
            Clipboard.SetText(string.Create(CultureInfo.InvariantCulture, $"{_menuWorld.X:0.###}, {_menuWorld.Y:0.###}")));
    }

    private void Teleport(bool onFloor)
    {
        if (Context.CurrentFrame is not { Mario: { } mario } frame)
            return;
        float x = _menuWorld.X, z = _menuWorld.Y;
        float? y = null;
        if (onFloor)
        {
            var heights = frame.Surfaces.All
                .Where(s => s.Class == SurfaceClass.Floor && s.ContainsXZ(x, z))
                .Select(s => s.HeightAt(x, z))
                .ToList();
            if (heights.Count == 0)
            {
                Context.ReportError("No floor under that point.");
                return;
            }
            var reachable = heights.Where(h => h <= mario.Position.Y + 100).ToList();
            y = reachable.Count > 0 ? reachable.Max() : heights.Max();
        }
        Context.Apply(MarioCommands.Teleport(frame, x, y, z), $"Mario teleported to ({x:0.#}, {y?.ToString("0.#", CultureInfo.InvariantCulture) ?? "same Y"}, {z:0.#})");
    }

    private Control BuildSidePanel()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(6) };

        panel.Controls.Add(new Label { Text = "View", Font = _boldFont, AutoSize = true, Margin = new Padding(3, 3, 3, 0) });
        var projection = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
        projection.Controls.AddRange([_radio2D, _radio3D]);
        _radio3D.CheckedChanged += (_, _) =>
        {
            if (_radio3D.Checked != Is3D)
                SetProjection(_radio3D.Checked);
        };
        panel.Controls.Add(projection);

        _follow.Checked = _viewSettings.Get(MapViewSettings.FollowMario);
        _follow.CheckedChanged += (_, _) => _viewSettings.Set(MapViewSettings.FollowMario, _follow.Checked);
        var options2D = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Margin = new Padding(0) };
        options2D.Controls.Add(_follow);
        options2D.Controls.Add(SettingsEditor.Build([MapViewSettings.Orientation], _viewSettings, Rebuild));
        options2D.Controls.Add(new Label
        {
            Text = "Wheel: zoom · drag: pan · double-click: follow Mario\nDrag Mario to move him · click an object to select it (Ctrl: add)\nRight-click: teleport, center, copy coordinates",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Margin = new Padding(3, 4, 3, 10),
        });
        _options2D = options2D;
        panel.Controls.Add(options2D);

        panel.Controls.Add(new Label { Text = "Layers", Font = _boldFont, AutoSize = true });
        foreach (var layer in _layers.Reverse())
            panel.Controls.Add(BuildLayerRow(layer));
        return panel;
    }

    private Control BuildLayerRow(IMapLayer layer)
    {
        var scope = LayerScope(layer);
        var visible = VisibleSetting(layer);
        var container = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Margin = new Padding(0) };
        var header = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
        var check = new CheckBox { Text = layer.Name, Checked = scope.Get(visible), AutoSize = true, Margin = new Padding(3, 5, 3, 3) };
        check.CheckedChanged += (_, _) =>
        {
            scope.Set(visible, check.Checked);
            Rebuild();
        };
        header.Controls.Add(check);

        if (layer.Settings.Count > 0)
        {
            var editor = SettingsEditor.Build(layer.Settings, scope, Rebuild);
            editor.Visible = false;
            var toggle = new Button { Text = "options ▾", AutoSize = true, FlatStyle = FlatStyle.Flat, Margin = new Padding(3, 1, 3, 1) };
            toggle.FlatAppearance.BorderSize = 0;
            toggle.Click += (_, _) =>
            {
                editor.Visible = !editor.Visible;
                toggle.Text = editor.Visible ? "options ▴" : "options ▾";
            };
            header.Controls.Add(toggle);
            container.Controls.Add(header);
            container.Controls.Add(editor);
        }
        else
            container.Controls.Add(header);
        return container;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Context.Selection.Changed -= Rebuild;
            Context.Session.RemoveTrack(_track);
            _renderer.Dispose();
            _menu.Dispose();
            _boldFont.Dispose();
        }
        base.Dispose(disposing);
    }
}
