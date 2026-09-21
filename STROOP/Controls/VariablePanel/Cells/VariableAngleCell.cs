using STROOP.Structs.Configurations;
using STROOP.Variables.VariablePanel.Cells;
using System;

namespace STROOP.Controls.VariablePanel.Cells;

public class VariableAngleCell<TNumber>(VariableNumberCell<TNumber> baseCell)
    : VariableAngleCell<WinFormsVariablePanelUiContext, TNumber>(baseCell)
    , INumberVariableCell
    where TNumber : struct, IConvertible
{
    protected override bool DisplayAsUnsigned() => SavedSettingsConfig.DisplayYawAnglesAsUnsigned;
}
