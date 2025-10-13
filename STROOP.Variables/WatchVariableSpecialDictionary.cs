using STROOP.Core.Utilities;
using STROOP.Variables.Views;

namespace STROOP.Variables;

public class WatchVariableSpecialDictionary
{
    private readonly Dictionary<string, IVariableView> _dictionary;

    public WatchVariableSpecialDictionary() => _dictionary = new Dictionary<string, IVariableView>();

    public bool TryGetValue(string key, out IVariableView getterSetter)
        => _dictionary.TryGetValue(key, out getterSetter);

    public void Add<T>(string key, IVariableView<T>.ValueGetter getter, IVariableView<T>.ValueSetter setter, string? subclass = null)
    {
        _dictionary[key] = new CustomVariableView<T>(subclass.DefaultIfNull<T>())
        {
            Name = key,
            getter = getter,
            setter = setter,
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

    public void Add<T>(string key, Func<T> getter, IVariableView<T>.ValueSetter setter, string? subclass = null)
    {
        _dictionary[key] = new CustomVariableView<T>(subclass.DefaultIfNull<T>())
        {
            Name = key,
            getter = () => getter().Yield(),
            setter = setter,
        };
    }
}
