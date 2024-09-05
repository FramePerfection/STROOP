﻿using System;
using STROOP.Structs;
using STROOP.Structs.Configurations;

namespace STROOP.Controls
{
    public class VarHackContainerDefaults
    {
        public static readonly string StaticVarName = "";
        public static readonly uint StaticAddress = 0x8033B1AC;
        public static readonly Type StaticMemoryType = typeof(float);
        public static readonly bool StaticUseHex = false;
        public static readonly uint StaticPointerOffset = 0x10;
        public static readonly bool StaticNoNum = false;

        public readonly string SpecialType;
        public readonly string VarName;
        public readonly uint Address;
        public readonly Type MemoryType;
        public readonly bool UseHex;
        public readonly uint? PointerOffset;
        public readonly bool NoNum;
        public readonly int XPos;
        public readonly int YPos;

        public VarHackContainerDefaults(int creationIndex)
        {
            XPos = VarHackConfig.DefaultXPos;
            YPos = VarHackConfig.DefaultYPos - creationIndex * VarHackConfig.DefaultYDelta;
            UseHex = false;
            PointerOffset = null;
            SpecialType = null;
            NoNum = false;

            switch (creationIndex)
            {
                case 0:
                    VarName = "HSPD " + VarHackConfig.EscapeChar;
                    Address = MarioConfig.StructAddress + MarioConfig.HSpeedOffset;
                    MemoryType = typeof(float);
                    break;
                case 1:
                    VarName = "Angle " + VarHackConfig.EscapeChar;
                    Address = MarioConfig.StructAddress + MarioConfig.FacingYawOffset;
                    MemoryType = typeof(ushort);
                    break;
                case 2:
                    VarName = "HP " + VarHackConfig.EscapeChar;
                    Address = MarioConfig.StructAddress + HudConfig.HpCountOffset;
                    MemoryType = typeof(short);
                    UseHex = true;
                    break;
                case 3:
                    VarName = "Floor Room " + VarHackConfig.EscapeChar;
                    Address = MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset;
                    MemoryType = typeof(byte);
                    PointerOffset = 0x05;
                    break;
                case 4:
                    VarName = "X " + VarHackConfig.EscapeChar;
                    Address = MarioConfig.StructAddress + MarioConfig.XOffset;
                    MemoryType = typeof(float);
                    break;
                case 5:
                    VarName = "Y " + VarHackConfig.EscapeChar;
                    Address = MarioConfig.StructAddress + MarioConfig.YOffset;
                    MemoryType = typeof(float);
                    break;
                case 6:
                    VarName = "Z " + VarHackConfig.EscapeChar;
                    Address = MarioConfig.StructAddress + MarioConfig.ZOffset;
                    MemoryType = typeof(float);
                    break;
                case 7:
                    VarName = "HOLP X " + VarHackConfig.EscapeChar;
                    Address = MarioConfig.StructAddress + MarioConfig.HolpXOffset;
                    MemoryType = typeof(float);
                    break;
                case 8:
                    VarName = "HOLP Y " + VarHackConfig.EscapeChar;
                    Address = MarioConfig.StructAddress + MarioConfig.HolpYOffset;
                    MemoryType = typeof(float);
                    break;
                case 9:
                default:
                    VarName = "HOLP Z " + VarHackConfig.EscapeChar;
                    Address = MarioConfig.StructAddress + MarioConfig.HolpZOffset;
                    MemoryType = typeof(float);
                    break;
            }
        }
    }
}
