using STROOP.Core;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables.VariablePanel.Cells;

namespace STROOP.Controls.VariablePanel.Cells;

public class VariableAddressCell(VariableNumberCell<uint> baseCell)
    : VariableAddressCell<WinFormsVariablePanelUiContext>(baseCell)
{
    protected override void ShowMemory(uint address)
    {
        if (address == 0)
            return;

        if (ObjectUtilities.IsObjectAddress(address))
            AccessScope<StroopMainForm>.content.GetTab<Tabs.MemoryTab>().SetObjectAddress(address);
        else
            AccessScope<StroopMainForm>.content.GetTab<Tabs.MemoryTab>().SetCustomAddress(address);
        Config.TabControlMain.SelectedTab = Config.TabControlMain.TabPages["tabPageMemory"];
    }
}
