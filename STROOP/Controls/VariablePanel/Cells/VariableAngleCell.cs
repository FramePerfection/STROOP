using STROOP.Structs.Configurations;
using STROOP.Variables;
using STROOP.Variables.VariablePanel.Cells;
using System;

namespace STROOP.Controls.VariablePanel.Cells;

// TODO: refactor this such that it takes a NumberWrapper as a base wrapper, similar to selection wrappers,
//       in order to avoid code duplication with TryParseValue and RoundToZero
public class VariableAngleCell<TNumber>(IVariable<TNumber> watchVar, WinFormsVariableControl watchVarControl)
    : VariableAngleCell<VariablePanelUiContext, TNumber>(watchVar, watchVarControl)
    where TNumber : struct, IConvertible
{
    protected override bool DisplayAsUnsigned() => SavedSettingsConfig.DisplayYawAnglesAsUnsigned;

    protected override bool RoundToZero() => !SavedSettingsConfig.DontRoundValuesToZero;
}
