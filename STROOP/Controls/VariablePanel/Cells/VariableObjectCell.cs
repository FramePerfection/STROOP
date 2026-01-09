using STROOP.Core;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.VariablePanel.Cells;

namespace STROOP.Controls.VariablePanel.Cells;

// TODO: refactor this such that it takes a NumberWrapper as a base wrapper, similar to selection wrappers,
//       in order to avoid code duplication with RoundToZero
public class VariableObjectCell(VariableAddressCell baseCell)
    : VariableObjectCell<WinFormsVariablePanelUiContext>(baseCell)
{
    protected override uint GetUnusedSlotAddress()
        => ObjectSlotsConfig.UnusedSlotAddress;

    protected override void SelectObject(uint address)
        => Config.ObjectSlotsManager.SelectSlotByAddress(address);

    protected override string GetDescriptiveSlotLabelFromAddress(uint address)
        => Config.ObjectSlotsManager.GetDescriptiveSlotLabelFromAddress(address, false);

    protected override uint? GetObjectAddressFromLabel(string label) => Config.ObjectSlotsManager.GetObjectFromLabel(label)?.Address;

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
