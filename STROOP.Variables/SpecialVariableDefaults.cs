namespace STROOP.Variables;

public static class SpecialVariableDefaults<T>
{
    public static readonly NamedVariableCollection.GetterFunction<T> DEFAULT_GETTER = () => Array.Empty<T>();
    public static readonly NamedVariableCollection.SetterFunction<T> DEFAULT_SETTER = _ => Array.Empty<bool>();
    public static readonly Func<T, uint, bool> DEFAULT_SETTER_WITH_ADDRESS = (_, _) => false;
}
