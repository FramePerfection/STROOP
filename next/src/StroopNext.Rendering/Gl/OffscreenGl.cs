using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace StroopNext.Rendering.Gl;

internal sealed class OffscreenGl : IDisposable
{
    private readonly OpenTK.Windowing.Desktop.NativeWindow _window;
    private readonly int _fbo, _color, _depth;

    public OffscreenGl(int width, int height)
    {
        Width = width;
        Height = height;
        _window = new OpenTK.Windowing.Desktop.NativeWindow(new NativeWindowSettings
        {
            StartVisible = false,
            StartFocused = false,
            ClientSize = new OpenTK.Mathematics.Vector2i(64, 64),
            APIVersion = new Version(3, 3),
            Profile = ContextProfile.Core,
            Flags = OperatingSystem.IsMacOS() ? ContextFlags.ForwardCompatible | ContextFlags.Offscreen : ContextFlags.Offscreen,
            Title = "STROOP Next offscreen",
        });
        _window.MakeCurrent();

        _fbo = GL.GenFramebuffer();
        _color = GL.GenRenderbuffer();
        _depth = GL.GenRenderbuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _color);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, width, height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, _color);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depth);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, width, height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depth);
        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
            throw new InvalidOperationException($"Offscreen framebuffer incomplete: {status}");
    }

    public int Width { get; }
    public int Height { get; }

    public void MakeCurrent()
    {
        _window.MakeCurrent();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
    }
    public string RendererName => GL.GetString(StringName.Renderer);

    public void Dispose()
    {
        GL.DeleteRenderbuffer(_color);
        GL.DeleteRenderbuffer(_depth);
        GL.DeleteFramebuffer(_fbo);
        _window.Dispose();
    }
}
