using STROOP.Variables.VariablePanel;
using System.Collections.Generic;

namespace STROOP.Controls.VariablePanel;

public partial class VariablePanel : IVariablePanel<WinFormsVariablePanelUiContext>
{
    bool IVariablePanel<WinFormsVariablePanelUiContext>.IsSelected => Focused;

    public IEnumerable<IWinFormsVariableCell> AddVariables(IEnumerable<IWinFormsVariableCell> cells)
    {
        if (!initialized)
            DeferredInitialize();

        var lst = new List<IWinFormsVariableCell>();
        foreach (var cell in cells)
        {
            lst.Add(cell);
            _allWatchVarControls.Add(cell);
            if (ShouldShow(cell))
                _shownWatchVarControls.Add(cell);
        }

        return lst;
    }

    public void RemoveVariables(IEnumerable<IWinFormsVariableCell> watchVarControls)
    {
        foreach (IWinFormsVariableCell watchVarControl in watchVarControls)
        {
            _reorderingWatchVarControls.Remove(watchVarControl);
            _allWatchVarControls.Remove(watchVarControl);
            _shownWatchVarControls.Remove(watchVarControl);
        }
    }
}
