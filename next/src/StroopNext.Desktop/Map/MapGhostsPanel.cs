using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using StroopNext.Game;
using StroopNext.Map;

namespace StroopNext.Desktop.Map;

internal sealed partial class MapModuleView
{
    private static readonly FilePickerFileType GhostFiles = new("Ghost") { Patterns = ["*.ghost"] };

    private static readonly StroopNext.Settings.Setting<bool> PlayAfterRecording = new("playAfterRecording", "Play right after recording", true);
    private static readonly StroopNext.Settings.Setting<string> GhostFolderSetting = new("folder", "Ghost folder", "");

    private StroopNext.Settings.SettingsScope GhostSettings => Context.Settings.Child("ghosts");

    private string GhostFolder
    {
        get
        {
            string folder = GhostSettings.Get(GhostFolderSetting);
            return string.IsNullOrWhiteSpace(folder)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StroopNext", "Ghosts")
                : folder;
        }
    }

    private readonly StackPanel _ghostRows = new() { Spacing = 2 };
    private readonly TextBlock _recordStatus = new() { Classes = { "muted" }, FontSize = 12, IsVisible = false };
    private GhostRecorder? _recorder;
    private DispatcherTimer? _recordTimer;
    private readonly TextBlock _folderLabel = new() { Classes = { "muted" }, FontSize = 11, TextTrimming = TextTrimming.CharacterEllipsis };

    private GhostsLayer GhostLayer => _layers.OfType<GhostsLayer>().First();

    internal IReadOnlyList<GhostEntry> Ghosts => GhostLayer.Ghosts;

    private Control BuildGhostsSection()
    {
        var section = new StackPanel { Spacing = 4 };
        section.Children.Add(new TextBlock { Text = "GHOSTS", Classes = { "title" }, Margin = new Thickness(0, 6, 0, 0) });

        var load = new Button { Content = "Load…", Padding = new Thickness(10, 3) };
        load.Click += async (_, _) => await LoadGhosts();
        var record = new Button { Content = "● Record", Padding = new Thickness(10, 3) };
        ToolTip.SetTip(record, "Records Mario every game frame, even with this tab hidden. Re-playing a part after loading a savestate overwrites it.");
        record.Click += (_, _) =>
        {
            if (_recorder is null)
                StartRecording(record);
            else
                StopRecording(record);
        };
        var bar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        bar.Children.AddRange([load, record]);
        section.Children.Add(bar);

        var play = new CheckBox { Content = "Play right after recording", IsChecked = GhostSettings.Get(PlayAfterRecording) };
        ToolTip.SetTip(play, "Checked: the new ghost shows on the map right away (it replays at the global timers it was recorded at — load the savestate from before to race it). Unchecked: it is only saved, and added hidden to the list.");
        play.IsCheckedChanged += (_, _) => GhostSettings.Set(PlayAfterRecording, play.IsChecked == true);
        section.Children.Add(play);

        var openFolder = new Button { Content = "Open folder", Padding = new Thickness(6, 1), FontSize = 11 };
        openFolder.Click += async (_, _) => await OpenGhostFolder();
        var changeFolder = new Button { Content = "Change…", Padding = new Thickness(6, 1), FontSize = 11 };
        changeFolder.Click += async (_, _) => await ChooseGhostFolder();
        var folderBar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        folderBar.Children.AddRange([openFolder, changeFolder]);
        UpdateFolderLabel();
        section.Children.Add(_folderLabel);
        section.Children.Add(folderBar);
        section.Children.Add(_recordStatus);
        section.Children.Add(_ghostRows);
        section.Children.Add(BuildInGameSection());

        GhostLayer.Changed += () =>
        {
            RefreshGhostRows();
            Rebuild();
        };
        RefreshGhostRows();
        return section;
    }

    private void StartRecording(Button button)
    {
        _recorder = Context.Session.AddTrack(new GhostRecorder());
        if (Context.CurrentFrame is { } current)
            _recorder.Record(current);
        button.Content = "■ Stop";
        _recordStatus.IsVisible = true;
        _recordTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _recordTimer.Tick += (_, _) => _recordStatus.Text = $"Recording… {_recorder?.Count ?? 0} frames";
        _recordTimer.Start();
        Context.ReportStatus("Recording a ghost: play the game, then press Stop.");
    }

    private void StopRecording(Button button)
    {
        var recorder = _recorder!;
        Context.Session.RemoveTrack(recorder);
        _recorder = null;
        _recordTimer?.Stop();
        _recordStatus.IsVisible = false;
        button.Content = "● Record";
        if (recorder.Count == 0)
        {
            Context.ReportError("No frame recorded: the game did not advance.");
            return;
        }
        var ghost = recorder.ToGhost($"ghost {DateTime.Now:yyyy-MM-dd HH-mm-ss}");
        bool play = GhostSettings.Get(PlayAfterRecording);
        uint recordedStart = ghost.FirstTimer;
        string? path = null;
        try
        {
            Directory.CreateDirectory(GhostFolder);
            path = UniquePath(Path.Combine(GhostFolder, ghost.Name + ".ghost"));
            using (var file = File.Create(path))
                ghost.Write(file);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            path = null;
            Context.ReportError($"Recorded, but could not save to {GhostFolder}: {e.Message}. Use Save… on the ghost.");
        }
        var entry = GhostLayer.Add(ghost, visible: play, filePath: path);
        entry.RecordedStart = recordedStart;
        if (play && Context.CurrentFrame is { } now)
        {
            ghost.StartAt(now.GlobalTimer + 1);
            GhostLayer.NotifyChanged();
        }
        if (path is not null)
            Context.ReportStatus($"Saved \"{Path.GetFileName(path)}\" ({ghost.Frames.Count} frames from timer {ghost.FirstTimer})"
                + (play ? " and playing it on the map." : ". Tick it in the list to play it."));
    }

    private static string UniquePath(string path)
    {
        if (!File.Exists(path))
            return path;
        string stem = Path.Combine(Path.GetDirectoryName(path)!, Path.GetFileNameWithoutExtension(path));
        for (int i = 2; ; i++)
            if (!File.Exists($"{stem} ({i}).ghost"))
                return $"{stem} ({i}).ghost";
    }

    private void UpdateFolderLabel()
    {
        _folderLabel.Text = $"Saved to {GhostFolder}";
        ToolTip.SetTip(_folderLabel, GhostFolder);
    }

    private async Task OpenGhostFolder()
    {
        Directory.CreateDirectory(GhostFolder);
        if (TopLevel.GetTopLevel(this)?.Launcher is { } launcher)
            await launcher.LaunchUriAsync(new Uri(GhostFolder + Path.DirectorySeparatorChar));
    }

    private async Task ChooseGhostFolder()
    {
        if (TopLevel.GetTopLevel(this)?.StorageProvider is not { } storage)
            return;
        var folders = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Folder for recorded ghosts" });
        if (folders.Count > 0 && folders[0].TryGetLocalPath() is { } folder)
        {
            GhostSettings.Set(GhostFolderSetting, folder);
            UpdateFolderLabel();
        }
    }

    private void RefreshGhostRows()
    {
        _ghostRows.Children.Clear();
        if (GhostLayer.Ghosts.Count == 0)
            _ghostRows.Children.Add(new TextBlock { Text = "No ghosts. Load a .ghost file or record one.", Classes = { "muted" }, FontSize = 12 });
        foreach (var g in GhostLayer.Ghosts.Where(e => !e.IsMarker && !e.IsLight))
            _ghostRows.Children.Add(GhostRow(g));
        if (GhostLayer.Ghosts.Count(e => e.IsLight) is int tries and > 0)
            _ghostRows.Children.Add(new TextBlock { Text = $"+ {tries} attempts of the engine (light, on the map)", Classes = { "muted" }, FontSize = 12 });
    }

    private Control GhostRow(GhostEntry g)
    {
        var visible = new CheckBox { IsChecked = g.Visible, MinWidth = 0, Padding = new Thickness(0) };
        visible.IsCheckedChanged += (_, _) =>
        {
            g.Visible = visible.IsChecked == true;
            GhostLayer.NotifyChanged();
        };
        var swatch = new Button
        {
            Width = 18, Height = 18, Padding = new Thickness(0), CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(Color.FromArgb(255, g.Color.R, g.Color.G, g.Color.B)),
            VerticalAlignment = VerticalAlignment.Center,
        };
        ToolTip.SetTip(swatch, "Next colour");
        swatch.Click += (_, _) =>
        {
            int i = Array.IndexOf(GhostsLayer.Colors, g.Color);
            g.Color = GhostsLayer.Colors[(i + 1) % GhostsLayer.Colors.Length];
            GhostLayer.NotifyChanged();
        };
        var name = new TextBlock
        {
            Text = $"{g.Ghost.Name} · {g.Ghost.Frames.Count} f",
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        ToolTip.SetTip(name, g.FilePath ?? "Not saved to a file yet: use Save…");
        var playNow = new Button { Content = "▶ Now", Padding = new Thickness(6, 1), FontSize = 11, MinWidth = 0 };
        ToolTip.SetTip(playNow, "Replay this ghost from its start, beginning at the next frame (here and in the game)");
        playNow.Click += (_, _) =>
        {
            if (Context.CurrentFrame is not { } now)
                return;
            g.Ghost.StartAt(now.GlobalTimer + 1);
            g.Visible = true;
            GhostLayer.NotifyChanged();
        };
        var original = new Button { Content = "↺", Padding = new Thickness(6, 1), FontSize = 11, MinWidth = 0, IsVisible = g.RecordedStart is not null };
        ToolTip.SetTip(original, "Back to the timers it was recorded at (to race it from a savestate made before)");
        original.Click += (_, _) =>
        {
            if (g.RecordedStart is uint start)
            {
                g.Ghost.StartAt(start);
                GhostLayer.NotifyChanged();
            }
        };
        var save = new Button { Content = "Save…", Padding = new Thickness(6, 1), FontSize = 11, MinWidth = 0 };
        save.Click += async (_, _) => await SaveGhost(g.Ghost);
        var remove = new Button { Content = "✕", Padding = new Thickness(5, 1), FontSize = 11, MinWidth = 0 };
        remove.Click += (_, _) => GhostLayer.Remove(g);

        var row = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*,Auto,Auto,Auto,Auto"), ColumnSpacing = 4 };
        Control[] cells = [visible, swatch, name, playNow, original, save, remove];
        for (int i = 0; i < cells.Length; i++)
        {
            Grid.SetColumn(cells[i], i);
            row.Children.Add(cells[i]);
        }

        var start = new NumericUpDown
        {
            Minimum = 1, Maximum = uint.MaxValue, Increment = 1, FormatString = "0", Width = 130,
            Value = g.Ghost.FirstTimer,
        };
        start.ValueChanged += (_, _) =>
        {
            if (start.Value is { } v && v >= 1)
            {
                g.Ghost.StartAt((uint)v);
                GhostLayer.NotifyChanged();
            }
        };
        var path = new CheckBox { Content = "Path", IsChecked = g.ShowPath };
        path.IsCheckedChanged += (_, _) =>
        {
            g.ShowPath = path.IsChecked == true;
            GhostLayer.NotifyChanged();
        };
        var options = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, Margin = new Thickness(26, 0, 0, 0) };
        options.Children.AddRange([new TextBlock { Text = "Starts at timer", FontSize = 12, VerticalAlignment = VerticalAlignment.Center }, start, path]);

        var container = new StackPanel { Spacing = 3 };
        container.Children.AddRange([row, options]);
        return new Border { Child = container, Padding = new Thickness(4, 3), CornerRadius = new CornerRadius(6), Background = AppTheme.SurfaceBrush };
    }

    private async Task LoadGhosts()
    {
        if (TopLevel.GetTopLevel(this)?.StorageProvider is not { } storage)
            return;
        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions { Title = "Load ghosts", AllowMultiple = true, FileTypeFilter = [GhostFiles] });
        foreach (var file in files)
        {
            try
            {
                await using var stream = await file.OpenReadAsync();
                using var copy = new MemoryStream();
                await stream.CopyToAsync(copy);
                copy.Position = 0;
                var ghost = Ghost.Read(copy, Path.GetFileNameWithoutExtension(file.Name));
                GhostLayer.Add(ghost, filePath: file.TryGetLocalPath());
                Context.ReportStatus($"Loaded \"{ghost.Name}\": {ghost.Frames.Count} frames from timer {ghost.FirstTimer}.");
            }
            catch (Exception e) when (e is IOException or InvalidDataException or EndOfStreamException)
            {
                Context.ReportError($"Cannot read {file.Name}: {e.Message}");
            }
        }
    }

    private async Task SaveGhost(Ghost ghost)
    {
        if (TopLevel.GetTopLevel(this)?.StorageProvider is not { } storage)
            return;
        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save ghost", SuggestedFileName = $"{ghost.Name}.ghost", FileTypeChoices = [GhostFiles],
        });
        if (file is null)
            return;
        await using var stream = await file.OpenWriteAsync();
        ghost.Write(stream);
        Context.ReportStatus($"Saved \"{ghost.Name}\" to {file.Name}.");
    }

    public void LoadGhostFile(string path)
    {
        using var stream = File.OpenRead(path);
        GhostLayer.Add(Ghost.Read(stream, Path.GetFileNameWithoutExtension(path)));
    }
}
