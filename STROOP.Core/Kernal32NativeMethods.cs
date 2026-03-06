using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;

namespace STROOP.Core;

public static class Kernal32NativeMethods
{
    [Flags]
    public enum MemoryType : uint
    {
        MEM_IMAGE = 0x1000000,
        MEM_MAPPED = 0x40000,
        MEM_PRIVATE = 0x20000,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryBasicInformation
    {
        public UIntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public MemoryType Type;
    }

    [DllImport("kernel32.dll")]
    public static extern IntPtr VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MemoryBasicInformation lpBuffer, IntPtr dwLength);

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
