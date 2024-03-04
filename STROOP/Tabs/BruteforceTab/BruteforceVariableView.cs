﻿using System;
using STROOP.Core.WatchVariables;
using STROOP.Utilities;

namespace STROOP.Tabs.BruteforceTab
{
    interface IBruteforceVariableView : NamedVariableCollection.IVariableView
    {
        object value { get; set; }
    }

    class BruteforceVariableView<T> : NamedVariableCollection.CustomView<T>, IBruteforceVariableView
    {
        private T _value;
        public T value
        {
            get => _value; set
            {
                _value = value;
                ValueSet?.Invoke();
            }
        }

        object IBruteforceVariableView.value
        {
            get => value; set
            {
                if (value is IConvertible convertible && typeof(IConvertible).IsAssignableFrom(typeof(T)))
                    this.value = (T)Convert.ChangeType(convertible, typeof(T));
            }
        }

        public BruteforceVariableView(string bruteforcerType, string name, T defaultValue = default(T))
            : base(BF_Utilities.BF_VariableUtilties.fallbackWrapperTypes[bruteforcerType])
        {
            Name = name;
            _value = defaultValue;
            _getterFunction = () => value.Yield();
            _setterFunction = value => { this.value = value; return true.Yield(); };
        }
    }
}
