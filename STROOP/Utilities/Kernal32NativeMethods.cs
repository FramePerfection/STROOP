using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace STROOP.Utilities
{
    public static class Kernal32NativeMethods
    {
        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [Flags]
        public enum ProcessAccess : int
        {
            VM_OPERATION = 0x0008,
            VM_READ = 0x0010,
            VM_WRITE = 0x0020,
            PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
            SUSPEND_RESUME = 0x0800,
        }

        [Flags]
        public enum MemoryType : uint
        {
            MEM_IMAGE = 0x1000000,
            MEM_MAPPED = 0x40000,
            MEM_PRIVATE = 0x20000
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

        [StructLayout(LayoutKind.Sequential)]
        public struct PsapiWorkingSetExInformation
        {
            public IntPtr VirtualAddress;
            public ulong VirtualAttributes;
        }

        /// <summary>
        /// C# representation of SYMBOL_INFO in dbghelp.h. <para/>
        /// https://learn.microsoft.com/de-de/windows/win32/api/dbghelp/ns-dbghelp-symbol_info <para/>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct Win32SymbolInfo
        {
            const int MaxSymbolLen = 2000;

            public static Win32SymbolInfo Create()
                => new Win32SymbolInfo
                {
                    MaxNameLen = MaxSymbolLen,
                    SizeOfStruct = 88
                };

            public int SizeOfStruct;
            public int TypeIndex;
            readonly ulong Reserved1, Reserved2;
            public int Index;
            public int Size;
            public ulong ModuleBase;
            public uint Flags;
            public long Value;
            public ulong Address;
            public uint Register;
            public uint Scope;
            public uint Tag;
            public uint NameLen;
            public int MaxNameLen;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxSymbolLen)]
            public string Name;
        }

        #region DLL Import

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess,
            UIntPtr lpBaseAddress, byte[] lpBuffer, IntPtr dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress,
            byte[] lpBuffer, IntPtr dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MemoryBasicInformation lpBuffer, IntPtr dwLength);

        [DllImport("psapi", SetLastError = true)]
        static extern bool QueryWorkingSetEx(IntPtr hProcess, out PsapiWorkingSetExInformation pv, uint cb);

        [DllImport("dbghelp", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SymInitializeW", ExactSpelling = true)]
        public static extern bool SymInitialize(IntPtr hProcess, string searchPath, bool invadeProcess);

        [DllImport("dbghelp", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SymCleanup", ExactSpelling = true)]
        public static extern bool SymCleanup(IntPtr hProcess);

        [DllImport("dbghelp", SetLastError = true)]
        public static extern bool SymFromName(IntPtr hProcess, string name, ref Win32SymbolInfo win32Symbol);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

        #endregion

        public static IntPtr ProcessGetHandleFromId(ProcessAccess dwDesiredAccess, bool bInheritHandle, int dwProcessId)
        {
            return OpenProcess(dwDesiredAccess, bInheritHandle, dwProcessId);
        }

        public static bool CloseProcess(IntPtr processHandle)
        {
            return CloseHandle(processHandle);
        }

        public static bool ProcessReadMemory(IntPtr hProcess,
            UIntPtr lpBaseAddress, byte[] lpBuffer, IntPtr dwSize, ref int lpNumberOfBytesRead)
        {
            return ReadProcessMemory(hProcess, lpBaseAddress, lpBuffer, dwSize, ref lpNumberOfBytesRead);
        }

        public static bool ProcessWriteMemory(IntPtr hProcess, UIntPtr lpBaseAddress,
            byte[] lpBuffer, IntPtr dwSize, ref int lpNumberOfBytesWritten)
        {
            return WriteProcessMemory(hProcess, lpBaseAddress, lpBuffer, dwSize, ref lpNumberOfBytesWritten);
        }

        public static void ResumeProcess(Process process)
        {
            // Resume all threads
            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                    continue;

                int suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
        }

        public static void SuspendProcess(Process process)
        {
            // Pause all threads
            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = Kernal32NativeMethods.OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                    continue;

                SuspendThread(pOpenThread);
                CloseHandle(pOpenThread);
            }
        }

        public static IntPtr VQueryEx(IntPtr hProcess, IntPtr lpAddress, out MemoryBasicInformation lpBuffer, IntPtr dwLength)
        {
            return VirtualQueryEx(hProcess, lpAddress, out lpBuffer, dwLength);
        }

        public static bool QWorkingSetEx(IntPtr hProcess, out PsapiWorkingSetExInformation pv, uint cb)
        {
            return QueryWorkingSetEx(hProcess, out pv, cb);
        }
    }
}
