using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace StroopNext.Platform;

[SupportedOSPlatform("windows")]
internal static partial class Native
{
    public const uint ProcessVmRead = 0x0010;
    public const uint ProcessVmWrite = 0x0020;
    public const uint ProcessVmOperation = 0x0008;
    public const uint ProcessQueryInformation = 0x0400;
    public const uint StillActive = 259;

    public const uint MemCommit = 0x1000;
    public const uint PageNoAccess = 0x01;
    public const uint PageGuard = 0x100;

    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryBasicInformation
    {
        public nint BaseAddress;
        public nint AllocationBase;
        public uint AllocationProtect;
        public ushort PartitionId;
        public nint RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial nint OpenProcess(uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, int processId);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool CloseHandle(nint handle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static unsafe partial bool ReadProcessMemory(nint process, nint address, byte* buffer, nint size, out nint read);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static unsafe partial bool WriteProcessMemory(nint process, nint address, byte* buffer, nint size, out nint written);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetExitCodeProcess(nint process, out uint exitCode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool IsWow64Process(nint process, [MarshalAs(UnmanagedType.Bool)] out bool wow64);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial nint VirtualQueryEx(nint process, nint address, out MemoryBasicInformation info, nint length);

    [LibraryImport("dbghelp.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16, EntryPoint = "SymInitializeW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SymInitialize(nint process, string? searchPath, [MarshalAs(UnmanagedType.Bool)] bool invadeProcess);

    [LibraryImport("dbghelp.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SymCleanup(nint process);

    [LibraryImport("dbghelp.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf8, EntryPoint = "SymFromName")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static unsafe partial bool SymFromName(nint process, string name, byte* symbolInfo);

    public static unsafe bool Read(nint process, nint address, Span<byte> buffer)
    {
        fixed (byte* p = buffer)
            return ReadProcessMemory(process, address, p, buffer.Length, out nint read) && read == buffer.Length;
    }

    public static unsafe bool Write(nint process, nint address, ReadOnlySpan<byte> buffer)
    {
        fixed (byte* p = buffer)
            return WriteProcessMemory(process, address, p, buffer.Length, out nint written) && written == buffer.Length;
    }
}
