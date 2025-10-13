namespace STROOP.Core.Utilities;

public class OrderComparer<T> : IComparer<T>
{
    private Func<T, T, int> func;

    public OrderComparer(Func<T, T, int> func) => this.func = func;

    int IComparer<T>.Compare(T x, T y) => func(x, y);
}
