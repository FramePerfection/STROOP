using System;
using System.Collections.Generic;
using System.Reflection;

namespace STROOP.Utilities
{
    public class Wrapper<T>
    {
        public T value;

        public Wrapper()
        {
        }

        public Wrapper(T value)
        {
            this.value = value;
        }
    }

    public class EqualityComparer<T> : IEqualityComparer<T>
    {
        Func<T, T, bool> equalsFunc;
        Func<T, int> getHashCodeFunc;

        public EqualityComparer(Func<T, T, bool> equalsFunc, Func<T, int> getHashCodeFunc = null)
        {
            this.equalsFunc = equalsFunc;
            this.getHashCodeFunc = getHashCodeFunc ?? (_ => _.GetHashCode());
        }

        bool IEqualityComparer<T>.Equals(T x, T y) => equalsFunc(x, y);

        int IEqualityComparer<T>.GetHashCode(T obj) => getHashCodeFunc(obj);
    }

    public class OrderComparer<T> : IComparer<T>
    {
        Func<T, T, int> func;

        public OrderComparer(Func<T, T, int> func)
        {
            this.func = func;
        }

        int IComparer<T>.Compare(T x, T y) => func(x, y);
    }

    public static class GeneralUtilities
    {
        static Type[] stroopTypes;

        static GeneralUtilities()
        {
            stroopTypes = typeof(GeneralUtilities).Assembly.GetTypes();
        }

        public static IEnumerable<Type> EnumerateTypes(Func<Type, bool> filter)
        {
            foreach (var t in stroopTypes)
                if (filter(t))
                    yield return t;
        }

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
