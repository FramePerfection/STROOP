using StroopNext.App.Modules;
using StroopNext.Memory;
using StroopNext.Platform;
using StroopNext.Session;
using StroopNext.Settings;
using StroopNext.Variables;

namespace StroopNext.App;

internal sealed class MainForm : Form
{
    private static readonly Setting<AngleDisplay> AngleSetting = new("angles", "Angles", AngleDisplay.UnitsAndDegrees);
    private static readonly Setting<int> TabSetting = new("tab", "Selected tab", 0);

    private readonly GameSession _session;
    private readonly SettingsScope _shellSettings;
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly List<ModuleView> _views = new();
    private readonly ToolStripDropDownButton _connect = new("Connect");
    private readonly ToolStripLabel _gameLabel = new() { Alignment = ToolStripItemAlignment.Right };
    private readonly ToolStripStatusLabel _status = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly ToolStripStatusLabel _rate = new();
    private readonly System.Windows.Forms.Timer _tick = new() { Interval = 15 };
    private DisplayOptions _display;
    private GameFrame? _current;
    private string? _pendingSourceMessage;

    public MainForm(GameSession session, Selection selection, SettingsStore settings, IReadOnlyList<IStroopModule> modules)
    {
        _session = session;
        _shellSettings = settings.Scope("shell");
        _display = DisplayOptions.Default with { Angles = _shellSettings.Get(AngleSetting) };

        Text = "STROOP Next";
        Width = 1280;
        Height = 820;
        StartPosition = FormStartPosition.CenterScreen;
        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch (Exception e) when (e is ArgumentException or IOException)
        {
        }

        foreach (var module in modules)
        {
            var context = new ModuleContext(session, selection, settings.Scope($"module.{module.Id}"), () => _display, () => _current, ShowStatus);
            var view = module.CreateView(context);
            _views.Add(view);
            var page = new TabPage(module.Title);
            page.Controls.Add(view);
            _tabs.TabPages.Add(page);
        }
        _tabs.SelectedIndex = Math.Clamp(_shellSettings.Get(TabSetting), 0, Math.Max(0, _tabs.TabCount - 1));
        _tabs.SelectedIndexChanged += (_, _) =>
        {
            _shellSettings.Set(TabSetting, _tabs.SelectedIndex);
            if (_current is not null)
                ActiveView?.OnFrame(_current);
        };

        Controls.Add(_tabs);
        Controls.Add(BuildStatusBar());
        Controls.Add(BuildToolbar());

        session.SourceChanged += OnSourceChanged;
        _tick.Tick += (_, _) => OnTick();
        _tick.Start();
    }

    private void OnSourceChanged(IRamSource? source) => _pendingSourceMessage = source is null ? "Detached." : $"Attached to {source.Name}.";

    private ModuleView? ActiveView => _tabs.SelectedIndex >= 0 && _tabs.SelectedIndex < _views.Count ? _views[_tabs.SelectedIndex] : null;

    private ToolStrip BuildToolbar()
    {
        var bar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Padding = new Padding(4, 2, 4, 2) };
        _connect.ToolTipText = "Attach to a running emulator";
        _connect.DropDownOpening += (_, _) => FillEmulatorMenu();
        FillEmulatorMenu();

        var open = new ToolStripButton("Open savestate…") { ToolTipText = "Inspect a Mupen .st file (gzip or LZ4) without an emulator" };
        open.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog { Filter = "Mupen savestates (*.st;*.savestate)|*.st;*.savestate|All files (*.*)|*.*" };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                OpenSavestate(dialog.FileName);
        };
        var detach = new ToolStripButton("Detach");
        detach.Click += (_, _) => _session.Detach();

        var angles = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 150, ToolTipText = "How angles are displayed" };
        angles.Items.AddRange(["Angles: units", "Angles: degrees", "Angles: units + degrees"]);
        angles.SelectedIndex = (int)_display.Angles;
        angles.SelectedIndexChanged += (_, _) =>
        {
            _display = _display with { Angles = (AngleDisplay)angles.SelectedIndex };
            _shellSettings.Set(AngleSetting, _display.Angles);
            foreach (var view in _views)
                view.OnDisplayOptionsChanged();
        };

        bar.Items.AddRange([_connect, open, detach, new ToolStripSeparator(), angles, _gameLabel]);
        return bar;
    }

    private StatusStrip BuildStatusBar()
    {
        var strip = new StatusStrip();
        strip.Items.AddRange([_status, _rate]);
        return strip;
    }

    private void FillEmulatorMenu()
    {
        var stale = _connect.DropDownItems.Cast<ToolStripItem>().ToList();
        _connect.DropDownItems.Clear();
        foreach (var item in stale)
            item.Dispose();
        var candidates = EmulatorDiscovery.Find();
        if (candidates.Count == 0)
            _connect.DropDownItems.Add(new ToolStripMenuItem("No emulator found (start Mupen, then reopen this menu)") { Enabled = false });
        foreach (var candidate in candidates)
            _connect.DropDownItems.Add(new ToolStripMenuItem(candidate.ToString(), null, (_, _) => Attach(candidate)));
    }

    private async void Attach(EmulatorCandidate candidate)
    {
        ShowStatus($"Looking for SM64 RAM in {candidate.ProcessName}…", false);
        _connect.Enabled = false;
        try
        {
            var source = await Task.Run(() => ProcessRamSource.Attach(candidate));
            _session.Attach(source);
            ShowStatus($"Attached to {source.Name} — RAM at 0x{source.RamBase:X} ({source.DetectionMethod}).", false);
        }
        catch (InvalidOperationException e)
        {
            ShowStatus(e.Message, true);
        }
        finally
        {
            _connect.Enabled = true;
        }
    }

    public void OpenSavestate(string path)
    {
        try
        {
            _session.Attach(Savestate.Load(path));
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
        _status.ForeColor = isError ? Color.Firebrick : SystemColors.ControlText;
    }

    private void OnTick()
    {
        if (Interlocked.Exchange(ref _pendingSourceMessage, null) is { } message)
            ShowStatus(message, false);

        var frame = _session.AcquireLatest();
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
        previous?.Release();

        _gameLabel.Text = frame.Version is { } version
            ? $"{_session.Source?.Name} · SM64 {version} · level {frame.LevelIndex} area {frame.AreaIndex} · timer {frame.GlobalTimer}"
            : $"{_session.Source?.Name} · not SM64 (or no ROM loaded)";
        _rate.Text = $"{_session.MeasuredFrameRate:0} reads/s";
    }

    public void AttachOnStartup(string nameOrId)
    {
        Shown += (_, _) =>
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

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _tick.Stop();
        _session.SourceChanged -= OnSourceChanged;
        _current?.Release();
        _current = null;
        base.OnFormClosed(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _tick.Dispose();
        base.Dispose(disposing);
    }
}
