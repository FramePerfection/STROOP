using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using OpenTK.Graphics.OpenGL4;
using SkiaSharp;
using StroopNext.Desktop.Map;

namespace StroopNext.Desktop.Diagnostics;

internal static class GlSelfTest
{
    public static string? Savestate { get; set; }
    public static string? Output { get; set; }
    public static int ExitCode { get; private set; } = 2;

    private static int _renders;
    private static MapModuleView? _popout;

    private static bool DragTest => Environment.GetEnvironmentVariable("STROOPNEXT_GL_DRAG") == "1";

    private static void SimulateDrag()
    {
        if ((Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow is not MainWindow window)
            return;
        var map = Environment.GetEnvironmentVariable("STROOPNEXT_GL_POPOUT") == "1" ? _popout : window.Views[1] as MapModuleView;
        if (map?.View3D is not { } view)
        {
            Console.Error.WriteLine($"Drag test: no 3D view (popout {_popout is not null}).");
            return;
        }
        var surface = view.Surface;
        Console.WriteLine($"before drag: {view.CameraState}");
        var pointer = new Avalonia.Input.Pointer(Avalonia.Input.Pointer.GetNextFreeId(), Avalonia.Input.PointerType.Mouse, true);
        var root = (Visual)TopLevel.GetTopLevel(surface)!;
        Point At(double x) => surface.TranslatePoint(new Point(x, surface.Bounds.Height / 2), root) ?? default;
        var pressed = new Avalonia.Input.PointerPointProperties(Avalonia.Input.RawInputModifiers.LeftMouseButton, Avalonia.Input.PointerUpdateKind.LeftButtonPressed);
        var moving = new Avalonia.Input.PointerPointProperties(Avalonia.Input.RawInputModifiers.LeftMouseButton, Avalonia.Input.PointerUpdateKind.Other);
        var released = new Avalonia.Input.PointerPointProperties(Avalonia.Input.RawInputModifiers.None, Avalonia.Input.PointerUpdateKind.LeftButtonReleased);
        double x0 = surface.Bounds.Width / 2 - 100;
        surface.RaiseEvent(new Avalonia.Input.PointerPressedEventArgs(surface, pointer, root, At(x0), 1, pressed, Avalonia.Input.KeyModifiers.None));
        for (int i = 1; i <= 10; i++)
            surface.RaiseEvent(new Avalonia.Input.PointerEventArgs(Avalonia.Input.InputElement.PointerMovedEvent, surface, pointer, root, At(x0 + 20 * i), (ulong)(1 + i), moving, Avalonia.Input.KeyModifiers.None));
        surface.RaiseEvent(new Avalonia.Input.PointerReleasedEventArgs(surface, pointer, root, At(x0 + 200), 20, released, Avalonia.Input.KeyModifiers.None, Avalonia.Input.MouseButton.Left));
        Console.WriteLine($"after drag:  {view.CameraState}");
        Console.WriteLine($"status: {window.StatusText}");
    }

    public static void Start(MainWindow window, IClassicDesktopStyleApplicationLifetime desktop)
    {
        window.ShowActivated = false;
        window.ShowInTaskbar = false;
        window.WindowDecorations = WindowDecorations.None;
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Position = new PixelPoint(-20000, -20000);
        window.Width = 1000;
        window.Height = 700;

        GlSurface.AfterRender = Capture;
        window.Opened += (_, _) =>
        {
            window.OpenSavestate(Savestate!);
            window.SelectedTab = 1;
            if (window.Views[1] is MapModuleView map)
                map.Projection3D = true;
            if (Environment.GetEnvironmentVariable("STROOPNEXT_GL_POPOUT") == "1" && window.PopOut("map") is { } popoutWindow)
            {
                popoutWindow.ShowActivated = false;
                popoutWindow.Position = new PixelPoint(-20000, -18000);
                _popout = popoutWindow.Content as MapModuleView;
                if (_popout is not null)
                    _popout.Projection3D = true;
            }
        };
        var pump = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        pump.Tick += (_, _) =>
        {
            (window.Views[1] as MapModuleView)?.Refresh();
            _popout?.Refresh();
        };
        pump.Start();
        DispatcherTimer.RunOnce(() =>
        {
            Console.Error.WriteLine($"No 3D frame captured in time ({_renders} renders).");
            desktop.Shutdown(ExitCode);
        }, TimeSpan.FromSeconds(20));
    }

    private static void Capture(int width, int height)
    {
        ++_renders;
        if (DragTest && _renders == 10)
        {
            Dispatcher.UIThread.Post(SimulateDrag);
            return;
        }
        if (_renders < (DragTest ? 30 : 10) || Output is null)
            return;
        var pixels = new byte[width * height * 4];
        GL.ReadPixels(0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
        var error = GL.GetError();
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
        int stride = width * 4;
        nint target = bitmap.GetPixels();
        for (int y = 0; y < height; y++)
            System.Runtime.InteropServices.Marshal.Copy(pixels, (height - 1 - y) * stride, target + y * bitmap.RowBytes, stride);
        using (var image = SKImage.FromBitmap(bitmap))
        using (var png = image.Encode(SKEncodedImageFormat.Png, 100))
        using (var file = File.Create(Output))
            png.SaveTo(file);
        Console.WriteLine($"{GL.GetString(StringName.Renderer)} · {GL.GetString(StringName.Version)} · {width}×{height} after {_renders} renders · GL error: {error} → {Output}");
        Output = null;
        ExitCode = error == ErrorCode.NoError ? 0 : 1;
        GlSurface.AfterRender = null;
        Dispatcher.UIThread.Post(() => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown(ExitCode));
    }
}
