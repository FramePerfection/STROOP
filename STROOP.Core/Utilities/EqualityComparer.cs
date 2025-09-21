namespace STROOP.Core.Utilities
{
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
}
