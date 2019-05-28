namespace MusicMachine
{
public static class EmptyArray
{
    private static class EmptyArrayHolder<T>
    {
        public static readonly T[] Arr = new T[0];
    }

    public static T[] Of<T>() => EmptyArrayHolder<T>.Arr;
}
}