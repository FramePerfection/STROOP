using System.Collections.Generic;
using STROOP.Structs;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.Utilities;

namespace STROOP.Tabs
{
    public partial class QuarterFramesTab : STROOPTab
    {
        [InitializeBaseAddress]
        static void InitBaseAddresses()
        {
            VariableUtilities.baseAddressGetters["HackedArea"] = () => new List<uint> { MiscConfig.HackedAreaAddress };
        }

        private static readonly List<string> ALL_VAR_GROUPS =
            new List<string>()
            {
                VariableGroup.QuarterFrameHack,
                VariableGroup.GhostHack,
                VariableGroup.Custom,
            };

        private static readonly List<string> VISIBLE_VAR_GROUPS =
            new List<string>()
            {
                VariableGroup.QuarterFrameHack,
                VariableGroup.Custom,
            };

        public QuarterFramesTab()
        {
            InitializeComponent();
            _variablePanelQuarterFrame.SetGroups(ALL_VAR_GROUPS, VISIBLE_VAR_GROUPS);
        }

        public override string GetDisplayName() => "QFrames";
    }
}
