using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace StroopNext.Desktop.Engine;

internal static partial class MupenRom
{
    private const uint ProcessQueryLimitedInformation = 0x1000;
    private const int ProcessCommandLineInformation = 60;
    private const int BufferSize = 32768;

    private static readonly byte[][] Magics =
    [
        [0x80, 0x37, 0x12, 0x40],
        [0x37, 0x80, 0x40, 0x12],
        [0x40, 0x12, 0x37, 0x80],
    ];

    [StructLayout(LayoutKind.Sequential)]
    private struct UnicodeString
    {
        public ushort Length;
        public ushort MaximumLength;
        public nint Buffer;
    }

    [GeneratedRegex("--rom\\s+(?:\"([^\"]+)\"|(\\S+))", RegexOptions.IgnoreCase)]
    private static partial Regex RomFlag();

    [GeneratedRegex("\"([^\"]+\\.(?:z64|n64|v64))\"|(\\S+\\.(?:z64|n64|v64))", RegexOptions.IgnoreCase)]
    private static partial Regex RomArgument();

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial nint OpenProcess(uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, int processId);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(nint handle);

    [LibraryImport("ntdll.dll")]
    private static partial int NtQueryInformationProcess(nint process, int infoClass, nint info, int length, out int returnLength);

    private static (string Name, string Path)? _byName;

    public static string? Detect(int processId)
    {
        string? fromCommand = RomInCommandLine(CommandLine(processId));
        if (fromCommand is not null && IsRom(fromCommand))
            return fromCommand;
        // A ROM opened from Mupen's menu is not on its command line, and Mupen writes its recent list on exit only:
        // the window title carries the loaded ROM's internal name, matched against the headers of known ROM files.
        string[] recent = RecentRoms().Where(IsRom).ToArray();
        MissingName = null;
        if (LoadedName(processId) is { } name)
        {
            if (FindByName(name, recent, processId) is { } byName)
                return byName;
            // Never another game's ROM: the engine would read the wrong levels (a ROM hack's state on vanilla geometry).
            MissingName = name;
            return null;
        }
        return recent.FirstOrDefault();
    }

    /// <summary>The internal name of the ROM Mupen runs when no file with that name was found (last Detect).</summary>
    public static string? MissingName { get; private set; }

    internal static string? LoadedName(int processId)
    {
        try
        {
            string title = System.Diagnostics.Process.GetProcessById(processId).MainWindowTitle;
            int dash = title.LastIndexOf(" - ", StringComparison.Ordinal);
            return dash >= 0 && title[(dash + 3)..].Trim() is { Length: > 0 } name ? name : null;
        }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException)
        {
            return null;
        }
    }

    private static string? FindByName(string name, string[] recent, int processId)
    {
        if (_byName is { } cached && cached.Name == name && File.Exists(cached.Path))
            return cached.Path;
        var folders = recent.Select(Path.GetDirectoryName).OfType<string>().ToList();
        try
        {
            if (System.Diagnostics.Process.GetProcessById(processId).MainModule?.FileName is { } exe)
                folders.Add(Path.Combine(Path.GetDirectoryName(exe)!, "roms"));
        }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
        }
        folders.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"));
        var candidates = recent.Concat(folders.Distinct(StringComparer.OrdinalIgnoreCase).Where(Directory.Exists)
            .SelectMany(f => Directory.EnumerateFiles(f).Where(p => RomArgument().IsMatch(p))));
        foreach (string path in candidates)
        {
            if (string.Equals(InternalName(path), name, StringComparison.OrdinalIgnoreCase))
            {
                _byName = (name, path);
                return path;
            }
        }
        return null;
    }

    private static string? InternalName(string path)
    {
        try
        {
            var header = new byte[0x40];
            using (var stream = File.OpenRead(path))
                stream.ReadExactly(header);
            if (header[0] == 0x37)
                for (int i = 0; i < header.Length; i += 2)
                    (header[i], header[i + 1]) = (header[i + 1], header[i]);
            else if (header[0] == 0x40)
                for (int i = 0; i < header.Length; i += 4)
                    (header[i], header[i + 1], header[i + 2], header[i + 3]) = (header[i + 3], header[i + 2], header[i + 1], header[i]);
            return header[0] == 0x80 ? System.Text.Encoding.ASCII.GetString(header, 0x20, 20).Trim() : null;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static string? CommandLine(int processId)
    {
        nint process = OpenProcess(ProcessQueryLimitedInformation, false, processId);
        if (process == 0)
            return null;
        nint buffer = Marshal.AllocHGlobal(BufferSize);
        try
        {
            if (NtQueryInformationProcess(process, ProcessCommandLineInformation, buffer, BufferSize, out _) != 0)
                return null;
            var text = Marshal.PtrToStructure<UnicodeString>(buffer);
            return Marshal.PtrToStringUni(buffer + Marshal.SizeOf<UnicodeString>(), text.Length / 2);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
            CloseHandle(process);
        }
    }

    private static string? RomInCommandLine(string? commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
            return null;
        if (RomFlag().Match(commandLine) is { Success: true } flag)
            return Clean(flag.Groups[1].Success ? flag.Groups[1].Value : flag.Groups[2].Value);
        if (RomArgument().Match(commandLine) is { Success: true } argument)
            return Clean(argument.Groups[1].Success ? argument.Groups[1].Value : argument.Groups[2].Value);
        return null;
    }

    private static string? Clean(string path) => Path.IsPathRooted(path) ? path : null;

    private static IEnumerable<string> RecentRoms()
    {
        string config = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "mupen64-rr-lua", "config.json");
        try
        {
            if (JsonNode.Parse(File.ReadAllText(config))?["frontend"]?["recent_rom_paths"] is JsonArray paths)
                return paths.Select(p => p?.ToString()).OfType<string>().ToArray();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
        }
        return [];
    }

    private static bool IsRom(string path)
    {
        try
        {
            if (!File.Exists(path))
                return false;
            var magic = new byte[4];
            using var stream = File.OpenRead(path);
            stream.ReadExactly(magic);
            return Magics.Any(m => m.SequenceEqual(magic));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
