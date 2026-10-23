using Avalonia;
using Avalonia.Headless;
using Avalonia.Threading;
using StroopNext.Desktop.Map;

namespace StroopNext.Desktop.Diagnostics;

internal static class UiSnapshot
{
    private sealed class SnapshotApp : Application
    {
        public override void Initialize() => AppTheme.Apply(this);
    }

    private static int Drag3D(MainWindow window)
    {
        if (window.Views[1] is not MapModuleView map || map.View3D is not { } view)
        {
            Console.Error.WriteLine("No 3D view.");
            return 1;
        }
        var surface = view.Surface;
        var center = surface.TranslatePoint(new Point(surface.Bounds.Width / 2, surface.Bounds.Height / 2), window) ?? default;
        Console.WriteLine($"3D surface {surface.Bounds.Width:0}×{surface.Bounds.Height:0} at {center}");
        for (var v = Avalonia.Input.InputExtensions.InputHitTest(window, center) as Visual; v is not null; v = Avalonia.VisualTree.VisualExtensions.GetVisualParent(v))
            Console.WriteLine($"  hit chain: {v.GetType().Name} {(v as StyledElement)?.Name} bounds {v.Bounds} visible {v.IsVisible} hitTest {(v as Avalonia.Input.InputElement)?.IsHitTestVisible} bg {(v as Avalonia.Controls.Border)?.Background}");
        void Pump()
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        }
        void DragWith(Avalonia.Input.MouseButton button, Avalonia.Input.RawInputModifiers held, string name)
        {
            Console.WriteLine($"before {name}: {view.CameraState}");
            window.MouseMove(center);
            window.MouseDown(center, button);
            Pump();
            for (int i = 1; i <= 10; i++)
            {
                window.MouseMove(center + new Point(20 * i, 8 * i), held);
                Pump();
            }
            window.MouseUp(center + new Point(200, 80), button);
            Pump();
            Console.WriteLine($"after  {name}: {view.CameraState}");
        }
        DragWith(Avalonia.Input.MouseButton.Left, Avalonia.Input.RawInputModifiers.LeftMouseButton, "left drag (orbit)");
        DragWith(Avalonia.Input.MouseButton.Right, Avalonia.Input.RawInputModifiers.RightMouseButton, "right drag (pan)");
        Console.WriteLine($"status: {window.StatusText}");
        window.Close();
        return 0;
    }

    private static int RecordGhost(MainWindow window)
    {
        void Pump()
        {
            for (int i = 0; i < 5; i++)
            {
                window.OnTick();
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                Thread.Sleep(15);
            }
        }
        Avalonia.Controls.Button? Find(string text) => Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(window)
            .OfType<Avalonia.Controls.Button>().FirstOrDefault(b => b.Content as string == text && b.IsEffectivelyVisible);
        void Click(string text)
        {
            var button = Find(text) ?? throw new InvalidOperationException($"No visible button \"{text}\".");
            Pump();
            var at = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window) ?? default;
            window.MouseDown(at, Avalonia.Input.MouseButton.Left);
            window.MouseUp(at, Avalonia.Input.MouseButton.Left);
            Pump();
            Console.WriteLine($"clicked \"{text}\" → status: {window.StatusText}");
        }
        Click("● Record");
        Click("■ Stop");
        if (window.Views[1] is MapModuleView map)
            foreach (var g in map.Ghosts)
                Console.WriteLine($"ghost \"{g.Ghost.Name}\": {g.Ghost.Frames.Count} frames, visible {g.Visible}, file {g.FilePath ?? "none"}");
        window.Close();
        return 0;
    }

    private static int InGame(MainWindow window, string controlFile)
    {
        string last = "", lastCommand = "";
        var end = DateTime.UtcNow.AddMinutes(15);
        while (DateTime.UtcNow < end)
        {
            window.OnTick();
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            string command = "";
            try { command = File.Exists(controlFile) ? File.ReadAllText(controlFile).Trim() : ""; } catch (IOException) { }
            if (command != lastCommand)
            {
                lastCommand = command;
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} command: {command}");
                if (command == "quit")
                    break;
                var engineView = (window.Views[1] as MapModuleView)?.EnginePanel;
                string[] words = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                float W(string v) => float.Parse(v, System.Globalization.CultureInfo.InvariantCulture);
                if (words is ["teleport", var tx, var tz])
                    (window.Views[1] as MapModuleView)?.TeleportForTest(W(tx), W(tz));
                if (words is ["golevel", var level, var area, .. var golevelRest] && window.Views[1] is MapModuleView levelMap)
                    levelMap.GoToLevelForTest(int.Parse(level, System.Globalization.CultureInfo.InvariantCulture), int.Parse(area, System.Globalization.CultureInfo.InvariantCulture), golevelRest is ["place"]);
                if (words is ["place", var px, var pz] && window.Views[1] is MapModuleView placeMap)
                    Console.WriteLine($"place click used: {placeMap.PlaceForTest(W(px), W(pz))}");
                if (words is ["levelmenu"] && window.Views[1] is MapModuleView menuMap)
                    Console.WriteLine(menuMap.LevelMenuForTest());
                if (words is ["teleport3d", var ux, var uz])
                    (window.Views[1] as MapModuleView)?.Teleport3DForTest(W(ux), W(uz));
                if (words is ["scroll", var offset] && window.Views[1] is MapModuleView scrolled)
                    foreach (var sv in Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(scrolled).OfType<Avalonia.Controls.ScrollViewer>())
                        sv.Offset = new Vector(0, W(offset));
                if (words is ["attach"])
                    window.AttachFirstEmulator();
                if (words is ["tab", var n])
                    window.SelectedTab = int.Parse(n, System.Globalization.CultureInfo.InvariantCulture);
                if (words is ["capture", var png])
                {
                    for (int i = 0; i < 10; i++)
                    {
                        window.OnTick();
                        Dispatcher.UIThread.RunJobs();
                        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                        Thread.Sleep(15);
                    }
                    using var shot = window.CaptureRenderedFrame();
                    shot?.Save(png, Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
                    Console.WriteLine($"captured {png}");
                }
                if (words is ["goto", var gx, var gy, var gz] && engineView is not null)
                {
                    float F(string v) => float.Parse(v, System.Globalization.CultureInfo.InvariantCulture);
                    _ = engineView.Engine.BringHereAsync(F(gx), gy == "any" ? ((window.Views[1] as MapModuleView)?.GroundY(F(gx), F(gz)) ?? 0) : F(gy), F(gz));
                }
                if (words is ["back"] && engineView is not null)
                    _ = engineView.Engine.BackAsync();
                if (words is ["save", var path] && engineView is not null)
                    _ = engineView.Engine.SavePathAsync(path).ContinueWith(t => Console.WriteLine($"saved: {t.Result ?? "nothing"}"));
                if (command.StartsWith("click ", StringComparison.Ordinal))
                {
                    string text = command[6..];
                    var button = Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(window)
                        .OfType<Avalonia.Controls.Button>().FirstOrDefault(b => b.Content as string == text && b.IsEffectivelyVisible);
                    if (button is null)
                        Console.WriteLine($"no visible button \"{text}\"");
                    else
                    {
                        Dispatcher.UIThread.RunJobs();
                        button.Command?.Execute(null);
                        button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
                        Console.WriteLine($"clicked \"{text}\" (enabled {button.IsEnabled})");
                    }
                }
            }
            string now = $"status: {window.StatusText} | {(window.Views[1] as MapModuleView)?.MarioText} | {(window.Views[1] as MapModuleView)?.PlacementText} | {(window.Views[1] as MapModuleView)?.InGameStatusText} | engine {(window.Views[1] as MapModuleView)?.EnginePanel?.Summary}";
            if (now != last)
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {now}");
                last = now;
            }
            Thread.Sleep(15);
        }
        window.Close();
        return 0;
    }

    public static int Run(string savestate, string output, string tab)
    {
        AppBuilder.Configure<SnapshotApp>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .WithInterFont()
            .SetupWithoutStarting();

        var window = App.CreateWindow(settingsPath: Environment.GetEnvironmentVariable("STROOPNEXT_SETTINGS"));
        window.Width = 1400;
        window.Height = 860;
        window.SelectedTab = tab is "map" or "drag3d" or "ghostrec" or "ingame" or "engine" ? 1 : 0;
        if (tab is "ghostrec" or "ingame")
            window.Height = 2000;
        window.Show();
        if (tab == "drag3d" && window.Views[1] is MapModuleView map3D)
            map3D.Projection3D = true;
        var popout = tab == "popout" ? window.PopOut("map") : null;
        if (popout is not null)
        {
            popout.Width = 1100;
            popout.Height = 760;
        }
        if (Environment.GetEnvironmentVariable("STROOPNEXT_TRACKERS") is { } trackers && File.Exists(trackers) && window.Views[1] is MapModuleView map)
            map.ImportTrackers(File.ReadAllText(trackers));
        if (Environment.GetEnvironmentVariable("STROOPNEXT_GHOST") is { } ghostFile && File.Exists(ghostFile) && window.Views[1] is MapModuleView ghostMap)
            ghostMap.LoadGhostFile(ghostFile);
        if (savestate == "mupen")
        {
            if (!window.AttachFirstEmulator())
            {
                Console.Error.WriteLine("No running emulator.");
                return 1;
            }
        }
        else
            window.OpenSavestate(savestate);

        var until = DateTime.UtcNow.AddSeconds(3);
        int frames = 0;
        while (DateTime.UtcNow < until && frames < 40)
        {
            window.OnTick();
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Thread.Sleep(15);
            frames++;
        }

        if (tab == "drag3d")
            return Drag3D(window);
        if (tab == "ghostrec")
            return RecordGhost(window);
        if (tab == "ingame")
            return InGame(window, output);

        using var frame = (popout ?? window).CaptureRenderedFrame();
        if (frame is null)
        {
            Console.Error.WriteLine("Nothing was rendered.");
            return 1;
        }
        frame.Save(output, Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
        Console.WriteLine($"{frame.PixelSize.Width}×{frame.PixelSize.Height}, tab '{tab}', status: {window.StatusText} → {output}");
        window.Close();
        return 0;
    }
}
