namespace STROOP.Structs
{
    public enum ExecuteModeType
    {
        Once,
        Always,
        UserCalledS
    };

    public class GameScript
    {
        public uint[] Script;
        public uint InsertAddress;
        public byte Reg1;
        public byte Reg2;
        public ExecuteModeType ExecuteMode;
        public uint ExecutionSpace;
        public byte[] JumpInstBytes;
        public uint JumpInstr;
        public uint PostInstrSpace;
        public bool Allocated;
    }
}
