using System.Text.Json.Nodes;

namespace StroopNext.TasEngine;

public sealed record EngineSetup(string Folder, string Python)
{
    public const int DefaultPort = 8066;

    public string ServerScript => Path.Combine(Folder, "tools", "builder", "builder_server.py");
    public string CommandLine => Path.Combine(Folder, "tools", "builder", "moteur.py");

    public string BridgeLua => Path.Combine(Folder, "tools", "romhack", "bridge.lua");

    public string? Problem()
    {
        if (string.IsNullOrWhiteSpace(Folder) || !Directory.Exists(Folder))
            return "Engine folder not found: choose the engine's folder (the one with tools\\builder).";
        if (!File.Exists(ServerScript))
            return $"Not an engine folder (no tools\\builder\\builder_server.py in {Folder}).";
        if (string.IsNullOrWhiteSpace(Python))
            return "No Python: choose python.exe (3.9 or later, the one the engine was installed with).";
        if (Path.IsPathRooted(Python) && !File.Exists(Python))
            return $"Python not found: {Python}";
        return null;
    }

    public static EngineSetup Detect(string? folder, string? python, IEnumerable<string>? configFiles = null, IEnumerable<string>? usualFolders = null)
    {
        string? foundFolder = null, foundPython = null;
        foreach (var file in configFiles ?? DefaultConfigFiles())
        {
            if (Read(file) is not { } config)
                continue;
            foundFolder ??= config.Engine;
            foundPython ??= config.Python;
        }
        foreach (var candidate in usualFolders ?? DefaultFolders())
            if (foundFolder is null && File.Exists(Path.Combine(candidate, "tools", "builder", "builder_server.py")))
                foundFolder = candidate;
        return new EngineSetup(
            Blank(folder) ? foundFolder ?? "" : folder!.Trim(),
            Blank(python) ? foundPython ?? "python" : python!.Trim());
    }

    private static bool Blank(string? s) => string.IsNullOrWhiteSpace(s);

    private static (string? Engine, string? Python)? Read(string file)
    {
        try
        {
            if (!File.Exists(file) || JsonNode.Parse(File.ReadAllText(file)) is not JsonObject json)
                return null;
            return (json["engine"]?.GetValue<string>(), json["python"]?.GetValue<string>());
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidOperationException or FormatException)
        {
            return null;
        }
    }

    private static IEnumerable<string> DefaultConfigFiles()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        yield return Path.Combine(home, ".claude", "skills", "moteur-sm64", "moteur.json");
        yield return Path.Combine(AppContext.BaseDirectory, "moteur.json");
    }

    private static IEnumerable<string> DefaultFolders()
    {
        if (Environment.GetEnvironmentVariable("MOTEUR_DIR") is { Length: > 0 } env)
            yield return env;
        foreach (var drive in new[] { "D", "G", "C", "E", "F" })
            yield return $@"{drive}:\moteur\send-result";
    }
}
