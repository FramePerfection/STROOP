using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using StroopNext.Desktop.Modules;
using StroopNext.TasEngine;
using StroopNext.Settings;
using Path = System.IO.Path;

namespace StroopNext.Desktop.Engine;

internal sealed class EnginePanel : UserControl, IDisposable
{
    private static readonly Setting<string> MovieSetting = new("movie", "Movie", "");
    private static readonly Setting<string> TaskSetting = new("task", "Written task", "");
    private static readonly Setting<string> OutputSetting = new("output", "Output folder", "");
    private static readonly Setting<double> MinutesSetting = new("minutes", "Minutes", 0) { Range = (0, 100000, 1) };
    private static readonly Setting<bool> KeepGoingSetting = new("keepGoing", "Keep going", false);
    private static readonly Setting<bool> CpuSetting = new("cpu", "CPU only", false);
    private static readonly Setting<bool> NoMupenSetting = new("noMupen", "No Mupen check", false);
    private static readonly Setting<bool> NoCompetitorsSetting = new("noCompetitors", "No competitors", false);

    private static readonly FilePickerFileType Movies = new("Movie (.m64)") { Patterns = ["*.m64"] };

    private readonly EngineService _engine;
    private readonly SettingsScope _s;
    private ModuleContext Context { get; }

    private readonly Ellipse _dot = new() { Width = 10, Height = 10, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) };
    private readonly TextBlock _state = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _path = new() { TextWrapping = TextWrapping.Wrap, FontWeight = FontWeight.SemiBold };
    private readonly TextBlock _phase = new() { TextWrapping = TextWrapping.Wrap, Foreground = AppTheme.AccentBrush, FontSize = 12 };
    private readonly Button _back = Small("↶ Back");
    private readonly Button _save = Small("Save movie…");
    private readonly Button _new = Small("New path");
    private readonly Button _stop = Small("Stop");
    private readonly TextBox _movie = new() { PlaceholderText = "the path saved last" };
    private readonly NumericUpDown _minutes = new() { Minimum = 0, Maximum = 100000, Increment = 1, FormatString = "0.#", Width = 110 };
    private readonly Button _run = new() { Padding = new Thickness(12, 3), FontWeight = FontWeight.SemiBold };
    private readonly TextBlock _search = new() { TextWrapping = TextWrapping.Wrap, FontSize = 12 };
    private readonly StackPanel _records = new() { Spacing = 2 };
    private readonly TextBlock _doing = new() { TextWrapping = TextWrapping.Wrap, Foreground = AppTheme.AccentBrush, FontSize = 12 };
    private readonly TextBlock _tried = new() { TextWrapping = TextWrapping.Wrap, FontFamily = AppTheme.Monospace, FontSize = 10.5, Classes = { "muted" } };
    private ScrollViewer? _triedBox;
    private ScrollViewer _triedScroll => _triedBox ??= new ScrollViewer { Content = _tried, MaxHeight = 220 };
    private readonly TextBox _log = new()
    {
        IsReadOnly = true, AcceptsReturn = true, TextWrapping = TextWrapping.NoWrap, Height = 200,
        FontFamily = AppTheme.Monospace, FontSize = 11,
    };
    private readonly DispatcherTimer _refresh = new() { Interval = TimeSpan.FromSeconds(1) };
    private string? _shownRecords;

    public EnginePanel(ModuleContext context, EngineService engine)
    {
        Context = context;
        _engine = engine;
        _s = engine.Settings;

        var column = new StackPanel { Spacing = 8 };
        column.Children.AddRange([BuildState(), BuildPath(), BuildImprove(), Fold("Setup", BuildSetup()), Fold("Log", _log)]);
        Content = column;

        _engine.Changed += Update;
        _engine.Status += OnStatus;
        _engine.Line += OnLine;
        _log.Text = _engine.ConsoleText;
        _refresh.Tick += (_, _) =>
        {
            Update();
            _ = _engine.CheckServerAsync();
        };
        _refresh.Start();
        _ = _engine.CheckServerAsync();
        Update();
    }

    internal EngineService Engine => _engine;

    internal string Summary => $"[{_state.Text}] [path: {_path.Text} | {_phase.Text}] [improve: {_search.Text}] [records {_records.Children.Count}]";

    private void OnStatus(string text, bool error)
    {
        if (error)
            Context.ReportError(text);
        else
            Context.ReportStatus(text);
    }

    private void OnLine(string line)
    {
        _log.Text = _engine.ConsoleText;
        _log.CaretIndex = _log.Text?.Length ?? 0;
    }


    private static Button Small(string text) => new() { Content = text, Padding = new Thickness(9, 2) };

    private static WrapPanel Row(params Control[] children)
    {
        var row = new WrapPanel { Orientation = Orientation.Horizontal };
        foreach (var c in children)
        {
            c.Margin = new Thickness(0, 0, 6, 4);
            c.VerticalAlignment = VerticalAlignment.Center;
        }
        row.Children.AddRange(children);
        return row;
    }

    private static TextBlock Title(string text) => new() { Text = text, Classes = { "title" }, Margin = new Thickness(0, 4, 0, 0) };
    private static TextBlock Muted(string text) => new() { Text = text, Classes = { "muted" }, TextWrapping = TextWrapping.Wrap, FontSize = 12 };

    private static Expander Fold(string header, Control content) => new()
    {
        Header = new TextBlock { Text = header, FontSize = 12 }, Content = content, HorizontalAlignment = HorizontalAlignment.Stretch, Padding = new Thickness(6),
    };

    private Control BuildState()
    {
        var row = new DockPanel();
        DockPanel.SetDock(_dot, Dock.Left);
        row.Children.AddRange([_dot, _state]);
        return row;
    }

    private Control BuildPath()
    {
        _back.Click += async (_, _) => await _engine.BackAsync();
        ToolTip.SetTip(_back, "Removes the last part of the path; Mupen goes back to the end of the one before.");
        _save.Click += async (_, _) =>
        {
            if (await SaveFile("Save the path as a movie", "path.m64") is { } path && await _engine.SavePathAsync(path) is { } movie)
            {
                _movie.Text = movie;
                _s.Set(MovieSetting, movie);
            }
        };
        ToolTip.SetTip(_save, "The path as a .m64, its starting .st next to it with the same name (Mupen plays them as they are).");
        _new.Click += (_, _) => _engine.NewPath();
        ToolTip.SetTip(_new, "Forget this path: the next Bring Mario here starts from where Mupen is then.");
        _stop.Click += async (_, _) => await _engine.StopAsync();
        var panel = new StackPanel { Spacing = 4 };
        panel.Children.AddRange([
            Title("PATH"),
            Muted("Right-click in the map → Bring Mario here: the engine adds the fastest way there to the path and Mupen plays it."),
            _path, Row(_back, _save, _new), Row(_stop, _phase),
        ]);
        return panel;
    }

    private Control BuildImprove()
    {
        _movie.Text = _s.Get(MovieSetting);
        _movie.LostFocus += (_, _) => _s.Set(MovieSetting, _movie.Text?.Trim().Trim('"') ?? "");
        var browse = new Button { Content = "…", Width = 30, Padding = new Thickness(0, 2) };
        browse.Click += async (_, _) =>
        {
            if (await PickMovie() is { } m)
            {
                _movie.Text = m;
                _s.Set(MovieSetting, m);
            }
        };
        var movieRow = new Grid { ColumnDefinitions = new ColumnDefinitions("*,4,Auto") };
        Grid.SetColumn(_movie, 0);
        Grid.SetColumn(browse, 2);
        movieRow.Children.AddRange([_movie, browse]);
        _minutes.Value = (decimal)_s.Get(MinutesSetting);
        _minutes.ValueChanged += (_, _) => _s.Set(MinutesSetting, (double)(_minutes.Value ?? 0));
        ToolTip.SetTip(_minutes, "0: until its first verified improvement.");
        _run.Click += (_, _) =>
        {
            if (_engine.Searching)
                _engine.StopSearch();
            else
                StartSearch();
        };

        var options = new StackPanel { Spacing = 2 };
        options.Children.AddRange([
            Check("Keep going after an improvement", KeepGoingSetting), Check("CPU only (no GPU)", CpuSetting),
            Check("Do not re-check records in Mupen", NoMupenSetting), Check("No competitors' movies", NoCompetitorsSetting),
        ]);
        var panel = new StackPanel { Spacing = 4 };
        panel.Children.AddRange([
            Title("IMPROVE A MOVIE"),
            movieRow,
            Row(new TextBlock { Text = "Minutes", FontSize = 12 }, _minutes, _run),
            Fold("Options", options),
            _search, _doing, _triedScroll, _records,
        ]);
        return panel;
    }

    private CheckBox Check(string text, Setting<bool> setting)
    {
        var box = new CheckBox { Content = text, IsChecked = _s.Get(setting) };
        box.IsCheckedChanged += (_, _) => _s.Set(setting, box.IsChecked == true);
        return box;
    }

    private Control BuildSetup()
    {
        var folder = new TextBox { Text = _engine.Setup.Folder };
        var python = new TextBox { Text = _engine.Setup.Python };
        var port = new NumericUpDown { Minimum = 1024, Maximum = 65535, Increment = 1, FormatString = "0", Value = _engine.Port, Width = 120, HorizontalAlignment = HorizontalAlignment.Left };
        var budget = new NumericUpDown { Minimum = 1, Maximum = 600, Increment = 1, FormatString = "0", Value = (decimal)_s.Get(EngineService.BudgetSetting), Width = 120, HorizontalAlignment = HorizontalAlignment.Left };
        budget.ValueChanged += (_, _) => _s.Set(EngineService.BudgetSetting, (double)(budget.Value ?? 60));
        var rom = new TextBox { Text = _s.Get(EngineService.RomSetting), PlaceholderText = "empty: the ROM Mupen runs" };
        rom.LostFocus += (_, _) => _s.Set(EngineService.RomSetting, rom.Text?.Trim().Trim('"') ?? "");
        var auto = new CheckBox { Content = "Start with Mupen (Connect)", IsChecked = _engine.AutoConnect };
        auto.IsCheckedChanged += (_, _) => _engine.AutoConnect = auto.IsChecked == true;
        var apply = Small("Apply");
        apply.Click += (_, _) =>
        {
            _engine.ChangeSetup(folder.Text?.Trim().Trim('"') ?? "", python.Text?.Trim().Trim('"') ?? "", (int)(port.Value ?? EngineSetup.DefaultPort));
            _ = _engine.CheckServerAsync();
        };
        var panel = new StackPanel { Spacing = 3 };
        panel.Children.AddRange([
            Label("Engine folder"), folder, Label("Python"), python, Label("Port"), port, apply,
            Label("Search time per step (s)"), budget, Label("ROM hack"), rom, auto,
        ]);
        return panel;
    }

    private static TextBlock Label(string text) => new() { Text = text, FontSize = 12, Margin = new Thickness(0, 4, 0, 0) };

    private void StartSearch()
    {
        string movie = _movie.Text?.Trim().Trim('"') ?? "";
        if (movie.Length == 0 || !File.Exists(movie))
        {
            Context.ReportError("Choose the movie to improve (Save movie… writes the path as one).");
            return;
        }
        if (SearchOptions.SavestateFor(movie) is not { } st)
        {
            Context.ReportError($"No .st next to {Path.GetFileName(movie)} (same name).");
            return;
        }
        string output = _s.Get(OutputSetting) is { Length: > 0 } o ? o
            : Path.Combine(Path.GetDirectoryName(Path.GetFullPath(movie)) ?? ".", Path.GetFileNameWithoutExtension(movie) + "_engine");
        var options = new SearchOptions
        {
            Savestate = st, Movie = movie, Output = output,
            Task = _s.Get(TaskSetting) is { Length: > 0 } t ? t : null,
            Rom = _engine.Rom(),
            Minutes = (double)(_minutes.Value ?? 0),
            KeepGoing = _s.Get(KeepGoingSetting), CpuOnly = _s.Get(CpuSetting),
            NoMupenCheck = _s.Get(NoMupenSetting), NoCompetitors = _s.Get(NoCompetitorsSetting),
        };
        _shownRecords = null;
        _engine.StartSearch(options);
        Update();
    }


    private void Update()
    {
        bool ready = _engine.ServerUp && (_engine.BridgeUp || _engine.LiveAttached);
        _dot.Fill = ready ? AppTheme.IncreaseBrush : _engine.AutoText is not null || _engine.ServerStarting ? AppTheme.AccentBrush : AppTheme.MutedBrush;
        _state.Text = _engine.Setup.Problem() is { } problem ? problem
            : _engine.AutoText is { } auto ? auto
            : ready ? "Ready with Mupen."
            : _engine.MupenProcess is null ? "Connect STROOP to Mupen (Connect ▾): the engine starts with it."
            : _engine.ServerUp ? "Engine running; bridge.lua not running in Mupen."
            : "Engine not running.";

        _path.Text = _engine.PathStart is null || _engine.PathParts == 0
            ? "No path yet."
            : $"{_engine.PathParts} part{(_engine.PathParts > 1 ? "s" : "")} · {_engine.PathFrames} frames";
        _phase.Text = _engine.PathPhase ?? "";
        _stop.IsVisible = _engine.PathBusy;
        _phase.IsVisible = _engine.PathBusy;
        _back.IsEnabled = !_engine.PathBusy && _engine.PathParts > 0;
        _save.IsEnabled = !_engine.PathBusy && _engine.PathFrames > 0;
        _new.IsEnabled = !_engine.PathBusy && _engine.PathStart is not null;

        _run.Content = _engine.Searching ? "■ Stop" : "▶ Improve";
        _run.Background = _engine.Searching ? AppTheme.DecreaseBrush : AppTheme.AccentBrush;
        if (_engine.Search is { } search)
        {
            string start = search.StartFrames is int s ? $"{s} f" : "…";
            string best = search.Records.Count > 0 ? $"{search.Records.Min(r => r.Frames)} f" : "no record yet";
            double minutes = (DateTime.Now - _engine.SearchStarted).TotalMinutes;
            _search.Text = _engine.Searching ? $"{start} → {best} · {minutes:0.0} min" : $"{start} → {best} · ended";
            _search.Foreground = search.Records.Count > 0 ? AppTheme.IncreaseBrush : AppTheme.MutedBrush;
            _doing.Text = _engine.Searching && search.Doing is { Length: > 0 } doing ? $"Now: {doing}" : "";
            var tried = (search.Output ?? _engine.SearchOptions?.Output) is { } output ? SearchProgress.JournalTail(output, 25) : [];
            string text = string.Join("\n", tried);
            if (text != _tried.Text)
            {
                _tried.Text = text;
                _triedScroll.ScrollToEnd();
            }
        }
        else
        {
            _search.Text = "";
            _doing.Text = _tried.Text = "";
        }
        _doing.IsVisible = _doing.Text.Length > 0;
        _triedScroll.IsVisible = (_tried.Text ?? "").Length > 0;
        UpdateRecords();
    }

    private void UpdateRecords()
    {
        var records = _engine.Search?.Records.OrderBy(r => r.Frames).ToList() ?? [];
        string shown = string.Join(",", records.Select(r => r.Movie));
        if (shown == _shownRecords)
            return;
        _shownRecords = shown;
        _records.Children.Clear();
        foreach (var r in records)
        {
            var save = new Button { Content = "Save as…", Padding = new Thickness(7, 1), FontSize = 11 };
            save.Click += async (_, _) => await SaveRecord(r);
            var text = new TextBlock { Text = $"{r.Frames} f · {r.Minutes:0.#} min", FontWeight = FontWeight.SemiBold };
            ToolTip.SetTip(text, r.Movie);
            _records.Children.Add(Row(text, save));
        }
    }

    private async Task SaveRecord(SearchRecord r)
    {
        if (await SaveFile("Save the record", Path.GetFileName(r.Movie)) is not { } path)
            return;
        try
        {
            File.Copy(r.Movie, path, overwrite: true);
            if (File.Exists(r.Savestate))
                File.Copy(r.Savestate, Path.ChangeExtension(path, ".st"), overwrite: true);
            Context.ReportStatus($"Saved {Path.GetFileName(path)} and its .st.");
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Context.ReportError($"Could not save: {e.Message}");
        }
    }


    private async Task<string?> PickMovie()
    {
        if (TopLevel.GetTopLevel(this)?.StorageProvider is not { } storage)
            return null;
        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions { Title = "Movie to improve (its .st next to it)", FileTypeFilter = [Movies] });
        return files.Count > 0 ? files[0].TryGetLocalPath() : null;
    }

    private async Task<string?> SaveFile(string title, string name)
    {
        if (TopLevel.GetTopLevel(this)?.StorageProvider is not { } storage)
            return null;
        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions { Title = title, SuggestedFileName = name, DefaultExtension = "m64", FileTypeChoices = [Movies] });
        return file?.TryGetLocalPath();
    }

    public void Dispose()
    {
        _refresh.Stop();
        _engine.Changed -= Update;
        _engine.Status -= OnStatus;
        _engine.Line -= OnLine;
        _engine.Dispose();
    }
}
