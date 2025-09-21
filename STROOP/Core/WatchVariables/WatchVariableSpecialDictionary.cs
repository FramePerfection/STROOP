using System;
using System.Collections.Generic;
using System.Linq;
using STROOP.Utilities;

namespace STROOP.Core.Variables
{
    public class WatchVariableSpecialDictionary
    {
        private readonly Dictionary<string, NamedVariableCollection.IView> _dictionary;

        public WatchVariableSpecialDictionary()
        {
            _dictionary = new Dictionary<string, NamedVariableCollection.IView>();
        }

        public bool TryGetValue(string key, out NamedVariableCollection.IView getterSetter)
            => _dictionary.TryGetValue(key, out getterSetter);

        public void Add<T>(string key, NamedVariableCollection.GetterFunction<T> getter, NamedVariableCollection.SetterFunction<T> setter, Type wrapperType = null)
        {
            _dictionary[key] = new NamedVariableCollection.CustomView<T>(wrapperType ?? WatchVariableUtilities.GetWrapperType(typeof(T)))
            {
                Name = key,
                _getterFunction = getter,
                _setterFunction = setter,
            };
        }

        public void Add<T>(string key, Func<T> getter, Func<T, bool> setter, Type wrapperType = null)
            => Add(key, () => getter().Yield(), value => setter(value).Yield(), wrapperType);

        public void Add<T>(string key, string baseAddressType, Func<uint, T> getter, Func<T, uint, bool> setter, Type wrapperType = null)
            => Add(key,
                () => WatchVariableUtilities.GetBaseAddresses(baseAddressType).Select(x => getter(x)),
                value => WatchVariableUtilities.GetBaseAddresses(baseAddressType).Select(x => setter(value, x)),
                wrapperType
            );

        public void Add<T>(string key, Func<T> getter, NamedVariableCollection.SetterFunction<T> setter, Type wrapperType = null)
        {
            _dictionary[key] = new NamedVariableCollection.CustomView<T>(wrapperType ?? WatchVariableUtilities.GetWrapperType(typeof(T)))
            {
                Name = key,
                _getterFunction = () => getter().Yield(),
                _setterFunction = setter,
            };
        }
    }
}
