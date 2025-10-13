using System;
using System.Linq;
using STROOP.Variables;
using STROOP.Variables.Views;

namespace STROOP.Controls.VariablePanel
{
    class WatchVariableNullableWrapper<TBaseWrapper, TBackingType> : WatchVariableWrapper<TBackingType?>
        where TBaseWrapper : WatchVariableWrapper<TBackingType>
        where TBackingType : struct
    {
        private TBaseWrapper baseWrapper;

        public WatchVariableNullableWrapper(IVariableView<TBackingType?> var, WatchVariableControl control)
            : base(var, control)
        {
            var interfaceType = view.GetType().GetInterfaces().First(x => x.Name == $"{nameof(IVariableView)}`1");
            interfaceType = interfaceType.GetGenericTypeDefinition().MakeGenericType(interfaceType.GenericTypeArguments[0].GenericTypeArguments[0]);
            baseWrapper = (TBaseWrapper)
                typeof(TBaseWrapper)
                    .GetConstructor(new Type[] { interfaceType, typeof(WatchVariableControl) })
                    .Invoke(new object[]
                    {
                        new NamedVariableCollection.CustomView<TBackingType>(var.Subclass)
                        {
                            Name = view.Name,
                            _getterFunction = () => view._getterFunction().Select(x => x.HasValue ? x.Value : default(TBackingType)).ToArray(),
                            _setterFunction = value => view._setterFunction(value),
                        },
                        control
                    });
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
