using STROOP.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.System.ProcessStatus;
using static STROOP.Core.Kernal32NativeMethods;

namespace STROOP.Core.GameMemoryAccess;

public class DolphinProcessIO : WindowsProcessRamIO
{
    public DolphinProcessIO(Process process, Emulator emulator)
        : base(process, emulator)
    {
    }

    protected override void CalculateOffset()
    {
        MemoryBasicInformation info;
        IntPtr infoSize = (IntPtr)Marshal.SizeOf(typeof(MemoryBasicInformation));

        _baseOffset = (UIntPtr)0;
        bool mem1Found = false;
        for (IntPtr p = new IntPtr();
             VirtualQueryEx(_processHandle, p, out info, infoSize) == infoSize;
             p = (IntPtr)(p.ToInt64() + info.RegionSize.ToInt64()))
        {
            if (mem1Found)
            {
                if (info.BaseAddress == _baseOffset + 0x10000000)
                {
                    break;
                }
                else if (info.BaseAddress.ToUInt64() > _baseOffset.ToUInt64() + 0x10000000)
                {
                    break;
                }

                continue;
            }

            if (info.RegionSize == (IntPtr)0x2000000 && info.Type == MemoryType.MEM_MAPPED)
            {
                // Here, it's likely the right page, but it can happen that multiple pages with these criteria
                // exists and have nothing to do with the emulated memory. Only the right page has valid
                // working set information so an additional check is required that it is backed by physical
                // memory.
                if (NativeMethodWrappers.QueryWorkingSetEx((HANDLE)_processHandle, info.BaseAddress, out PSAPI_WORKING_SET_EX_INFORMATION wsInfo))
                {
                    if ((wsInfo.VirtualAttributes.Flags & 0x01) != 0)
                    {
                        _baseOffset = info.BaseAddress;
                        mem1Found = true;
                    }
                }
            }
        }

        if (_baseOffset.ToUInt64() == 0)
            throw new DolphinNotRunningGameException();

        _baseOffset = (UIntPtr)(_baseOffset.ToUInt64() + _emulator.RamStart);
    }
}
