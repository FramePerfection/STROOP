using System.Globalization;
using Avalonia;
using Avalonia.OpenGL;
using StroopNext.Desktop.Diagnostics;
using StroopNext.Platform;
using StroopNext.Rendering.Diagnostics;
using StroopNext.Rendering.Gl;
using StroopNext.Rendering.Skia;

namespace StroopNext.Desktop;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args is ["--bench", var benchState, .. var benchRest])
            return Bench.Run(
                benchState,
                benchRest.Length > 0 ? int.Parse(benchRest[0], CultureInfo.InvariantCulture) : 20,
                benchRest.Length > 2 && benchRest[1] == "--without" ? benchRest[2].Split(',') : null,
                TimerResolution.OneMillisecond());
        if (args is ["--bench-3d", var benchState3D, .. var benchFrames])
            return Snapshot3D.Bench(benchState3D, benchFrames.Length > 0 ? int.Parse(benchFrames[0], CultureInfo.InvariantCulture) : 300);
        if (args is ["--render-3d", var state3D, var output3D, .. var options3D])
            return Snapshot3D.Run(state3D, output3D, options3D);
        if (args is ["--render-map", var savestate, var output, .. var rest])
            return MapSnapshot.Run(savestate, output, rest.Length > 0 ? double.Parse(rest[0], CultureInfo.InvariantCulture) : 0.25);
        if (args is ["--ui-snapshot", var uiState, var uiOutput, .. var uiRest])
            return UiSnapshot.Run(uiState, uiOutput, uiRest.Length > 0 ? uiRest[0] : "mario");

        if (args is ["--ghost-serve", var control, .. var serveRest])
            return GhostServe.Run(control, serveRest.Length > 0 ? serveRest[0] : null);
        if (args is ["--dump-ram", var dumpState, var dumpAddress, var dumpCount])
        {
            using var dumpSource = StroopNext.Memory.Savestate.Load(dumpState);
            var dumpBytes = new byte[dumpSource.Size];
            dumpSource.TryRead(dumpBytes);
            var dumpRam = new StroopNext.Memory.RamImage(dumpBytes);
            uint from = Convert.ToUInt32(dumpAddress, 16);
            for (uint i = 0; i < uint.Parse(dumpCount, CultureInfo.InvariantCulture); i++)
                Console.WriteLine($"{from + i * 4:X8}: {dumpRam.U32(from + i * 4):X8}");
            return 0;
        }
        if (args is ["--ghost-probe"])
            return GhostProbe.Run();
        if (args is ["--gl-selftest", var glState, var glOutput])
        {
            GlSelfTest.Savestate = glState;
            GlSelfTest.Output = glOutput;
            BuildAvaloniaApp().StartWithClassicDesktopLifetime([]);
            return GlSelfTest.ExitCode;
        }

        if (args is ["--attach", var emulator])
            App.AttachOnStartup = emulator;
        else if (args.Length == 1 && File.Exists(args[0]))
            App.OpenOnStartup = args[0];

        using var timer = TimerResolution.OneMillisecond();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        return 0;
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions
            {
                RenderingMode = [Win32RenderingMode.Wgl, Win32RenderingMode.AngleEgl, Win32RenderingMode.Software],
                WglProfiles = [new GlVersion(GlProfileType.OpenGL, 4, 6), new GlVersion(GlProfileType.OpenGL, 4, 0), new GlVersion(GlProfileType.OpenGL, 3, 3)],
            })
            .With(new X11PlatformOptions { RenderingMode = [X11RenderingMode.Glx, X11RenderingMode.Egl, X11RenderingMode.Software] })
            .With(new AvaloniaNativePlatformOptions { RenderingMode = [AvaloniaNativeRenderingMode.OpenGl, AvaloniaNativeRenderingMode.Software] })
            .WithInterFont()
            .LogToTrace();
}
