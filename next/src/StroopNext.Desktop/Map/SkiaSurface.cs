using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace StroopNext.Desktop.Map;

internal sealed class SkiaSurface : Control
{
    public object SyncRoot { get; } = new();

    public Action<SKCanvas, int, int, GRContext?>? PaintSkia { get; set; }

    public double Scaling => TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;

    public System.Numerics.Vector2 ToPixels(Point point) => new((float)(point.X * Scaling), (float)(point.Y * Scaling));

    public int PixelWidth => Math.Max(1, (int)Math.Ceiling(Bounds.Width * Scaling));
    public int PixelHeight => Math.Max(1, (int)Math.Ceiling(Bounds.Height * Scaling));

    public SkiaSurface()
    {
        ClipToBounds = true;
        Focusable = true;
    }

    public override void Render(DrawingContext context) => context.Custom(new DrawOperation(this, new Rect(Bounds.Size), Scaling));

    private sealed class DrawOperation(SkiaSurface owner, Rect bounds, double scaling) : ICustomDrawOperation
    {
        public Rect Bounds => bounds;
        public bool HitTest(Point p) => bounds.Contains(p);
        public bool Equals(ICustomDrawOperation? other) => false;
        public void Dispose() { }

        public void Render(ImmediateDrawingContext context)
        {
            if (context.TryGetFeature<ISkiaSharpApiLeaseFeature>() is not { } feature)
                return;
            using var lease = feature.Lease();
            var canvas = lease.SkCanvas;
            int width = Math.Max(1, (int)Math.Ceiling(bounds.Width * scaling)), height = Math.Max(1, (int)Math.Ceiling(bounds.Height * scaling));
            canvas.Save();
            canvas.ClipRect(new SKRect(0, 0, (float)bounds.Width, (float)bounds.Height));
            canvas.Scale((float)(1 / scaling));
            lock (owner.SyncRoot)
                owner.PaintSkia?.Invoke(canvas, width, height, lease.GrContext);
            canvas.Restore();
        }
    }
}
