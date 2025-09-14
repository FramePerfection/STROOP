using System.Diagnostics;
using STROOP.Structs;

namespace STROOP.Utilities
{
    public interface IEmuRamIO
    {
        string Name { get; }
        Process Process { get; }
        string GetLastMessages();
        bool Suspend();
        bool Resume();

        bool IsSuspended { get; }

        bool ReadRelative(uint address, byte[] buffer, EndiannessType endianness);
        bool ReadAbsolute(UIntPtr address, byte[] buffer, EndiannessType endianness);
        bool WriteRelative(uint address, byte[] buffer, EndiannessType endianness);
        bool WriteAbsolute(UIntPtr address, byte[] buffer, EndiannessType endianness);
        byte[] ReadAllMemory();

        UIntPtr GetAbsoluteAddress(uint n64Address, int size);
        uint GetRelativeAddress(UIntPtr absoluteAddress, int size);

        event EventHandler OnClose;
    }
}
