using System.Reflection;

namespace STROOP.Core.Utilities;

public static class GeneralUtilities
{
    public static IEnumerable<T> Yield<T>(this T value)
    {
        yield return value;
    }

    public static EqualityComparer<T> GetEqualityComparer<T>(Func<T, T, bool> equalsFunc, Func<T, int> getHashCodeFunc = null)
        => new EqualityComparer<T>(equalsFunc, getHashCodeFunc);

    public static void ExecuteInitializers<T>(params object[] args) where T : InitializerAttribute
    {
        foreach (Assembly? assembly in AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName?.StartsWith("STROOP") ?? false))
            foreach (Type? type in assembly.GetTypes())
                foreach (MethodInfo? m in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
                    if (m.GetParameters().Length == 0 && m.GetCustomAttribute<T>() != null)
                        m.Invoke(null, args);
    }

    public static List<TOut> ConvertAndRemoveNull<TIn, TOut>(this IEnumerable<TIn> lstIn, Func<TIn, TOut> converter) where TOut : class
    {
        List<TOut>? lstOut = new List<TOut>();
        foreach (TIn? obj in lstIn)
        {
            TOut? convertedObj = converter(obj);
            if (convertedObj != null)
                lstOut.Add(convertedObj);
        }

        return lstOut;
    }

    public static List<TOut> ConvertAndRemoveNull<TOut>(this System.Collections.IEnumerable lstIn, Func<object, TOut> converter) where TOut : class
    {
        List<TOut>? lstOut = new List<TOut>();
        foreach (object? obj in lstIn)
        {
            TOut? convertedObj = converter(obj);
            if (convertedObj != null)
                lstOut.Add(convertedObj);
        }

        return lstOut;
    }

    public static IEnumerable<TOut> ConvertAll<TIn, TOut>(this IEnumerable<TIn> lstIn, Func<TIn, TOut> converter)
    {
        foreach (TIn? obj in lstIn)
            yield return converter(obj);
    }


    public static T GetMeaningfulValue<T>(Func<IEnumerable<T>> values, T fail, T @default)
    {
        T result = @default;
        foreach (T? value in values())
            GetMeaningfulValue(ref result, value, fail);
        return result;
    }

    public static void GetMeaningfulValue<T>(ref T result, T input, T fail)
    {
        if (!(result?.Equals(fail) ?? fail == null))
            if (result == null)
                result = input;
            else if (!(input?.Equals(result) ?? result == null))
                result = fail;
    }

    public static Dictionary<V, K> ReverseDictionary<K, V>(Dictionary<K, V> dictionary)
    {
        Dictionary<V, K> reverseDictionary = new Dictionary<V, K>();
        dictionary.ToList().ForEach(keyValuePair => { reverseDictionary.Add(keyValuePair.Value, keyValuePair.Key); });
        return reverseDictionary;
    }
}
