using System;

namespace STROOP.Tabs.MapTab.DataUtil
{
    public class Rdram
    {
        public readonly byte[] data;
        readonly uint[] segments = new uint[32];

        public Rdram(byte[] littleEndianWords)
        {
            data = littleEndianWords;
        }

        public void LoadSegmentTable(uint segmentTable)
        {
            for (uint i = 0; i < 32; i++)
                segments[i] = U32((segmentTable | 0x80000000) + 4 * i);
        }

        public uint Phys(uint a) => (a & 0x80000000) != 0 ? a & 0x7FFFFF : (segments[(a >> 24) & 0x1F] + (a & 0xFFFFFF)) & 0x7FFFFF;

        public bool Ok(uint physical, int length) => physical + length <= data.Length;

        public static bool IsPointer(uint a) => a >= 0x80000000 && a < 0x80800000;

        public byte U8Phys(uint p) => p < data.Length ? data[p ^ 3] : (byte)0;

        public ushort U16Phys(uint p) => (ushort)(U8Phys(p) << 8 | U8Phys(p + 1));

        public uint U32Phys(uint p)
        {
            if ((p & 3) == 0)
                return p + 4 <= data.Length ? BitConverter.ToUInt32(data, (int)p) : 0;
            return (uint)(U8Phys(p) << 24 | U8Phys(p + 1) << 16 | U8Phys(p + 2) << 8 | U8Phys(p + 3));
        }

        public byte U8(uint a) => U8Phys(Phys(a));
        public ushort U16(uint a) => U16Phys(Phys(a));
        public short S16(uint a) => (short)U16(a);
        public uint U32(uint a) => U32Phys(Phys(a));
        public int S32(uint a) => (int)U32(a);
        public float F32(uint a) => BitConverter.Int32BitsToSingle((int)U32(a));
    }
}
