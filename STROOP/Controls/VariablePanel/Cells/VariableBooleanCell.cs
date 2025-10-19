using STROOP.Variables;
using STROOP.Variables.Utilities;
using STROOP.Variables.VariablePanel.Cells;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace STROOP.Controls.VariablePanel.Cells;

public class VariableBooleanCell<TNumber>(IVariable<TNumber> watchVar, WinFormsVariableControl watchVarControl)
    : VariableBooleanCell<VariablePanelUiContext, TNumber>(watchVar, watchVarControl)
    where TNumber : struct, IConvertible
{
    protected override void DrawCheckbox(VariablePanelUiContext uiContext)
    {
        var combinedValues = this.CombineValues();
        CheckState state;
        if (combinedValues.meaning != CombinedValuesMeaning.SameValue)
            state = CheckState.Indeterminate;
        else
            state = (Convert.ToDecimal(combinedValues.value) != 0 ^ _displayAsInverted) ? CheckState.Checked : CheckState.Unchecked;

        Image checkboxImage;
        switch (state)
        {
            case CheckState.Checked:
                checkboxImage = Properties.Resources.checkbox_checked;
                break;
            case CheckState.Unchecked:
                checkboxImage = Properties.Resources.checkbox_unchecked;
                break;
            default:
                checkboxImage = Properties.Resources.checkbox_indeterminate;
                break;
        }

        var margin = 2;
        var imgHeight = uiContext.drawRegion.Height - margin * 2;
        uiContext.graphics.DrawImage(checkboxImage, uiContext.drawRegion.Right - imgHeight - margin * 2, uiContext.drawRegion.Top + margin, imgHeight, imgHeight);

    }
}
