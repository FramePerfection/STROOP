using STROOP.Core;
using STROOP.Core.Utilities;

namespace STROOP.Variables;

public interface IMemoryVariable : IVariable
{
    MemoryDescriptor memoryDescriptor { get; }
    DescribedMemoryState describedMemoryState { get; }
}

public class MemoryVariable<T>
    : CustomVariable, IMemoryVariable, IVariable<T>
    where T : struct, IConvertible
{
    public MemoryDescriptor memoryDescriptor { get; }
    public DescribedMemoryState describedMemoryState { get; }

    public MemoryVariable(string subclass, MemoryDescriptor memoryDescriptor)
        : base(subclass, memoryDescriptor.ClrType)
    {
        this.memoryDescriptor = memoryDescriptor;
        describedMemoryState = new DescribedMemoryState(memoryDescriptor);
        getter = () => GetValues(describedMemoryState);
        setter = (T value) => SetAll(describedMemoryState, value);
    }

    public IVariable<T>.ValueGetter getter { get; private set; }
    public IVariable<T>.ValueSetter setter { get; private set; }

    private static IEnumerable<T> GetValues(DescribedMemoryState memoryState)
        => memoryState.GetAddressList().ConvertAll(address => (T)ProcessStream.Instance.GetValue(
            typeof(T),
            address,
            false,
            memoryState.descriptor.Mask,
            memoryState.descriptor.Shift
        ));

    private static IEnumerable<bool> SetAll(DescribedMemoryState memoryState, T value)
        => Static._memoryWriters.TryGetValue(typeof(T), out Static.MemoryWriter writer)
            ? memoryState.GetAddressList().Select(address => writer(value, address, memoryState.descriptor)).ToArray()
            : [false];
}

file static class Static
{
    internal delegate bool MemoryWriter(object value, uint address, MemoryDescriptor memoryDescriptor);
    internal static readonly Dictionary<Type, MemoryWriter> _memoryWriters = new Dictionary<Type, MemoryWriter>();

    static Static()
    {
        _memoryWriters[typeof(ulong)] = (value, address, descriptor)
            => ProcessStream.Instance.SetValue((ulong)value, address, false, descriptor.Mask, descriptor.Shift);
        _memoryWriters[typeof(uint)] = (value, address, descriptor)
            => ProcessStream.Instance.SetValue((uint)value, address, false, descriptor.Mask, descriptor.Shift);
        _memoryWriters[typeof(ushort)] = (value, address, descriptor)
            => ProcessStream.Instance.SetValue((ushort)value, address, false, descriptor.Mask, descriptor.Shift);
        _memoryWriters[typeof(byte)] = (value, address, descriptor)
            => ProcessStream.Instance.SetValue((byte)value, address, false, descriptor.Mask, descriptor.Shift);

        _memoryWriters[typeof(long)] = (value, address, descriptor)
            => ProcessStream.Instance.SetValue((ulong)value, address, false, descriptor.Mask, descriptor.Shift);
        _memoryWriters[typeof(int)] = (value, address, descriptor)
            => ProcessStream.Instance.SetValue((uint)value, address, false, descriptor.Mask, descriptor.Shift);
        _memoryWriters[typeof(short)] = (value, address, descriptor)
            => ProcessStream.Instance.SetValue((ushort)value, address, false, descriptor.Mask, descriptor.Shift);
        _memoryWriters[typeof(sbyte)] = (value, address, descriptor)
            => ProcessStream.Instance.SetValue((byte)value, address, false, descriptor.Mask, descriptor.Shift);

        _memoryWriters[typeof(double)] = (value, address, descriptor)
            => ProcessStream.Instance.SetValue((double)value, address, false, descriptor.Mask, descriptor.Shift);
        _memoryWriters[typeof(float)] = (value, address, descriptor)
            => ProcessStream.Instance.SetValue((float)value, address, false, descriptor.Mask, descriptor.Shift);
    }
}
