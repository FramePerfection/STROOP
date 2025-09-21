using STROOP.Core;
using System;

namespace STROOP.Structs
{
    public class Emulator
    {
        public string Name;
        public string ProcessName;
        public uint RamStart;
        public bool AllowAutoDetect;
        public string Dll;
        public Type IOType;
        public EndiannessType Endianness = EndiannessType.Little;
    }
}
