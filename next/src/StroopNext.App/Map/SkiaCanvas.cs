using System.Drawing.Imaging;
using SkiaSharp;

namespace StroopNext.App.Map;

internal sealed class SkiaCanvas : Control
{
    private Bitmap? _bitmap;

    public SkiaCanvas()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
        SetStyle(ControlStyles.Opaque, true);
    }

    public event Action<SKCanvas, int, int>? PaintSkia;

    protected override void OnPaint(PaintEventArgs e)
    {
        int width = Math.Max(1, ClientSize.Width), height = Math.Max(1, ClientSize.Height);
        if (_bitmap is null || _bitmap.Width != width || _bitmap.Height != height)
        {
            _bitmap?.Dispose();
            _bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
        }

        var data = _bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
        try
        {
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info, data.Scan0, data.Stride);
            PaintSkia?.Invoke(surface.Canvas, width, height);
            surface.Canvas.Flush();
        }
        finally
        {
            _bitmap.UnlockBits(data);
        }
        e.Graphics.DrawImageUnscaled(_bitmap, 0, 0);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        Focus();
        base.OnMouseDown(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _bitmap?.Dispose();
        base.Dispose(disposing);
    }
}
