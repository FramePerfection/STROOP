namespace STROOP.Core
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
