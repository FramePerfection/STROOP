namespace STROOP.Variables
{
    public static class SpecialVariableDefaults<T>
    {
        public readonly static NamedVariableCollection.GetterFunction<T> DEFAULT_GETTER = () => Array.Empty<T>();
        public readonly static NamedVariableCollection.SetterFunction<T> DEFAULT_SETTER = _ => Array.Empty<bool>();
        public readonly static Func<T, uint, bool> DEFAULT_SETTER_WITH_ADDRESS = (_, _) => false;
    }
}
