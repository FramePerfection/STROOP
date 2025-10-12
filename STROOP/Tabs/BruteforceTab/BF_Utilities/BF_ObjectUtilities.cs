using System.Collections.Generic;
using STROOP.Structs;
using STROOP.Structs.Configurations;

namespace STROOP.Tabs.BruteforceTab.BF_Utilities
{
    public static class BF_ObjectUtilities
    {
        private static readonly Dictionary<byte, uint> BehaviorCommandSize = new Dictionary<byte, uint>()
        {
            [0x1C] = 3,
            [0x2C] = 3,
            [0x29] = 3,
            [0x0C] = 2,
            [0x36] = 2,
            [0x13] = 2,
            [0x14] = 2,
            [0x15] = 2,
            [0x16] = 2,
            [0x27] = 2,
            [0x23] = 2,
            [0x2E] = 2,
            [0x2B] = 3,
            [0x2A] = 2,
            [0x2F] = 2,
            [0x30] = 5,
            [0x33] = 2,
            [0x37] = 2,
        };

        private static uint GetCommandSize(byte command) => BehaviorCommandSize.TryGetValue(command, out var val) ? val : 1;

        public static uint[] GetAndUnrollBehaviorScript(uint absoluteBehaviorScriptAddress, out uint? segmentedCollisionPointer)
        {
            segmentedCollisionPointer = null;
            var startCommand = Config.Stream.GetUInt32(absoluteBehaviorScriptAddress);
            // We may need to read backwards to find the 'BEGIN' command of the behavior script (probably not, this is a garbage idea!)
            while (startCommand >> 0x18 != 0)
                startCommand = Config.Stream.GetUInt32(absoluteBehaviorScriptAddress -= 4);

            var commands = new List<uint>() { startCommand };
            var cursor = absoluteBehaviorScriptAddress + 4;
            uint nextCommand = startCommand;
            while (true)
            {
                nextCommand = Config.Stream.GetUInt32(cursor);
                var cmdByte = (byte)(nextCommand >> 0x18);
                switch (cmdByte)
                {
                    case 0: // start of next script
                        return commands.ToArray();
                    case 0x04:
                        var segmentedAddress = Config.Stream.GetUInt32(cursor + 4);
                        cursor = SegmentationUtilities.SegmentedToVirtual(segmentedAddress);
                        continue;
                    case 0x2A: // LOAD_COLLISION_DATA
                        segmentedCollisionPointer = Config.Stream.GetUInt32(cursor + 4);
                        break;
                }

                var commandSize = GetCommandSize(cmdByte);
                for (int i = 0; i < commandSize; i++)
                {
                    commands.Add(Config.Stream.GetUInt32(cursor));
                    cursor += 4;
                }
            }
        }

        public static ushort[] GetCollisionData(uint absoluteCollisionPtr)
        {
            var lst = new List<ushort>();
            ushort nextValue;
            do
            {
                nextValue = Config.Stream.GetUInt16(absoluteCollisionPtr);
                lst.Add(nextValue);
                absoluteCollisionPtr += 2;
            } while (nextValue != 0x41);

            return lst.ToArray();
        }

        public static string GetObjectCollisionOverride(uint segmentedPointer)
        {
            return $@"
    {{
        ""overrides"": ""0x{segmentedPointer.ToString("X8")}"",
        ""data"": [{string.Join(", ", GetCollisionData(SegmentationUtilities.SegmentedToVirtual(segmentedPointer)))}]
    }}";
        }
    }
}
