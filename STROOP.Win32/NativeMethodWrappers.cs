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
        var symbol = new SYMBOL_INFO
        {
            MaxNameLen = 2000,
            SizeOfStruct = 88,
        };
        var result = PInvoke.SymFromName(hProcess, name, &symbol);
        address = symbol.Address;
        return result;
    }
}
