using STROOP.Core;

namespace STROOP.Variables.SM64MemoryLayout
{
    public static class RomVersionConfig
    {
        public static RomVersion Version = RomVersion.US;

        public static uint RomVersionTellAddress = 0x802F0000;
        public static uint RomVersionTellValueUS = 0xC58400A4;
        public static uint RomVersionTellValueJP = 0x27BD0020;
        public static uint RomVersionTellValueSH = 0x8F250004;
        public static uint RomVersionTellValueEU = 0x0C0BD4AC;

        public static RomVersion? GetRomVersionUsingTell()
        {
            uint tell = ProcessStream.Instance.GetUInt32(RomVersionTellAddress);
            if (tell == RomVersionTellValueUS) return RomVersion.US;
            if (tell == RomVersionTellValueJP) return RomVersion.JP;
            if (tell == RomVersionTellValueSH) return RomVersion.SH;
            if (tell == RomVersionTellValueEU) return RomVersion.EU;
            return RomVersion.US;
        }

        public static uint SwitchMap(uint? valUS = null, uint? valJP = null, uint? valSH = null, uint? valEU = null)
        {
            uint address = SwitchOnly(valUS, valJP, valSH, valEU);
            address = MappingConfig.HandleMapping(address);
            return address;
        }

        public static uint SwitchReverseMap(uint? valUS = null, uint? valJP = null, uint? valSH = null, uint? valEU = null)
        {
            uint address = SwitchOnly(valUS, valJP, valSH, valEU);
            address = MappingConfig.HandleReverseMapping(address);
            return address;
        }

        public static uint SwitchOnly(uint? valUS = null, uint? valJP = null, uint? valSH = null, uint? valEU = null)
        {
            switch (Version)
            {
                case RomVersion.US:
                    if (valUS.HasValue) return valUS.Value;
                    break;
                case RomVersion.JP:
                    if (valJP.HasValue) return valJP.Value;
                    break;
                case RomVersion.SH:
                    if (valSH.HasValue) return valSH.Value;
                    break;
                case RomVersion.EU:
                    if (valEU.HasValue) return valEU.Value;
                    break;
            }

            return 0;
        }

        public static ushort Switch(ushort? valUS = null, ushort? valJP = null, ushort? valSH = null, ushort? valEU = null)
        {
            switch (Version)
            {
                case RomVersion.US:
                    if (valUS.HasValue) return valUS.Value;
                    break;
                case RomVersion.JP:
                    if (valJP.HasValue) return valJP.Value;
                    break;
                case RomVersion.SH:
                    if (valSH.HasValue) return valSH.Value;
                    break;
                case RomVersion.EU:
                    if (valEU.HasValue) return valEU.Value;
                    break;
            }

            return 0;
        }
    }
}
