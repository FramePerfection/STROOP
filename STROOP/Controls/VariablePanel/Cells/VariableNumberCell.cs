using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.Utilities;
using STROOP.Variables.VariablePanel;
using STROOP.Variables.VariablePanel.Cells;
using System;

namespace STROOP.Controls.VariablePanel.Cells;

public interface INumberVariableCell : IVariableCellData<IConvertible>, IVariableCellUi<VariablePanelUiContext>;

public class VariableNumberCell<TNumber>(IVariable<TNumber> view, WinFormsVariableControl control)
    : VariableNumberCell<VariablePanelUiContext, TNumber>(view, control)
    , INumberVariableCell
    where TNumber : struct, IConvertible
{
    protected override bool RoundToZero() => !SavedSettingsConfig.DontRoundValuesToZero;

    public override bool TryParseValue(string value, out TNumber result)
        => ParsingUtilities.TryParseNumber(value, out result);
}
