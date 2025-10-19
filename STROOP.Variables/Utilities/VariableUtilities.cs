using STROOP.Variables.SM64MemoryLayout;
using STROOP.Variables.VariablePanel;

namespace STROOP.Variables.Utilities;

public static class IVariableViewExtensions
{
    public static IVariable<T> WithKeyedValue<T, TValue>(this IVariable<T> view, string key, TValue value)
    {
        view.SetValueByKey(key, value);
        return view;
    }

    public static string GetJsonName(this IVariable view)
    {
        string? explicitJsonName = view.GetValueByKey("jsonName");
        if (explicitJsonName == "")
            return null;
        if (explicitJsonName != null)
            return explicitJsonName;
        return $"{view.Name}".Replace(' ', '_').ToLower();
    }

    public static IEnumerable<T> GetNumberValues<T>(this IVariable view) where T : struct, IConvertible
    {
        if (view.TryGetNumberValues<T>(out IEnumerable<T>? result))
            return result;
        throw new InvalidOperationException($"'{view.GetType().FullName}' is not a vaild number type.");
    }

    public static bool TryGetNumberValues<T>(this IVariable view, out IEnumerable<T> result)
        where T : struct, IConvertible
    {
        bool Get<Q>(out IEnumerable<T> innerResult)
        {
            innerResult = null;
            if (view is IVariable<Q> qView)
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

    public static IEnumerable<bool> TrySetValue<T>(this IVariable view, T value) where T : IConvertible
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

            if (view is IVariable<Q> qView)
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

    public static (CombinedValuesMeaning meaning, T value) CombineValues<T>(this IVariableCellData<T> view)
    {
        T[]? values = view.GetValues();
        if (values.Length == 0)
            return (CombinedValuesMeaning.NoValue, default(T));

        T firstValue = values[0];
        for (int i = 1; i < values.Length; i++)
            if (!Equals(values[i], firstValue))
                return (CombinedValuesMeaning.MultipleValues, default(T));

        return (CombinedValuesMeaning.SameValue, firstValue);
    }
}

public static class VariableUtilities
{
    public static SortedDictionary<string, Func<IEnumerable<uint>>> baseAddressGetters = new SortedDictionary<string, Func<IEnumerable<uint>>>();

    [InitializeBaseAddress]
    private static void InitBaseAddresses()
    {
        baseAddressGetters[BaseAddressType.None] = GetBaseAddressListEmpty;
        baseAddressGetters[BaseAddressType.Relative] = GetBaseAddressListZero;
    }

    public static List<string> ParseVariableGroupList(string stringValue)
        => [..Array.ConvertAll(stringValue.Split('|', ','), s => s.Trim())];

    public static readonly List<uint> BaseAddressListZero = [0];
    public static readonly List<uint> BaseAddressListEmpty = [];
    private static List<uint> GetBaseAddressListZero() => BaseAddressListZero;
    private static List<uint> GetBaseAddressListEmpty() => BaseAddressListEmpty;

    public static IEnumerable<uint> GetBaseAddresses(string baseAddressType)
        => baseAddressGetters.TryGetValue(baseAddressType, out Func<IEnumerable<uint>> result) ? result() : [];

    public static IEnumerable<IEnumerable<double>> GetNumberValues<TCell>(IEnumerable<TCell> cells)
        where TCell : IVariableCell
        => cells.OfType<IVariableCellData<IConvertible>>().Where(x => x is not IVariableCellData<string>)
            .Select(x => x.GetValues().Select(y => (double)Convert.ChangeType(y, TypeCode.Double)));
}
