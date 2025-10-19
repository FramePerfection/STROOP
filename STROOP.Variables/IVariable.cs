namespace STROOP.Variables;

public interface IVariable
{
    /// <summary>
    /// Invoked when any value was successfully set on this <see cref="IVariable"/>.
    /// </summary>
    Action ValueSet { get; set; }

    /// <summary>
    /// Invoked when this <see cref="IVariable"/> becomes meaningless.
    /// </summary>
    Action OnDelete { get; set; }

    /// <summary>
    /// The name by which this <see cref="IVariable"/> should be displayed to the user.
    /// <para> This should be moved to the UI layer eventually, specifically by facilitating <see cref="GetValueByKey"/>. </para>
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Associates an arbitrary key with an arbitrary value on this <see cref="IVariable"/>.
    /// </summary>
    /// <param name="key"> The key to associate the value with. </param>
    /// <param name="value"> The value to associate. </param>
    /// <returns> True if the association could be made. False otherwise. </returns>
    bool SetValueByKey(string key, object value);

    /// <summary>
    /// LOL! Look at <see cref="SetValueByKey"/>, this does not match!
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    string GetValueByKey(string key);
    int DisplayPriority { get; }
    string Subclass { get; }

    /// <summary>
    /// The CLR (Common Language Runtime) Type by which to determine how to handle values .NET values interfacing with this variable.
    /// </summary>
    public Type ClrType { get; }
}

public interface IVariable<T> : IVariable
{
    public delegate IEnumerable<T> ValueGetter();
    public delegate IEnumerable<bool> ValueSetter(T value);

    ValueGetter getter { get; }
    ValueSetter setter { get; }
}
