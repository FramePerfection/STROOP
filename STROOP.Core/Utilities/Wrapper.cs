namespace STROOP.Core.Utilities;

public class Wrapper<T>
{
    public T value;

    public Wrapper()
    {
    }

    public Wrapper(T value) => this.value = value;
}
