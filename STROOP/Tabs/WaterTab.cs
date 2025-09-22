using System.Collections.Generic;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables.Utilities;

namespace STROOP.Tabs
{
    public partial class WaterTab : STROOPTab
    {
        [InitializeBaseAddress]
        static void InitBaseAddresses()
        {
            WatchVariableUtilities.baseAddressGetters["Water"] = () =>
            {
                uint waterAddress = Config.Stream.GetUInt32(MiscConfig.WaterPointerAddress);
                return waterAddress != 0 ? new List<uint>() { waterAddress } : WatchVariableUtilities.BaseAddressListEmpty;
            };
        }

        public WaterTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "Water";
    }
}
