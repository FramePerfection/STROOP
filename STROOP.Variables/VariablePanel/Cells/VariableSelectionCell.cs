namespace STROOP.Variables.VariablePanel.Cells
{
    public abstract class VariableSelectionCell<TUiContext, TBaseWrapper, TBackingValue>
        : VariableCell<TUiContext, TBackingValue>
        where TBaseWrapper : VariableCell<TUiContext, TBackingValue>
        where TUiContext : IUiContext
    {
        public bool DisplaySingleOption = false;

        public List<(string name, Func<TBackingValue> func)> options = new List<(string, Func<TBackingValue>)>();

        protected readonly TBaseWrapper baseWrapper;
        protected bool isSingleOption => DisplaySingleOption && options.Count == 1;

        (string name, Func<TBackingValue> getter) selectedOption;

        public VariableSelectionCell(IVariable<TBackingValue> var, VariableCellControl<TUiContext> cellControl) : base(var, cellControl)
        {
            var interfaceType = view.GetType().GetInterfaces().First(x => x.Name == $"{nameof(IVariable)}`1");
            baseWrapper = (TBaseWrapper)
                typeof(TBaseWrapper)
                    .GetConstructor(new Type[] { interfaceType, typeof(VariableCellControl<>).MakeGenericType(typeof(TUiContext)) })
                    .Invoke(new object[] { view, cellControl });
            view.ValueSet += () => selectedOption = (null, null);
        }

        public override string GetClass() => $"Selection for {baseWrapper.GetClass()}";

        public override void UpdateControls() => baseWrapper.UpdateControls();

        protected void SetOption((string name, Func<TBackingValue> func) option)
        {
            view.setter(option.func());
            selectedOption = option;
        }

        public void SelectOption(int index) => SetOption(options[index]);

        public void UpdateOption(int index)
        {
            if (selectedOption.Equals(options[index]))
            {
                view.setter(selectedOption.getter());
                selectedOption = options[index];
            }
        }

        public override string DisplayValue(TBackingValue value) => baseWrapper.DisplayValue(value);

        public override bool TryParseValue(string value, out TBackingValue result) => baseWrapper.TryParseValue(value, out result);
    }
}
