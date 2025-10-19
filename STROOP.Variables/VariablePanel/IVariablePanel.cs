namespace STROOP.Variables.VariablePanel;

public interface IVariablePanel<TUiContext>
    where TUiContext : IUiContext
{
    bool IsSelected { get; }
    void RemoveVariable(IVariableCellUi<TUiContext> variableCell);
}
