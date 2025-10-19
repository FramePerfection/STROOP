namespace STROOP.Variables;

public class CustomVariable : IVariable
{
    public Action ValueSet { get; set; }
    public Action OnDelete { get; set; }
    public string Name { get; set; }
    public string Subclass { get; }
    public Type ClrType { get; }

    public string Color
    {
        set => SetValueByKey(CommonVariableProperties.color, value);
    }

    public string Display
    {
        set => SetValueByKey(CommonVariableProperties.display, value);
    }

    public int DisplayPriority { get; }

    private Dictionary<string, string> keyedValues = new Dictionary<string, string>();

    public CustomVariable(string subclass, Type clrType)
    {
        Subclass = subclass;
        ClrType = clrType;
    }

    public virtual string GetValueByKey(string key)
        => keyedValues.GetValueOrDefault(key);

    public virtual bool SetValueByKey(string key, string value)
    {
        keyedValues[key] = value;
        return true;
    }
}

public class CustomVariable<T> : CustomVariable, IVariable<T>
{
    public IVariable<T>.ValueGetter getter { get; set; }
    public IVariable<T>.ValueSetter setter { get; set; }

    public CustomVariable(string subclass) : base(subclass, typeof(T))
    {
        getter = SpecialVariableDefaults<T>.DEFAULT_GETTER;
        setter = SpecialVariableDefaults<T>.DEFAULT_SETTER;
    }
}
