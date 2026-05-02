using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Diagnostics.Debug;
using Windows.Win32.System.ProcessStatus;

namespace STROOP.Win32;

public static class NativeMethodWrappers
{
    public static unsafe bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] buffer)
    {
        var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        UIntPtr lpNumberOfBytesRead;
        try
        {
            return PInvoke.ReadProcessMemory(
                (HANDLE)hProcess,
                (void*)lpBaseAddress,
                (void*)Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0),
                (uint)buffer.Length,
                &lpNumberOfBytesRead
            );
        }
        finally
        {
            gcHandle.Free();
        }
    }

    public static unsafe bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] buffer)
    {
        UIntPtr numOfBytes = 0;
        var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            return PInvoke.WriteProcessMemory(
                (HANDLE)hProcess,
                (void*)lpBaseAddress,
                (void*)Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0),
                (UIntPtr)buffer.Length,
                &numOfBytes);
        }
        finally
        {
            gcHandle.Free();
        }
    }

    public static unsafe bool QueryWorkingSetEx(IntPtr hProcess, UIntPtr lpBaseAddress, out PSAPI_WORKING_SET_EX_INFORMATION wsInfo)
    {
        PSAPI_WORKING_SET_EX_INFORMATION tmp;
        tmp.VirtualAddress = (HANDLE)lpBaseAddress;
        uint setInfoSize = (uint)Marshal.SizeOf(typeof(PSAPI_WORKING_SET_EX_INFORMATION));
        var result = PInvoke.QueryWorkingSetEx((HANDLE)hProcess, &tmp, setInfoSize);
        wsInfo = tmp;
        return result;
    }

    public static unsafe bool GetSymbolAddress(SafeHandle hProcess, string name, out ulong address)
    {
        const int MAX_NAME_LENGTH = 2000;

        // See https://learn.microsoft.com/en-us/windows/win32/debug/retrieving-symbol-information-by-name
        var symbol = (SYMBOL_INFO*)Marshal.AllocHGlobal(Marshal.SizeOf<SYMBOL_INFO>() + MAX_NAME_LENGTH * sizeof(CHAR) + (sizeof(ulong) - 1) / sizeof(ulong));
        symbol->MaxNameLen = MAX_NAME_LENGTH;
        symbol->SizeOfStruct = (uint)sizeof(SYMBOL_INFO);
        var result = PInvoke.SymFromName(hProcess, name, symbol);
        address = symbol->Address;
        Marshal.FreeHGlobal((IntPtr)symbol);
        return result;
    }
}
