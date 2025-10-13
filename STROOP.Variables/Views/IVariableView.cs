namespace STROOP.Variables.Views;


public delegate IEnumerable<T> GetterFunction<out T>();

public delegate IEnumerable<bool> SetterFunction<T>(T value);

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
    GetterFunction<T> _getterFunction { get; }
    SetterFunction<T> _setterFunction { get; }
}
