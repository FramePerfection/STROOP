using STROOP.Utilities;
using STROOP.Variables.Utilities;

namespace STROOP.Variables.VariablePanel.Cells
{
    public abstract class VariableBooleanCellBase<TUiContext, T>
        : VariableCell<TUiContext, T>
        where TUiContext : IUiContext
    {

        public static readonly VariableCellSetting<TUiContext> DisplayAsCheckboxCellSetting = new VariableCellSetting<TUiContext>(
            "Boolean: Display as Checkbox",
            CreateBoolWithDefault<VariableBooleanCellBase<TUiContext, T>>((wrapper, val) => wrapper._displayAsCheckbox = val, wrapper => wrapper._displayAsCheckbox),
            ("Default", () => true, WrapperProperty<VariableBooleanCellBase<TUiContext, T>>(wr => wr._displayAsCheckbox == true)),
            ("Display as Checkbox", () => true, WrapperProperty<VariableBooleanCellBase<TUiContext, T>>(wr => wr._displayAsCheckbox)),
            ("Don't display as Checkbox", () => false, WrapperProperty<VariableBooleanCellBase<TUiContext, T>>(wr => !wr._displayAsCheckbox))
        );

        public static readonly VariableCellSetting<TUiContext> DisplayAsInverted = new VariableCellSetting<TUiContext>(
            "Boolean: Display as Inverted",
            CreateBoolWithDefault<VariableBooleanCellBase<TUiContext, T>>((wrapper, val) => wrapper._displayAsInverted = val, wrapper => wrapper._displayAsInverted),
            ("Default", () => false, WrapperProperty<VariableBooleanCellBase<TUiContext, T>>(wr => wr._displayAsInverted == false)),
            ("Display as Inverted", () => true, WrapperProperty<VariableBooleanCellBase<TUiContext, T>>(wr => wr._displayAsInverted)),
            ("Don't display as Inverted", () => false, WrapperProperty<VariableBooleanCellBase<TUiContext, T>>(wr => !wr._displayAsInverted))
        );

        private bool _displayAsCheckbox;
        protected bool _displayAsInverted { get ; private set; }

        protected abstract T falseValue { get; }
        protected abstract T trueValue { get; }

        public override void SingleClick(TUiContext uiContext)
        {
            if (_displayAsCheckbox)
                Edit(uiContext);
        }

        public override void DoubleClick(TUiContext uiContext)
        {
            if (!_displayAsCheckbox)
                Edit(uiContext);
        }

        public VariableBooleanCellBase(IVariable<T> watchVar, VariableCellControl<TUiContext> varCellControl)
            : base(watchVar, varCellControl)
        {
            _displayAsCheckbox = true;
            if (bool.TryParse(varCellControl.view.GetValueByKey(CommonVariableProperties.invertBool), out var invertBool))
                _displayAsInverted = invertBool;
            else
                _displayAsInverted = false;

            AddBooleanContextMenuStripItems();
        }

        private void AddBooleanContextMenuStripItems()
        {
            control.AddSetting(DisplayAsCheckboxCellSetting);
            control.AddSetting(DisplayAsInverted);
        }

        public override IVariableCellUi<TUiContext>.CustomDraw CustomDrawOperation => _displayAsCheckbox ? DrawCheckbox : null;

        public override sealed void Edit(TUiContext uiContext)
        {
            if (_displayAsCheckbox)
            {
                if (lastValueMeaning != CombinedValuesMeaning.SameValue)
                    nextValue = falseValue;
                else
                    nextValue = lastValue.Equals(falseValue) ? trueValue : falseValue;
            }
            else
                base.Edit(uiContext);
        }

        protected abstract void DrawCheckbox(TUiContext uiContext);

        private bool HandleInverting(bool boolValue) => boolValue != _displayAsInverted;

        public override string GetClass() => "Boolean";
    }

    public abstract class VariableBooleanCell<TUiContext>
        : VariableBooleanCellBase<TUiContext, bool>
        where TUiContext : IUiContext
    {
        protected override bool falseValue => false;

        protected override bool trueValue => true;

        public VariableBooleanCell(IVariable<bool> watchVar, VariableCellControl<TUiContext> varCellControl)
            : base(watchVar, varCellControl)
        {
        }

        public override bool TryParseValue(string value, out bool result)
            => bool.TryParse(value, out result);

        public override string DisplayValue(bool value) => value.ToString();
    }

    public abstract class VariableBooleanCell<TUiContext, TNumber>
        : VariableBooleanCellBase<TUiContext, TNumber>
        where TUiContext : IUiContext
        where TNumber : struct, IConvertible
    {
        public VariableBooleanCell(IVariable<TNumber> watchVar, VariableCellControl<TUiContext> varCellControl)
            : base(watchVar, varCellControl)
        {
        }

        private static TNumber MaxValue = (TNumber)typeof(TNumber).GetField(nameof(MaxValue)).GetValue(null);

        protected override TNumber falseValue => (TNumber)Convert.ChangeType(0, typeof(TNumber));
        protected override TNumber trueValue => MaxValue;

        public override bool TryParseValue(string value, out TNumber result)
            => ParsingUtilities.TryParseNumber(value, out result);

        public override string DisplayValue(TNumber value) => value.ToString();
    }
}
