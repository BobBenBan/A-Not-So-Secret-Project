using System;

namespace MusicMachine
{
public class Wrapper<T>
    where T : class
{
    private T _obj;

    protected T Self
    {
        get => _obj ?? throw new InvalidOperationException("obj is null");
        set => _obj = value ?? throw new NullReferenceException();
    }
}
}