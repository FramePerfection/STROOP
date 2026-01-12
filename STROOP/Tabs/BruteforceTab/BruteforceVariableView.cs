using STROOP.Core.Utilities;
using System;
using STROOP.Core.Variables;
using STROOP.Utilities;
using STROOP.Variables;

namespace STROOP.Tabs.BruteforceTab
{
    interface IBruteforceVariableView : NamedVariableCollection.IView
    {
        object value { get; set; }
    }

    class BruteforceVariableView<T> : NamedVariableCollection.CustomView<T>, IBruteforceVariableView
    {
        private T _value;

        public T value
        {
            get => _value;
            set
            {
                _value = value;
                ValueSet?.Invoke();
            }
        }

        object IBruteforceVariableView.value
        {
            get => value;
            set
            {
                if (value is IConvertible convertible && typeof(IConvertible).IsAssignableFrom(typeof(T)))
                    this.value = (T)Convert.ChangeType(convertible, typeof(T));
            }
        }

        public BruteforceVariableView(string bruteforcerType, string name, T defaultValue = default(T))
            : this(BF_Utilities.BF_VariableUtilties.fallbackWrapperTypes[bruteforcerType], name, defaultValue)
        {
        }

        public BruteforceVariableView(WatchVariableSubclass subclass, string name, T defaultValue = default(T))
            : base(subclass)
        {
            Name = name;
            _value = defaultValue;
            _getterFunction = () => value.Yield();
            _setterFunction = value =>
            {
                this.value = value;
                return true.Yield();
            };
        }
    }
}
