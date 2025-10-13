namespace STROOP.Variables.Views;

public interface IVariableView
{
    Action ValueSet { get; set; }
    Action OnDelete { get; set; }
    string Name { get; }
    bool SetValueByKey(string key, object value);
    string GetValueByKey(string key);
    int DislpayPriority { get; }
    string Subclass { get; }
    public Type ClrType { get; }
}

public interface IVariableView<T> : IVariableView
{
    public delegate IEnumerable<T> ValueGetter();
    public delegate IEnumerable<bool> ValueSetter(T value);

    ValueGetter getter { get; }
    ValueSetter setter { get; }
}
