namespace STROOP.Variables;

public class DescribedMemoryState
{
    public readonly MemoryDescriptor descriptor;

    private Func<IEnumerable<uint>> _fixedAddressGetter = null;
    public bool fixedAddresses => _fixedAddressGetter != null;

    public DescribedMemoryState(MemoryDescriptor memoryDescriptor) => descriptor = memoryDescriptor;

    public IEnumerable<uint> GetAddressList()
        => _fixedAddressGetter?.Invoke() ?? descriptor.GetBaseAddressList().Select(x => x + descriptor.Offset);

    public void ToggleFixedAddress(bool? fix)
    {
        bool doFix = fix ?? _fixedAddressGetter == null;
        _fixedAddressGetter = null;
        if (doFix)
        {
            IEnumerable<uint> capture = GetAddressList();
            _fixedAddressGetter = () => capture;
        }
    }
}
