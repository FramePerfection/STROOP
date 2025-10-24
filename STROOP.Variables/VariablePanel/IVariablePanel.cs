namespace STROOP.Variables.VariablePanel;

public interface IVariablePanel<TUiContext>
    where TUiContext : IUiContext
{
    bool IsSelected { get; }
    void RemoveVariables(IEnumerable<IVariableCellUi<TUiContext>> variableCell);
    IEnumerable<IVariableCellUi<TUiContext>> AddVariables(IEnumerable<IVariableCellUi<TUiContext>> vars);
}

public static class IVariablePanelExtensions
{
    public static IEnumerable<IVariableCellUi<TUiContext>> AddVariables<TUiContext>(
        this IVariablePanel<TUiContext> @this,
        IEnumerable<(string name, IVariable variable)> vars
    )
        where TUiContext : IUiContext
        => @this.AddVariables(vars.Select(
            var => new VariableCellControl<TUiContext>(@this, var.variable) { VarName = var.name }.varCell
        ));

    public static IVariableCellUi<TUiContext> AddVariable<TUiContext>(this IVariablePanel<TUiContext> @this, (string name, IVariable variable) var)
        where TUiContext : IUiContext
        => @this.AddVariables([ var ]).First();

    public static void RemoveVariable<TUiContext>(this IVariablePanel<TUiContext> @this, IVariableCellUi<TUiContext> variableCell)
        where TUiContext : IUiContext
        => @this.RemoveVariables([ variableCell ]);
}
