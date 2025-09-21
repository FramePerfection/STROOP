namespace STROOP.Structs.Configurations
{
    public static class InputConfig
    {
        public static uint CurrentInputAddress
        {
            get => RomVersionConfig.SwitchMap(CurrentInputAddressUS, CurrentInputAddressJP, CurrentInputAddressSH);
        }

        public static readonly uint CurrentInputAddressUS = 0x8033AFF8;
        public static readonly uint CurrentInputAddressJP = 0x80339C88;
        public static readonly uint CurrentInputAddressSH = 0x8031D5D0;

        public static uint JustPressedInputAddress
        {
            get => RomVersionConfig.SwitchMap(JustPressedInputAddressUS, JustPressedInputAddressJP);
        }

        public static readonly uint JustPressedInputAddressUS = 0x8033AFA2;
        public static readonly uint JustPressedInputAddressJP = 0x80339C32;

        public static uint BufferedInputAddress
        {
            get => RomVersionConfig.SwitchMap(BufferedInputAddressUS, BufferedInputAddressJP);
        }

        public static readonly uint BufferedInputAddressUS = 0x80367054;
        public static readonly uint BufferedInputAddressJP = 0x80365CE4;

        public static readonly uint ControlStickXOffset = 0x02;
        public static readonly uint ControlStickYOffset = 0x03;

        public enum ButtonMask : ushort
        {
            A = 0x8000,
            B = 0x4000,
            Z = 0x2000,
            Start = 0x1000,
            DUp = 0x0800,
            DDown = 0x0400,
            DLeft = 0x0200,
            DRight = 0x0100,

            Unused2 = 0x0080,
            Unused1 = 0x0040,
            L = 0x0020,
            R = 0x0010,
            CUp = 0x08,
            CDown = 0x04,
            CLeft = 0x02,
            CRight = 0x01
        }
    }
}
