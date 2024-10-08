﻿using STROOP.Structs.Configurations;

namespace STROOP.Structs
{
    public static class MarioConfig
    {
        public static uint StructAddress { get => RomVersionConfig.SwitchMap(StructAddressUS, StructAddressJP, StructAddressSH, StructAddressEU); }
        public static readonly uint StructAddressUS = 0x8033B170;
        public static readonly uint StructAddressJP = 0x80339E00;
        public static readonly uint StructAddressSH = 0x8031D9C0;
        public static readonly uint StructAddressEU = 0x80309430;

        public static readonly uint XOffset = 0x3C;
        public static readonly uint YOffset = 0x40;
        public static readonly uint ZOffset = 0x44;

        public static readonly uint XSpeedOffset = 0x48;
        public static readonly uint YSpeedOffset = 0x4C;
        public static readonly uint ZSpeedOffset = 0x50;
        public static readonly uint HSpeedOffset = 0x54;

        public static readonly uint FacingYawOffset = 0x2E;
        public static readonly uint FacingPitchOffset = 0x2C;
        public static readonly uint FacingRollOffset = 0x30;

        public static readonly uint IntendedYawOffset = 0x24;
        public static readonly uint IntendedPitchOffset = 0x22;
        public static readonly uint IntendedRollOffset = 0x26;

        public static readonly uint MovingYawOffset = 0x38;
        public static readonly uint ScaledMagnitudeOffset = 0x20;

        public static readonly uint SlidingSpeedXOffset = 0x58;
        public static readonly uint SlidingSpeedZOffset = 0x5C;
        public static readonly uint SlidingYawOffset = 0x38;

        public static readonly uint HolpXOffset = 0x258;
        public static readonly uint HolpYOffset = 0x25C;
        public static readonly uint HolpZOffset = 0x260;
        public static readonly uint HolpTypeOffset = 0x24A;

        public static uint StoodOnObjectPointerAddress { get => RomVersionConfig.SwitchMap(StoodOnObjectPointerAddressUS, StoodOnObjectPointerAddressJP, StoodOnObjectPointerAddressSH, StoodOnObjectPointerAddressEU); }
        public static readonly uint StoodOnObjectPointerAddressUS = 0x80330E34;
        public static readonly uint StoodOnObjectPointerAddressJP = 0x8032FED4;
        public static readonly uint StoodOnObjectPointerAddressSH = 0x80310564;
        public static readonly uint StoodOnObjectPointerAddressEU = 0x802FCFF4;
        
        public static readonly uint InteractionObjectPointerOffset = 0x78;
        public static readonly uint HeldObjectPointerOffset = 0x7C;
        public static readonly uint UsedObjectPointerOffset = 0x80;
        public static readonly uint RiddenObjectPointerOffset = 0x84;

        public static readonly uint WallTriangleOffset = 0x60;
        public static readonly uint FloorTriangleOffset = 0x68;
        public static readonly uint CeilingTriangleOffset = 0x64;

        public static readonly uint FloorYOffset = 0x70;
        public static readonly uint CeilingYOffset = 0x6C;

        public static readonly uint FloorYawOffset = 0x74;

        public static readonly uint ActionOffset = 0x0C;
        public static readonly uint PrevActionOffset = 0x10;
        public static readonly uint FreeMovementAction = 0x0000130F;
        public static readonly uint RidingShellAction = 0x20810446;
        public static readonly uint IdleAction = 0x0C400201;

        public static readonly uint TwirlYawOffset = 0x3A;
        public static readonly uint PeakHeightOffset = 0xBC;
        public static readonly uint WaterLevelOffset = 0x76;
        public static readonly uint AreaPointerOffset = 0x90;
    }
}
