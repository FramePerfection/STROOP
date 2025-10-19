namespace STROOP.Variables.VariablePanel.Cells
{
    class VariableNullableCell<TUiContext, TBaseWrapper, TBackingType>
        : VariableCell<TUiContext, TBackingType?>
        where TUiContext : IUiContext
        where TBaseWrapper : VariableCell<TUiContext, TBackingType>
        where TBackingType : struct
    {
        private TBaseWrapper baseWrapper;

        public VariableNullableCell(IVariable<TBackingType?> var, VariableCellControl<TUiContext> cellControl)
            : base(var, cellControl)
        {
            var interfaceType = view.GetType().GetInterfaces().First(x => x.Name == $"{nameof(IVariable)}`1");
            interfaceType = interfaceType.GetGenericTypeDefinition().MakeGenericType(interfaceType.GenericTypeArguments[0].GenericTypeArguments[0]);
            baseWrapper = (TBaseWrapper)
                typeof(TBaseWrapper)
                    .GetConstructor([ interfaceType, typeof(VariableCellControl<TUiContext>) ])
                    ?.Invoke([
                        new CustomVariable<TBackingType>(var.Subclass)
                        {
                            Name = view.Name,
                            getter = () => view.getter().Select(x => x.HasValue ? x.Value : default(TBackingType)).ToArray(),
                            setter = value => view.setter(value),
                        },
                        cellControl,
                    ]);
        }

        public override sealed bool TryParseValue(string value, out TBackingType? result)
        {
            if (!baseWrapper.TryParseValue(value, out var baseResult))
            {
                result = null;
                return false;
            }

            result = baseResult;
            return true;
        }

        public override sealed string DisplayValue(TBackingType? value)
        {
            if (!value.HasValue)
                return "<null>";
            return baseWrapper.DisplayValue(value.Value);
        }

        public override sealed void UpdateControls() => baseWrapper.UpdateControls();

        public override sealed string GetClass() => $"nullable {baseWrapper.GetClass()}";
    }
}
