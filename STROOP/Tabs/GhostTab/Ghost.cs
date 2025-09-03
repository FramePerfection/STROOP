using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Mathematics;
using STROOP.Utilities;

namespace STROOP.Tabs.GhostTab
{
    class Ghost
    {
        public class GhostPositionAngle : PositionAngle
        {
            GhostFrame currentFrame;
            readonly Ghost g;
            public GhostPositionAngle(Ghost g) : base()
            {
                this.g = g;
            }

            public Vector4 color => g.hatColor;
            Vector3 Position => currentFrame.position;
            public override double X => Position.X;
            public override double Y => Position.Y;
            public override double Z => Position.Z;
            public override double Angle => currentFrame.oYaw;

            public override bool SetX(double value) => false;
            public override bool SetY(double value) => false;
            public override bool SetZ(double value) => false;
            public override bool SetAngle(double value) => false;
            public override string GetMapName() => "Ghost";

            public void SetGlobalTimer(uint globalTimer)
            {
                GhostFrame newFrame;
                if (g.playbackFrames.TryGetValue(globalTimer, out newFrame))
                    currentFrame = newFrame;
            }

            public override Vector4 GetArrowColor(Vector4 baseColor) => color;
        }

        public uint playbackBaseFrame = 0;
        public Dictionary<uint, GhostFrame> playbackFrames = new Dictionary<uint, GhostFrame>();
        public GhostFrame lastValidPlaybackFrame, currentFrame;
        public uint originalPlaybackBaseFrame { get; private set; }
        public uint maxFrame { get; private set; }
        public uint numFrames => maxFrame + 1;
        public string name, fileName = "-";
        public Vector4 hatColor = new Vector4(0, 1, 0, 1);
        public GhostPositionAngle positionAngle { get; private set; }
        public bool transparent = true;

        public Ghost()
        {
            positionAngle = new GhostPositionAngle(this);
        }
        public Ghost(uint playbackBaseFrame, Dictionary<uint, GhostFrame> playbackFrames)
        {
            this.playbackBaseFrame = originalPlaybackBaseFrame = playbackBaseFrame;
            this.playbackFrames = playbackFrames;

        }
        public static Ghost FromFile(BinaryReader reader)
        {
            Ghost result = new Ghost();
            try
            {
                result.playbackBaseFrame = result.originalPlaybackBaseFrame = reader.ReadUInt32();
                int numFrames = reader.ReadInt32();
                for (int i = 0; i < numFrames; i++)
                {
                    var index = reader.ReadUInt32();
                    var frame = GhostFrame.ReadFrom(reader);
                    result.playbackFrames[index] = frame;
                    result.maxFrame = Math.Max(result.maxFrame, index);
                }
            }
            catch (IOException)
            {
                return null;
            }
            return result;
        }

        public override string ToString()
        {
            return name ?? "<unnamed ghost>";
        }
    }
}
