using STROOP.Variables.Formatting;
using STROOP.Variables.Utilities;

namespace STROOP.Variables.VariablePanel
{
    public interface IUiContext
    {
        IValueEditBox CreateValueBox(string lastValue);
    }

    public interface IValueEditBox
    {
        Action<string> Accept { get; set; }
        Action Cancel { get; set; }
        void Dispose();
    }

    public interface IVariableCellUi<TUiContext>
        : IVariableCell
        where TUiContext : IUiContext
    {
        public delegate void CustomDraw(TUiContext context);

        public CustomDraw CustomDrawOperation { get; }

        VariableCellControl<TUiContext> control { get; }

        void SingleClick(TUiContext uiContext);
        void DoubleClick(TUiContext uiContext);
    }

    public interface IVariableCell
    {
        void Update();
        string GetValueText();
        string GetClass();
        bool TrySetValue(string value);
        DescribedMemoryState memory { get ; }
        List<string> GetVarInfo();
    }

    public interface IVariableCellData<out TValue>
        : IVariableCell
    {
        TValue[] GetValues();
    }

    public abstract class VariableCell<TUiContext, TBackingValue>(IVariable<TBackingValue> view, VariableCellControl<TUiContext> cellControl)
        : IVariableCell, IVariableCellData<TBackingValue>, IVariableCellUi<TUiContext>
        where TUiContext : IUiContext
    {
        protected static Func<VariableCellControl<TUiContext>, object, bool> CreateBoolWithDefault<TWrapper>(
            Action<TWrapper, bool> setValue,
            Func<TWrapper, bool> getDefault
        ) where TWrapper : VariableCell<TUiContext, TBackingValue> =>
            (ctrl, obj) =>
            {
                if (ctrl.varCellInternal is TWrapper num)
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

        protected static Func<VariableCellControl<TUiContext>, bool> WrapperProperty<TWrapper>(Func<TWrapper, bool> func)
            where TWrapper : VariableCell<TUiContext, TBackingValue> =>
            (ctrl) =>
            {
                if (ctrl.varCellInternal is TWrapper wrapper)
                    return func(wrapper);
                return false;
            };


        protected readonly IVariable<TBackingValue> view = view;

        protected Action editValueHandler;
        protected IValueEditBox TextEditBox;
        protected TBackingValue lastValue { get; private set; }
        protected CombinedValuesMeaning lastValueMeaning { get; private set; }

        public VariableCellControl<TUiContext> control => cellControl;
        public DescribedMemoryState memory => (view as IMemoryVariable)?.describedMemoryState;
        public virtual IVariableCellUi<TUiContext>.CustomDraw CustomDrawOperation => null;


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

        public abstract string GetClass();
        public virtual void SingleClick(TUiContext uiContext) { }
        public virtual void DoubleClick(TUiContext uiContext) => Edit(uiContext);

        protected void OnValueSet() => view.ValueSet?.Invoke();

        public TBackingValue[] GetValues() => view.getter().ToArray();

        public void Update()
        {
            (lastValueMeaning, lastValue) = this.CombineValues();
            if (hasNextValue)
            {
                if (view.setter(nextValue).Aggregate(true, (a, b) => a && b))
                    OnValueSet();
                hasNextValue = false;
            }

            UpdateControls();
            control.Update();
        }

        public virtual void UpdateControls() { }

        public virtual void Edit(TUiContext uiContext)
        {
            if (editValueHandler != null)
                editValueHandler();
            else
            {
                TextEditBox = uiContext.CreateValueBox(DisplayValue(lastValue));
                TextEditBox.Accept += text =>
                {
                    if (TryParseValue(text, out var nextValue))
                        this.nextValue = nextValue;
                    TextEditBox.Dispose();
                };
                TextEditBox.Cancel += TextEditBox.Dispose;
            }
        }

        public string GetValueText()
        {
            var combinedValues = this.CombineValues();
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

        public bool TrySetValue(string value)
        {
            var success = TryParseValue(value, out var result);
            if (success)
            {
                view.setter(result);
                OnValueSet();
            }

            return success;
        }

        public abstract bool TryParseValue(string value, out TBackingValue result);

        public abstract string DisplayValue(TBackingValue value);

        public List<string> GetVarInfo()
        {
            var memoryDescriptor = (view as IMemoryVariable)?.memoryDescriptor;
            return
            [
                control.VarName,
                GetClass(),
                memoryDescriptor?.GetTypeDescription() ?? "special",
                memoryDescriptor?.GetBaseTypeOffsetDescription(),
                memoryDescriptor?.GetBaseAddressListString(),
                memoryDescriptor?.GetProcessAddressListString(),
                memoryDescriptor?.GetRamAddressListString(true),
                memoryDescriptor?.GetProcessAddressListString(),
            ];
        }
    }
}
