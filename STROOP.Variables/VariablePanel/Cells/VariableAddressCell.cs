using STROOP.Variables.Utilities;

namespace STROOP.Variables.VariablePanel.Cells
{
    public abstract class VariableAddressCell<TUiContext> : VariableNumberCell<TUiContext, uint>
        where TUiContext : IUiContext
    {
        private static VariableCellSetting<TUiContext> _viewAddressCellSetting = new VariableCellSetting<TUiContext>(
            "View Address",
            (ctrl, obj) =>
            {
                if (ctrl.varCellInternal is VariableAddressCell<TUiContext> addressWrapper)
                    addressWrapper.ShowMemory(addressWrapper.view.getter().FirstOrDefault());

                return false;
            });

        protected abstract void ShowMemory(uint address);

        public VariableAddressCell(IVariable<uint> watchVar, VariableCellControl<TUiContext> varCellControl)
            : base(watchVar.WithKeyedValue(CommonVariableProperties.useHex, true.ToString()), varCellControl)
        {
            varCellControl.AddSetting(_viewAddressCellSetting);
        }

        public override string GetClass() => "Address";
    }
}
