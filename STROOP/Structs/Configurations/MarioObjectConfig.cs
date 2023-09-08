﻿using STROOP.Structs.Configurations;

namespace STROOP.Structs
{
    public static class MarioObjectConfig
    {
        public static uint PointerAddress { get => RomVersionConfig.SwitchMap(PointerAddressUS, PointerAddressJP, PointerAddressSH); }
        public static readonly uint PointerAddressUS = 0x80361158;
        public static readonly uint PointerAddressJP = 0x8035FDE8;
        public static readonly uint PointerAddressSH = 0x80343318;

        public static readonly uint AnimationOffset = 0x38;
        public static readonly uint AnimationTimerOffset = 0x40;

        public static readonly uint GraphicValue = 0x800F0860;
        public static readonly uint BehaviorValue = 0x13002EC0;
    }
}
