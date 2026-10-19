using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using Avalonia.Controls;
using Avalonia.Input;
using StroopNext.Desktop.Controls;
using StroopNext.Game;
using StroopNext.Platform;
using StroopNext.Session;
using StroopNext.Settings;
using StroopNext.Variables;

namespace StroopNext.Desktop.Map;

internal sealed partial class MapModuleView
{
    private static readonly Setting<int> GoToAct = new("goToAct", "Act when going to a level", 1) { Range = (LevelWarp.FirstAct, LevelWarp.LastAct, 1) };

    private static readonly TimeSpan LoadTimeout = TimeSpan.FromSeconds(20);

    // "Go to level, then place Mario": once the level is up (Ready) the next click in the map puts Mario there.
    private sealed class Placement(int level, int area, string where, uint startTimer)
    {
        public int Level { get; } = level;
        public int Area { get; } = area;
        public string Where { get; } = where;
        public uint StartTimer { get; } = startTimer;
        public Stopwatch Age { get; } = Stopwatch.StartNew();
        public bool Ready { get; set; }
    }

    private Placement? _placement;

    private string? GoToUnavailable()
    {
        if (Context.Session.Source is null)
            return "Connect to an emulator first.";
        if (Context.Session.Source is not ProcessRamSource)
            return "Needs a running emulator: a savestate file has no game running to change level.";
        return LevelWarp.Unavailable(Context.CurrentFrame);
    }

    private (int Pid, string? Name, long At)? _romName;

    // The ROM Mupen runs, from its window title: the original game is "SUPER MARIO 64" (US and JP), a ROM hack has
    // its own name. Read every 2 s at most (the LEVEL section refreshes every frame).
    private string? RomHackName(ProcessRamSource source)
    {
        if (!source.Name.StartsWith("mupen", StringComparison.OrdinalIgnoreCase))
            return null;
        long now = Environment.TickCount64;
        if (_romName is not { } cached || cached.Pid != source.ProcessId || now - cached.At > 2000)
            _romName = cached = (source.ProcessId, Engine.MupenRom.LoadedName(source.ProcessId), now);
        return cached.Name is { } name && !name.Equals("SUPER MARIO 64", StringComparison.OrdinalIgnoreCase) ? name : null;
    }

    private const string HackWarning = " On a ROM hack it lists the hack's own levels (read from its ROM) and the warps of the current area.";

    private readonly Button _goToButton = new() { Content = "Go to level ▾", Padding = new Avalonia.Thickness(10, 3) };
    private readonly Button _goPlaceButton = new() { Content = "Go, then place Mario ▾", Padding = new Avalonia.Thickness(10, 3) };
    private readonly Button _cancelPlacement = new() { Content = "Cancel placing", Padding = new Avalonia.Thickness(10, 3), IsVisible = false };
    private readonly TextBlock _levelStatus = new() { FontSize = 12, TextWrapping = Avalonia.Media.TextWrapping.Wrap, Opacity = 0.75 };

    // The side panel's LEVEL section: the level list opens from its buttons (not from the map's right-click menu).
    private Control BuildLevelSection()
    {
        var section = new StackPanel { Spacing = 4 };
        section.Children.Add(new TextBlock { Text = "LEVEL", Classes = { "title" }, Margin = new Avalonia.Thickness(0, 6, 0, 0) });
        ToolTip.SetShowOnDisabled(_goToButton, true);
        ToolTip.SetShowOnDisabled(_goPlaceButton, true);
        _goToButton.Click += (_, _) => OpenLevelList(_goToButton, thenPlace: false);
        _goPlaceButton.Click += (_, _) => OpenLevelList(_goPlaceButton, thenPlace: true);
        _cancelPlacement.Click += (_, _) => EndPlacement();
        var row = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 6 };
        row.Children.AddRange([_goToButton, _goPlaceButton]);
        section.Children.Add(row);
        section.Children.Add(SettingsEditor.Build([GoToAct], _viewSettings, () => { }));
        section.Children.Add(_levelStatus);
        section.Children.Add(_cancelPlacement);
        RefreshLevelSection();
        return section;
    }

    private string? _levelWhy = "";

    private void RefreshLevelSection()
    {
        string? why = GoToUnavailable();
        if (why != _levelWhy)
        {
            _levelWhy = why;
            _goToButton.IsEnabled = _goPlaceButton.IsEnabled = why is null;
            ToolTip.SetTip(_goToButton, why ?? "Makes the game load the level by itself (Mario arrives at the level's usual start)." + HackWarning);
            ToolTip.SetTip(_goPlaceButton, why ?? "Makes the game load the level by itself, then the next click in the map puts Mario there." + HackWarning);
        }
        string status = _placement switch
        {
            { Ready: true } p => $"{p.Where} is loaded: click in the map to place Mario.",
            { } p => $"Loading {p.Where}…",
            null when why is not null => why,
            null when CurrentHack() is { } hack => $"{hack}: its own levels, read from its ROM, and the warps of the current area.",
            null when Context.CurrentFrame?.Version is { } version && Sm64Addresses.WarpCheckpointActive.For(version) is null
                => "On this version the game shows its own act select screen for courses, and what is picked there counts.",
            null => "",
        };
        if (_levelStatus.Text != status)
            _levelStatus.Text = status;
        _levelStatus.IsVisible = status.Length > 0;
        _cancelPlacement.IsVisible = _placement is not null;
    }

    private void OpenLevelList(Control anchor, bool thenPlace)
    {
        var menu = new ContextMenu();
        foreach (var item in CurrentHack() is null ? LevelItems(thenPlace) : HackItems(thenPlace))
            menu.Items.Add(item);
        menu.Open(anchor);
    }

    private string? CurrentHack() => Context.Session.Source is ProcessRamSource source ? RomHackName(source) : null;

    private (string Path, DateTime Written, IReadOnlyDictionary<int, IReadOnlyList<RomArea>>? Levels)? _romLevels;

    // Every level of the hack Mupen runs, read from its ROM file (cached per file).
    private IReadOnlyDictionary<int, IReadOnlyList<RomArea>>? HackRomLevels(GameFrame frame)
    {
        if (Context.Session.Source is not ProcessRamSource source || Engine.MupenRom.Detect(source.ProcessId) is not { } path)
            return null;
        DateTime written = File.GetLastWriteTimeUtc(path);
        if (_romLevels is not { } cached || cached.Path != path || cached.Written != written)
        {
            IReadOnlyDictionary<int, IReadOnlyList<RomArea>>? levels = null;
            try
            {
                levels = RomLevels.Scan(File.ReadAllBytes(path), frame);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException)
            {
            }
            _romLevels = cached = (path, written, levels);
        }
        return cached.Levels;
    }

    // A ROM hack: its own levels from its ROM, and the warps the current area has, with the hack's own course names.
    private IEnumerable<MenuItem> HackItems(bool thenPlace)
    {
        if (Context.CurrentFrame is not { } frame)
            yield break;
        if (HackRomLevels(frame) is { Count: > 0 } levels)
        {
            var names = HackLevels.CourseNames(frame);
            var all = new MenuItem { Header = "All levels (ROM)" };
            foreach (var (level, areas) in levels)
            {
                string name = HackLevels.LevelName(frame, names, level);
                if (areas.Count == 1)
                {
                    var only = new WarpTarget(level, areas[0].Index, areas[0].Entry, $"{name} · area {areas[0].Index}");
                    all.Items.Add(MenuItem(name, () => GoToTarget(only, thenPlace)));
                    continue;
                }
                var levelItem = new MenuItem { Header = name };
                foreach (var area in areas)
                {
                    var target = new WarpTarget(level, area.Index, area.Entry, $"{name} · area {area.Index}");
                    levelItem.Items.Add(MenuItem($"Area {area.Index}", () => GoToTarget(target, thenPlace)));
                }
                all.Items.Add(levelItem);
            }
            yield return all;
        }
        var targets = HackLevels.Targets(frame);
        if (targets.Count == 0)
        {
            yield return new MenuItem { Header = "No warp in this area", IsEnabled = false };
            yield break;
        }
        var from = new MenuItem { Header = "Warps from here" };
        var here = new MenuItem { Header = "This area" };
        foreach (var t in targets)
        {
            var target = t;
            var item = MenuItem(target.Label, () => GoToTarget(target, thenPlace));
            (target.Label.StartsWith("This area", StringComparison.Ordinal) ? here : from).Items.Add(item);
        }
        if (from.Items.Count > 0)
            yield return from;
        if (here.Items.Count > 0)
            yield return here;
    }

    private IEnumerable<MenuItem> LevelItems(bool thenPlace)
    {
        foreach (var group in GameLevels.All.GroupBy(l => l.Group))
        {
            var groupItem = new MenuItem { Header = GameLevels.GroupName(group.Key) };
            foreach (var level in group)
            {
                if (level.Areas.Count == 1)
                {
                    groupItem.Items.Add(MenuItem(level.Name, () => GoToLevel(level, level.Areas[0], thenPlace)));
                    continue;
                }
                var levelItem = new MenuItem { Header = level.Name };
                foreach (var area in level.Areas)
                    levelItem.Items.Add(MenuItem($"{area.Index}: {area.Name}", () => GoToLevel(level, area, thenPlace)));
                groupItem.Items.Add(levelItem);
            }
            yield return groupItem;
        }
    }

    private void GoToLevel(GameLevel level, LevelArea area, bool thenPlace)
    {
        if (GoToUnavailable() is { } why)
        {
            Context.ReportError(why);
            return;
        }
        if (Context.CurrentFrame is not { } frame)
            return;
        if (LevelWarp.IsPaused(frame))
        {
            Context.ReportError("The game is paused: unpause it first, the pause screen would stay on in the next level.");
            return;
        }
        StartWarp(frame, level.Id, area.Index, area.ArrivalNode, level.Describe(area), thenPlace);
    }

    private void GoToTarget(WarpTarget target, bool thenPlace)
    {
        if (GoToUnavailable() is { } why)
        {
            Context.ReportError(why);
            return;
        }
        if (Context.CurrentFrame is not { } frame)
            return;
        if (LevelWarp.IsPaused(frame))
        {
            Context.ReportError("The game is paused: unpause it first, the pause screen would stay on in the next level.");
            return;
        }
        StartWarp(frame, target.Level, target.Area, target.Node, target.Label, thenPlace);
    }

    private void StartWarp(GameFrame frame, int level, int area, int node, string where, bool thenPlace)
    {
        EndPlacement();
        var plan = LevelWarp.Plan(frame, level, area, node, _viewSettings.Get(GoToAct));
        if (plan.Count == 0 || !plan.All(patch => Context.Session.Apply(patch)))
        {
            Context.ReportError($"Write failed: going to {where}.");
            return;
        }
        if (thenPlace)
            _placement = new Placement(level, area, where, frame.GlobalTimer);
        Context.ReportStatus(thenPlace
            ? $"Going to {where}: click in the map once it is loaded to place Mario."
            : $"Going to {where} (act {_viewSettings.Get(GoToAct)}).");
    }

    private void UpdatePlacement(GameFrame frame)
    {
        if (_placement is not { Ready: false } placement)
            return;
        // Frames read before the write can still arrive: the game must have run on since.
        bool ran = (int)(frame.GlobalTimer - placement.StartTimer) >= 8;
        if (ran && LevelWarp.Read(frame) is { } progress && progress.Arrived(placement.Level, placement.Area))
        {
            placement.Ready = true;
            SetPlacementCursor(true);
            Context.ReportStatus($"{placement.Where} is loaded: click in the map to place Mario (a course's intro text may need A first).");
        }
        else if (placement.Age.Elapsed > LoadTimeout)
        {
            EndPlacement();
            Context.ReportError("The level did not load: Mario was not placed.");
        }
    }

    private void EndPlacement()
    {
        if (_placement is null)
            return;
        _placement = null;
        SetPlacementCursor(false);
    }

    private void SetPlacementCursor(bool on)
    {
        var cursor = on ? new Cursor(StandardCursorType.Cross) : null;
        _canvas.Cursor = cursor;
        _view3D?.SetCursor(cursor);
    }

    private bool PlacementArmed => _placement is { Ready: true };

    // The click is consumed once armed, even when it finds no floor (the error says so and Mario stays armed).
    private bool PlaceMario2D(Vector2 world)
    {
        if (!PlacementArmed)
            return false;
        _menuWorld = world;
        if (Teleport(onFloor: true))
            EndPlacement();
        return true;
    }

    private bool PlaceMario3D(Vector3 point)
    {
        if (!PlacementArmed || Context.CurrentFrame is not { Mario: not null } frame)
            return false;
        if (Context.Apply(MarioCommands.Teleport(frame, point.X, point.Y, point.Z), string.Create(CultureInfo.InvariantCulture, $"Mario placed at ({point.X:0.#}, {point.Y:0.#}, {point.Z:0.#})")))
            EndPlacement();
        return true;
    }

    internal void GoToLevelForTest(int level, int area, bool thenPlace)
    {
        if (GameLevels.Find(level) is { } found && found.Area(area) is { } where)
            GoToLevel(found, where, thenPlace);
        else
            Context.ReportError($"No level {level}, area {area} in the list.");
    }

    internal bool PlaceForTest(float x, float z) => PlaceMario2D(new Vector2(x, z));

    internal string PlacementText => _placement is { } p ? $"placing after {p.Where} ({(p.Ready ? "ready" : "loading")})" : "not placing";

    internal string LevelMenuForTest()
    {
        RefreshLevelSection();
        var menu = new ContextMenu();
        menu.Items.Add(new MenuItem { Header = _goToButton.Content, IsEnabled = _goToButton.IsEnabled });
        if (_goToButton.IsEnabled)
            foreach (var item in LevelItems(thenPlace: false))
                menu.Items.Add(item);
        var text = new System.Text.StringBuilder();
        if (!_goToButton.IsEnabled)
            text.AppendLine($"(unavailable: {_levelStatus.Text})");
        void Describe(IEnumerable<object?> items, int depth)
        {
            foreach (var item in items)
                if (item is MenuItem m)
                {
                    text.Append(' ', depth * 2).Append(m.Header).Append(m.IsEnabled ? "" : " [disabled]");
                    if (ToolTip.GetTip(m) is string tip && !m.IsEnabled)
                        text.Append(" — ").Append(tip);
                    text.AppendLine();
                    Describe(m.Items.Cast<object?>(), depth + 1);
                }
        }
        Describe(menu.Items.Cast<object?>(), 0);
        return text.ToString();
    }
}
