namespace STROOP.Variables;

/// <summary>
/// Represents a single semantic of information that can be read, and optionally set, implemented via the type-safe <see cref="IVariable{T}"/>.
/// <para> Note that a single <see cref="IVariable"/> may yield many (or zero) values when being read depending on its semantic. </para>
/// <para> Classes must not implement <see cref="IVariable"/> directly. Implement <see cref="IVariable{T}"/>. </para>
/// </summary>
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
    /// Associates an arbitrary key with an arbitrary value on this <see cref="IVariable"/>.
    /// </summary>
    /// <param name="key"> The key to associate the value with. </param>
    /// <param name="value"> The value to associate. </param>
    /// <returns> True if the association could be made. False otherwise. </returns>
    bool SetValueByKey(string key, string value);

    /// <summary>
    /// Retrieves the value associated with this an arbitrary key on this <see cref="IVariable"/>.
    /// </summary>
    /// <param name="key"> The key. </param>
    /// <returns> The associated value, or null if no value is associated for the provided key. </returns>
    string GetValueByKey(string key);

    int DisplayPriority { get; }
    string Subclass { get; }

    /// <summary>
    /// The CLR (Common Language Runtime) Type by which to determine how to handle values .NET values interfacing with this variable.
    /// </summary>
    public Type ClrType { get; }
}

/// <summary>
/// Represents a single semantic of information with type <typeparamref name="T"/> that can be read, and optionally set.
/// <para> Note that a single <see cref="IVariable{T}"/> may yield many (or zero) values when being read depending on its semantic. </para>
/// </summary>
/// <typeparam name="T"> The type of information being retrieved and stored in this <see cref="IVariable{T}"/>. </typeparam>
public interface IVariable<T> : IVariable
{
    /// <summary>
    /// Retrieves all values that this <see cref="IVariable{T}"/> currently represents.
    /// </summary>
    public delegate IEnumerable<T> ValueGetter();

    /// <summary>
    /// Attempts to set all underlying values that this <see cref="IVariable{T}"/> currently represents to <paramref name="value"/>.
    /// </summary>
    /// <returns> An ordered enumerable of booleans, where each value indicates whether the respective underlying value was set. </returns>
    public delegate IEnumerable<bool> ValueSetter(T value);

    ValueGetter getter { get; }
    ValueSetter setter { get; }
}
