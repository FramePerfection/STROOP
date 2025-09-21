using System.Reflection;

namespace STROOP.Core.Utilities
{
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
            foreach (var type in typeof(T).Assembly.GetTypes())
                foreach (var m in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
                    if (m.GetParameters().Length == 0 && m.GetCustomAttribute<T>() != null)
                        m.Invoke(null, args);
        }

        public static List<TOut> ConvertAndRemoveNull<TIn, TOut>(this IEnumerable<TIn> lstIn, Func<TIn, TOut> converter) where TOut : class
        {
            var lstOut = new List<TOut>();
            foreach (var obj in lstIn)
            {
                var convertedObj = converter(obj);
                if (convertedObj != null)
                    lstOut.Add(convertedObj);
            }

            return lstOut;
        }

        public static List<TOut> ConvertAndRemoveNull<TOut>(this System.Collections.IEnumerable lstIn, Func<object, TOut> converter) where TOut : class
        {
            var lstOut = new List<TOut>();
            foreach (var obj in lstIn)
            {
                var convertedObj = converter(obj);
                if (convertedObj != null)
                    lstOut.Add(convertedObj);
            }

            return lstOut;
        }

        public static IEnumerable<TOut> ConvertAll<TIn, TOut>(this IEnumerable<TIn> lstIn, Func<TIn, TOut> converter)
        {
            foreach (var obj in lstIn)
                yield return converter(obj);
        }


        public static T GetMeaningfulValue<T>(Func<IEnumerable<T>> values, T fail, T @default)
        {
            T result = @default;
            foreach (var value in values())
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
    }
}
