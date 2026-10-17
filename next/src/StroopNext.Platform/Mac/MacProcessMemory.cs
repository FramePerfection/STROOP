using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace StroopNext.Platform;

[SupportedOSPlatform("macos")]
internal sealed unsafe partial class MacProcessMemory : IProcessMemory
{
    private const string LibSystem = "/usr/lib/libSystem.dylib";
    private const int KernSuccess = 0;
    private const int VmRegionBasicInfo64 = 9;
    private const uint VmRegionBasicInfoCount64 = 9;
    private const int VmProtRead = 1;
    private const int Eperm = 1;

    private readonly int _pid;
    private uint _task;

    private MacProcessMemory(int pid, uint task)
    {
        _pid = pid;
        _task = task;
    }

    public static MacProcessMemory Open(int pid)
    {
        int result = task_for_pid(TaskSelf(), pid, out uint task);
        if (result != KernSuccess || task == 0)
            throw new InvalidOperationException(
                $"macOS refused access to its memory (task_for_pid error {result}). macOS only lets root or debugger-entitled tools "
                + "read another process: run STROOP Next with sudo.");
        return new MacProcessMemory(pid, task);
    }

    public bool IsWritable => true;
    public int PointerSize => 8;

    public bool IsAlive
    {
        get
        {
            if (kill(_pid, 0) == 0)
                return true;
            return Marshal.GetLastPInvokeError() == Eperm;
        }
    }

    public bool Read(nint address, Span<byte> buffer)
    {
        if (_task == 0 || buffer.IsEmpty)
            return _task != 0;
        fixed (byte* p = buffer)
            return mach_vm_read_overwrite(_task, (ulong)address, (ulong)buffer.Length, (ulong)p, out ulong read) == KernSuccess
                   && read == (ulong)buffer.Length;
    }

    public bool Write(nint address, ReadOnlySpan<byte> buffer)
    {
        if (_task == 0 || buffer.IsEmpty)
            return _task != 0;
        fixed (byte* p = buffer)
            return mach_vm_write(_task, (ulong)address, (nint)p, (uint)buffer.Length) == KernSuccess;
    }

    public IEnumerable<MemoryRegion> Regions()
    {
        ulong address = 0;
        while (_task != 0 && NextRegion(ref address, out ulong size, out bool readable))
        {
            yield return new MemoryRegion((nint)address, (nint)size, readable);
            if (address + size <= address)
                yield break;
            address += size;
        }
    }

    private bool NextRegion(ref ulong address, out ulong size, out bool readable)
    {
        int* info = stackalloc int[16];
        uint count = VmRegionBasicInfoCount64;
        size = 0;
        int result = mach_vm_region(_task, ref address, ref size, VmRegionBasicInfo64, info, ref count, out uint objectName);
        if (objectName != 0)
            mach_port_deallocate(TaskSelf(), objectName);
        readable = result == KernSuccess && (info[0] & VmProtRead) != 0;
        return result == KernSuccess;
    }

    public nint? ModuleBase(string moduleName) => null;

    public nint? Symbol(string name) => null;

    public void Dispose()
    {
        if (_task != 0)
            mach_port_deallocate(TaskSelf(), _task);
        _task = 0;
    }

    private static uint TaskSelf()
    {
        if (NativeLibrary.TryLoad(LibSystem, out nint library) && NativeLibrary.TryGetExport(library, "mach_task_self_", out nint variable))
            return *(uint*)variable;
        return task_self_trap();
    }

    [LibraryImport(LibSystem)]
    private static partial uint task_self_trap();

    [LibraryImport(LibSystem)]
    private static partial int task_for_pid(uint targetTport, int pid, out uint task);

    [LibraryImport(LibSystem)]
    private static partial int mach_vm_region(uint task, ref ulong address, ref ulong size, int flavor, int* info, ref uint count, out uint objectName);

    [LibraryImport(LibSystem)]
    private static partial int mach_vm_read_overwrite(uint task, ulong address, ulong size, ulong data, out ulong outSize);

    [LibraryImport(LibSystem)]
    private static partial int mach_vm_write(uint task, ulong address, nint data, uint count);

    [LibraryImport(LibSystem)]
    private static partial int mach_port_deallocate(uint task, uint name);

    [LibraryImport(LibSystem, SetLastError = true)]
    private static partial int kill(int pid, int signal);
}
