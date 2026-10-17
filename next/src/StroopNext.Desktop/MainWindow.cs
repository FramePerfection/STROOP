using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using StroopNext.Desktop.Modules;
using StroopNext.Memory;
using StroopNext.Platform;
using StroopNext.Session;
using StroopNext.Settings;
using StroopNext.Variables;

namespace StroopNext.Desktop;

public sealed class MainWindow : Window
{
    private static readonly Setting<AngleDisplay> AngleSetting = new("angles", "Angles", AngleDisplay.UnitsAndDegrees);
    private static readonly Setting<int> TabSetting = new("tab", "Selected tab", 0);

    private readonly SettingsStore _settings;
    private readonly SettingsScope _shellSettings;
    private readonly TabControl _tabs = new();
    private readonly List<ModuleView> _views = new();
    private readonly List<(Window Window, ModuleView View)> _popouts = new();
    private readonly IReadOnlyList<IStroopModule> _modules;
    private readonly Selection _selection;
    private readonly Button _connect = new() { Content = "Connect ▾" };
    private readonly TextBlock _gameLabel = new() { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0), TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly TextBlock _status = new() { VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly TextBlock _rate = new() { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) };
    private readonly DispatcherTimer _tick = new(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(15) };
    private DisplayOptions _display;
    private GameFrame? _current;
    private string? _pendingSourceMessage;
    private bool _shutDown;

    public MainWindow(GameSession session, Selection selection, SettingsStore settings, IReadOnlyList<IStroopModule> modules)
    {
        Session = session;
        _settings = settings;
        _modules = modules;
        _selection = selection;
        _shellSettings = settings.Scope("shell");
        _display = DisplayOptions.Default with { Angles = _shellSettings.Get(AngleSetting) };

        Title = "STROOP Next";
        Width = 1280;
        Height = 820;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        foreach (var module in modules)
        {
            var context = new ModuleContext(session, selection, settings.Scope($"module.{module.Id}"), () => _display, () => _current, ShowStatus);
            var view = module.CreateView(context);
            _views.Add(view);
            _tabs.Items.Add(new TabItem { Header = module.Title, Content = view });
        }
        _tabs.SelectedIndex = Math.Clamp(_shellSettings.Get(TabSetting), 0, Math.Max(0, _views.Count - 1));
        _tabs.SelectionChanged += (_, _) =>
        {
            _shellSettings.Set(TabSetting, _tabs.SelectedIndex);
            if (_current is not null)
                ActiveView?.OnFrame(_current);
        };

        var root = new DockPanel();
        var toolbar = BuildToolbar();
        DockPanel.SetDock(toolbar, Dock.Top);
        var statusBar = BuildStatusBar();
        DockPanel.SetDock(statusBar, Dock.Bottom);
        root.Children.Add(toolbar);
        root.Children.Add(statusBar);
        root.Children.Add(_tabs);
        Content = root;

        session.SourceChanged += OnSourceChanged;
        _tick.Tick += (_, _) => OnTick();
        _tick.Start();
        Closed += (_, _) => Shutdown();
    }

    public GameSession Session { get; }

    public IReadOnlyList<ModuleView> Views => _views;

    public int SelectedTab
    {
        get => _tabs.SelectedIndex;
        set => _tabs.SelectedIndex = value;
    }

    public string StatusText => _status.Text ?? "";

    private void OnSourceChanged(IRamSource? source) => _pendingSourceMessage = source is null ? "Detached." : $"Attached to {source.Name}.";

    private ModuleView? ActiveView => _tabs.SelectedIndex >= 0 && _tabs.SelectedIndex < _views.Count ? _views[_tabs.SelectedIndex] : null;

    private Control BuildToolbar()
    {
        ToolTip.SetTip(_connect, "Attach to a running emulator");
        _connect.Click += (_, _) => ShowEmulatorMenu();

        var open = new Button { Content = "Open savestate…" };
        ToolTip.SetTip(open, "Inspect a Mupen .st file (gzip or LZ4) without an emulator");
        open.Click += async (_, _) =>
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open a savestate",
                FileTypeFilter = [new FilePickerFileType("Mupen savestates") { Patterns = ["*.st", "*.savestate"] }, FilePickerFileTypes.All],
            });
            if (files.Count > 0 && files[0].TryGetLocalPath() is { } path)
                OpenSavestate(path);
        };
        var detach = new Button { Content = "Detach" };
        detach.Click += (_, _) => Session.Detach();
        var popout = new Button { Content = "⧉ Map window" };
        ToolTip.SetTip(popout, "Opens the map in its own window, updated live: watch it next to the Mario tab");
        popout.Click += (_, _) => PopOut("map");

        var angles = new ComboBox { Width = 220, ItemsSource = new[] { "Angles: units", "Angles: degrees", "Angles: units + degrees" }, SelectedIndex = (int)_display.Angles };
        ToolTip.SetTip(angles, "How angles are displayed");
        angles.SelectionChanged += (_, _) =>
        {
            if (angles.SelectedIndex < 0)
                return;
            _display = _display with { Angles = (AngleDisplay)angles.SelectedIndex };
            _shellSettings.Set(AngleSetting, _display.Angles);
            foreach (var view in _views)
                view.OnDisplayOptionsChanged();
        };

        var bar = new DockPanel { Margin = new Thickness(6, 4) };
        var left = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        left.Children.AddRange([_connect, open, detach, popout, angles]);
        DockPanel.SetDock(left, Dock.Left);
        bar.Children.Add(left);
        _gameLabel.HorizontalAlignment = HorizontalAlignment.Right;
        bar.Children.Add(_gameLabel);
        return bar;
    }

    private Control BuildStatusBar()
    {
        var bar = new DockPanel { Margin = new Thickness(8, 3) };
        DockPanel.SetDock(_rate, Dock.Right);
        bar.Children.Add(_rate);
        bar.Children.Add(_status);
        return new Border { Child = bar, BorderThickness = new Thickness(0, 1, 0, 0), BorderBrush = Brushes.Gray };
    }

    private void ShowEmulatorMenu()
    {
        var menu = new MenuFlyout();
        var candidates = EmulatorDiscovery.Find();
        if (candidates.Count == 0)
            menu.Items.Add(new MenuItem { Header = "No emulator found (start one, then reopen this menu)", IsEnabled = false });
        foreach (var candidate in candidates)
        {
            var item = new MenuItem { Header = candidate.ToString() };
            item.Click += (_, _) => Attach(candidate);
            menu.Items.Add(item);
        }
        menu.ShowAt(_connect);
    }

    private async void Attach(EmulatorCandidate candidate)
    {
        ShowStatus($"Looking for SM64 RAM in {candidate.ProcessName}…", false);
        _connect.IsEnabled = false;
        try
        {
            var source = await Task.Run(() => ProcessRamSource.Attach(candidate));
            Session.Attach(source);
            ShowStatus($"Attached to {source.Name} — RAM at 0x{source.RamBase:X} ({source.DetectionMethod}).", false);
        }
        catch (InvalidOperationException e)
        {
            ShowStatus(e.Message, true);
        }
        finally
        {
            _connect.IsEnabled = true;
        }
    }

    internal bool AttachFirstEmulator()
    {
        int? pid = int.TryParse(Environment.GetEnvironmentVariable("STROOPNEXT_PID"), out int p) ? p : null;
        if (EmulatorDiscovery.Find().FirstOrDefault(c => pid is null || c.ProcessId == pid) is not { } candidate)
            return false;
        var source = ProcessRamSource.Attach(candidate);
        Session.Attach(source);
        ShowStatus($"Attached to {source.Name}.", false);
        return true;
    }

    public void OpenSavestate(string path)
    {
        try
        {
            Session.Attach(Savestate.Load(path));
            ShowStatus($"Opened {Path.GetFileName(path)}. Edits only change the in-memory copy.", false);
        }
        catch (Exception e) when (e is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            ShowStatus($"Cannot open {Path.GetFileName(path)}: {e.Message}", true);
        }
    }

    private void ShowStatus(string message, bool isError)
    {
        _status.Text = message;
        _status.Foreground = isError ? Brushes.Firebrick : null;
        if (!isError)
            _status.ClearValue(TextBlock.ForegroundProperty);
    }

    public void OnTick()
    {
        if (Interlocked.Exchange(ref _pendingSourceMessage, null) is { } message)
            ShowStatus(message, false);

        var frame = Session.AcquireLatest();
        if (frame is null)
        {
            if (_current is not null)
            {
                _current.Release();
                _current = null;
            }
            _gameLabel.Text = "Not attached";
            _rate.Text = "";
            return;
        }
        if (_current is not null && frame.Sequence == _current.Sequence)
        {
            frame.Release();
            return;
        }

        var previous = _current;
        _current = frame;
        ActiveView?.OnFrame(frame);
        foreach (var (_, view) in _popouts)
            view.OnFrame(frame);
        previous?.Release();

        _gameLabel.Text = frame.Version is { } version
            ? $"{Session.Source?.Name} · SM64 {version} · level {frame.LevelIndex} area {frame.AreaIndex} · timer {frame.GlobalTimer}"
            : $"{Session.Source?.Name} · not SM64 (or no ROM loaded)";
        _rate.Text = $"{Session.MeasuredFrameRate:0} reads/s";
    }

    public Window? PopOut(string moduleId)
    {
        if (_modules.FirstOrDefault(m => m.Id == moduleId) is not { } module)
            return null;
        var context = new ModuleContext(Session, _selection, _settings.Scope($"module.{module.Id}.popout"), () => _display, () => _current, ShowStatus);
        var view = module.CreateView(context);
        var window = new Window
        {
            Title = $"STROOP Next · {module.Title}",
            Width = 960,
            Height = 720,
            Content = view,
        };
        _popouts.Add((window, view));
        window.Closed += (_, _) =>
        {
            _popouts.RemoveAll(p => p.Window == window);
            (view as IDisposable)?.Dispose();
        };
        window.Show(this);
        if (_current is not null)
            view.OnFrame(_current);
        return window;
    }

    public void AttachWhenShown(string nameOrId)
    {
        Opened += (_, _) =>
        {
            var match = EmulatorDiscovery.Find().FirstOrDefault(c =>
                c.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture) == nameOrId ||
                c.ProcessName.StartsWith(nameOrId, StringComparison.OrdinalIgnoreCase));
            if (match is null)
                ShowStatus($"No running emulator matches '{nameOrId}'.", true);
            else
                Attach(match);
        };
    }

    public void Shutdown()
    {
        if (_shutDown)
            return;
        _shutDown = true;
        _tick.Stop();
        Session.SourceChanged -= OnSourceChanged;
        _current?.Release();
        _current = null;
        foreach (var (window, _) in _popouts.ToList())
            window.Close();
        foreach (var view in _views.OfType<IDisposable>())
            view.Dispose();
        Session.Dispose();
        try
        {
            _settings.Save();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
    }
}
