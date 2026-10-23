using System.Numerics;
using SkiaSharp;
using StroopNext.Map;

namespace StroopNext.Rendering.Skia;

internal sealed class SkiaMapRenderer : IDisposable
{
    public static readonly SKColor Background = new(0x1B, 0x1C, 0x24);

    private readonly SKPaint _fill = new() { Style = SKPaintStyle.Fill, IsAntialias = false };
    private readonly SKPaint _smoothFill = new() { Style = SKPaintStyle.Fill, IsAntialias = true };
    private readonly SKPaint _stroke = new() { Style = SKPaintStyle.Stroke, IsAntialias = true, StrokeJoin = SKStrokeJoin.Round, StrokeCap = SKStrokeCap.Round };
    private readonly SKPaint _text = new() { IsAntialias = true, Color = SKColors.White };
    private readonly SKTypeface _typeface = SKTypeface.FromFamilyName("Segoe UI");
    private readonly SKTypeface _boldTypeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold);
    private readonly SKFont _font;
    private readonly SKFont _boldFont;

    private readonly SKPath _path = new();
    private readonly SkiaGameRenderer _game = new();
    private readonly GlTopDown _topDown = new();

    public bool GameLevelOnGpu => GpuContext is not null || (UseOffscreenGl && _topDown.Available);

    public GRRecordingContext? GpuContext
    {
        get => _gpuContext;
        set
        {
            if (ReferenceEquals(value, _gpuContext))
                return;
            _cache?.Dispose();
            _cache = null;
            _cacheKey = null;
            _gpuContext = value;
        }
    }

    private GRRecordingContext? _gpuContext;

    public bool UseOffscreenGl { get; set; } = true;

    private readonly SKPaint _copy = new() { BlendMode = SKBlendMode.Src };

    private SKImage? _cache;
    private SceneKey? _cacheKey;
    private (double Scale, double Rotation, float Width, float Height, float Margin) _cacheView;
    private Vector2 _cacheCenter;

    public SkiaMapRenderer()
    {
        _font = new SKFont(_typeface, 12);
        _boldFont = new SKFont(_boldTypeface, 12);
    }

    public int BackgroundRenders { get; private set; }
    public int BackgroundReuses { get; private set; }
    public TimeSpan BackgroundTime { get; private set; }

    private static SKColor C(Argb8Color  c) => new(c.R, c.G, c.B, c.A);
    private static SKPoint P(Vector2 v) => new(v.X, v.Y);

    public void Draw(SKCanvas canvas, MapScene scene, MapView view, MapPrimitive? hovered)
    {
        canvas.Clear(Background);
        long started = System.Diagnostics.Stopwatch.GetTimestamp();
        DrawBackground(canvas, scene, view);
        BackgroundTime += System.Diagnostics.Stopwatch.GetElapsedTime(started);
        if (scene.GameVisuals is { } game && scene.GameVisualsOptions is { } options)
            _game.DrawLive(canvas, game, options, scene.GameShadows, view);
        foreach (var item in scene.Items)
            DrawItem(canvas, item, view, emphasis: false);
        if (hovered is not null)
            DrawItem(canvas, hovered, view, emphasis: true);
        DrawScaleBar(canvas, view);
        if (view.Rotation != 0)
            DrawNorth(canvas, view);
    }

    private void DrawBackground(SKCanvas canvas, MapScene scene, MapView view)
    {
        float margin = view.CullMarginPx;
        if (margin <= 0)
        {
            DrawGameLevel(canvas, scene, view, 0);
            foreach (var item in scene.Background)
                DrawItem(canvas, item, view, emphasis: false);
            return;
        }

        var key = scene.BackgroundKey;
        var viewKey = (view.Scale, view.Rotation, view.Width, view.Height, margin);
        if (_cache is not null && viewKey == _cacheView && key.Equals(_cacheKey))
        {
            var shift = view.WorldToScreen(_cacheCenter) - new Vector2(view.Width / 2, view.Height / 2);
            if (MathF.Abs(shift.X) <= margin && MathF.Abs(shift.Y) <= margin)
            {
                canvas.DrawImage(_cache, MathF.Round(shift.X - margin), MathF.Round(shift.Y - margin), _copy);
                BackgroundReuses++;
                return;
            }
        }

        var info = new SKImageInfo((int)MathF.Ceiling(view.Width + 2 * margin), (int)MathF.Ceiling(view.Height + 2 * margin), SKColorType.Bgra8888, SKAlphaType.Premul);
        using (var surface = (GpuContext is { } gpu ? SKSurface.Create(gpu, false, info) : null) ?? SKSurface.Create(info))
        {
            surface.Canvas.Clear(Background);
            surface.Canvas.Translate(margin, margin);
            DrawGameLevel(surface.Canvas, scene, view, margin);
            foreach (var item in scene.Background)
                DrawItem(surface.Canvas, item, view, emphasis: false);
            _cache?.Dispose();
            _cache = surface.Snapshot();
        }
        _cacheKey = key;
        _cacheView = viewKey;
        _cacheCenter = view.Center;
        BackgroundRenders++;
        canvas.DrawImage(_cache, -margin, -margin, _copy);
    }

    private void DrawGameLevel(SKCanvas canvas, MapScene scene, MapView view, float margin)
    {
        if (scene.GameVisuals is not { } game || scene.GameVisualsOptions is not { Level: true } options)
            return;
        if (GpuContext is not null || !UseOffscreenGl || !_topDown.TryDraw(canvas, game, options, view, margin))
            _game.DrawLevel(canvas, game, options.MaxY, view);
    }

    private void DrawItem(SKCanvas canvas, MapPrimitive item, MapView view, bool emphasis)
    {
        switch (item)
        {
            case MapPolygon polygon:
                _path.Reset();
                _path.MoveTo(P(view.WorldToScreen(polygon.Points[0])));
                for (int i = 1; i < polygon.Points.Length; i++)
                    _path.LineTo(P(view.WorldToScreen(polygon.Points[i])));
                _path.Close();
                if (emphasis)
                {
                    Stroke(canvas, _path, new Argb8Color(255, 255, 255, 255), 2.5f);
                    break;
                }
                if (polygon.Fill.IsVisible)
                {
                    _fill.Color = C(polygon.Fill);
                    canvas.DrawPath(_path, _fill);
                }
                if (polygon.Stroke.IsVisible)
                    Stroke(canvas, _path, polygon.Stroke, polygon.StrokePx);
                break;

            case MapPolyline line:
                _path.Reset();
                _path.MoveTo(P(view.WorldToScreen(line.Points[0])));
                for (int i = 1; i < line.Points.Length; i++)
                    _path.LineTo(P(view.WorldToScreen(line.Points[i])));
                Stroke(canvas, _path, emphasis ? new Argb8Color(255, 255, 255, 255) : line.Stroke, emphasis ? line.StrokePx + 2 : line.StrokePx);
                break;

            case MapCircle circle:
            {
                var center = P(view.WorldToScreen(circle.Center));
                float radius = (float)(circle.Radius * view.Scale);
                if (emphasis)
                {
                    _stroke.Color = SKColors.White;
                    _stroke.StrokeWidth = 2.5f;
                    canvas.DrawCircle(center, Math.Max(radius, 4), _stroke);
                    break;
                }
                if (circle.Fill.IsVisible)
                {
                    _smoothFill.Color = C(circle.Fill);
                    canvas.DrawCircle(center, radius, _smoothFill);
                }
                if (circle.Stroke.IsVisible)
                {
                    _stroke.Color = C(circle.Stroke);
                    _stroke.StrokeWidth = circle.StrokePx;
                    canvas.DrawCircle(center, radius, _stroke);
                }
                break;
            }

            case MapArrow arrow:
                DrawArrow(canvas, view, arrow);
                break;

            case MapMarker marker:
                DrawMarker(canvas, view, marker, emphasis);
                break;

            case MapLabel label:
            {
                var at = P(view.WorldToScreen(label.At));
                at.Offset(label.OffsetXPx, label.OffsetYPx);
                _text.Color = SKColors.Black.WithAlpha(180);
                canvas.DrawText(label.Text, at.X + 1, at.Y + 1, _font, _text);
                _text.Color = C(label.Color);
                canvas.DrawText(label.Text, at.X, at.Y, _font, _text);
                break;
            }
        }
    }

    private void Stroke(SKCanvas canvas, SKPath path, Argb8Color color, float width)
    {
        _stroke.Color = C(color);
        _stroke.StrokeWidth = width;
        canvas.DrawPath(path, _stroke);
    }

    private void DrawArrow(SKCanvas canvas, MapView view, MapArrow arrow)
    {
        var from = view.WorldToScreen(arrow.From);
        double angle = view.ScreenAngleOfYaw(arrow.Yaw);
        var direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        var tip = from + direction * arrow.LengthPx;
        var normal = new Vector2(-direction.Y, direction.X);
        float head = Math.Min(8, arrow.LengthPx / 2.5f);

        _stroke.Color = C(arrow.Color);
        _stroke.StrokeWidth = arrow.StrokePx;
        canvas.DrawLine(P(from), P(tip), _stroke);
        _path.Reset();
        _path.MoveTo(P(tip));
        _path.LineTo(P(tip - direction * head + normal * head * 0.55f));
        _path.LineTo(P(tip - direction * head - normal * head * 0.55f));
        _path.Close();
        _smoothFill.Color = C(arrow.Color);
        canvas.DrawPath(_path, _smoothFill);
    }

    private void DrawMarker(SKCanvas canvas, MapView view, MapMarker marker, bool emphasis)
    {
        var center = view.WorldToScreen(marker.Center);
        float r = marker.SizePx / 2;
        _path.Reset();
        switch (marker.Shape)
        {
            case MarkerShape.Circle:
                _path.AddCircle(center.X, center.Y, r);
                break;
            case MarkerShape.Square:
                _path.AddRect(new SKRect(center.X - r, center.Y - r, center.X + r, center.Y + r));
                break;
            case MarkerShape.Diamond:
                _path.MoveTo(center.X, center.Y - r);
                _path.LineTo(center.X + r, center.Y);
                _path.LineTo(center.X, center.Y + r);
                _path.LineTo(center.X - r, center.Y);
                _path.Close();
                break;
            case MarkerShape.Triangle:
            {
                double angle = marker.Yaw is double yaw ? view.ScreenAngleOfYaw(yaw) : -Math.PI / 2;
                var d = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                var n = new Vector2(-d.Y, d.X);
                _path.MoveTo(P(center + d * r * 1.3f));
                _path.LineTo(P(center - d * r * 0.8f + n * r * 0.85f));
                _path.LineTo(P(center - d * r * 0.4f));
                _path.LineTo(P(center - d * r * 0.8f - n * r * 0.85f));
                _path.Close();
                break;
            }
        }
        if (emphasis)
        {
            Stroke(canvas, _path, new Argb8Color(255, 255, 255, 255), 2.5f);
            return;
        }
        if (marker.Fill.IsVisible)
        {
            _smoothFill.Color = C(marker.Fill);
            canvas.DrawPath(_path, _smoothFill);
        }
        if (marker.Stroke.IsVisible)
            Stroke(canvas, _path, marker.Stroke, 1.5f);
    }

    private void DrawScaleBar(SKCanvas canvas, MapView view)
    {
        double target = 80 / view.Scale;
        double power = Math.Pow(10, Math.Floor(Math.Log10(target)));
        double units = new[] { 1, 2, 5, 10 }.Select(m => m * power).First(u => u >= target);
        float length = (float)(units * view.Scale);
        float x = 14, y = view.Height - 16;
        _stroke.Color = SKColors.White.WithAlpha(200);
        _stroke.StrokeWidth = 2;
        canvas.DrawLine(x, y, x + length, y, _stroke);
        canvas.DrawLine(x, y - 5, x, y + 3, _stroke);
        canvas.DrawLine(x + length, y - 5, x + length, y + 3, _stroke);
        _text.Color = SKColors.White.WithAlpha(220);
        canvas.DrawText($"{units:0.##} units", x, y - 8, _font, _text);
    }

    private void DrawNorth(SKCanvas canvas, MapView view)
    {
        var arrow = new MapArrow(view.ScreenToWorld(new Vector2(view.Width - 30, 46)), 0x8000, 22, Argb8Color.Rgb(230, 230, 240));
        DrawArrow(canvas, view, arrow);
        _text.Color = SKColors.White;
        canvas.DrawText("−Z", view.Width - 40, 18, _boldFont, _text);
    }

    public void DrawTooltip(SKCanvas canvas, MapView view, Vector2 cursor, IMapHit hit)
    {
        var lines = hit.Details.ToList();
        float width = Math.Max(_boldFont.MeasureText(hit.Title), lines.Count == 0 ? 0 : lines.Max(l => _font.MeasureText(l))) + 16;
        float height = 22 + lines.Count * 16 + 6;
        float x = Math.Min(cursor.X + 16, view.Width - width - 4);
        float y = Math.Min(cursor.Y + 16, view.Height - height - 4);
        var rect = new SKRect(x, y, x + width, y + height);
        _smoothFill.Color = new SKColor(0x10, 0x10, 0x16, 235);
        canvas.DrawRoundRect(rect, 6, 6, _smoothFill);
        _stroke.Color = new SKColor(255, 255, 255, 60);
        _stroke.StrokeWidth = 1;
        canvas.DrawRoundRect(rect, 6, 6, _stroke);
        _text.Color = SKColors.White;
        canvas.DrawText(hit.Title, x + 8, y + 17, _boldFont, _text);
        _text.Color = new SKColor(0xC8, 0xC8, 0xD4);
        for (int i = 0; i < lines.Count; i++)
            canvas.DrawText(lines[i], x + 8, y + 36 + i * 16, _font, _text);
    }

    public void DrawCursorReadout(SKCanvas canvas, MapView view, Vector2 world)
    {
        string text = $"X {world.X:0.0}   Z {world.Y:0.0}   ·   {view.Scale:0.####} px/unit";
        _text.Color = SKColors.White.WithAlpha(200);
        canvas.DrawText(text, view.Width - _font.MeasureText(text) - 12, view.Height - 10, _font, _text);
    }

    public void Dispose()
    {
        _fill.Dispose();
        _smoothFill.Dispose();
        _stroke.Dispose();
        _text.Dispose();
        _font.Dispose();
        _boldFont.Dispose();
        _typeface.Dispose();
        _boldTypeface.Dispose();
        _path.Dispose();
        _cache?.Dispose();
        _copy.Dispose();
        _game.Dispose();
        _topDown.Dispose();
    }
}
