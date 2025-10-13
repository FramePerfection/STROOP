using STROOP.Core.Utilities;
using STROOP.Utilities;
using STROOP.Variables.Views;

namespace STROOP.Variables;

public class WatchVariableSpecialDictionary
{
    private readonly Dictionary<string, IVariableView> _dictionary;

    public WatchVariableSpecialDictionary() => _dictionary = new Dictionary<string, IVariableView>();

    public bool TryGetValue(string key, out IVariableView getterSetter)
        => _dictionary.TryGetValue(key, out getterSetter);

    public void Add<T>(string key, GetterFunction<T> getter, SetterFunction<T> setter, string? subclass = null)
    {
        _dictionary[key] = new NamedVariableCollection.CustomView<T>(subclass.DefaultIfNull<T>())
        {
            Name = key,
            _getterFunction = getter,
            _setterFunction = setter,
        };
    }

    public void Add<T>(string key, Func<T> getter, Func<T, bool> setter, string? subclass = null)
        => Add(key, () => getter().Yield(), value => setter(value).Yield(), subclass);

    public void Add<T>(string key, string baseAddressType, Func<uint, T> getter, Func<T, uint, bool> setter, string? subclass = null)
        => Add(key,
            () => WatchVariableUtilities.GetBaseAddresses(baseAddressType).Select(x => getter(x)),
            value => WatchVariableUtilities.GetBaseAddresses(baseAddressType).Select(x => setter(value, x)),
            subclass
        );

    public void Add<T>(string key, Func<T> getter, SetterFunction<T> setter, string? subclass = null)
    {
        _dictionary[key] = new NamedVariableCollection.CustomView<T>(subclass.DefaultIfNull<T>())
        {
            Name = key,
            _getterFunction = () => getter().Yield(),
            _setterFunction = setter,
        };
    }
}
