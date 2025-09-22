using STROOP.Core;
using STROOP.Structs;
using STROOP.Variables.SM64MemoryLayout;
using STROOP.Variables.Utilities;

namespace STROOP.Variables
{
    public static class IVariableViewExtensions
    {
        public static NamedVariableCollection.IView<T> WithKeyedValue<T, TValue>(this NamedVariableCollection.IView<T> view, string key, TValue value)
        {
            view.SetValueByKey(key, value);
            return view;
        }

        public static string GetJsonName(this NamedVariableCollection.IView view)
        {
            var explicitJsonName = view.GetValueByKey("jsonName");
            if (explicitJsonName == "")
                return null;
            if (explicitJsonName != null)
                return explicitJsonName;
            return $"{view.Name}".Replace(' ', '_').ToLower();
        }

        public static IEnumerable<T> GetNumberValues<T>(this NamedVariableCollection.IView view) where T : struct, IConvertible
        {
            if (view.TryGetNumberValues<T>(out var result))
                return result;
            throw new InvalidOperationException($"'{view.GetType().FullName}' is not a vaild number type.");
        }

        public static bool TryGetNumberValues<T>(this NamedVariableCollection.IView view, out IEnumerable<T> result)
            where T : struct, IConvertible
        {
            bool Get<Q>(out IEnumerable<T> innerResult)
            {
                innerResult = null;
                if (view is NamedVariableCollection.IView<Q> qView)
                {
                    innerResult = qView._getterFunction().Select(x => (T)Convert.ChangeType(x, typeof(T)));
                    return true;
                }

                return false;
            }

            return Get<byte>(out result)
                   || Get<sbyte>(out result)
                   || Get<ushort>(out result)
                   || Get<short>(out result)
                   || Get<uint>(out result)
                   || Get<int>(out result)
                   || Get<ulong>(out result)
                   || Get<long>(out result)
                   || Get<float>(out result)
                   || Get<double>(out result)
                ;
        }

        public static IEnumerable<bool> TrySetValue<T>(this NamedVariableCollection.IView view, T value) where T : IConvertible
        {
            IEnumerable<bool> Set<Q>()
            {
                Q convertedValue = default(Q);
                try
                {
                    convertedValue = (Q)Convert.ChangeType(value, typeof(Q));
                }
                catch (Exception)
                {
                    return null;
                }

                if (view is NamedVariableCollection.IView<Q> qView)
                    return qView._setterFunction(convertedValue);
                return null;
            }

            return Set<byte>()
                   ?? Set<sbyte>()
                   ?? Set<ushort>()
                   ?? Set<short>()
                   ?? Set<uint>()
                   ?? Set<int>()
                   ?? Set<ulong>()
                   ?? Set<long>()
                   ?? Set<float>()
                   ?? Set<double>()
                   ?? Array.Empty<bool>()
                ;
        }

        public static (CombinedValuesMeaning meaning, T value) CombineValues<T>(this NamedVariableCollection.IView view) where T : struct, IConvertible
        {
            var values = view.GetNumberValues<T>().ToArray();
            if (values.Length == 0) return (CombinedValuesMeaning.NoValue, default(T));
            T firstValue = values[0];
            for (int i = 1; i < values.Length; i++)
                if (!Equals(values[i], firstValue))
                    return (CombinedValuesMeaning.MultipleValues, default(T));
            return (CombinedValuesMeaning.SameValue, firstValue);
        }
    }

    public static class WatchVariableUtilities
    {
        public static SortedDictionary<string, Func<IEnumerable<uint>>> baseAddressGetters = new SortedDictionary<string, Func<IEnumerable<uint>>>();

        //TODO: Move some of these where they belong
        [InitializeBaseAddress]
        static void InitBaseAddresses()
        {
            baseAddressGetters[BaseAddressType.None] = GetBaseAddressListZero;
            baseAddressGetters[BaseAddressType.Absolute] = GetBaseAddressListZero;
            baseAddressGetters[BaseAddressType.Relative] = GetBaseAddressListZero;

            baseAddressGetters[BaseAddressType.Mario] = () => new List<uint> { MarioConfig.StructAddress };
            baseAddressGetters[BaseAddressType.MarioObj] = () => new List<uint> { ProcessStream.Instance.GetUInt32(MarioObjectConfig.PointerAddress) };

            baseAddressGetters[BaseAddressType.Camera] = () => new List<uint> { CameraConfig.StructAddress };
            baseAddressGetters[BaseAddressType.CameraStruct] = () => new List<uint> { CameraConfig.CamStructAddress };
            baseAddressGetters[BaseAddressType.LakituStruct] = () => new List<uint> { CameraConfig.LakituStructAddress };
            baseAddressGetters[BaseAddressType.CameraModeInfo] = () => new List<uint> { CameraConfig.ModeInfoAddress };
            baseAddressGetters[BaseAddressType.CameraModeTransition] = () => new List<uint> { CameraConfig.ModeTransitionAddress };
            baseAddressGetters[BaseAddressType.CameraSettings] = () =>
            {
                uint a1 = 0x8033B910;
                uint a2 = ProcessStream.Instance.GetUInt32(a1);
                uint a3 = ProcessStream.Instance.GetUInt32(a2 + 0x10);
                uint a4 = ProcessStream.Instance.GetUInt32(a3 + 0x08);
                uint a5 = ProcessStream.Instance.GetUInt32(a4 + 0x10);
                return new List<uint> { a5 };
            };
        }

        public static Coordinate GetCoordinate(string stringValue)
        {
            return (Coordinate)Enum.Parse(typeof(Coordinate), stringValue);
        }

        public static List<string> ParseVariableGroupList(string stringValue) =>
            new List<string>(Array.ConvertAll(stringValue.Split('|', ','), _ => _.Trim()));

        public static readonly List<uint> BaseAddressListZero = new List<uint> { 0 };
        public static readonly List<uint> BaseAddressListEmpty = new List<uint> { };
        private static List<uint> GetBaseAddressListZero() => BaseAddressListZero;
        private static List<uint> GetBaseAddressListEmpty() => BaseAddressListEmpty;

        public static IEnumerable<uint> GetBaseAddresses(string baseAddressType)
        {
            if (baseAddressGetters.TryGetValue(baseAddressType, out var result))
                return result();
            return new List<uint>();
        }
    }
}
