using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using OpenTK.Graphics.OpenGL4;

namespace StroopNext.Desktop.Map;

internal sealed class GlSurface : OpenGlControlBase, Avalonia.Rendering.ICustomHitTest
{
    private bool _failed;

    public bool HitTest(Avalonia.Point point) => new Avalonia.Rect(Bounds.Size).Contains(point);

    public object SyncRoot { get; } = new();

    public Action? Init { get; set; }

    public Action<int, int>? RenderGl { get; set; }

    public Action? Deinit { get; set; }

    public static Action<int, int>? AfterRender { get; set; }

    public event Action<string>? Failed;

    public double Scaling => TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;

    private sealed class Bindings(GlInterface gl) : OpenTK.IBindingsContext
    {
        public nint GetProcAddress(string procName) => gl.GetProcAddress(procName);
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        GL.LoadBindings(new Bindings(gl));
        try
        {
            lock (SyncRoot)
                Init?.Invoke();
        }
        catch (Exception e)
        {
            Fail(e);
        }
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        lock (SyncRoot)
            Deinit?.Invoke();
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (_failed)
            return;
        try
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fb);
            int width = Math.Max(1, (int)Math.Ceiling(Bounds.Width * Scaling)), height = Math.Max(1, (int)Math.Ceiling(Bounds.Height * Scaling));
            lock (SyncRoot)
                RenderGl?.Invoke(width, height);
            AfterRender?.Invoke(width, height);
        }
        catch (Exception e)
        {
            Fail(e);
        }
    }

    private void Fail(Exception e)
    {
        if (_failed)
            return;
        _failed = true;
        string message = $"3D map unavailable: {e.Message}";
        Avalonia.Threading.Dispatcher.UIThread.Post(() => Failed?.Invoke(message));
    }
}
