using System;
using System.Drawing;
using System.Windows.Forms;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.Views;

namespace STROOP.Controls.VariablePanel
{
    public abstract class WatchVariableBooleanWrapperBase<T> : WatchVariableWrapper<T>
    {
        public static readonly WatchVariableSetting DisplayAsCheckboxSetting = new WatchVariableSetting(
            "Boolean: Display as Checkbox",
            CreateBoolWithDefault<WatchVariableBooleanWrapperBase<T>>((wrapper, val) => wrapper._displayAsCheckbox = val, wrapper => wrapper._displayAsCheckbox),
            ("Default", () => true, WrapperProperty<WatchVariableBooleanWrapperBase<T>>(wr => wr._displayAsCheckbox == true)),
            ("Display as Checkbox", () => true, WrapperProperty<WatchVariableBooleanWrapperBase<T>>(wr => wr._displayAsCheckbox)),
            ("Don't display as Checkbox", () => false, WrapperProperty<WatchVariableBooleanWrapperBase<T>>(wr => !wr._displayAsCheckbox))
        );

        public static readonly WatchVariableSetting DisplayAsInverted = new WatchVariableSetting(
            "Boolean: Display as Inverted",
            CreateBoolWithDefault<WatchVariableBooleanWrapperBase<T>>((wrapper, val) => wrapper._displayAsInverted = val, wrapper => wrapper._displayAsInverted),
            ("Default", () => false, WrapperProperty<WatchVariableBooleanWrapperBase<T>>(wr => wr._displayAsInverted == false)),
            ("Display as Inverted", () => true, WrapperProperty<WatchVariableBooleanWrapperBase<T>>(wr => wr._displayAsInverted)),
            ("Don't display as Inverted", () => false, WrapperProperty<WatchVariableBooleanWrapperBase<T>>(wr => !wr._displayAsInverted))
        );

        private bool _displayAsCheckbox;
        private bool _displayAsInverted;

        protected abstract T falseValue { get; }
        protected abstract T trueValue { get; }

        public override void SingleClick(Control parent, Rectangle bounds)
        {
            if (_displayAsCheckbox)
                Edit(parent, bounds);
        }

        public override void DoubleClick(Control parent, Rectangle bounds)
        {
            if (!_displayAsCheckbox)
                Edit(parent, bounds);
        }

        public WatchVariableBooleanWrapperBase(IVariableView<T> watchVar, WatchVariableControl watchVarControl)
            : base(watchVar, watchVarControl)
        {
            _displayAsCheckbox = true;
            if (bool.TryParse(watchVarControl.view.GetValueByKey(CommonViewProperties.invertBool), out var invertBool))
                _displayAsInverted = invertBool;
            else
                _displayAsInverted = false;

            AddBooleanContextMenuStripItems();
        }

        private void AddBooleanContextMenuStripItems()
        {
            _watchVarControl.AddSetting(DisplayAsCheckboxSetting);
            _watchVarControl.AddSetting(DisplayAsInverted);
        }

        public override WatchVariablePanel.CustomDraw CustomDrawOperation => _displayAsCheckbox ? DrawCheckbox : (WatchVariablePanel.CustomDraw)null;

        public override sealed void Edit(Control parent, Rectangle bounds)
        {
            if (_displayAsCheckbox)
            {
                if (lastValueMeaning != CombinedValuesMeaning.SameValue)
                    nextValue = falseValue;
                else
                    nextValue = lastValue.Equals(falseValue) ? trueValue : falseValue;
            }
            else
                base.Edit(parent, bounds);
        }

        void DrawCheckbox(Graphics g, Rectangle rect)
        {
            var combinedValues = CombineValues();
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
            var imgHeight = rect.Height - margin * 2;
            g.DrawImage(checkboxImage, rect.Right - imgHeight - margin * 2, rect.Top + margin, imgHeight, imgHeight);
        }

        protected CheckState ConvertValueToCheckState(object value)
        {
            double? doubleValueNullable = ParsingUtilities.ParseDoubleNullable(value);
            if (!doubleValueNullable.HasValue) return CheckState.Unchecked;
            double doubleValue = doubleValueNullable.Value;
            return HandleInverting(doubleValue == 0) ? CheckState.Unchecked : CheckState.Checked;
        }

        private bool HandleInverting(bool boolValue) => boolValue != _displayAsInverted;

        public override string GetClass() => "Boolean";
    }

    public class WatchVariableBooleanWrapper : WatchVariableBooleanWrapperBase<bool>
    {
        protected override bool falseValue => false;

        protected override bool trueValue => true;

        public WatchVariableBooleanWrapper(IVariableView<bool> watchVar, WatchVariableControl watchVarControl)
            : base(watchVar, watchVarControl)
        {
        }

        public override bool TryParseValue(string value, out bool result)
            => bool.TryParse(value, out result);

        public override string DisplayValue(bool value) => value.ToString();
    }

    public class WatchVariableBooleanWrapper<TNumber> : WatchVariableBooleanWrapperBase<TNumber> where TNumber : struct, IConvertible
    {
        public WatchVariableBooleanWrapper(IVariableView<TNumber> watchVar, WatchVariableControl watchVarControl)
            : base(watchVar, watchVarControl)
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
