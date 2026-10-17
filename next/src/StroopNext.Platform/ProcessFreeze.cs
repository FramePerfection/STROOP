using System.Runtime.InteropServices;

namespace StroopNext.Platform;

internal static partial class ProcessFreeze
{
    public static IDisposable Freeze(int processId)
    {
        try
        {
            if (OperatingSystem.IsWindows())
                return WindowsFreeze(processId);
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                bool mac = OperatingSystem.IsMacOS();
                int stop = mac ? 17 : 19, cont = mac ? 19 : 18;
                if (kill(processId, stop) == 0)
                    return new Resume(() => kill(processId, cont));
            }
        }
        catch (Exception e) when (e is DllNotFoundException or EntryPointNotFoundException)
        {
        }
        return new Resume(null);
    }

    private static IDisposable WindowsFreeze(int processId)
    {
        const uint ProcessSuspendResume = 0x0800;
        nint handle = OpenProcess(ProcessSuspendResume, false, processId);
        if (handle == 0)
            return new Resume(null);
        if (NtSuspendProcess(handle) != 0)
        {
            CloseHandle(handle);
            return new Resume(null);
        }
        return new Resume(() =>
        {
            NtResumeProcess(handle);
            CloseHandle(handle);
        });
    }

    private sealed class Resume(Action? resume) : IDisposable
    {
        private Action? _resume = resume;

        public void Dispose()
        {
            _resume?.Invoke();
            _resume = null;
        }
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial nint OpenProcess(uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, int processId);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(nint handle);

    [LibraryImport("ntdll.dll")]
    private static partial int NtSuspendProcess(nint handle);

    [LibraryImport("ntdll.dll")]
    private static partial int NtResumeProcess(nint handle);

    [LibraryImport("libc", SetLastError = true)]
    private static partial int kill(int pid, int signal);
}
