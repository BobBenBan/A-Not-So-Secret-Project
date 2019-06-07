namespace MusicMachine.Util
{
public sealed class Holder<T>
{
    public T Value;

    public Holder(T value)
    {
        Value = value;
    }

    public Holder()
    {
    }
}
}