using STROOP.Structs.Configurations;
using STROOP.Variables;
using System.Collections.Generic;

namespace STROOP.Structs
{
    public static class WatchVariableLockManager
    {
        private static HashSet<NamedVariableCollection.IView> _lockList = new HashSet<NamedVariableCollection.IView>();

        public static void AddLocks(NamedVariableCollection.IView variable)
        {
            _lockList.Add(variable);
        }

        public static void RemoveAllLocks()
        {
            // TODO: work out locking feature
            //foreach (var l in _lockList)
            //    l.ClearLocks();
            //_lockList.Clear();
        }

        public static void Update()
        {
            // TODO: work out locking feature
            //if (LockConfig.LockingDisabled || _lockList.Count == 0) return;
            //using (Config.Stream.Suspend())
            //{
            //    var removeList = new List<NamedVariableCollection.IVariableView>();
            //    foreach (var varLock in _lockList)
            //    {
            //        if (!varLock.InvokeLocks())
            //            removeList.Add(varLock);
            //    }
            //    foreach (var remove in removeList)
            //        _lockList.Remove(remove);
            //}
        }

        public static bool ContainsAnyLocksForObject(uint baseAddress)
        {
            return false;
        }
    };
}
