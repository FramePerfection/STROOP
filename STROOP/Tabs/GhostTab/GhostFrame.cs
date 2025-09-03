using System.IO;
using OpenTK;
using OpenTK.Mathematics;

namespace STROOP.Tabs.GhostTab
{
    struct GhostFrame
    {
        public Vector3 position;
        public short animationFrame, animationIndex;
        public uint oPitch, oYaw, oRoll;

        public void WriteTo(BinaryWriter wr)
        {
            wr.Write(position.X); wr.Write(position.Y); wr.Write(position.Z);
            wr.Write(animationIndex);
            wr.Write(animationFrame);
            wr.Write(oPitch);
            wr.Write(oYaw);
            wr.Write(oRoll);
        }

        public static GhostFrame ReadFrom(BinaryReader rd)
        {
            return new GhostFrame()
            {
                position = new Vector3(rd.ReadSingle(), rd.ReadSingle(), rd.ReadSingle()),
                animationIndex = rd.ReadInt16(),
                animationFrame = rd.ReadInt16(),
                oPitch = rd.ReadUInt32(),
                oYaw = rd.ReadUInt32(),
                oRoll = rd.ReadUInt32()
            };
        }
    }
}
