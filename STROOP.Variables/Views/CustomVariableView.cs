namespace STROOP.Variables.Views;

public class CustomVariableView : IVariableView
{
    public Action ValueSet { get; set; }
    public Action OnDelete { get; set; }
    public string Name { get; set; }
    public string Subclass { get; }
    public Type ClrType { get; }

    public string Color
    {
        set => SetValueByKey(CommonViewProperties.color, value);
    }

    public string Display
    {
        set => SetValueByKey(CommonViewProperties.display, value);
    }

    public int DislpayPriority { get; }

    private Dictionary<string, string> keyedValues = new Dictionary<string, string>();

    public CustomVariableView(string subclass, Type clrType)
    {
        Subclass = subclass;
        ClrType = clrType;
    }

    public virtual string GetValueByKey(string key)
    {
        if (keyedValues.TryGetValue(key, out string? result))
            return result;
        return null;
    }

    public virtual bool SetValueByKey(string key, object value)
    {
        keyedValues[key] = value.ToString();
        return true;
    }
}

public class CustomVariableView<T> : CustomVariableView, IVariableView<T>
{
    public IVariableView<T>.ValueGetter getter { get; set; }
    public IVariableView<T>.ValueSetter setter { get; set; }

    public CustomVariableView(string subclass) : base(subclass, typeof(T))
    {
        getter = SpecialVariableDefaults<T>.DEFAULT_GETTER;
        setter = SpecialVariableDefaults<T>.DEFAULT_SETTER;
    }
}
