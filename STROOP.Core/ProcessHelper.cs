using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;

namespace STROOP.Core;

public static class ProcessHelper
{
    public static void ResumeProcess(Process process)
    {
        // Resume all threads
        foreach (ProcessThread pT in process.Threads)
        {
            HANDLE pOpenThread = PInvoke.OpenThread(THREAD_ACCESS_RIGHTS.THREAD_SUSPEND_RESUME, false, (uint)pT.Id);

            if (pOpenThread == IntPtr.Zero)
                continue;

            uint suspendCount = 0;
            do
            {
                suspendCount = PInvoke.ResumeThread(pOpenThread);
            } while (suspendCount > 0);

            PInvoke.CloseHandle(pOpenThread);
        }
    }

    public static void SuspendProcess(Process process)
    {
        // Pause all threads
        foreach (ProcessThread pT in process.Threads)
        {
            HANDLE pOpenThread = PInvoke.OpenThread(THREAD_ACCESS_RIGHTS.THREAD_SUSPEND_RESUME, false, (uint)pT.Id);

            if (pOpenThread == IntPtr.Zero)
                continue;

            PInvoke.SuspendThread(pOpenThread);
            PInvoke.CloseHandle(pOpenThread);
        }
    }
}
