using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using StroopNext.Desktop.Map;
using StroopNext.Desktop.Mario;
using StroopNext.Desktop.Modules;
using StroopNext.Session;
using StroopNext.Settings;

namespace StroopNext.Desktop;

internal sealed class App : Application
{
    public static string? AttachOnStartup { get; set; }
    public static string? OpenOnStartup { get; set; }

    public GameSession? Session { get; private set; }
    public MainWindow? Window { get; private set; }

    public override void Initialize()
    {
        AppTheme.Apply(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (Diagnostics.GlSelfTest.Savestate is null)
            {
                Graphics.MarioAnimations.PersistIn(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StroopNext", "animations"));
                if (Environment.GetEnvironmentVariable("SM64_ROM") is { Length: > 0 } rom)
                    Graphics.MarioAnimations.LoadFromRom(rom);
            }
            Window = CreateWindow(Diagnostics.GlSelfTest.Savestate is null || Environment.GetEnvironmentVariable("STROOPNEXT_SETTINGS") is not null ? SettingsPath() : null);
            Session = Window.Session;
            desktop.MainWindow = Window;
            desktop.Exit += (_, _) => Window.Shutdown();
            if (Diagnostics.GlSelfTest.Savestate is not null)
                Diagnostics.GlSelfTest.Start(Window, desktop);
            if (AttachOnStartup is { } emulator)
                Window.AttachWhenShown(emulator);
            if (OpenOnStartup is { } savestate)
                Window.OpenSavestate(savestate);
        }
        base.OnFrameworkInitializationCompleted();
    }

    public static MainWindow CreateWindow(string? settingsPath)
    {
        var settings = settingsPath is null ? new SettingsStore() : new SettingsStore(settingsPath);
        var session = new GameSession();
        var engine = new Engine.EngineService(settings.Scope("module.engine"), session);
        IStroopModule[] modules = [new MarioModule(), new MapModule(engine)];
        return new MainWindow(session, new Selection(), settings, modules);
    }

    private static string SettingsPath() =>
        Environment.GetEnvironmentVariable("STROOPNEXT_SETTINGS")
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StroopNext", "settings.json");
}
