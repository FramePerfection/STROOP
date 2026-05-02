using STROOP.Structs.Configurations;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Structs
{
    public static class InGameFunctionCall
    {
        private static uint LUI(uint reg, ushort value)
        {
            return ((0b0011110000000000 | reg) << 16 | value);
        }

        private static uint ORI(uint resultReg, uint operandReg, ushort value)
        {
            return ((0b0011010000000000 | resultReg << 5 | operandReg) << 16 | value);
        }

        private static uint JAL(uint address)
        {
            return (0x0C000000 | ((address & 0xFFFFFF) / 4));
        }

        private static uint J(uint address)
        {
            return (0x08000000 | ((address & 0xFFFFFF) / 4));
        }

        private static void WriteWords(ref uint address, params uint[] words)
        {
            for (int i = 0; i < words.Length; i++)
            {
                if (!Config.Stream.SetValue(words[i], address))
                    throw null;
                address += 4;
            }
        }

        private static void WriteRegisterAssign(ref uint address, uint register, uint value)
        {
            WriteWords(ref address, LUI(register, (ushort)(value >> 16)), ORI(register, register, (ushort)(value & 0xFFFF)));
        }

        // Inject asm in the game that executes a function call once and then removes
        // itself. Example:
        //
        // WriteInGameFunctionCall(0x8031F690, 0, 33, 0); // play music with index 33 (JP version)
        //
        // It works by changing the function pointer of the level script update function 
        // to some custom asm. Since this is an indirect call to something that hasn't 
        // been code yet, it will work even if the emulator isn't in pure interpreter
        // mode and caches code. A memcpy at the end removes the injected asm and also forces
        // the emulator to remove the cache so another function can be injected later.
        public static void WriteInGameFunctionCall(uint address, params uint[] arguments)
        {
            const int maxArguments = 4;
            if (arguments.Length > maxArguments)
            {
                throw new System.Exception("trying to call function with " + arguments.Length + " arguments, max is " + maxArguments);
            }

            uint startAddress = 0x803FFF00; // some free 0-memory (hopefully)
            uint currAddress = startAddress;
            const uint A0 = 4; // register index for argument 0

            // Stack frame:
            //ADDIU SP, SP, 0xFFE0
            //SW RA, 0x0014 (SP)
            WriteWords(ref currAddress, 0x27BDFFE0, 0xAFBF0014);

            // Restore level script pointer:
            //LI T0, 0x8037EB04
            //LUI AT, 0x8039
            //SW T0, 0xB900(AT)
            WriteWords(ref currAddress, 0x3C088037, 0x3508EB04, 0x3C018039, 0xAC28B900);

            // Write function call
            for (int i = 0; i < arguments.Length; i++)
            {
                uint reg = (uint)(A0 + i);
                WriteRegisterAssign(ref currAddress, reg, arguments[i]);
                //WriteWords(ref baseAddress, LUI(reg, (ushort) (arguments[i] >> 16)), ORI(reg, reg, (ushort) (arguments[i] & 0xFFFF)));
            }

            WriteWords(ref currAddress, JAL(address), 0x00000000); // NOP for delay slot

            // Erase self and return as if nothing happened:

            uint memcpyAddress = RomVersionConfig.SwitchMap(0x803273F0, 0x803264C0);
            const uint eraseBytes = 16 * 4 + maxArguments * 8; //fixed instructions + 2 instructions per argument
            WriteWords(ref currAddress, 0x3C1F8037, 0x37FFEB0C); //LI RA, 0x8037EB0C
            WriteRegisterAssign(ref currAddress, A0, currAddress);
            WriteRegisterAssign(ref currAddress, A0 + 1, currAddress - 1);
            WriteWords(ref currAddress, J(memcpyAddress), 0x24060000 | eraseBytes); //ADDIU A2, R0, eraseBytes

            // Hijack level script function pointer to point to injected asm
            Config.Stream.SetValue(startAddress | 0x80000000, 0x8038B900);
        }

        public static void CreateInGameSemaphore()
        {
            uint startAddress = 0x803FFF00; // some free 0-memory (hopefully)
            uint currAddress = startAddress;
            const uint A0 = 4; // register index for argument 0

            // Stack frame:
            //ADDIU SP, SP, 0xFFE0
            //SW RA, 0x0014 (SP)
            WriteWords(ref currAddress, 0x27BDFFE0, 0xAFBF0014);

            // Restore level script pointer:
            //LI T0, 0x8037EB04
            //LUI AT, 0x8039
            //SW T0, 0xB900(AT)
            WriteWords(ref currAddress, 0x3C088037, 0x3508EB04, 0x3C018039, 0xAC28B900);


            // Create semaphore:
            //ORI AT, R0, $0001
            //LUI T0, $8040
            //SW AT, $FEFC (T0)
            //LW T1, $FEFC (T0)
            //BNEZ T1, $<-1 instruction>
            //NOP
            WriteWords(ref currAddress, 0x34010001, 0x3C088040, 0xAD01FEFC, 0x8D09FEFC, 0x1520FFFE, 0x00000000);

            // Erase self and return as if nothing happened:

            uint memcpyAddress = RomVersionConfig.SwitchMap(0x803273F0, 0x803264C0);
            const uint eraseBytes = 15 * 4 + 6 * 4; //fixed instructions + 2 instructions per argument
            WriteWords(ref currAddress, 0x3C1F8037, 0x37FFEB0C); //LI RA, 0x8037EB0C
            WriteRegisterAssign(ref currAddress, A0, currAddress);
            WriteRegisterAssign(ref currAddress, A0 + 1, currAddress - 1);
            WriteWords(ref currAddress, J(memcpyAddress), 0x24060000 | eraseBytes); //ADDIU A2, R0, eraseBytes

            // Hijack level script function pointer to point to injected asm
            Config.Stream.SetValue(startAddress | 0x80000000, 0x8038B900);
        }

        public static bool IsSemaphoreSet() => Config.Stream.GetUInt32(0x803FFEFC) == 1;
        public static void ClearSemaphore() => Config.Stream.SetValue((uint)0, 0x803FFEFC);


        public static void WriteInGameLevelScriptCall(params uint[] cmds)
        {
            uint cmdStartAddress = 0x80700000;
            uint cmdCurrAddress = cmdStartAddress;

            WriteWords(ref cmdCurrAddress, cmds);
            WriteWords(ref cmdCurrAddress, 0x05000000, 0);

            uint startAddress = 0x803FFF00; // some free 0-memory (hopefully)
            uint currAddress = startAddress;
            const uint A0 = 4; // register index for argument 0

            // Restore level script function pointer:
            //LI T0, 0x8037EB04
            //LUI AT, 0x8039
            //SW T0, 0xB900(AT)
            WriteWords(ref currAddress, 0x3C088037, 0x3508EB04, 0x3C018039, 0xAC28B900);

            //LW T0, $BE28 (AT)
            //LI T1, cmdStartAddress
            //SW T0, cmdStartAddress + (cmds.Length + 1) * 4
            //SW T1, $BE28 (AT)
            WriteWords(ref currAddress,
                0x3C090000 | (cmdStartAddress >> 0x10), 0x35290000 | (cmdStartAddress & 0xFFFF),
                0x8C28BE28,
                0xAD280000 | (ushort)((cmds.Length + 1) * 4),
                0xAC29BE28
            );

            // Erase self and return as if nothing happened:

            uint memcpyAddress = RomVersionConfig.SwitchMap(0x803273F0, 0x803264C0);
            const uint eraseBytes = 16 * 4 + 4 * 8; //fixed instructions + 2 instructions per argument
            WriteRegisterAssign(ref currAddress, A0, currAddress);
            WriteRegisterAssign(ref currAddress, A0 + 1, currAddress - 1);
            WriteWords(ref currAddress, J(memcpyAddress), 0x24060000 | eraseBytes); //ADDIU A2, R0, eraseBytes

            // Hijack level script function pointer to point to injected asm
            Config.Stream.SetValue(startAddress | 0x80000000, 0x8038B900);
        }

        static uint virtual_to_segmented(uint segment, uint addr)
        {
            var segmentAddress = Config.Stream.GetUInt32(0x8033b400 + segment * 4); //= sSegmentTable[segment]
            var offset = ((uint)addr & 0x1FFFFFFF) - segmentAddress;
            return (uint)((segment << 24) + offset);
        }
    }
}
