using System.Runtime.InteropServices;

namespace STROOP.Win32;

/// <summary>
/// The VirtualQueryEx method cannot be generated via CsWin32 unless the C# project itself is compiled for a specific platform.<br/>
/// Invoking it may thus cause a runtime exception on certain platforms, but as this is only used for the Dolphin IO, I'll accept this for now.
/// <para> See https://github.com/microsoft/CsWin32/issues/722 for details. </para>
/// </summary>
public static class VirtualQueryEx
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

    [DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx")]
    public static extern IntPtr Invoke(IntPtr hProcess, IntPtr lpAddress, out MemoryBasicInformation lpBuffer, IntPtr dwLength);
}
