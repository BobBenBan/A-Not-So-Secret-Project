using System;

namespace MusicMachine.Util
{
public class Wrapper<T>
    where T : class
{
    private T _self;

    protected T Self
    {
        get => _self ?? throw new NullReferenceException("No self is set.");
        set => _self = value ?? throw new ArgumentNullException(nameof(Self));
    }
}
}