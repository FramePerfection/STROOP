using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StroopNext.Platform;

public static partial class ChildProcesses
{
    private static nint _job;
    private static readonly object Lock = new();

    public static void Adopt(Process process)
    {
        if (!OperatingSystem.IsWindows())
            return;
        try
        {
            lock (Lock)
            {
                if (_job == 0)
                {
                    nint job = CreateJobObjectW(0, null);
                    if (job == 0)
                        return;
                    var info = new byte[144];
                    BitConverter.TryWriteBytes(info.AsSpan(16), 0x2000u);
                    if (!SetInformationJobObject(job, 9, info, info.Length))
                    {
                        CloseHandle(job);
                        return;
                    }
                    _job = job;
                }
                AssignProcessToJobObject(_job, process.Handle);
            }
        }
        catch (Exception e) when (e is DllNotFoundException or EntryPointNotFoundException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
        }
    }

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint CreateJobObjectW(nint attributes, string? name);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetInformationJobObject(nint job, int infoClass, byte[] info, int length);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AssignProcessToJobObject(nint job, nint process);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(nint handle);
}
