using Avalonia.Media.Imaging;

namespace StroopNext.Desktop.Controls;

// Legacy STROOP's object images, copied next to the app (Assets/Object Images), decoded on first use at a small width.
internal static class ObjectImages
{
    private static readonly string Folder = Path.Combine(AppContext.BaseDirectory, "Assets", "Object Images");
    private static readonly Dictionary<(string, int), Bitmap?> Cache = new();

    public static Bitmap? Get(string? file, int width = 64)
    {
        if (string.IsNullOrEmpty(file))
            return null;
        if (Cache.TryGetValue((file, width), out var cached))
            return cached;
        Bitmap? bitmap = null;
        try
        {
            string path = Path.Combine(Folder, file);
            if (File.Exists(path))
            {
                using var stream = File.OpenRead(path);
                bitmap = Bitmap.DecodeToWidth(stream, width, BitmapInterpolationMode.HighQuality);
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        {
        }
        return Cache[(file, width)] = bitmap;
    }
}
