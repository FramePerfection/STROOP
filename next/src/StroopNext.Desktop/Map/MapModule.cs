using System.Globalization;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using StroopNext.Desktop.Controls;
using StroopNext.Desktop.Modules;
using StroopNext.Game;
using StroopNext.Map;
using StroopNext.Map3D;
using StroopNext.Rendering.Skia;
using StroopNext.Session;
using StroopNext.Settings;
using StroopNext.Variables;

namespace StroopNext.Desktop.Map;

internal sealed class MapModule(Engine.EngineService? engine = null) : IStroopModule
{
    public string Id => "map";
    public string Title => "Map";
    public ModuleView CreateView(ModuleContext context) => new MapModuleView(context, engine);
}

internal sealed partial class MapModuleView : ModuleView, IDisposable
{
    private static readonly Setting<double> ScaleSetting = new("scale", "Zoom", 0.08);
    private static readonly Setting<bool> LayerVisible = new("visible", "Visible", true);
    private static readonly Setting<bool> Show3D = new("3d", "3D view", false);

    private enum Drag { None, Pan, Mario, Draw }

    private MapToolsLayer Tools => _layers.OfType<MapToolsLayer>().First();

    internal Map3DView? View3D => _view3D;

    private readonly IReadOnlyList<IMapLayer> _layers;
    private readonly SampleTrack<Vector3> _track;
    private readonly SettingsScope _viewSettings;
    private readonly MapView _view = new();
    private readonly MapScene _scene = new();
    private readonly SkiaMapRenderer _renderer = new() { UseOffscreenGl = false };
    private readonly SkiaSurface _canvas = new();
    private readonly CheckBox _follow = new() { Content = MapViewSettings.FollowMario.Label };
    private readonly Panel _host = new();
    private readonly RadioButton _radio2D = new() { Content = "2D (top-down)", GroupName = "projection" };
    private readonly RadioButton _radio3D = new() { Content = "3D", GroupName = "projection" };
    private Control? _options2D;
    private Map3DView? _view3D;

    private MapPrimitive? _hovered;
    private Vector2? _cursor;
    private Drag _drag;
    private Vector2 _dragStart, _dragLast;
    private bool _dragMoved;
    private Vector2 _menuWorld;
    private IMapHit? _menuHit;
    private bool _is3D;
    private bool _disposed;

    public MapModuleView(ModuleContext context, Engine.EngineService? engine = null) : base(context)
    {
        _engine = engine;
        _track = context.Session.AddTrack(StandardLayers.CreateMarioPathTrack());
        _layers = StandardLayers.Create(_track);
        Tools.Changed += Rebuild;
        _viewSettings = context.Settings.Child("view");
        _view.Scale = _viewSettings.Get(ScaleSetting);

        _canvas.PaintSkia = Paint;
        _canvas.PointerWheelChanged += OnWheel;
        _canvas.PointerPressed += OnPointerPressed;
        _canvas.PointerMoved += OnPointerMoved;
        _canvas.PointerReleased += OnPointerReleased;
        _canvas.PointerExited += (_, _) =>
        {
            _cursor = null;
            _hovered = null;
            _canvas.InvalidateVisual();
        };
        _canvas.DoubleTapped += (_, e) => OnMapDoubleTapped(e);
        _canvas.KeyDown += (_, e) =>
        {
            if (e.Key != Key.Escape || _placement is null)
                return;
            EndPlacement();
            Context.ReportStatus("Placing Mario after the level loads: cancelled.");
            e.Handled = true;
        };
        _canvas.SizeChanged += (_, _) => Rebuild();

        _host.Children.Add(_canvas);
        var side = new ScrollViewer { Content = BuildSidePanel() };
        var layout = new Grid { ColumnDefinitions = new ColumnDefinitions("*,6,330") };
        Grid.SetColumn(_host, 0);
        var splitter = new GridSplitter { ResizeDirection = GridResizeDirection.Columns };
        Grid.SetColumn(splitter, 1);
        Grid.SetColumn(side, 2);
        layout.Children.AddRange([_host, splitter, side]);
        Content = layout;

        SetProjection(_viewSettings.Get(Show3D));
        context.Selection.Changed += Rebuild;
        if (engine is not null)
        {
            engine.GhostsChanged += SyncEngineGhosts;
            engine.Animated += () =>
            {
                if (IsEffectivelyVisible)
                    Rebuild();
            };
            SyncEngineGhosts();
        }
    }

    private readonly Engine.EngineService? _engine;
    private Engine.EnginePanel? _enginePanel;
    private static readonly Setting<bool> EngineOpen = new("engineOpen", "Engine panel open", true);

    internal Engine.EnginePanel? EnginePanel => _enginePanel;

    private Control BuildEngineSection(Engine.EngineService engine)
    {
        _enginePanel = new Engine.EnginePanel(Context, engine);
        var expander = new Expander
        {
            Header = new TextBlock { Text = "ENGINE (SM64 TAS engine)", Classes = { "title" } },
            Content = _enginePanel,
            IsExpanded = Context.Settings.Get(EngineOpen),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 6, 0, 0),
        };
        expander.Expanded += (_, _) => Context.Settings.Set(EngineOpen, true);
        expander.Collapsed += (_, _) => Context.Settings.Set(EngineOpen, false);
        return expander;
    }
    private readonly Dictionary<string, (Engine.EngineGhost Source, GhostEntry Entry)> _engineEntries = new();

    private void SyncEngineGhosts()
    {
        if (_engine is null)
            return;
        using var batch = GhostLayer.Batch();
        var current = _engine.Ghosts.ToDictionary(g => g.Key);
        foreach (var (key, (source, entry)) in _engineEntries.ToList())
            if (!current.TryGetValue(key, out var now) || !ReferenceEquals(now, source))
            {
                GhostLayer.Remove(entry);
                _engineEntries.Remove(key);
                if (now is not null)
                {
                    var replacement = AddEngineGhost(now);
                    replacement.Visible = entry.Visible;
                    replacement.ShowPath = entry.ShowPath;
                    replacement.Color = entry.Color;
                }
            }
        foreach (var g in current.Values)
            if (!_engineEntries.ContainsKey(g.Key))
                AddEngineGhost(g);
    }

    private GhostEntry AddEngineGhost(Engine.EngineGhost g)
    {
        var entry = GhostLayer.Add(g.Ghost);
        entry.Color = g.Color;
        var engine = _engine!;
        entry.PreviewFrame = () => engine.PreviewIndex(g);
        entry.Bold = g.Key == "path";
        entry.IsMarker = g.Marker;
        entry.IsLight = g.Light;
        entry.ShowPath = !g.Marker;
        _engineEntries[g.Key] = (g, entry);
        GhostLayer.NotifyChanged();
        return entry;
    }

    private void AddEngineMenuItem(ContextMenu menu, Vector3 point)
    {
        if (_engine is not { } engine)
            return;
        string? notReady = engine.PathBusy ? "The engine is already working on the path: wait, or Stop (ENGINE section)."
            : !engine.ServerUp ? "The engine server is not running (ENGINE section)."
            : !engine.BridgeUp ? "Not connected to Mupen yet (ENGINE section)."
            : null;
        var item = MenuItem("Bring Mario here (engine)", async () =>
        {
            await engine.BringHereAsync(point.X, point.Y, point.Z);
        });
        item.IsEnabled = notReady is null;
        ToolTip.SetShowOnDisabled(item, true);
        ToolTip.SetTip(item, notReady ?? "The engine finds the fastest way to this point, adds it to the path and Mupen plays it (ENGINE section: Back, Save movie).");
        int at = 0;
        menu.Items.Insert(at++, item);
        if (engine.PathParts > 0)
            menu.Items.Insert(at++, MenuItem("↶ Back one part (engine)", async () => await engine.BackAsync()));
        menu.Items.Insert(at, new Separator());
    }

    // Double-click: on an object the engine brings Mario to it; anywhere else the map follows Mario again.
    private void OnMapDoubleTapped(Avalonia.Input.TappedEventArgs e)
    {
        IMapHit? hit;
        lock (_canvas.SyncRoot)
            hit = _scene.HitTest(_view, _canvas.ToPixels(e.GetPosition(_canvas)))?.Hit;
        if (hit is not ObjectHit objectHit || !BringMarioTo(objectHit))
            SetFollow(true);
    }

    // The engine brings Mario to the object; false when there is no engine (the double-click then does its usual thing).
    private bool BringMarioTo(ObjectHit objectHit)
    {
        if (_engine is not { } engine)
            return false;
        if (engine.PathBusy || !engine.ServerUp || !engine.BridgeUp)
        {
            Context.ReportError(engine.PathBusy ? "The engine is already working on the path: wait, or Stop (ENGINE section)."
                : !engine.ServerUp ? "The engine server is not running (ENGINE section)." : "Not connected to Mupen yet (ENGINE section).");
            return true;
        }
        var p = objectHit.Object.Position;
        Context.ReportStatus($"The engine looks for the way to {objectHit.Title}.");
        _ = engine.BringHereAsync(p.X, p.Y, p.Z);
        return true;
    }

    private bool Is3D => _is3D && _view3D is not null;

    public void Refresh() => Rebuild();

    internal void TeleportForTest(float x, float z)
    {
        _menuWorld = new Vector2(x, z);
        Teleport(onFloor: true);
    }

    internal void Teleport3DForTest(float x, float z)
    {
        if (Context.CurrentFrame is not { } frame)
            return;
        if (Picking.TeleportTarget(frame.Surfaces, new Ray(new Vector3(x, 30000, z), -Vector3.UnitY)) is { } p)
            Context.Apply(MarioCommands.Teleport(frame, p.X, p.Y, p.Z), $"Mario teleported to ({p.X:0.#}, {p.Y:0.#}, {p.Z:0.#})");
        else
            Context.ReportError("No floor there.");
    }

    internal float GroundY(float x, float z)
    {
        float y = Context.CurrentFrame?.Mario?.Position.Y ?? 0;
        return Context.CurrentFrame is { } frame && GameCollision.FindFloor(frame.Surfaces, x, y + 100, z) is { } floor ? floor.Height : y;
    }

    internal string MarioText => Context.CurrentFrame?.Mario is { } m
        ? string.Create(CultureInfo.InvariantCulture, $"Mario ({m.Position.X:0.#}, {m.Position.Y:0.#}, {m.Position.Z:0.#}) timer {Context.CurrentFrame.GlobalTimer}")
        : "no Mario";

    public bool Projection3D
    {
        get => _is3D;
        set => SetProjection(value);
    }

    public override void OnFrame(GameFrame frame)
    {
        UpdatePlacement(frame);
        RefreshLevelSection();
        if (Is3D)
        {
            _view3D!.OnFrame(frame);
            return;
        }
        if (_drag == Drag.None)
            lock (_canvas.SyncRoot)
                MapViewSettings.Apply(_view, frame, _follow.IsChecked == true, _viewSettings.Get(MapViewSettings.Orientation), _viewSettings.Get(MapViewSettings.CustomYaw));
        Rebuild(frame);
        UpdateInfo(frame);
    }

    private readonly TextBlock _info = new() { Classes = { "mono" }, FontSize = 12, TextWrapping = TextWrapping.Wrap };

    private void UpdateInfo(GameFrame frame)
    {
        if (frame.Mario is not { } m)
        {
            _info.Text = "";
            return;
        }
        int puX = MarioPrediction.ParallelUniverse(m.Position.X), puZ = MarioPrediction.ParallelUniverse(m.Position.Z);
        int puY = MarioPrediction.ParallelUniverse(m.Position.Y);
        string floor = frame.Surfaces.Find(m.FloorTri) is { } tri ? $"0x{tri.Address:X8} · normal Y {tri.Normal.Y:0.####}" : "none";
        string text = string.Create(CultureInfo.InvariantCulture,
            $"PU      [{puX}:{puY}:{puZ}]   QPU [{puX / 4.0:0.##}:{puZ / 4.0:0.##}]\n" +
            $"Level   {frame.LevelIndex} · area {frame.AreaIndex}\n" +
            $"Unit    ({GameCollision.Truncate(m.Position.X)}, {GameCollision.Truncate(m.Position.Z)})   cell ({MarioPrediction.Cell(m.Position.X)}, {MarioPrediction.Cell(m.Position.Z)})\n" +
            $"Floor   {floor}");
        if (_info.Text != text)
            _info.Text = text;
    }

    private void Rebuild()
    {
        if (Is3D)
            _view3D!.Rebuild();
        else if (Context.CurrentFrame is { } frame)
            Rebuild(frame);
        else
            _canvas.InvalidateVisual();
    }

    private void SetProjection(bool is3D)
    {
        _is3D = is3D;
        _viewSettings.Set(Show3D, is3D);
        if (is3D && _view3D is null)
        {
            _view3D = new Map3DView(Context, Context.Settings.Child("view3d"), VisibleLayers3D) { ExtendMenu = (menu, hit, point) => { AddToolMenuItems(menu, point); AddTrackMenuItems(menu, hit, point); AddEngineMenuItem(menu, point); }, Placing = () => PlacementArmed, PlaceAt = PlaceMario3D, ObjectDoubleClicked = BringMarioTo };
            _host.Children.Add(_view3D);
        }
        _canvas.IsVisible = !is3D;
        if (_view3D is not null)
            _view3D.IsVisible = is3D;
        if (_options2D is not null)
            _options2D.IsVisible = !is3D;
        _radio2D.IsChecked = !is3D;
        _radio3D.IsChecked = is3D;
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
        lock (_canvas.SyncRoot)
        {
            _view.Width = _canvas.PixelWidth;
            _view.Height = _canvas.PixelHeight;
            _view.CullMarginPx = 0.25f * Math.Max(_view.Width, _view.Height);
            _scene.Clear();
            foreach (var layer in _layers)
            {
                var scope = LayerScope(layer);
                if (scope.Get(VisibleSetting(layer)))
                    layer.Build(new MapContext(frame, _view, _scene, scope, Context.Selection));
            }
            _hovered = _cursor is { } c && _drag == Drag.None ? _scene.HitTest(_view, c) : null;
        }
        _canvas.InvalidateVisual();
    }

    private SettingsScope LayerScope(IMapLayer layer) => Context.Settings.Child($"layer.{layer.Id}");
    private static Setting<bool> VisibleSetting(IMapLayer layer) => new(LayerVisible.Key, LayerVisible.Label, layer.VisibleByDefault);

    private void Paint(SkiaSharp.SKCanvas canvas, int width, int height, SkiaSharp.GRContext? gpu)
    {
        if (_disposed)
            return;
        _renderer.GpuContext = gpu;
        if (Context.Session.Source is null)
        {
            canvas.Clear(SkiaMapRenderer.Background);
            using var paint = new SkiaSharp.SKPaint { Color = SkiaSharp.SKColors.Gray, IsAntialias = true };
            using var font = new SkiaSharp.SKFont(SkiaSharp.SKTypeface.Default, 16 * (float)_canvas.Scaling);
            canvas.DrawText("Connect to an emulator or open a savestate (toolbar above).", 24, 40, font, paint);
            return;
        }
        _view.Width = width;
        _view.Height = height;
        _renderer.Draw(canvas, _scene, _view, _hovered);
        if (_cursor is { } cursor)
        {
            _renderer.DrawCursorReadout(canvas, _view, _view.ScreenToWorld(cursor));
            if (_hovered?.Hit is { } hit && _drag == Drag.None)
                _renderer.DrawTooltip(canvas, _view, cursor, hit);
        }
    }

    private void OnWheel(object? sender, PointerWheelEventArgs e)
    {
        double factor = e.Delta.Y > 0 ? 1.25 : 0.8;
        lock (_canvas.SyncRoot)
        {
            var anchor = _follow.IsChecked == true ? new Vector2(_view.Width / 2, _view.Height / 2) : _canvas.ToPixels(e.GetPosition(_canvas));
            _view.ZoomAt(anchor, factor);
        }
        _viewSettings.Set(ScaleSetting, _view.Scale);
        Rebuild();
        e.Handled = true;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _canvas.Focus();
        var point = e.GetCurrentPoint(_canvas);
        var at = _canvas.ToPixels(point.Position);
        if (point.Properties.IsRightButtonPressed)
        {
            lock (_canvas.SyncRoot)
            {
                _menuWorld = _view.ScreenToWorld(at);
                _menuHit = _scene.HitTest(_view, at)?.Hit;
            }
            ShowMenu();
            return;
        }
        if (!point.Properties.IsLeftButtonPressed)
            return;
        if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
        {
            _drag = Drag.Draw;
            Vector2 start;
            lock (_canvas.SyncRoot)
                start = _view.ScreenToWorld(at);
            Tools.BeginStroke(start);
            _dragStart = _dragLast = at;
            _dragMoved = true;
            e.Pointer.Capture(_canvas);
            return;
        }
        lock (_canvas.SyncRoot)
            _drag = _scene.HitTest(_view, at)?.Hit is MarioHit ? Drag.Mario : Drag.Pan;
        _dragStart = _dragLast = at;
        _dragMoved = false;
        e.Pointer.Capture(_canvas);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var at = _canvas.ToPixels(e.GetPosition(_canvas));
        _cursor = at;
        if (_drag != Drag.None && Vector2.Distance(at, _dragStart) > 3)
            _dragMoved = true;

        switch (_drag)
        {
            case Drag.Pan when _dragMoved:
                if (_follow.IsChecked == true)
                    SetFollow(false);
                lock (_canvas.SyncRoot)
                    _view.PanByScreen(at - _dragLast);
                Rebuild();
                break;
            case Drag.Draw:
                Vector2 point;
                float spacing;
                lock (_canvas.SyncRoot)
                {
                    point = _view.ScreenToWorld(at);
                    spacing = (float)(2 / _view.Scale);
                }
                Tools.ExtendStroke(point, spacing);
                break;
            case Drag.Mario when _dragMoved && Context.CurrentFrame is { } frame:
                Vector2 world;
                lock (_canvas.SyncRoot)
                    world = _view.ScreenToWorld(at);
                Context.Apply(MarioCommands.Teleport(frame, world.X, null, world.Y), $"Mario dragged to ({world.X:0.#}, {world.Y:0.#})");
                break;
            case Drag.None:
                lock (_canvas.SyncRoot)
                    _hovered = _scene.HitTest(_view, at);
                _canvas.InvalidateVisual();
                break;
        }
        _dragLast = at;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Left)
            return;
        e.Pointer.Capture(null);
        bool click = _drag == Drag.Pan && !_dragMoved;
        _drag = Drag.None;
        if (!click)
            return;

        IMapHit? hit;
        Vector2 world;
        lock (_canvas.SyncRoot)
        {
            var at = _canvas.ToPixels(e.GetPosition(_canvas));
            hit = _scene.HitTest(_view, at)?.Hit;
            world = _view.ScreenToWorld(at);
        }
        if (PlaceMario2D(world))
            return;
        if (hit is ObjectHit objectHit)
        {
            if ((e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Meta)) != 0)
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
        _follow.IsChecked = follow;
        Rebuild();
    }

    private void ShowMenu()
    {
        var menu = new ContextMenu();
        menu.Items.Add(MenuItem("Teleport Mario here (keep Y)", () => Teleport(onFloor: false)));
        menu.Items.Add(MenuItem("Teleport Mario onto the floor here", () => Teleport(onFloor: true)));
        menu.Items.Add(new Separator());
        menu.Items.Add(MenuItem("Center map here", () =>
        {
            SetFollow(false);
            lock (_canvas.SyncRoot)
                _view.Center = _menuWorld;
            Rebuild();
        }));
        menu.Items.Add(MenuItem("Copy coordinates", async () =>
        {
            if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
                await clipboard.SetTextAsync(string.Create(CultureInfo.InvariantCulture, $"{_menuWorld.X:0.###}, {_menuWorld.Y:0.###}"));
        }));
        float y = Context.CurrentFrame?.Mario?.Position.Y ?? 0;
        if (Context.CurrentFrame is { } frame && GameCollision.FindFloor(frame.Surfaces, _menuWorld.X, y + 100, _menuWorld.Y) is { } floor)
            y = floor.Height;
        var here = new Vector3(_menuWorld.X, y, _menuWorld.Y);
        AddToolMenuItems(menu, here);
        AddTrackMenuItems(menu, _menuHit, here);
        AddEngineMenuItem(menu, here);
        menu.Open(_canvas);
    }

    private void AddToolMenuItems(ContextMenu menu, Vector3 here)
    {
        menu.Items.Add(new Separator());
        var measure = new MenuItem { Header = "Tape measure" };
        measure.Items.Add(MenuItem("Measure from here", () =>
        {
            Tools.SetStart(new MeasureEnd(here));
            Context.ReportStatus("Tape measure: now right-click the end point and choose \"Measure to here\".");
        }));
        if (Tools.Start is not null)
            measure.Items.Add(MenuItem("Measure to here", () => ReportMeasure(() => Tools.SetEnd(new MeasureEnd(here)))));
        measure.Items.Add(MenuItem("Measure from Mario to here (follows Mario)", () => ReportMeasure(() =>
        {
            Tools.SetStart(MeasureEnd.Mario);
            Tools.SetEnd(new MeasureEnd(here));
        })));
        if (Tools.Start is not null || Tools.End is not null)
            measure.Items.Add(MenuItem("Clear measurement", Tools.ClearMeasure));
        menu.Items.Add(measure);
        if (Tools.Strokes.Count > 0)
            menu.Items.Add(MenuItem("Clear drawings (Shift + drag draws)", Tools.ClearDrawings));
    }

    private void ReportMeasure(Action set)
    {
        set();
        if (Context.CurrentFrame is { } frame && Tools.Measure(frame) is { } m)
            Context.ReportStatus($"Tape measure: {m.Summary} · ΔY {m.DY:0.###} · yaw {m.Yaw}");
    }

    private static MenuItem MenuItem(string header, Action onClick)
    {
        var item = new MenuItem { Header = header };
        item.Click += (_, _) => onClick();
        return item;
    }

    private bool Teleport(bool onFloor)
    {
        if (Context.CurrentFrame is not { Mario: { } mario } frame)
            return false;
        float x = _menuWorld.X, z = _menuWorld.Y;
        float? y = null;
        if (onFloor)
        {
            if (GameCollision.FindFloor(frame.Surfaces, x, mario.Position.Y, z) is { } found)
                y = found.Height;
            else if (GameCollision.FindFloor(frame.Surfaces, x, 1e6f, z) is { } highest)
                y = highest.Height;
            else
            {
                Context.ReportError("No floor under that point.");
                return false;
            }
        }
        return Context.Apply(MarioCommands.Teleport(frame, x, y, z), $"Mario teleported to ({x:0.#}, {y?.ToString("0.#", CultureInfo.InvariantCulture) ?? "same Y"}, {z:0.#})");
    }

    private Control BuildSidePanel()
    {
        var panel = new StackPanel { Spacing = 4, Margin = new Thickness(8) };

        panel.Children.Add(new TextBlock { Text = "VIEW", Classes = { "title" } });
        var projection = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        projection.Children.AddRange([_radio2D, _radio3D]);
        _radio3D.IsCheckedChanged += (_, _) =>
        {
            if (_radio3D.IsChecked is bool is3D && is3D != _is3D)
                SetProjection(is3D);
        };
        panel.Children.Add(projection);

        _follow.IsChecked = _viewSettings.Get(MapViewSettings.FollowMario);
        _follow.IsCheckedChanged += (_, _) => _viewSettings.Set(MapViewSettings.FollowMario, _follow.IsChecked == true);
        var options2D = new StackPanel { Spacing = 4 };
        options2D.Children.Add(_follow);
        options2D.Children.Add(SettingsEditor.Build([MapViewSettings.Orientation, MapViewSettings.CustomYaw], _viewSettings, Rebuild));

        var rotate = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        rotate.Children.AddRange([
            SmallButton("⟲ 45°", () => Rotate(8192)),
            SmallButton("⟳ 45°", () => Rotate(-8192)),
            SmallButton("North", () => SetUpYaw(32768)),
        ]);
        options2D.Children.Add(rotate);
        var zoom = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        zoom.Children.Add(SmallButton("Level", FitLevel, "The whole ±8192 level"));
        foreach (int px in new[] { 1, 4, 12, 40 })
            zoom.Children.Add(SmallButton($"{px} px/u", () => SetScale(px), $"{px} pixels per game unit"));
        options2D.Children.Add(zoom);
        options2D.Children.Add(new TextBlock
        {
            Text = "Wheel: zoom · drag: pan · double-click: follow Mario (on an object: the engine brings Mario there)\nDrag Mario to move him · click an object to select it (Ctrl/⌘: add)\nRight-click: teleport, center, copy coordinates",
            Opacity = 0.65,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 2, 0, 10),
        });
        _options2D = options2D;
        panel.Children.Add(options2D);
        panel.Children.Add(BuildLevelSection());

        panel.Children.Add(new TextBlock { Text = "WHERE", Classes = { "title" }, Margin = new Thickness(0, 6, 0, 0) });
        panel.Children.Add(_info);
        panel.Children.Add(BuildTrackersSection());
        panel.Children.Add(BuildGhostsSection());
        if (_engine is not null)
            panel.Children.Add(BuildEngineSection(_engine));
        panel.Children.Add(new TextBlock { Text = "LAYERS", Classes = { "title" }, Margin = new Thickness(0, 6, 0, 0) });
        foreach (var layer in _layers.Reverse())
            panel.Children.Add(BuildLayerRow(layer));
        return panel;
    }

    private static Button SmallButton(string text, Action onClick, string? tip = null)
    {
        var button = new Button { Content = text, Padding = new Thickness(8, 3), FontSize = 12 };
        button.Click += (_, _) => onClick();
        if (tip is not null)
            ToolTip.SetTip(button, tip);
        return button;
    }

    private void SetUpYaw(int yaw)
    {
        _viewSettings.Set(MapViewSettings.CustomYaw, (int)(ushort)yaw);
        _viewSettings.Set(MapViewSettings.Orientation, MapOrientation.Custom);
        lock (_canvas.SyncRoot)
            _view.PointYawUp((ushort)yaw);
        Rebuild();
    }

    private void Rotate(int delta)
    {
        double upYaw = _view.Rotation * Angle.UnitsPerTurn / (2 * Math.PI) - 32768;
        SetUpYaw((int)Math.Round(upYaw) + delta);
    }

    private void FitLevel()
    {
        SetFollow(false);
        lock (_canvas.SyncRoot)
        {
            _view.Center = Vector2.Zero;
            _view.Scale = 0.95 * Math.Min(_view.Width, _view.Height) / 16384;
        }
        _viewSettings.Set(ScaleSetting, _view.Scale);
        Rebuild();
    }

    private void SetScale(double pixelsPerUnit)
    {
        lock (_canvas.SyncRoot)
            _view.Scale = pixelsPerUnit * _canvas.Scaling;
        _viewSettings.Set(ScaleSetting, _view.Scale);
        Rebuild();
    }

    private Control BuildLayerRow(IMapLayer layer)
    {
        var scope = LayerScope(layer);
        var visible = VisibleSetting(layer);
        var container = new StackPanel();
        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        var check = new CheckBox { Content = layer.Name, IsChecked = scope.Get(visible) };
        check.IsCheckedChanged += (_, _) =>
        {
            scope.Set(visible, check.IsChecked == true);
            Rebuild();
        };
        header.Children.Add(check);
        container.Children.Add(header);

        if (layer.Settings.Count > 0)
        {
            var editor = SettingsEditor.Build(layer.Settings, scope, Rebuild);
            editor.IsVisible = false;
            var toggle = new Button { Content = "options ▾", Background = Brushes.Transparent, Padding = new Thickness(6, 2), VerticalAlignment = VerticalAlignment.Center };
            toggle.Click += (_, _) =>
            {
                editor.IsVisible = !editor.IsVisible;
                toggle.Content = editor.IsVisible ? "options ▴" : "options ▾";
            };
            header.Children.Add(toggle);
            container.Children.Add(editor);
        }
        return container;
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        Context.Selection.Changed -= Rebuild;
        Context.Session.RemoveTrack(_track);
        if (_recorder is not null)
            Context.Session.RemoveTrack(_recorder);
        _recordTimer?.Stop();
        _inGameTimer?.Stop();
        if (_inGameOwner == this)
            _inGameOwner = null;
        _enginePanel?.Dispose();
        lock (_canvas.SyncRoot)
            _disposed = true;
    }
}
