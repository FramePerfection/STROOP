using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using STROOP.Forms;
using STROOP.Variables;
using STROOP.Variables.Formatting;
using STROOP.Variables.Views;

namespace STROOP.Controls.VariablePanel
{
    public abstract class WatchVariableWrapper
    {
        public readonly IVariableView _view;

        public event Action ValueSet = () => { };

        protected readonly WatchVariableControl _watchVarControl;
        protected CarretlessTextBox textBox = null;
        protected Action editValueHandler;

        public virtual WatchVariablePanel.CustomDraw CustomDrawOperation => null;

        protected WatchVariableWrapper(IVariableView watchVar, WatchVariableControl watchVarControl)
        {
            _view = watchVar;
            _watchVarControl = watchVarControl;
        }

        public void ShowControllerForm()
        {
            new VariableControllerForm(_watchVarControl.VarName, this).Show();
        }

        public void ShowVarInfo()
        {
            var memoryDescriptor = (_view as IMemoryBasedVariableView)?.memoryDescriptor;
            VariableViewerForm varInfo =
                new VariableViewerForm(
                    name: _watchVarControl.VarName,
                    clazz: GetClass(),
                    type: memoryDescriptor?.GetTypeDescription() ?? "special",
                    baseTypeOffset: memoryDescriptor?.GetBaseTypeOffsetDescription() ?? "<none>",
                    n64BaseAddress: memoryDescriptor?.GetBaseAddressListString() ?? "<none>",
                    emulatorBaseAddress: memoryDescriptor?.GetProcessAddressListString() ?? "<none>",
                    n64Address: memoryDescriptor?.GetRamAddressListString(true) ?? "<none>",
                    emulatorAddress: memoryDescriptor?.GetProcessAddressListString() ?? "<none>");
            varInfo.Show();
        }

        public List<string> GetVarInfo()
        {
            var memoryDescriptor = (_view as IMemoryBasedVariableView)?.memoryDescriptor;
            return new List<string>()
            {
                _watchVarControl.VarName,
                GetClass(),
                memoryDescriptor?.GetTypeDescription() ?? "special",
                memoryDescriptor?.GetBaseTypeOffsetDescription(),
                memoryDescriptor?.GetBaseAddressListString(),
                memoryDescriptor?.GetProcessAddressListString(),
                memoryDescriptor?.GetRamAddressListString(true),
                memoryDescriptor?.GetProcessAddressListString(),
            };
        }

        public static List<string> GetVarInfoLabels()
        {
            return new List<string>()
            {
                "Name",
                "Class",
                "Type",
                "BaseType + Offset",
                "N64 Base Address",
                "Emulator Base Address",
                "N64 Address",
                "Emulator Address",
            };
        }

        public void ShowBitForm()
        {
            if (_view is IMemoryBasedVariableView compatible)
                new VariableBitForm(compatible.Name, compatible.memoryDescriptor, true).Show();
        }

        public abstract bool TrySetValue(string value);

        public virtual void SingleClick(Control parentCtrl, Rectangle bounds)
        {
        }

        public virtual void DoubleClick(Control parentCtrl, Rectangle bounds) => Edit(parentCtrl, bounds);

        public abstract void Edit(Control parent, Rectangle bounds);
        public abstract string GetClass();
        public abstract string GetValueText();
        public abstract void Update();

        public virtual void ToggleDisplay()
        {
        }

        protected void OnValueSet()
        {
            ValueSet();
            _view.ValueSet?.Invoke();
        }
    }

    public abstract class WatchVariableWrapper<TBackingValue> : WatchVariableWrapper
    {
        protected static Func<WatchVariableControl, object, bool> CreateBoolWithDefault<TWrapper>(
            Action<TWrapper, bool> setValue,
            Func<TWrapper, bool> getDefault
        ) where TWrapper : WatchVariableWrapper<TBackingValue> =>
            (ctrl, obj) =>
            {
                if (ctrl.WatchVarWrapper is TWrapper num)
                    if (obj is bool b)
                        setValue(num, b);
                    else if (obj == null)
                        setValue(num, getDefault(num));
                    else
                        return false;
                else
                    return false;
                return true;
            };

        protected static Func<WatchVariableControl, bool> WrapperProperty<TWrapper>(Func<TWrapper, bool> func) where TWrapper : WatchVariableWrapper<TBackingValue> =>
            (ctrl) =>
            {
                if (ctrl.WatchVarWrapper is TWrapper wrapper)
                    return func(wrapper);
                return false;
            };

        public IVariableView<TBackingValue> view => (IVariableView<TBackingValue>)_view;

        protected TBackingValue lastValue { get; private set; }
        protected CombinedValuesMeaning lastValueMeaning { get; private set; }

        private bool hasNextValue = false;
        private TBackingValue _nextValue;

        protected TBackingValue nextValue
        {
            get => _nextValue;
            set
            {
                hasNextValue = true;
                _nextValue = value;
            }
        }

        protected WatchVariableWrapper(IVariableView watchVar, WatchVariableControl watchVarControl)
            : base(watchVar, watchVarControl)
        {
        }

        public (CombinedValuesMeaning meaning, TBackingValue value) CombineValues()
        {
            var values = view._getterFunction().ToArray();
            if (values.Length == 0) return (CombinedValuesMeaning.NoValue, default(TBackingValue));
            TBackingValue firstValue = values[0];
            for (int i = 1; i < values.Length; i++)
                if (!Equals(values[i], firstValue))
                    return (CombinedValuesMeaning.MultipleValues, default(TBackingValue));
            return (CombinedValuesMeaning.SameValue, firstValue);
        }

        public override sealed void Update()
        {
            (lastValueMeaning, lastValue) = CombineValues();
            if (hasNextValue)
            {
                if (view._setterFunction(nextValue).Aggregate(true, (a, b) => a && b))
                    OnValueSet();
                hasNextValue = false;
            }

            UpdateControls();
        }

        public virtual void UpdateControls()
        {
        }

        public override void Edit(Control parent, Rectangle bounds)
        {
            if (editValueHandler != null)
                editValueHandler();
            else
            {
                textBox = new CarretlessTextBox();
                textBox.Bounds = bounds;
                textBox.Text = DisplayValue(lastValue);

                bool updateValue = true;
                textBox.Multiline = false;
                textBox.KeyDown += (_, e) =>
                {
                    updateValue = true;
                    if (e.KeyCode == Keys.Enter)
                        textBox.Parent.Focus();
                    else if (e.KeyCode == Keys.Escape)
                    {
                        updateValue = false;
                        textBox.Parent.Focus();
                    }
                };
                EventHandler HandleLostFocus = null;
                HandleLostFocus = (_, e) =>
                {
                    if (updateValue && TryParseValue(textBox.Text, out var nextValue))
                        this.nextValue = nextValue;
                    textBox.LostFocus -= HandleLostFocus;
                    textBox.Parent.Controls.Remove(textBox);
                    textBox.Dispose();
                };

                textBox.LostFocus += HandleLostFocus;
                parent.Controls.Add(textBox);
                textBox.Focus();
            }
        }

        public override sealed string GetValueText()
        {
            var combinedValues = CombineValues();
            switch (combinedValues.meaning)
            {
                case CombinedValuesMeaning.NoValue:
                    return "(none)";
                case CombinedValuesMeaning.SameValue:
                    return DisplayValue(combinedValues.value);
                case CombinedValuesMeaning.MultipleValues:
                    return "(multiple values)";
                default:
                    return "<invalid>";
            }
        }

        public override sealed bool TrySetValue(string value)
        {
            var success = TryParseValue(value, out var result);
            if (success)
            {
                view._setterFunction(result);
                OnValueSet();
            }

            return success;
        }

        public abstract bool TryParseValue(string value, out TBackingValue result);

        public abstract string DisplayValue(TBackingValue value);
    }
}
