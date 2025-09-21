using System;
using System.Collections.Generic;
using System.Linq;
using STROOP.Structs.Configurations;
using STROOP.Utilities;

namespace STROOP.Core.Variables
{
    public class DescribedMemoryState
    {
        public readonly MemoryDescriptor descriptor;

        public bool locked => HasLocks() != System.Windows.Forms.CheckState.Unchecked;
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

        public void ViewInMemoryTab()
        {
            List<uint> addressList = GetAddressList().ToList();
            if (addressList.Count == 0) return;
            uint address = addressList[0];
            var tab = AccessScope<StroopMainForm>.content.GetTab<Tabs.MemoryTab>();
            tab.UpdateOrInitialize(true);
            Config.TabControlMain.SelectedTab = tab.Tab;
            tab.SetCustomAddress(address);
            tab.UpdateHexDisplay();
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

        public System.Windows.Forms.CheckState HasLocks()
        {
            bool? firstLockValue = null;
            foreach (var addr in GetAddressList())
            {
                var v = locks.TryGetValue(addr, out _);
                if (firstLockValue == null)
                    firstLockValue = v;
                else if (v != firstLockValue)
                    return System.Windows.Forms.CheckState.Indeterminate;
            }

            if (!firstLockValue.HasValue)
                return System.Windows.Forms.CheckState.Unchecked;
            return firstLockValue.Value ? System.Windows.Forms.CheckState.Checked : System.Windows.Forms.CheckState.Unchecked;
        }
    }
}
