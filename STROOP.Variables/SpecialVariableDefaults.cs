namespace STROOP.Variables;

public static class SpecialVariableDefaults<T>
{
    public static readonly IVariable<T>.ValueGetter DEFAULT_GETTER = () => Array.Empty<T>();
    public static readonly IVariable<T>.ValueSetter DEFAULT_SETTER = _ => Array.Empty<bool>();
    public static readonly Func<T, uint, bool> DEFAULT_SETTER_WITH_ADDRESS = (_, _) => false;
}
