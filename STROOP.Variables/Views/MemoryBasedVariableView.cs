using STROOP.Core;
using STROOP.Core.Utilities;

namespace STROOP.Variables.Views;

public interface IMemoryBasedVariableView : IVariableView
{
    string Name { get; set; }
    MemoryDescriptor memoryDescriptor { get; }
    DescribedMemoryState describedMemoryState { get; }
}

public class MemoryDescriptorView : CustomVariableView, IMemoryBasedVariableView
{
    public MemoryDescriptor memoryDescriptor { get; }
    public DescribedMemoryState describedMemoryState { get; }

    public MemoryDescriptorView(string subclass, MemoryDescriptor memoryDescriptor)
        : base(subclass, memoryDescriptor.ClrType)
    {
        this.memoryDescriptor = memoryDescriptor;
        describedMemoryState = new DescribedMemoryState(memoryDescriptor);
    }
}

public class MemoryBasedVariableView<T> : MemoryDescriptorView, IVariableView<T> where T : struct, IConvertible
{
    public MemoryBasedVariableView(string subclass, MemoryDescriptor memoryDescriptor)
        : base(subclass, memoryDescriptor)
    {
        getter = () => GetValues(describedMemoryState);
        setter = (T value) => SetAll(describedMemoryState, value);
    }

    public IVariableView<T>.ValueGetter getter { get; private set; }
    public IVariableView<T>.ValueSetter setter { get; private set; }

    private static IEnumerable<T> GetValues(DescribedMemoryState memoryState)
        => memoryState.GetAddressList().ConvertAll(address => (T)ProcessStream.Instance.GetValue(
            typeof(T),
            address,
            false,
            memoryState.descriptor.Mask,
            memoryState.descriptor.Shift
        ));

    private static IEnumerable<bool> SetAll(DescribedMemoryState memoryState, T value)
        => memoryState.GetAddressList().Select(address => NamedVariableCollection.SetVariableValue(
            ProcessStream.Instance,
            typeof(T),
            value,
            address,
            memoryState.descriptor.Mask,
            memoryState.descriptor.Shift
        )).ToArray();
}
