using System.Diagnostics;

namespace StroopNext.Platform;

public sealed record EmulatorProfile(string Name, string ProcessName, long RamStart, string? ModuleName = null);

public static class EmulatorProfiles
{
    public static readonly IReadOnlyList<EmulatorProfile> Known =
    [
        new("Mupen 5.0 RR", "mupen64", 0x01000000),
        new("Mupen 5.0 RR", "mupen64-rerecording", 0x008EBA80),
        new("Mupen 5.0 RR", "mupen64-screen-recording", 0x004FA6F0),
        new("Mupen 5.0 RR", "mupen64-pucrash", 0x00912300),
        new("Mupen 5.0 RR", "mupen64_lua", 0x00888F60),
        new("Mupen 5.0 RR", "mupen64-wiivc", 0x00901920),
        new("Mupen 5.0 RR", "mupen64-RTZ", 0x00901920),
        new("Mupen 5.0 RR", "mupen64-rerecording-v2-reset", 0x008ECA90),
        new("Mupen 5.0 RR", "mupen64-rrv8-avisplit", 0x008ECBB0),
        new("Nemu64 0.8", "Nemu64", 0x10020000),
        new("BizHawk", "EmuHawk", 0x6A1C0, "mupen64plus.dll"),
        new("Project64 Debugger", "Project64d", 0x20000000),
        new("Project64", "Project64", 0x4BAF0000),
        new("Mupen64Plus", "mupen64plus", 0x008FF142),
    ];

    private static readonly string[] NameFragments = ["mupen", "project64", "simple64", "retroarch"];
    private static readonly string[] ExactNames = ["rmg", "ares"];

    public static EmulatorProfile? For(string processName)
    {
        string name = StripExe(processName);
        return Known.FirstOrDefault(p => string.Equals(p.ProcessName, name, StringComparison.OrdinalIgnoreCase));
    }

    public static bool LooksLikeEmulator(string processName)
    {
        string name = StripExe(processName);
        return For(name) is not null
               || NameFragments.Any(f => name.Contains(f, StringComparison.OrdinalIgnoreCase))
               || ExactNames.Any(n => string.Equals(name, n, StringComparison.OrdinalIgnoreCase));
    }

    private static string StripExe(string name)
        => name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? name[..^4] : name;
}

public sealed record EmulatorCandidate(int ProcessId, string ProcessName, string WindowTitle, EmulatorProfile? Profile)
{
    public override string ToString() => string.IsNullOrWhiteSpace(WindowTitle) ? $"{ProcessName} ({ProcessId})" : $"{ProcessName} ({ProcessId}) — {WindowTitle}";
}

public static class EmulatorDiscovery
{
    public static IReadOnlyList<EmulatorCandidate> Find()
    {
        var result = new List<EmulatorCandidate>();
        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                try
                {
                    string name = process.ProcessName;
                    if (!EmulatorProfiles.LooksLikeEmulator(name))
                    {
                        if (LinuxExecutableName(process.Id) is not { } full || !EmulatorProfiles.LooksLikeEmulator(full))
                            continue;
                        name = full;
                    }
                    result.Add(new EmulatorCandidate(process.Id, name, WindowTitle(process), EmulatorProfiles.For(name)));
                }
                catch (Exception e) when (e is InvalidOperationException or NotSupportedException or System.ComponentModel.Win32Exception)
                {
                }
            }
        }
        return result;
    }

    private static string WindowTitle(Process process)
    {
        if (!OperatingSystem.IsWindows())
            return "";
        try
        {
            return process.MainWindowTitle;
        }
        catch (Exception e) when (e is InvalidOperationException or NotSupportedException)
        {
            return "";
        }
    }

    private static string? LinuxExecutableName(int pid)
    {
        if (!OperatingSystem.IsLinux())
            return null;
        try
        {
            string cmdline = File.ReadAllText($"/proc/{pid}/cmdline");
            int end = cmdline.IndexOf('\0');
            string executable = end >= 0 ? cmdline[..end] : cmdline;
            int slash = executable.LastIndexOfAny(['/', '\\']);
            string name = executable[(slash + 1)..];
            return name.Length > 0 ? name : null;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
