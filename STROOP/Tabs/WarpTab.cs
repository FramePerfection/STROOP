using System.Collections.Generic;
using System.Linq;
using STROOP.Controls.VariablePanel;
using STROOP.Core.Variables;
using STROOP.Forms;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;

namespace STROOP.Tabs
{
    public partial class WarpTab : STROOPTab
    {
        private List<uint> _warpNodeAddresses;

        private static readonly List<string> ALL_VAR_GROUPS =
            new List<string>()
            {
                VariableGroup.Basic,
                VariableGroup.WarpNode,
                VariableGroup.Custom,
            };

        private static readonly List<string> VISIBLE_VAR_GROUPS =
            new List<string>()
            {
                VariableGroup.Basic,
                VariableGroup.WarpNode,
                VariableGroup.Custom,
            };

        public WarpTab()
        {
            InitializeComponent();
            watchVariablePanelWarp.SetGroups(ALL_VAR_GROUPS, VISIBLE_VAR_GROUPS);
        }

        public override string GetDisplayName() => "Warp";

        public override void InitializeTab()
        {
            base.InitializeTab();
            _warpNodeAddresses = new List<uint>();

            buttonWarpInstructions.Click += (sender, e) =>
            {
                InfoForm.ShowValue(
                    string.Join("\r\n", _instructions),
                    "Instructions",
                    "Instructions");
            };
            buttonWarpHookUpTeleporters.Click += (sender, e) => HookUpTeleporters();
        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;

            List<uint> warpNodeAddresses = WatchVariableSpecialUtilities.GetWarpNodeAddresses();
            if (!Enumerable.SequenceEqual(warpNodeAddresses, _warpNodeAddresses))
            {
                watchVariablePanelWarp.RemoveVariableGroup(VariableGroup.WarpNode);
                watchVariablePanelWarp.AddVariables(GetWarpNodeVariables(warpNodeAddresses));
                _warpNodeAddresses = warpNodeAddresses;
            }

            base.Update(updateView);
        }

        private List<NamedVariableCollection.CustomView> GetWarpNodeVariables(List<uint> addresses)
        {
            var controls = new List<NamedVariableCollection.CustomView>();
            int i = 0;
            foreach (var address in addresses)
                controls.AddRange(GetWarpNodeVariables(address, i++));
            return controls;
        }

        private IEnumerable<NamedVariableCollection.CustomView> GetWarpNodeVariables(uint address, int index)
        {
            return new NamedVariableCollection.CustomView[]
            {
                new NamedVariableCollection.CustomView<byte>(typeof(WatchVariableNumberWrapper<byte>))
                {
                    Name = $"Warp {index} ID",
                    _getterFunction = () => Config.Stream.GetByte(address).Yield(),
                    _setterFunction = (val) => Config.Stream.SetValue(val, address).Yield()
                },
                new NamedVariableCollection.CustomView<byte>(typeof(WatchVariableNumberWrapper<byte>))
                {
                    Name = $"Warp {index} Dest Level",
                    _getterFunction = () => Config.Stream.GetByte(address + 0x1).Yield(),
                    _setterFunction = val => Config.Stream.SetValue(val, address + 0x1).Yield()
                },
                new NamedVariableCollection.CustomView<byte>(typeof(WatchVariableNumberWrapper<byte>))
                {
                    Name = $"Warp {index} Dest Area",
                    _getterFunction = () => Config.Stream.GetByte(address + 0x2).Yield(),
                    _setterFunction = val => Config.Stream.SetValue(val, address + 0x2).Yield()
                },
                new NamedVariableCollection.CustomView<byte>(typeof(WatchVariableNumberWrapper<byte>))
                {
                    Name = $"Warp {index} Dest Node",
                    _getterFunction = () => Config.Stream.GetByte(address + 0x3).Yield(),
                    _setterFunction = val => Config.Stream.SetValue(val, address + 0x3).Yield()
                },
                new NamedVariableCollection.CustomView<uint>(typeof(WatchVariableNumberWrapper<uint>))
                {
                    Name = $"Warp {index} Object",
                    _getterFunction = () => Config.Stream.GetUInt32(address + 0x4).Yield(),
                    _setterFunction = val => Config.Stream.SetValue(val, address + 0x4).Yield()
                },
                new NamedVariableCollection.CustomView<uint>(typeof(WatchVariableNumberWrapper<uint>))
                {
                    Name = $"Warp {index} Next",
                    _getterFunction = () => Config.Stream.GetUInt32(address + 0x8).Yield(),
                    _setterFunction = val => Config.Stream.SetValue(val, address + 0x8).Yield()
                },
            };
        }

        public void HookUpTeleporters()
        {
            uint mainSegmentEnd = 0x80367460;
            //uint engineSegmentStart = 0x80378800;

            uint lastWarpNodeAddress = WatchVariableSpecialUtilities.GetWarpNodeAddresses().LastOrDefault();
            if (lastWarpNodeAddress == 0) return;

            List<uint> objAddresses = Config.ObjectSlotsManager.SelectedObjects.ConvertAll(obj => obj.Address);
            if (objAddresses.Count < 2) return;

            uint teleporter1Address = objAddresses[0];
            uint teleporter2Address = objAddresses[1];
            short teleporter1Id = Config.Stream.GetInt16(teleporter1Address + 0x188);
            short teleporter2Id = Config.Stream.GetInt16(teleporter2Address + 0x188);

            uint warpNode1Address = mainSegmentEnd;
            uint warpNode2Address = mainSegmentEnd + 0xC;

            byte level = Config.Stream.GetByte(MiscConfig.WarpDestinationAddress + MiscConfig.LevelOffset);
            byte area = Config.Stream.GetByte(MiscConfig.WarpDestinationAddress + MiscConfig.AreaOffset);

            Config.Stream.SetValue((byte)teleporter1Id, warpNode1Address + 0x0);
            Config.Stream.SetValue(level, warpNode1Address + 0x1);
            Config.Stream.SetValue(area, warpNode1Address + 0x2);
            Config.Stream.SetValue((byte)teleporter2Id, warpNode1Address + 0x3);
            Config.Stream.SetValue(teleporter1Address, warpNode1Address + 0x4);
            Config.Stream.SetValue(warpNode2Address, warpNode1Address + 0x8);

            Config.Stream.SetValue((byte)teleporter2Id, warpNode2Address + 0x0);
            Config.Stream.SetValue(level, warpNode2Address + 0x1);
            Config.Stream.SetValue(area, warpNode2Address + 0x2);
            Config.Stream.SetValue((byte)teleporter1Id, warpNode2Address + 0x3);
            Config.Stream.SetValue(teleporter2Address, warpNode2Address + 0x4);
            Config.Stream.SetValue(0x00000000U, warpNode2Address + 0x8);

            Config.Stream.SetValue(warpNode1Address, lastWarpNodeAddress + 0x8);
        }

        private readonly List<string> _instructions = new List<string>()
        {
            @"The ""Hook Up Teleporters"" button can be used to enable teleporters in courses without any teleporters.",
            @"To use it properly, follow these instructions:",
            @"",
            @"First, go to a course with working teleporters, like BoB.",
            @"Select 2 teleporters that link to each other.",
            @"Right click on one of the slots, and click ""Copy Object"" with control held.",
            @"Go to the course where you would like to place the new teleporters.",
            @"Select the first 2 vacant slots.",
            @"Right click on one of the slots, and click ""Paste Object"" with control held.",
            @"Press the left arrow button (on the top right) with control held until the teleporters are at the end of the dark blue slots.",
            @"Click the ""Hook Up Teleporters"" button.",
            @"",
            @"Now the teleporters should link to each other, and you can place them wherever you want.",
        };
    }
}
