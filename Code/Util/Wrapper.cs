using System;

namespace MusicMachine.Util
{
public class Wrapper<T>
    where T : class
{
    private T _self;

    protected T Self
    {
        get => _self ?? throw new InvalidOperationException("obj is null");
        set => _self = value ?? throw new NullReferenceException();
    }
}
}