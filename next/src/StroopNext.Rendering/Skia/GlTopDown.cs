using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using SkiaSharp;
using StroopNext.Rendering.Gl;
using StroopNext.Graphics;
using StroopNext.Map;
using StroopNext.Map3D;

namespace StroopNext.Rendering.Skia;

internal sealed class GlTopDown : IDisposable
{
    private OffscreenGl? _gl;
    private GameSceneRenderer? _renderer;
    private SKBitmap? _bitmap;
    private byte[] _pixels = [];
    private bool _failed;
    private readonly Camera3D _camera = new();

    public bool Available => !_failed;

    public bool TryDraw(SKCanvas canvas, GameScene scene, GameVisualsOptions options, MapView view, float margin)
    {
        if (_failed)
            return false;
        int width = (int)MathF.Ceiling(view.Width + 2 * margin), height = (int)MathF.Ceiling(view.Height + 2 * margin);
        if (width <= 0 || height <= 0 || width > 8192 || height > 8192)
            return false;
        try
        {
            Ensure(width, height);
            _gl!.MakeCurrent();

            var center = view.Center;
            var up = view.ScreenToWorld(new Vector2(view.Width / 2, view.Height / 2 - 1)) - center;
            _camera.Target = new Vector3(center.X, -30000, center.Y);
            _camera.Eye = new Vector3(center.X, 30000, center.Y);
            _camera.Up = Vector3.Normalize(new Vector3(up.X, 0, up.Y));
            _camera.OrthographicWidth = (float)(width / view.Scale);
            _camera.Width = width;
            _camera.Height = height;
            _camera.Far = 70000;

            GL.Viewport(0, 0, width, height);
            var background = SkiaMapRenderer.Background;
            GL.ClearColor(background.Red / 255f, background.Green / 255f, background.Blue / 255f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _renderer!.Render(scene, options with { Objects = false, Sky = false, Fog = false, CullBackFaces = true, KeepMarioVisible = false }, _camera, width, height);

            int stride = width * 4;
            if (_pixels.Length < stride * height)
                _pixels = new byte[stride * height];
            GL.ReadPixels(0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, _pixels);
            nint target = _bitmap!.GetPixels();
            int targetStride = _bitmap.RowBytes;
            for (int y = 0; y < height; y++)
                Marshal.Copy(_pixels, (height - 1 - y) * stride, target + y * targetStride, stride);
            _bitmap.NotifyPixelsChanged();
            canvas.DrawBitmap(_bitmap, new SKRect(0, 0, width, height), new SKRect(-margin, -margin, width - margin, height - margin));
            return true;
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"2D game level on the GPU failed, using Skia: {e.Message}");
            _failed = true;
            Release();
            return false;
        }
    }

    private void Ensure(int width, int height)
    {
        if (_gl is null || _gl.Width < width || _gl.Height < height)
        {
            int w = Math.Max(width, _gl?.Width ?? 0), h = Math.Max(height, _gl?.Height ?? 0);
            if (_gl is not null)
                w += 256;
            Release();
            _gl = new OffscreenGl(w, h + (w > width ? 256 : 0));
            _renderer = new GameSceneRenderer();
        }
        if (_bitmap is null || _bitmap.Width != width || _bitmap.Height != height)
        {
            _bitmap?.Dispose();
            _bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
        }
    }

    private void Release()
    {
        if (_gl is not null)
        {
            try
            {
                _gl.MakeCurrent();
                _renderer?.Dispose();
            }
            catch
            {
            }
            _gl.Dispose();
        }
        _renderer = null;
        _gl = null;
    }

    public void Dispose()
    {
        Release();
        _bitmap?.Dispose();
        _bitmap = null;
    }
}
