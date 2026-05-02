using STROOP.Variables.Utilities;

namespace STROOP.Variables.VariablePanel.Cells
{
    public abstract class VariableObjectCell<TUiContext>
        : VariableAddressCell<TUiContext>
        where TUiContext : IUiContext
    {
        static VariableCellSetting<TUiContext> _displayAsObjectCellSetting = new VariableCellSetting<TUiContext>(
            "Display as Object",
            (ctrl, obj) =>
            {
                if (ctrl.varCellInternal is VariableObjectCell<TUiContext> objectWrapper)
                    if (obj is bool doDisplayAsObject)
                        objectWrapper._displayAsObject = doDisplayAsObject;
                    else
                        return false;
                return true;
            },
            ("Object", () => true, WrapperProperty<VariableObjectCell<TUiContext>>(o => o._displayAsObject)),
            ("Address", () => true, WrapperProperty<VariableObjectCell<TUiContext>>(o => !o._displayAsObject))
        );

        private static VariableCellSetting<TUiContext> _selectObjectCellSetting = new VariableCellSetting<TUiContext>(
            "Select Object",
            (ctrl, obj) =>
            {
                if (ctrl.varCellInternal is VariableObjectCell<TUiContext> objectCell)
                {
                    var value = objectCell.CombineValues<uint>();
                    if (value.meaning == CombinedValuesMeaning.SameValue)
                        objectCell.SelectObject(value.value);
                }

                return false;
            });

        private bool _displayAsObject;

        public VariableObjectCell(VariableNumberCell<TUiContext, uint> baseCell)
            : base(baseCell)
        {
            _displayAsObject = true;

            AddObjectContextMenuStripItems();
        }

        protected abstract uint GetUnusedSlotAddress();
        protected abstract void SelectObject(uint address);
        protected abstract string GetDescriptiveSlotLabelFromAddress(uint address);
        protected abstract uint? GetObjectAddressFromLabel(string label);

        private void AddObjectContextMenuStripItems()
        {
            control.AddSetting(_displayAsObjectCellSetting);
            control.AddSetting(_selectObjectCellSetting);
        }

        public override string GetClass() => "Object";

        public override string DisplayValue(uint value)
            => _displayAsObject ? GetDescriptiveSlotLabelFromAddress(value) : base.DisplayValue(value);

        public override bool TryParseValue(string value, out uint result)
        {
            string slotName = value.ToLower();

            if (slotName == "(no object)" || slotName == "no object")
            {
                result = 0;
                return true;
            }

            if (slotName == "(unused object)" || slotName == "unused object")
            {
                result = GetUnusedSlotAddress();
                return true;
            }

            if (slotName.StartsWith("slot"))
            {
                var addr = GetObjectAddressFromLabel(slotName.Remove(0, "slot".Length).Trim());
                if (addr != null)
                {
                    result = addr.Value;
                    return true;
                }
            }

            return base.TryParseValue(value, out result);
        }
    }
}
