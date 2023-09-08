﻿namespace STROOP.Structs.Configurations
{
    public static class CamHackConfig
    {
        public static uint StructAddress { get => RomVersionConfig.SwitchMap(StructAddressUS, StructAddressJP); }
        public static readonly uint StructAddressUS = 0x803E0000;
        public static readonly uint StructAddressJP = 0x803E0000;

        public static readonly uint CameraModeOffset = 0x00;
        public static readonly uint CameraXOffset = 0x04;
        public static readonly uint CameraYOffset = 0x08;
        public static readonly uint CameraZOffset = 0x0C;
        public static readonly uint FocusXOffset = 0x10;
        public static readonly uint FocusYOffset = 0x14;
        public static readonly uint FocusZOffset = 0x18;
        public static readonly uint AbsoluteAngleOffset = 0x1C;
        public static readonly uint ThetaOffset = 0x1E;
        public static readonly uint RadiusOffset = 0x20;
        public static readonly uint RelativeHeightOffset = 0x24;
        public static readonly uint ObjectOffset = 0x28;

    }
}