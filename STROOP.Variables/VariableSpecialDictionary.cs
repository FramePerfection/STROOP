using STROOP.Core.Utilities;
using STROOP.Variables.Utilities;

namespace STROOP.Variables;

public class VariableSpecialDictionary
{
    private readonly Dictionary<string, IVariable> _dictionary;

    public VariableSpecialDictionary() => _dictionary = new Dictionary<string, IVariable>();

    public bool TryGetValue(string key, out IVariable getterSetter)
        => _dictionary.TryGetValue(key, out getterSetter);

    public void Add<T>(string key, IVariable<T>.ValueGetter getter, IVariable<T>.ValueSetter setter, string? subclass = null)
    {
        _dictionary[key] = new CustomVariable<T>(subclass.DefaultIfNull<T>())
        {
            getter = getter,
            setter = setter,
        };
    }

    public void Add<T>(string key, Func<T> getter, Func<T, bool> setter, string? subclass = null)
        => Add(key, () => getter().Yield(), value => setter(value).Yield(), subclass);

    public void Add<T>(string key, string baseAddressType, Func<uint, T> getter, Func<T, uint, bool> setter, string? subclass = null)
        => Add(key,
            () => VariableUtilities.GetBaseAddresses(baseAddressType).Select(x => getter(x)),
            value => VariableUtilities.GetBaseAddresses(baseAddressType).Select(x => setter(value, x)),
            subclass
        );

    public void Add<T>(string key, Func<T> getter, IVariable<T>.ValueSetter setter, string? subclass = null)
    {
        _dictionary[key] = new CustomVariable<T>(subclass.DefaultIfNull<T>())
        {
            getter = () => getter().Yield(),
            setter = setter,
        };
    }
}
