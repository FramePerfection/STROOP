using STROOP.Variables.SM64MemoryLayout;
using STROOP.Variables.Utilities;
using STROOP.Variables.Views;

namespace STROOP.Variables;

public static class IVariableViewExtensions
{
    public static IVariableView<T> WithKeyedValue<T, TValue>(this IVariableView<T> view, string key, TValue value)
    {
        view.SetValueByKey(key, value);
        return view;
    }

    public static string GetJsonName(this IVariableView view)
    {
        string? explicitJsonName = view.GetValueByKey("jsonName");
        if (explicitJsonName == "")
            return null;
        if (explicitJsonName != null)
            return explicitJsonName;
        return $"{view.Name}".Replace(' ', '_').ToLower();
    }

    public static IEnumerable<T> GetNumberValues<T>(this IVariableView view) where T : struct, IConvertible
    {
        if (view.TryGetNumberValues<T>(out IEnumerable<T>? result))
            return result;
        throw new InvalidOperationException($"'{view.GetType().FullName}' is not a vaild number type.");
    }

    public static bool TryGetNumberValues<T>(this IVariableView view, out IEnumerable<T> result)
        where T : struct, IConvertible
    {
        bool Get<Q>(out IEnumerable<T> innerResult)
        {
            innerResult = null;
            if (view is IVariableView<Q> qView)
            {
                innerResult = qView.getter().Select(x => (T)Convert.ChangeType(x, typeof(T)));
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

    public static IEnumerable<bool> TrySetValue<T>(this IVariableView view, T value) where T : IConvertible
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

            if (view is IVariableView<Q> qView)
                return qView.setter(convertedValue);
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

    public static (CombinedValuesMeaning meaning, T value) CombineValues<T>(this IVariableView view) where T : struct, IConvertible
    {
        T[]? values = view.GetNumberValues<T>().ToArray();
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

    [InitializeBaseAddress]
    private static void InitBaseAddresses()
    {
        baseAddressGetters[BaseAddressType.None] = GetBaseAddressListEmpty;
        baseAddressGetters[BaseAddressType.Relative] = GetBaseAddressListZero;
    }

    public static Coordinate GetCoordinate(string stringValue) => (Coordinate)Enum.Parse(typeof(Coordinate), stringValue);

    public static List<string> ParseVariableGroupList(string stringValue) =>
        new List<string>(Array.ConvertAll(stringValue.Split('|', ','), _ => _.Trim()));

    public static readonly List<uint> BaseAddressListZero = new List<uint> { 0 };
    public static readonly List<uint> BaseAddressListEmpty = new List<uint> { };
    private static List<uint> GetBaseAddressListZero() => BaseAddressListZero;
    private static List<uint> GetBaseAddressListEmpty() => BaseAddressListEmpty;

    public static IEnumerable<uint> GetBaseAddresses(string baseAddressType)
    {
        if (baseAddressGetters.TryGetValue(baseAddressType, out Func<IEnumerable<uint>>? result))
            return result();
        return new List<uint>();
    }
}
