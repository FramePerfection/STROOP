using System.Collections.Generic;
using STROOP.Structs;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Variables.Utilities;

namespace STROOP.Tabs
{
    public partial class InputTab : STROOPTab
    {
        [InitializeBaseAddress]
        static void InitBaseAddresses()
        {
            WatchVariableUtilities.baseAddressGetters["InputCurrent"] = () => new List<uint> { InputConfig.CurrentInputAddress };
            WatchVariableUtilities.baseAddressGetters["InputJustPressed"] = () => new List<uint> { InputConfig.JustPressedInputAddress };
            WatchVariableUtilities.baseAddressGetters["InputBuffered"] = () => new List<uint> { InputConfig.BufferedInputAddress };
        }

        List<InputImageGui> _guiList;

        public InputTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "Input";

        public override void InitializeTab()
        {
            base.InitializeTab();

            _guiList = XmlConfigParser.CreateInputImageAssocList(@"Config/InputImageAssociations.xml");
            ;

            inputDisplayPanel.SetInputDisplayGui(_guiList);
        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;
            base.Update(updateView);

            inputDisplayPanel.UpdateInputs();
            inputDisplayPanel.Invalidate();
        }
    }
}
