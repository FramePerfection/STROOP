namespace STROOP.Variables
{
    public class DescribedMemoryState
    {
        public readonly MemoryDescriptor descriptor;

        public bool locked => HasLocks() != false;
        Dictionary<uint, object> locks = new Dictionary<uint, object>();

        private Func<IEnumerable<uint>> _fixedAddressGetter = null;
        public bool fixedAddresses => _fixedAddressGetter != null;

        public DescribedMemoryState(MemoryDescriptor memoryDescriptor)
        {
            this.descriptor = memoryDescriptor;
        }

        public IEnumerable<uint> GetAddressList()
            => _fixedAddressGetter?.Invoke() ?? descriptor.GetBaseAddressList().Select(x => x + descriptor.Offset);

        public void ToggleFixedAddress(bool? fix)
        {
            bool doFix = fix ?? _fixedAddressGetter == null;
            _fixedAddressGetter = null;
            if (doFix)
            {
                var capture = GetAddressList();
                _fixedAddressGetter = () => capture;
            }
        }

        public void ToggleLocked(bool? locked)
        {
            // TODO: work out locking feature
        }

        public bool SetLocked(bool locked, List<uint> addresses)
        {
            // TODO: work out locking feature
            //var addressList = addresses ?? GetAddressList(null);
            //if (!locked)
            //    foreach (var address in addressList)
            //        locks.Remove(address);
            //else
            //{
            //    WatchVariableLockManager.AddLocks(this);
            //    if (view is IVariableView<T> compatibleView)
            //    {
            //        var setter = compatibleView._setterFunction;
            //        foreach (var address in addressList)
            //            locks[address] = new Wrapper<(SetterFunction<object> setter, object value)>(((SetterFunction<Wrapper<object>>)setter, compatibleView._getterFunction(new[] { address }).FirstOrDefault()));
            //    }
            //}
            return false;
        }

        public void ClearLocks() => locks.Clear();

        public bool InvokeLocks()
        {
            return false;

            if (locks.Count == 0)
                return false;
            //foreach (var l in locks)
            //    l.Value.value.setter(l.Value.value.value, l.Key);
            return true;
        }

        public bool? HasLocks()
        {
            bool? firstLockValue = null;
            foreach (var addr in GetAddressList())
            {
                var v = locks.TryGetValue(addr, out _);
                if (firstLockValue == null)
                    firstLockValue = v;
                else if (v != firstLockValue)
                    return null;
            }

            return firstLockValue ?? false;
        }
    }
}
