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
                if (g.frames.TryGetValue(globalTimer, out newFrame))
                    currentFrame = newFrame;
            }

            public override Vector4 GetArrowColor(Vector4 baseColor) => color;
        }

        public record struct PlaybackFrame(GhostFrame frame, uint animation);

        const uint OBJECT_EXTRA_MAGIC = 0x4F424A54;

        public uint playbackBaseFrame = 0;
        public Dictionary<uint, GhostFrame> frames = new Dictionary<uint, GhostFrame>();
        public PlaybackFrame lastValidPlaybackFrame;
        public GhostFrame currentFrame;
        public uint originalPlaybackBaseFrame { get; private set; }
        public uint maxFrame { get; private set; }
        public uint numFrames => maxFrame + 1;
        public string name, fileName = "-";
        public Vector4 hatColor = new Vector4(0, 1, 0, 1);
        public GhostPositionAngle positionAngle { get; private set; }
        public bool transparent = true;

        /// <summary> The graphics pointer to pass to the hack - 0 is interpreted as "Mario" by the hack. </summary>
        public uint nonMarioGraphics = 0;
        public Dictionary<uint, uint> animationSwitches = new();

        public Ghost()
        {
            positionAngle = new GhostPositionAngle(this);
        }

        public Ghost(uint playbackBaseFrame, Dictionary<uint, GhostFrame> frames)
        {
            this.playbackBaseFrame = originalPlaybackBaseFrame = playbackBaseFrame;
            this.frames = frames;
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
                    result.frames[index] = frame;
                    result.maxFrame = Math.Max(result.maxFrame, index);
                }

                if (reader.BaseStream.Position > reader.BaseStream.Length - 4 || reader.ReadUInt32() != OBJECT_EXTRA_MAGIC)
                    return result;

                result.nonMarioGraphics = reader.ReadUInt32();
                var numAnimationSwitches = reader.ReadUInt32();
                for (int i = 0; i < numAnimationSwitches; i++)
                {
                    var key = reader.ReadUInt32();
                    var value = reader.ReadUInt32();
                    result.animationSwitches[key] = value;
                }
            }
            catch (IOException)
            {
                return null;
            }

            return result;
        }
        public void ToFile(BinaryWriter wr)
        {
            wr.Write(originalPlaybackBaseFrame);
            wr.Write(frames.Count);
            foreach (var frame in frames)
            {
                wr.Write(frame.Key);
                frame.Value.WriteTo(wr);
            }

            wr.Write(OBJECT_EXTRA_MAGIC);
            wr.Write(animationSwitches.Count);
            foreach (var kvp in animationSwitches)
            {
                wr.Write(kvp.Key);
                wr.Write(kvp.Value);
            }
        }

        public override string ToString()
        {
            return name ?? "<unnamed ghost>";
        }
    }
}
