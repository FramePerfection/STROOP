using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace StroopNext.Platform;

[SupportedOSPlatform("windows")]
internal sealed class WindowsProcessMemory : IProcessMemory
{
    private readonly int _pid;
    private nint _handle;

    private WindowsProcessMemory(int pid, nint handle)
    {
        _pid = pid;
        _handle = handle;
        PointerSize = Native.IsWow64Process(handle, out bool wow) && wow ? 4 : 8;
    }

    public static WindowsProcessMemory Open(int pid)
    {
        nint handle = Native.OpenProcess(Native.ProcessVmRead | Native.ProcessVmWrite | Native.ProcessVmOperation | Native.ProcessQueryInformation, false, pid);
        if (handle == 0)
            throw new InvalidOperationException("access denied (try running STROOP Next with the same privileges as the emulator).");
        return new WindowsProcessMemory(pid, handle);
    }

    public bool IsWritable => true;
    public int PointerSize { get; }

    public bool IsAlive => _handle != 0 && Native.GetExitCodeProcess(_handle, out uint code) && code == Native.StillActive;

    public bool Read(nint address, Span<byte> buffer) => _handle != 0 && Native.Read(_handle, address, buffer);

    public bool Write(nint address, ReadOnlySpan<byte> buffer) => _handle != 0 && Native.Write(_handle, address, buffer);

    public IEnumerable<MemoryRegion> Regions()
    {
        nint address = 0x10000;
        int infoSize = Marshal.SizeOf<Native.MemoryBasicInformation>();
        while (_handle != 0 && Native.VirtualQueryEx(_handle, address, out var info, infoSize) != 0)
        {
            bool readable = info.State == Native.MemCommit && (info.Protect & (Native.PageNoAccess | Native.PageGuard)) == 0;
            yield return new MemoryRegion(info.BaseAddress, info.RegionSize, readable);
            nint regionEnd = info.BaseAddress + info.RegionSize;
            if (regionEnd <= address)
                yield break;
            address = regionEnd;
        }
    }

    public nint? ModuleBase(string moduleName)
    {
        try
        {
            using var process = Process.GetProcessById(_pid);
            nint? result = null;
            foreach (ProcessModule module in process.Modules)
            {
                using (module)
                    if (result is null && string.Equals(module.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase))
                        result = module.BaseAddress;
            }
            return result;
        }
        catch (Exception e) when (e is InvalidOperationException or System.ComponentModel.Win32Exception or ArgumentException)
        {
            return null;
        }
    }

    public unsafe nint? Symbol(string name)
    {
        if (_handle == 0)
            return null;
        try
        {
            if (!Native.SymInitialize(_handle, null, true))
                return null;
            try
            {
                byte* info = stackalloc byte[88 + 256];
                new Span<byte>(info, 88 + 256).Clear();
                *(uint*)info = 88;
                *(uint*)(info + 80) = 255;
                if (!Native.SymFromName(_handle, name, info))
                    return null;
                return (nint)(*(ulong*)(info + 56));
            }
            finally
            {
                Native.SymCleanup(_handle);
            }
        }
        catch (DllNotFoundException)
        {
            return null;
        }
        catch (EntryPointNotFoundException)
        {
            return null;
        }
    }

    public void Dispose()
    {
        if (_handle != 0)
            Native.CloseHandle(_handle);
        _handle = 0;
    }
}
