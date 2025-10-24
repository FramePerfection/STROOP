using STROOP.Core;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.VariablePanel.Cells;

namespace STROOP.Controls.VariablePanel.Cells;

// TODO: refactor this such that it takes a NumberWrapper as a base wrapper, similar to selection wrappers,
//       in order to avoid code duplication with RoundToZero
public class VariableAddressCell(IVariable<uint> watchVar, WinFormsVariableControl watchVarControl)
    : VariableAddressCell<WinFormsVariablePanelUiContext>(watchVar, watchVarControl)
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

    protected override bool RoundToZero() => !SavedSettingsConfig.DontRoundValuesToZero;
}
