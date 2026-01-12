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

        private VariableNumberCell<TUiContext, uint> baseCell;

        protected abstract void ShowMemory(uint address);

        public VariableAddressCell(VariableNumberCell<TUiContext, uint> baseCell)
            : base((IVariable<uint>)baseCell.control.view, baseCell.control)
        {
            control.view.SetValueByKey(CommonVariableProperties.useHex, true.ToString());
            control.AddSetting(_viewAddressCellSetting);
        }

        public override string GetClass() => "Address";

        internal protected sealed override bool RoundToZero() => baseCell.RoundToZero();
    }
}
