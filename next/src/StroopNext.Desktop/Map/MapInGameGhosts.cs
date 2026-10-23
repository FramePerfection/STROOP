using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using StroopNext.Ghosts;
using StroopNext.Map;
using StroopNext.Memory;

namespace StroopNext.Desktop.Map;

internal sealed partial class MapModuleView
{
    private static readonly StroopNext.Settings.Setting<bool> TransparentInGame = new("inGameTransparent", "Transparent", true);
    private static readonly StroopNext.Settings.Setting<bool> AttemptsInGame = new("inGameAttempts", "Engine attempts too", false);

    private static InGameGhosts _inGame = new();
    private static MapModuleView? _inGameOwner;

    // At the base the game leaves free (ROM hacks such as PSM or B3313 use the default one), or where they are.
    private static InGameGhosts InGameFor(StroopNext.Session.GameFrame frame)
    {
        ushort hi = InGameGhosts.BaseFor(frame) ?? InGameGhosts.DefaultBaseHi;
        if (_inGame.BaseHi != hi)
            _inGame = new InGameGhosts(hi);
        return _inGame;
    }

    private readonly TextBlock _inGameStatus = new() { FontSize = 12, TextWrapping = TextWrapping.Wrap };
    private readonly Button _inGameButton = new() { Padding = new Thickness(10, 3) };
    private DispatcherTimer? _inGameTimer;
    private bool _disableRequested;
    // Set while the user wants the ghosts in the game: loading a savestate made without the hack wipes it, so it is
    // written again.
    private bool _wantInGame;
    private uint _reinstallTriedAt = uint.MaxValue;

    private Control BuildInGameSection()
    {
        var section = new StackPanel { Spacing = 4 };
        section.Children.Add(new TextBlock { Text = "IN THE GAME (MUPEN)", Classes = { "title" }, Margin = new Thickness(0, 6, 0, 0) });
        var transparent = new CheckBox { Content = "Transparent", IsChecked = GhostSettings.Get(TransparentInGame) };
        transparent.IsCheckedChanged += (_, _) => GhostSettings.Set(TransparentInGame, transparent.IsChecked == true);
        var attempts = new CheckBox { Content = AttemptsInGame.Label, IsChecked = GhostSettings.Get(AttemptsInGame) };
        attempts.IsCheckedChanged += (_, _) => GhostSettings.Set(AttemptsInGame, attempts.IsChecked == true);
        ToolTip.SetTip(attempts, "Also plays the engine's attempts (the light ghosts of a search) in the game, after the other ghosts.");
        ToolTip.SetTip(_inGameButton,
            "Writes legacy STROOP's ghost hack into the running game, which then draws the ticked ghosts itself, animated, with their cap colour. "
            + "Needs the Expansion Pak. Do not record a movie from a savestate made with the hack enabled.");
        _inGameButton.Click += (_, _) => ToggleInGame();
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        row.Children.AddRange([_inGameButton, transparent, attempts]);
        section.Children.Add(row);
        section.Children.Add(_inGameStatus);

        _inGameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _inGameTimer.Tick += (_, _) => InGameTick();
        _inGameTimer.Start();
        InGameTick();
        return section;
    }

    private bool LiveEmulator => Context.Session.Source is { } source && source is not BufferRamSource && source.IsWritable;

    private void ToggleInGame()
    {
        if (Context.CurrentFrame is not { } frame)
        {
            Context.ReportError("Attach to the emulator first.");
            return;
        }
        if (!LiveEmulator)
        {
            Context.ReportError("In-game ghosts need a running emulator (Connect), not a savestate file.");
            return;
        }
        var state = InGameFor(frame).State(frame);
        if (state is InGameGhostState.Disabled && _wantInGame)
        {
            _wantInGame = false;
            Context.ReportStatus("The ghosts stay out of the game.");
        }
        else if (state is InGameGhostState.Disabled)
        {
            var patch = InGameFor(frame).Install(frame, out string? error);
            if (patch is null)
            {
                Context.ReportError(error ?? "Cannot install the ghost hack.");
                return;
            }
            if (Context.Session.Apply(patch, atomic: true))
            {
                Context.ReportStatus("Ghost hack written into the game.");
                _inGameOwner = this;
                _disableRequested = false;
                _wantInGame = true;
            }
            else
                Context.ReportError("Could not write the ghost hack into the game.");
        }
        else if (state is InGameGhostState.Installed or InGameGhostState.Running)
        {
            Context.Session.Apply(InGameFor(frame).RequestDisable());
            _disableRequested = true;
            _wantInGame = false;
            _inGameOwner = this;
            Context.ReportStatus("Removing the ghosts from the game: let it run (or frame advance) a frame.");
        }
        InGameTick();
    }

    private void InGameTick()
    {
        if (Context.CurrentFrame is not { } frame)
        {
            ShowInGame("Attach to the emulator to show ghosts in the game.", "Show ghosts in the game", false);
            return;
        }
        if (InGameGhosts.Unavailable(frame) is { } why)
        {
            ShowInGame(why, "Show ghosts in the game", false);
            return;
        }
        if (!LiveEmulator)
        {
            ShowInGame("Needs a running emulator (Connect), not a savestate file.", "Show ghosts in the game", false);
            return;
        }
        var state = InGameFor(frame).State(frame);
        bool owner = _inGameOwner == this || _inGameOwner is null;
        switch (state)
        {
            case InGameGhostState.Disabled:
                _disableRequested = false;
                if (_wantInGame && owner && frame.GlobalTimer != _reinstallTriedAt)
                {
                    _reinstallTriedAt = frame.GlobalTimer;
                    if (InGameFor(frame).Install(frame, out _) is { } again && Context.Session.Apply(again, atomic: true))
                    {
                        Context.ReportStatus("A savestate removed the ghost hack: written again.");
                        ShowInGame("Hack written again after a savestate load: let the game run a frame.", "Remove from the game", true);
                        break;
                    }
                }
                if (_wantInGame)
                {
                    ShowInGame("A savestate removed the hack: it is written again as soon as the game allows it.", "Remove from the game", true);
                    break;
                }
                ShowInGame("Not in the game. Ticked ghosts will play there once enabled.", "Show ghosts in the game", true);
                break;
            case InGameGhostState.Disabling:
                if (owner && InGameFor(frame).ReadyToRestore(frame))
                {
                    if (InGameFor(frame).Restore(frame) is { } restore)
                    {
                        Context.Session.Apply(restore, atomic: true);
                        Context.ReportStatus("Ghost hack removed from the game; everything it changed in the game is back.");
                    }
                    else
                    {
                        Context.Session.Apply(new RamPatch().WriteU8(((uint)InGameFor(frame).BaseHi << 16) + 0x7FFC, 0));
                        Context.ReportError("The ghosts are gone, but this hack was installed before STROOP started: load a savestate made without it to remove its code.");
                    }
                    _disableRequested = false;
                    _inGameOwner = null;
                }
                if (owner && InGameFor(frame).Unstick(frame) is { } unstick)
                    Context.Session.Apply(unstick, atomic: true);
                ShowInGame("Removing… let the game run a frame (frame advance if paused).", "Removing…", false);
                break;
            case InGameGhostState.Installed or InGameGhostState.Running:
                if (owner && !_disableRequested)
                {
                    _inGameOwner = this;
                    if (InGameFor(frame).ColoredCaps(frame) is { } caps)
                        Context.Session.Apply(caps, atomic: true);
                    var ghosts = InGameCandidates().Take(InGameGhosts.MaxGhosts).Select(g => (g.Ghost, g.Color)).ToList();
                    var feed = InGameFor(frame).Feed(frame, ghosts, GhostSettings.Get(TransparentInGame));
                    if (!feed.IsEmpty)
                        Context.Session.Apply(feed);
                }
                int count = InGameCandidates().Count();
                ShowInGame(state == InGameGhostState.Running || InGameGhosts.LoopHasRun(frame)
                            ? (count == 0 ? "Hack running. Tick a ghost in the list to play it." : WhenGhostsPlay(frame))
                            : Context.Session.Source is { CanInvalidateCode: true }
                                ? "Hack written: let the game run (frame advance if paused); the ghosts appear in a level."
                                : "Hack written. Now, in a level: save state, load state, then frame advance (not needed with the Pure Interpreter).",
                    "Remove from the game", true);
                break;
        }
    }

    // The ghosts the game plays: the visible ones, then (when ticked) the engine's attempts; markers never.
    private IEnumerable<GhostEntry> InGameCandidates()
    {
        var visible = GhostLayer.Ghosts.Where(g => g.Visible && !g.IsMarker).ToList();
        return GhostSettings.Get(AttemptsInGame) ? visible.Where(g => !g.IsLight).Concat(visible.Where(g => g.IsLight)) : visible.Where(g => !g.IsLight);
    }

    private string WhenGhostsPlay(StroopNext.Session.GameFrame frame)
    {
        var visible = InGameCandidates().Take(InGameGhosts.MaxGhosts).ToList();
        int now = visible.Count(g => g.Ghost.FrameAt(frame.GlobalTimer) is not null);
        if (now == visible.Count)
            return $"In the game: {now} ghost(s) playing.";
        var next = visible.Where(g => g.Ghost.FrameAt(frame.GlobalTimer) is null)
            .Select(g => $"\"{g.Ghost.Name}\" plays at timers {g.Ghost.FirstTimer}–{g.Ghost.FirstTimer + (uint)g.Ghost.Frames.Count - 1}")
            .First();
        return $"{now} playing now. {next} (now {frame.GlobalTimer}): ▶ Now to replay it from here, or load a savestate from then.";
    }

    internal string InGameStatusText => $"[{_inGameButton.Content} {(_inGameButton.IsEnabled ? "on" : "off")}] {_inGameStatus.Text}";

    private void ShowInGame(string status, string button, bool enabled)
    {
        if (_inGameStatus.Text != status)
            _inGameStatus.Text = status;
        if (_inGameButton.Content as string != button)
            _inGameButton.Content = button;
        _inGameButton.IsEnabled = enabled;
    }
}
