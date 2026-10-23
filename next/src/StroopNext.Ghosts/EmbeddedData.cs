namespace StroopNext.Ghosts;

internal static class EmbeddedData
{
    public static string LoadText(string name)
    {
        using var stream = typeof(EmbeddedData).Assembly.GetManifestResourceStream($"StroopNext.Data.{name}")
                           ?? throw new InvalidOperationException($"Missing embedded resource {name}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
