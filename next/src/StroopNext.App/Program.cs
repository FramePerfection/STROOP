using StroopNext.Rendering.Skia;
using StroopNext.Rendering.Gl;
using StroopNext.Rendering.Diagnostics;
using StroopNext.App.Map;
using StroopNext.App.Mario;
using StroopNext.App.Modules;
using StroopNext.Session;
using StroopNext.Settings;

namespace StroopNext.App;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args is ["--bench", var benchState, .. var benchRest])
            return Bench.Run(
                benchState,
                benchRest.Length > 0 ? int.Parse(benchRest[0], System.Globalization.CultureInfo.InvariantCulture) : 20,
                benchRest.Length > 2 && benchRest[1] == "--without" ? benchRest[2].Split(',') : null,
                StroopNext.Platform.TimerResolution.OneMillisecond());
        if (args is ["--bench-3d", var benchState3D, .. var benchFrames])
            return Snapshot3D.Bench(benchState3D, benchFrames.Length > 0 ? int.Parse(benchFrames[0], System.Globalization.CultureInfo.InvariantCulture) : 300);
        if (args is ["--render-3d", var state3D, var output3D, .. var options3D])
            return Snapshot3D.Run(state3D, output3D, options3D);
        if (args is ["--render-map", var savestate, var output, .. var rest])
            return MapSnapshot.Run(savestate, output, rest.Length > 0 ? double.Parse(rest[0], System.Globalization.CultureInfo.InvariantCulture) : 0.25);

        ApplicationConfiguration.Initialize();

        string settingsPath = Environment.GetEnvironmentVariable("STROOPNEXT_SETTINGS")
                              ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StroopNext", "settings.json");
        var settings = new SettingsStore(settingsPath);
        using var timer = StroopNext.Platform.TimerResolution.OneMillisecond();
        using var session = new GameSession();
        var selection = new Selection();

        IStroopModule[] modules = [new MarioModule(), new MapModule()];

        using var form = new MainForm(session, selection, settings, modules);
        if (args is ["--attach", var emulator])
            form.AttachOnStartup(emulator);
        else if (args.Length == 1 && File.Exists(args[0]))
            form.OpenSavestate(args[0]);
        Application.Run(form);
        settings.Save();
        return 0;
    }
}
