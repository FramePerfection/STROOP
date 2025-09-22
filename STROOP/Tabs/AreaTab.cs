using System.Collections.Generic;
using System.Windows.Forms;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Tabs
{
    public partial class AreaTab : STROOPTab
    {
        public uint SelectedAreaAddress { get; private set; }

        List<RadioButton> _selectedAreaRadioButtons;

        public AreaTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "Area";

        public override void InitializeTab()
        {
            base.InitializeTab();
            SelectedAreaAddress = AreaUtilities.GetAreaAddress(0);

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
                    SelectedAreaAddress = AreaUtilities.GetAreaAddress(index);
                };
            }
        }

        public override void Update(bool updateView)
        {
            if (checkBoxSelectCurrentArea.Checked)
            {
                SelectedAreaAddress = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.AreaPointerOffset);
            }

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
