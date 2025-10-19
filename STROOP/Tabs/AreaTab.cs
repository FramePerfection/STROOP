using STROOP.Core;
using System.Collections.Generic;
using System.Windows.Forms;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Variables;
using STROOP.Variables.SM64MemoryLayout;
using STROOP.Variables.Utilities;

namespace STROOP.Tabs
{
    public partial class AreaTab : STROOPTab
    {
        [InitializeBaseAddress]
        static void InitializeBaseAddress()
        {
            VariableUtilities.baseAddressGetters[BaseAddressType.Area] = () => [SelectedAreaAddress];
        }

        public static uint SelectedAreaAddress
        {
            get
            {
                var tab = AccessScope<StroopMainForm>.content.GetTab<AreaTab>();
                return tab.checkBoxSelectCurrentArea.Checked
                    ? Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.AreaPointerOffset)
                    : AreaUtilities.GetAreaAddress(tab.SelectedAreaIndex);
            }
        }

        int SelectedAreaIndex;
        List<RadioButton> _selectedAreaRadioButtons;

        public AreaTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "Area";

        public override void InitializeTab()
        {
            base.InitializeTab();
            SelectedAreaIndex = 0;

            _selectedAreaRadioButtons = new List<RadioButton>();
            for (int i = 0; i < 8; i++)
            {
                _selectedAreaRadioButtons.Add(splitContainerArea.Panel1.Controls["radioButtonArea" + i] as RadioButton);
            }

            for (int i = 0; i < _selectedAreaRadioButtons.Count; i++)
            {
                int index = i;
                _selectedAreaRadioButtons[i].Click += (sender, e) =>
                {
                    checkBoxSelectCurrentArea.Checked = false;
                    SelectedAreaIndex = index;
                };
            }
        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;

            base.Update(updateView);

            int? currentAreaIndex = AreaUtilities.GetAreaIndex(SelectedAreaAddress);
            for (int i = 0; i < _selectedAreaRadioButtons.Count; i++)
            {
                _selectedAreaRadioButtons[i].Checked = i == currentAreaIndex;
            }
        }
    }
}
