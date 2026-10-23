using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using Avalonia.Threading;
using StroopNext.TasEngine;
using StroopNext.Ghosts;
using StroopNext.Memory;
using StroopNext.Platform;
using StroopNext.Session;
using StroopNext.Settings;

namespace StroopNext.Desktop.Engine;

internal sealed class EngineGhost(string key, Ghost ghost, Argb8Color color, bool marker = false, bool light = false)
{
    public string Key { get; } = key;
    public Ghost Ghost { get; } = ghost;
    public Argb8Color Color { get; } = color;

    public bool Marker { get; } = marker;

    public bool Light { get; } = light;
}

internal sealed class EngineService : IDisposable
{
    public static readonly Setting<string> FolderSetting = new("folder", "Engine folder", "");
    public static readonly Setting<string> PythonSetting = new("python", "Python", "");
    public static readonly Setting<int> PortSetting = new("port", "Server port", EngineSetup.DefaultPort) { Range = (1024, 65535, 1) };
    public static readonly Setting<string> RomSetting = new("rom", "ROM hack", "");
    public static readonly Setting<bool> AutoSetting = new("auto", "Connect the engine with Mupen automatically", true);
    public static readonly Setting<double> BudgetSetting = new("stepTime", "Search time per step (s)", 120) { Range = (5, 1200, 5) };

    public static readonly Argb8Color PathColor = Argb8Color.Rgb(0xF5, 0x9E, 0x2E);
    public static readonly Argb8Color BestColor = Argb8Color.Rgb(0x8C, 0xE0, 0x3A);
    public static readonly Argb8Color ReferenceColor = Argb8Color.Rgb(0xC8, 0xCC, 0xD8);
    public static readonly Argb8Color TargetColor = Argb8Color.Rgb(0xFF, 0x5A, 0x5A);
    public static readonly Argb8Color[] TryColors =
    [
        Argb8Color.Rgb(0x5A, 0xC8, 0xFA), Argb8Color.Rgb(0xFF, 0xD4, 0x3B), Argb8Color.Rgb(0xC0, 0x7C, 0xFF), Argb8Color.Rgb(0xFF, 0x8C, 0x5A),
        Argb8Color.Rgb(0x4C, 0xE0, 0xB3), Argb8Color.Rgb(0xFF, 0x6E, 0xC7), Argb8Color.Rgb(0x9A, 0xD8, 0x5A), Argb8Color.Rgb(0x7A, 0x9C, 0xFF),
    ];

    private readonly SettingsScope _settings;
    private readonly DispatcherTimer _clock = new() { Interval = TimeSpan.FromMilliseconds(33) };
    private readonly Dictionary<string, EngineGhost> _ghosts = new();
    private readonly StringBuilder _console = new();
    private EngineClient _client;
    private Process? _server;
    private Process? _search;
    private CancellationTokenSource? _searchPoll;
    private string? _mupenRom;
    private double _liveSince;
    private int _tick;
    private bool _disposed;

    public EngineService(SettingsScope settings, GameSession? session = null)
    {
        _settings = settings;
        if (session is not null)
            session.SourceChanged += source => Post(() => _ = OnEmulatorAsync(source));
        Setup = EngineSetup.Detect(settings.Get(FolderSetting), settings.Get(PythonSetting));
        _client = EngineClient.Local(Port);
        _clock.Tick += (_, _) =>
        {
            _tick++;
            if (_ghosts.Count > 0)
                Animated?.Invoke();
        };
        _clock.Start();
    }


    public EngineSetup Setup { get; private set; }
    public SettingsScope Settings => _settings;
    public int Port => _settings.Get(PortSetting);

    public bool ServerUp { get; private set; }
    public bool OwnsServer => _server is { HasExited: false };
    public bool ServerStarting { get; private set; }

    public int? MupenProcess { get; private set; }
    public bool BridgeUp { get; private set; }
    public bool LiveAttached { get; private set; }
    public string? AutoText { get; private set; }

    public bool Searching => _search is { HasExited: false };
    public SearchOptions? SearchOptions { get; private set; }
    public SearchProgress? Search { get; private set; }
    public DateTime SearchStarted { get; private set; }

    public string ConsoleText => _console.ToString();
    public IReadOnlyCollection<EngineGhost> Ghosts => _ghosts.Values;

    public event Action? Changed;
    public event Action<string>? Line;
    public event Action<string, bool>? Status;
    public event Action? GhostsChanged;
    public event Action? Animated;

    public bool AutoConnect
    {
        get => _settings.Get(AutoSetting);
        set
        {
            _settings.Set(AutoSetting, value);
            if (value && MupenProcess is int pid)
                _ = ConnectWithMupenAsync(pid);
            Notify();
        }
    }

    public int PreviewIndex(EngineGhost g)
    {
        int count = g.Ghost.Frames.Count;
        if (count == 0)
            return 0;
        int longest = count;
        foreach (var other in _ghosts.Values)
            if (!other.Marker)
                longest = Math.Max(longest, other.Ghost.Frames.Count);
        return Math.Min(_tick % (longest + 30), count - 1);
    }

    private void Post(Action action)
    {
        if (_disposed)
            return;
        if (Dispatcher.UIThread.CheckAccess())
            action();
        else
            Dispatcher.UIThread.Post(() =>
            {
                if (!_disposed)
                    action();
            });
    }

    private void Say(string text, bool error = false) => Post(() => Status?.Invoke(text, error));

    public void Log(string line) => Post(() =>
    {
        if (_console.Length > 200_000)
            _console.Remove(0, _console.Length - 100_000);
        _console.AppendLine(line);
        Line?.Invoke(line);
    });

    private void Notify() => Post(() => Changed?.Invoke());


    public void ChangeSetup(string folder, string python, int port)
    {
        _settings.Set(FolderSetting, folder);
        _settings.Set(PythonSetting, python);
        if (port != Port)
        {
            _settings.Set(PortSetting, port);
            _client.Dispose();
            _client = EngineClient.Local(port);
        }
        Setup = EngineSetup.Detect(folder, python);
        Notify();
    }

    public async Task<bool> CheckServerAsync()
    {
        bool up = await _client.IsAliveAsync().ConfigureAwait(false);
        if (up != ServerUp)
        {
            ServerUp = up;
            Notify();
        }
        return up;
    }

    public async Task<bool> EnsureServerAsync()
    {
        if (await CheckServerAsync().ConfigureAwait(false))
            return true;
        if (Setup.Problem() is { } problem)
        {
            Say(problem, true);
            return false;
        }
        if (!OwnsServer)
        {
            try
            {
                _server = StartPython([Setup.ServerScript, "--port", Port.ToString(CultureInfo.InvariantCulture)], line => Log("[engine] " + line));
                Log($"STROOP: engine server starting on port {Port} ({Setup.Folder})");
            }
            catch (Exception e) when (e is System.ComponentModel.Win32Exception or InvalidOperationException)
            {
                Say($"Cannot start the engine ({Setup.Python}): {e.Message}", true);
                return false;
            }
        }
        ServerStarting = true;
        Notify();
        try
        {
            for (int i = 0; i < 90; i++)
            {
                if (_server is { HasExited: true } exited)
                {
                    Say($"The engine stopped at start (exit code {exited.ExitCode}): see the log.", true);
                    return false;
                }
                if (await CheckServerAsync().ConfigureAwait(false))
                    return true;
                await Task.Delay(1000).ConfigureAwait(false);
            }
            Say("The engine did not answer within 90 s: see the log.", true);
            return false;
        }
        finally
        {
            ServerStarting = false;
            Notify();
        }
    }

    private Process StartPython(IEnumerable<string> arguments, Action<string> onLine)
    {
        var info = new ProcessStartInfo(Setup.Python)
        {
            WorkingDirectory = Setup.Folder,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        info.ArgumentList.Add("-u");
        foreach (var a in arguments)
            info.ArgumentList.Add(a);
        info.Environment["PYTHONIOENCODING"] = "utf-8";
        info.Environment["PYTHONUNBUFFERED"] = "1";
        var process = new Process { StartInfo = info, EnableRaisingEvents = true };
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                onLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                onLine(e.Data);
        };
        process.Start();
        ChildProcesses.Adopt(process);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    private static void Kill(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch (Exception e) when (e is InvalidOperationException or System.ComponentModel.Win32Exception or NotSupportedException)
        {
        }
    }


    private static string BridgeStatusFile => Path.Combine(Path.GetTempPath(), "sm64rh", "attach", "status.json");

    public static bool BridgeAlive()
    {
        try
        {
            return File.Exists(BridgeStatusFile) && (DateTime.UtcNow - File.GetLastWriteTimeUtc(BridgeStatusFile)).TotalSeconds < 3;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private void LoadAnimationsNear(int mupenPid)
    {
        if (Graphics.MarioAnimations.RomLoaded is not null)
            return;
        try
        {
            using var process = Process.GetProcessById(mupenPid);
            if (Path.GetDirectoryName(process.MainModule?.FileName) is not { } dir)
                return;
            int n = Graphics.MarioAnimations.LoadFromRomFolder(Path.Combine(dir, "roms"));
            if (n == 0)
                n = Graphics.MarioAnimations.LoadFromRomFolder(dir);
            if (n > 0)
                Log($"STROOP: {n} Mario animations read from {Graphics.MarioAnimations.RomLoaded} (the ghosts' own poses).");
        }
        catch (Exception e) when (e is InvalidOperationException or ArgumentException or System.ComponentModel.Win32Exception)
        {
        }
    }

    private async Task OnEmulatorAsync(IRamSource? source)
    {
        MupenProcess = source is ProcessRamSource p && p.Name.StartsWith("mupen", StringComparison.OrdinalIgnoreCase) ? p.ProcessId : null;
        if (MupenProcess is int mupenPid)
        {
            _ = Task.Run(() => LoadAnimationsNear(mupenPid));
            _mupenRom = MupenRom.Detect(mupenPid);
            if (_mupenRom is { } rom && _settings.Get(RomSetting) is not { Length: > 0 })
                Log($"STROOP: ROM {rom} (from Mupen)");
        }
        if (MupenProcess is null)
        {
            _mupenRom = null;
            BridgeUp = false;
            AutoText = null;
            if (LiveAttached)
                await LetGoAsync().ConfigureAwait(false);
            Notify();
            return;
        }
        if (AutoConnect)
            await ConnectWithMupenAsync(MupenProcess.Value).ConfigureAwait(false);
    }

    public async Task<bool> ConnectWithMupenAsync(int processId)
    {
        AutoText = "Starting the engine…";
        Notify();
        if (!await EnsureServerAsync().ConfigureAwait(false))
        {
            AutoText = "The engine did not start: see Setup and the log.";
            Notify();
            return false;
        }
        if (!BridgeAlive())
        {
            AutoText = "Starting bridge.lua in Mupen…";
            Notify();
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(BridgeStatusFile)!);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                Log($"STROOP: cannot create the bridge folder: {e.Message}");
            }
            Log($"STROOP: starting {Setup.BridgeLua} in Mupen (Lua menu)");
            string? error = await MupenLua.StartScriptAsync(processId, Setup.BridgeLua).ConfigureAwait(false);
            for (int i = 0; error is null && i < 30 && !BridgeAlive(); i++)
                await Task.Delay(200).ConfigureAwait(false);
            if (error is not null || !BridgeAlive())
            {
                BridgeUp = false;
                AutoText = $"bridge.lua could not be started for you ({error ?? "no heartbeat"}): in Mupen, Lua Script → Show Instances → Add instance → {Setup.BridgeLua}, Start.";
                Log("STROOP: " + AutoText);
                Notify();
                return false;
            }
        }
        BridgeUp = true;
        AutoText = null;
        Say("Engine ready: right-click in the map → Bring Mario here (engine).");
        Notify();
        return true;
    }

    private async Task<bool> TakeOverAsync()
    {
        if (LiveAttached)
            return true;
        if (!BridgeAlive())
        {
            if (MupenProcess is not int pid || !await ConnectWithMupenAsync(pid).ConfigureAwait(false))
            {
                Say("The engine cannot drive Mupen: connect STROOP to Mupen (Connect ▾).", true);
                return false;
            }
        }
        string? rom = Rom();
        try
        {
            await _client.AttachAsync(rom).ConfigureAwait(false);
        }
        catch (EngineException e) when (e.Message.Contains("déjà piloté", StringComparison.OrdinalIgnoreCase))
        {
            await _client.LiveStopAsync().ConfigureAwait(false);
            await _client.AttachAsync(rom).ConfigureAwait(false);
        }
        LiveAttached = true;
        Log("STROOP: the engine drives Mupen.");
        Notify();
        return true;
    }

    public async Task LetGoAsync()
    {
        try
        {
            await _client.LiveStopAsync().ConfigureAwait(false);
        }
        catch (Exception e) when (IsEngineFailure(e))
        {
        }
        LiveAttached = false;
        Notify();
    }

    private string? _warnedMissingRom;

    // Detected again at each use: the ROM can change inside the same Mupen process.
    public string? Rom()
    {
        if (_settings.Get(RomSetting) is { Length: > 0 } rom)
            return rom;
        if (MupenProcess is not int pid)
            return _mupenRom;
        _mupenRom = MupenRom.Detect(pid);
        if (_mupenRom is null && MupenRom.MissingName is { } name && name != _warnedMissingRom)
        {
            _warnedMissingRom = name;
            Say($"Mupen runs {name} but its ROM file was not found: set it in Engine → ROM hack.", true);
        }
        return _mupenRom;
    }


    public string? PathStart { get; private set; }
    public int PathParts { get; private set; }
    public int PathFrames { get; private set; }
    public bool PathBusy { get; private set; }
    public string? PathPhase { get; private set; }

    private int _mupenFrames;
    private Vector3? _mupenEnd;

    private static string WorkFolder => Path.Combine(Path.GetTempPath(), "StroopNext", "engine-path");

    public async Task BringHereAsync(float x, float y, float z)
    {
        if (PathBusy)
        {
            Say("The engine is already working on the path: wait, or Stop.", true);
            return;
        }
        PathBusy = true;
        Notify();
        try
        {
            if (!await TakeOverAsync().ConfigureAwait(false))
                return;
            if (PathStart is not null && _mupenEnd is { } end
                && await _client.LiveStateAsync().ConfigureAwait(false) is { } now
                && Vector3.Distance(end, now.Position) > 5)
            {
                Log($"STROOP: Mario moved in Mupen since the end of the path: a new path starts from ({now.Position.X:0}, {now.Position.Y:0}, {now.Position.Z:0}).");
                PathStart = null;
                _steps.Clear();
                PathParts = PathFrames = 0;
                _mupenFrames = 0;
                _mupenEnd = null;
                Post(() => RemoveGhost("path"));
            }
            if (PathStart is null || !File.Exists(PathStart))
                await StartPathAsync().ConfigureAwait(false);
            int partsBefore = PathParts, framesBefore = PathFrames;
            try
            {
                var ground = await _client.GetAsync(string.Create(System.Globalization.CultureInfo.InvariantCulture,
                    $"api/ground?x={x}&y={y}&z={z}")).ConfigureAwait(false);
                if (ground["y"] is JsonValue gy && gy.TryGetValue(out double gyv) && Math.Abs(gyv - y) > 0.5)
                {
                    Log($"STROOP: the point is put on the engine's floor: y {y:0} → {gyv:0}.");
                    y = (float)gyv;
                }
            }
            catch (Exception e) when (IsEngineFailure(e))
            {
            }
            ShowTarget(new Vector3(x, y, z));
            Phase($"the engine looks for the way to ({x:0}, {y:0}, {z:0})");
            double budget = _settings.Get(BudgetSetting);
            await StartJobAsync("api/go", new JsonObject
            {
                ["steps"] = new JsonArray(new JsonObject { ["x"] = x, ["y"] = y, ["z"] = z, ["snap"] = true }),
                ["budget"] = budget,
            }).ConfigureAwait(false);
            _lastJobPhase = null;
            var job = await WaitJobAsync(budget).ConfigureAwait(false);
            if (job.Error is { } error)
            {
                Say($"Bring Mario here failed: {error}", true);
                return;
            }
            ReadPath(job.Result?["state"]);
            bool reached = job.Result?["done"]?.GetValue<bool>() == true;
            if (PathParts > partsBefore)
                _steps.Push(partsBefore);
            if (PathFrames == framesBefore)
            {
                Say(reached ? "Mario is already there: the path is unchanged."
                    : $"The engine found no way from here{FailedWhy(job)}: the path is unchanged.", !reached);
                return;
            }
            Phase("Mupen plays it");
            await SyncMupenAsync().ConfigureAwait(false);
            await RefreshPathGhostAsync().ConfigureAwait(false);
            Say(reached
                ? $"Mario got there · path: {PathFrames} frames."
                : $"Point not reached{FailedWhy(job)}: the path goes as far as the engine got · {PathFrames} frames.", !reached);
        }
        catch (Exception e) when (IsEngineFailure(e))
        {
            Say($"Bring Mario here failed: {Message(e)}", true);
        }
        finally
        {
            PathBusy = false;
            PathPhase = null;
            Post(() => RemoveGhost("target"));
            ClearTries();
            Notify();
        }
    }

    private string FailedWhy(EngineJob job)
    {
        var failed = job.Result?["failed"];
        if (failed is null)
            return "";
        string what = failed["label"]?.ToString() is { Length: > 0 } label ? label
            : failed["kind"]?.ToString() switch
            {
                "warp" => $"going through the door at ({Number(failed["x"]):0}, {Number(failed["z"]):0})",
                "collect" => $"collecting {failed["bhv"]}",
                "floor" => "standing on the floor there",
                { Length: > 0 } kind => $"{kind} {failed["bhv"]}".Trim(),
                _ => "",
            };
        string last = _lastJobPhase is { Length: > 0 } p ? $"; last try: {p}" : "";
        return what.Length > 0 || last.Length > 0 ? $" ({what}{last})" : "";
    }

    private string? _lastJobPhase;

    private readonly Stack<int> _steps = new();

    private async Task StartJobAsync(string route, JsonObject body)
    {
        try
        {
            await _client.PostAsync(route, body).ConfigureAwait(false);
        }
        catch (EngineException e) when (e.Status == 409)
        {
            Phase("an earlier search is still running: stopping it");
            await _client.StopJobAsync().ConfigureAwait(false);
            for (int i = 0; i < 120 && !(await _client.JobAsync().ConfigureAwait(false)).Done; i++)
                await Task.Delay(500).ConfigureAwait(false);
            await _client.PostAsync(route, (JsonObject)body.DeepClone()).ConfigureAwait(false);
        }
    }

    private void ShowTarget(Vector3 target)
    {
        var ghost = new Ghost { Name = "Engine target" };
        ghost.Frames[0] = new GhostFrame(target, 0, 0, 0, 0, 0);
        Post(() => SetGhost(new EngineGhost("target", ghost, TargetColor, marker: true)));
    }

    private static float Number(JsonNode? n) => n is JsonValue v && v.TryGetValue(out double d) ? (float)d : 0;

    private void Phase(string text)
    {
        PathPhase = text;
        Log("STROOP: " + text);
        Notify();
    }

    private async Task StartPathAsync()
    {
        Directory.CreateDirectory(WorkFolder);
        string st = Path.Combine(WorkFolder, $"start_{DateTime.Now:yyyyMMdd_HHmmss}.st");
        Phase("the path starts here: Mupen saves its state");
        await _client.PostAsync("api/live/save_st", new JsonObject { ["path"] = st }, TimeSpan.FromSeconds(60)).ConfigureAwait(false);
        for (int i = 0; i < 75 && (!File.Exists(st) || new FileInfo(st).Length == 0); i++)
            await Task.Delay(200).ConfigureAwait(false);
        if (!File.Exists(st) || new FileInfo(st).Length == 0)
            throw new EngineException("Mupen did not write its savestate");
        await Task.Delay(300).ConfigureAwait(false);
        var opened = await _client.OpenAsync(st, null, Rom()).ConfigureAwait(false);
        PathStart = st;
        _mupenFrames = 0;
        _mupenEnd = Position(opened["state"]);
        ReadPath(opened["state"]);
    }

    private void ReadPath(JsonNode? state)
    {
        PathFrames = (int)Number(state?["frames"]);
        PathParts = state?["segments"] is JsonArray parts ? parts.Count : 0;
        Notify();
    }

    private static Vector3? Position(JsonNode? state)
        => state is null ? null : new Vector3(Number(state["x"]), Number(state["y"]), Number(state["z"]));

    private async Task<EngineJob> WaitJobAsync(double budget)
    {
        string? lastPhase = null, lastProgress = null, live = null;
        var clock = Stopwatch.StartNew();
        var still = Stopwatch.StartNew();
        var triesClock = Stopwatch.StartNew();
        double stall = Math.Max(StallSeconds, budget * 1.5);
        bool stopAsked = false;
        ClearTries();
        while (true)
        {
            await Task.Delay(400).ConfigureAwait(false);
            var (job, progress) = await _client.JobWithProgressAsync().ConfigureAwait(false);
            if (live is null && progress.Length > 0)
            {
                try
                {
                    live = JsonNode.Parse(progress)?["live"]?.ToString();
                }
                catch (System.Text.Json.JsonException)
                {
                }
            }
            if (live is not null && !job.Done && triesClock.Elapsed.TotalSeconds >= 2.5)
            {
                triesClock.Restart();
                await PollTriesAsync(_client, live).ConfigureAwait(false);
            }
            if (job.Phase is { } phase && phase != lastPhase)
            {
                lastPhase = _lastJobPhase = phase;
                Log("[engine] " + phase + (job.Best is var (e, f) ? $" (best so far: {e} {f} f)" : ""));
            }
            if (job.Done)
                return job;
            if (progress != lastProgress)
            {
                lastProgress = progress;
                still.Restart();
            }
            double t = clock.Elapsed.TotalSeconds;
            string experts = string.Join(" · ", job.Experts.Select(kv => $"{kv.Key} {(kv.Value is int fr ? $"{fr} f" : "—")}"));
            PathPhase = $"{lastPhase ?? "searching"} · {t:0} s{(experts.Length > 0 ? " · " + experts : "")}";
            Notify();
            if (!stopAsked && still.Elapsed.TotalSeconds > stall)
            {
                stopAsked = true;
                Log($"STROOP: no progress from the engine for {stall:0} s: stopped.");
                await _client.StopJobAsync().ConfigureAwait(false);
                still.Restart();
            }
            else if (stopAsked && still.Elapsed.TotalSeconds > 60)
            {
                Log("STROOP: the engine does not stop: it is restarted (the path is kept).");
                await RestartAsync().ConfigureAwait(false);
                throw new EngineException("the engine was stuck and has been restarted: try again");
            }
        }
    }

    private const double StallSeconds = 120;

    public async Task RestartAsync()
    {
        if (!OwnsServer)
        {
            Say("The engine is stuck: it was not started by STROOP, restart it yourself.", true);
            return;
        }
        Kill(_server!);
        _server = null;
        ServerUp = false;
        LiveAttached = false;
        Notify();
        if (!await EnsureServerAsync().ConfigureAwait(false) || !await TakeOverAsync().ConfigureAwait(false))
            return;
        string movie = Path.Combine(WorkFolder, "path.m64");
        if (PathStart is not null && File.Exists(PathStart))
        {
            var opened = await _client.OpenAsync(PathStart, PathFrames > 0 && File.Exists(movie) ? movie : null, Rom()).ConfigureAwait(false);
            ReadPath(opened["state"]);
            _steps.Clear();
            _mupenEnd = null;
        }
    }

    private async Task SyncMupenAsync()
    {
        string movie = Path.Combine(WorkFolder, "path.m64");
        IReadOnlyList<int[]> inputs = [];
        if (PathFrames > 0)
        {
            await _client.PostAsync("api/export", new JsonObject { ["path"] = movie }).ConfigureAwait(false);
            inputs = Movie.Inputs(movie);
        }
        var live = await _client.LiveStateAsync().ConfigureAwait(false);
        bool inPlace = _mupenEnd is { } end && live is not null && Vector3.Distance(end, live.Position) < 1 && _mupenFrames <= inputs.Count;
        if (!inPlace)
        {
            await _client.PostAsync("api/live/load_st", new JsonObject { ["path"] = PathStart }, TimeSpan.FromSeconds(60)).ConfigureAwait(false);
            _mupenFrames = 0;
        }
        var rest = inputs.Skip(_mupenFrames).Select(f => (JsonNode)new JsonArray(f[0], f[1], f[2], f[3])).ToArray();
        if (rest.Length > 0)
            await _client.PostAsync("api/live/inputs", new JsonObject { ["frames"] = new JsonArray(rest) }, TimeSpan.FromMinutes(10)).ConfigureAwait(false);
        _mupenFrames = inputs.Count;
        _mupenEnd = Position((await _client.GetAsync("api/state").ConfigureAwait(false))["state"]);
    }

    private async Task RefreshPathGhostAsync()
    {
        if (PathFrames == 0)
        {
            Post(() => RemoveGhost("path"));
            return;
        }
        var anim = await _client.MovieAnimationAsync().ConfigureAwait(false);
        var ghost = EngineGhosts.FromAnimation(anim, $"Engine path ({PathFrames} f)", SavestateTimer(PathStart) ?? 0);
        if (ghost is not null)
            Post(() => SetGhost(new EngineGhost("path", ghost, PathColor)));
    }

    public async Task BackAsync()
    {
        if (PathBusy || PathStart is null || PathParts == 0)
            return;
        PathBusy = true;
        Notify();
        try
        {
            int target = _steps.Count > 0 ? _steps.Pop() : PathParts - 1;
            while (PathParts > target)
            {
                var state = await _client.PostAsync("api/undo").ConfigureAwait(false);
                int before = PathParts;
                ReadPath(state["state"]);
                if (PathParts >= before)
                    break;
            }
            Phase("Mupen goes back");
            _mupenEnd = null;
            await SyncMupenAsync().ConfigureAwait(false);
            await RefreshPathGhostAsync().ConfigureAwait(false);
            Say($"Back · path: {PathParts} part(s), {PathFrames} frames.");
        }
        catch (Exception e) when (IsEngineFailure(e))
        {
            Say($"Back failed: {Message(e)}", true);
        }
        finally
        {
            PathBusy = false;
            PathPhase = null;
            Notify();
        }
    }

    public void NewPath()
    {
        PathStart = null;
        _steps.Clear();
        PathParts = PathFrames = 0;
        _mupenFrames = 0;
        _mupenEnd = null;
        RemoveGhost("path");
        Notify();
    }

    public async Task<string?> SavePathAsync(string path)
    {
        if (PathStart is null || PathFrames == 0)
        {
            Say("No path yet: right-click in the map → Bring Mario here.", true);
            return null;
        }
        try
        {
            var r = await _client.PostAsync("api/export", new JsonObject { ["path"] = path }).ConfigureAwait(false);
            string movie = r["m64"]?.ToString() ?? path;
            Say($"Saved {Path.GetFileName(movie)} ({PathFrames} frames) and its .st.");
            return movie;
        }
        catch (Exception e) when (IsEngineFailure(e))
        {
            Say($"Save failed: {Message(e)}", true);
            return null;
        }
    }

    public async Task StopAsync()
    {
        try
        {
            await _client.StopJobAsync().ConfigureAwait(false);
        }
        catch (Exception e) when (IsEngineFailure(e))
        {
        }
    }


    public bool StartSearch(SearchOptions options)
    {
        if (Searching)
            return false;
        if (Setup.Problem() is { } problem)
        {
            Say(problem, true);
            return false;
        }
        try
        {
            Directory.CreateDirectory(options.Output);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Say($"Cannot create the output folder: {e.Message}", true);
            return false;
        }
        var progress = new SearchProgress();
        SearchOptions = options;
        Search = progress;
        SearchStarted = DateTime.Now;
        _liveSince = 0;
        Log($"STROOP: improving {Path.GetFileName(options.Movie)} → {options.Output}");
        try
        {
            _search = StartPython(options.Arguments(Setup.CommandLine), line =>
            {
                Log(line);
                Post(() =>
                {
                    if (progress.Read(line) is { } record)
                        Say($"Engine RECORD: {record.Frames} frames ({record.Minutes:0.#} min).");
                    Changed?.Invoke();
                });
            });
        }
        catch (Exception e) when (e is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            Say($"Cannot start the engine ({Setup.Python}): {e.Message}", true);
            Search = null;
            return false;
        }
        var search = _search;
        search.Exited += (_, _) => Post(() => SearchEnded(search));
        StartSearchPolling(progress);
        Notify();
        return true;
    }

    private void SearchEnded(Process search)
    {
        _searchPoll?.Cancel();
        int code = 0;
        try
        {
            code = search.ExitCode;
        }
        catch (InvalidOperationException)
        {
        }
        var best = Search?.Records.MinBy(r => r.Frames);
        Log($"STROOP: the search ended (code {code}){(best is null ? "" : $"; best {best.Frames} frames: {best.Movie}")}");
        Say(best is null ? "The engine found no improvement." : $"Engine: best {best.Frames} frames.", best is null && code != 0);
        RemoveGhost("best");
        RemoveGhost("reference");
        Notify();
    }

    public void StopSearch()
    {
        if (_search is { HasExited: false } search)
        {
            Kill(search);
            Log("STROOP: search stopped.");
        }
    }

    private void StartSearchPolling(SearchProgress progress)
    {
        _searchPoll?.Cancel();
        var cancel = (_searchPoll = new CancellationTokenSource()).Token;
        ClearTries();
        uint? stTimer = null;
        _ = Task.Run(async () =>
        {
            while (!cancel.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(2000, cancel).ConfigureAwait(false);
                    if (progress.Port is not int port || (progress.Output ?? SearchOptions?.Output) is not { } output)
                        continue;
                    if (SearchProgress.LiveFile(output) is not { } live)
                        continue;
                    using var client = EngineClient.Local(port);
                    await PollTriesAsync(client, live, cancel).ConfigureAwait(false);
                    var json = await client.LiveFileAsync(live, _liveSince, cancel).ConfigureAwait(false);
                    if (json["changed"]?.GetValue<bool>() != true)
                        continue;
                    _liveSince = Number(json["mtime"]);
                    stTimer ??= SavestateTimer(json["st"]?.ToString() ?? SearchOptions?.Savestate);
                    var best = EngineGhosts.FromAnimation(json["anim"], "Engine: best attempt", stTimer ?? 0);
                    var reference = EngineGhosts.FromReference(json["ref"], "Engine: reference", stTimer ?? 0);
                    Post(() =>
                    {
                        if (best is not null)
                            SetGhost(new EngineGhost("best", best, BestColor));
                        if (reference is not null)
                            SetGhost(new EngineGhost("reference", reference, ReferenceColor));
                    });
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e) when (IsEngineFailure(e))
                {
                }
            }
        }, cancel);
    }


    private void SetGhost(EngineGhost ghost)
    {
        _ghosts[ghost.Key] = ghost;
        GhostsChanged?.Invoke();
    }

    private void RemoveGhost(string key)
    {
        if (_ghosts.Remove(key))
            GhostsChanged?.Invoke();
    }


    private double _triesSince;
    private int _tryCount;
    private readonly Dictionary<string, uint> _stTimers = new(StringComparer.OrdinalIgnoreCase);

    private async Task PollTriesAsync(EngineClient client, string live, CancellationToken cancel = default)
    {
        try
        {
            var json = await client.TriesAsync(live, _triesSince, cancel).ConfigureAwait(false);
            if (json["changed"]?.GetValue<bool>() != true)
                return;
            _triesSince = Number(json["mtime"]);
            string? st = json["st"]?.ToString();
            uint timer = 0;
            if (st is not null && !_stTimers.TryGetValue(st, out timer))
                _stTimers[st] = timer = SavestateTimer(st) ?? 0;
            ShowTries(EngineGhosts.FromTries(json, timer));
        }
        catch (Exception e) when (IsEngineFailure(e) || e is InvalidOperationException or FormatException)
        {
        }
    }

    private void ShowTries(IReadOnlyList<Ghost> ghosts) => Post(() =>
    {
        for (int i = 0; i < ghosts.Count; i++)
            _ghosts[$"try{i}"] = new EngineGhost($"try{i}", ghosts[i], TryColors[i % TryColors.Length], light: true);
        for (int i = ghosts.Count; i < _tryCount; i++)
            _ghosts.Remove($"try{i}");
        _tryCount = ghosts.Count;
        GhostsChanged?.Invoke();
    });

    private void ClearTries()
    {
        _triesSince = 0;
        ShowTries([]);
    }

    public static uint? SavestateTimer(string? path)
    {
        if (path is null || !File.Exists(path))
            return null;
        try
        {
            using var source = Savestate.Load(path);
            var bytes = new byte[source.Size];
            return source.TryRead(bytes) ? GameFrame.Create(new RamImage(bytes)).GlobalTimer : null;
        }
        catch (Exception e) when (e is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static bool IsEngineFailure(Exception e)
        => e is EngineException or HttpRequestException or TaskCanceledException or System.Text.Json.JsonException or IOException or InvalidOperationException or InvalidDataException;

    private static string Message(Exception e) => e switch
    {
        HttpRequestException => "the engine does not answer",
        TaskCanceledException => "the engine took too long to answer",
        _ => e.Message,
    };

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _clock.Stop();
        _searchPoll?.Cancel();
        if (_search is { } search)
            Kill(search);
        if (LiveAttached)
        {
            try
            {
                _client.LiveStopAsync().Wait(TimeSpan.FromSeconds(2));
            }
            catch (AggregateException)
            {
            }
        }
        if (_server is { } server)
            Kill(server);
        _client.Dispose();
    }
}
