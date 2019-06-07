using System;
using Godot;

namespace MusicMachine.Interaction
{
public abstract class BaseAwaiter<T> :IAwaiter<T>
{
    private Action _continuation;

    public bool Began { get; private set; }

    protected T Result { get; private set; }


    public void OnCompleted(Action continuation)
    {
        _continuation = continuation;
    }

    public T GetResult() => IsCompleted ? Result : default;

    public bool IsCompleted { get; private set; }

    public void EnsureBegin()
    {
        if (!Began) Begin();
    }

    public void Begin()
    {
        DoBegin();
        Began = true;
    }

    protected abstract void DoBegin();

    protected void Complete(T result)
    {
        Result = result;
        IsCompleted = true;
        _continuation?.Invoke();
    }

    protected void Fail()
    {
        Result = default;
        IsCompleted = true;
        _continuation = null;
    }
}

public class SimpleAwaitable<T> : BaseAwaiter<T>
{
    private readonly Action _begin;

    public SimpleAwaitable(Action begin)
    {
        _begin = begin ?? throw new ArgumentNullException(nameof(begin));
    }

    public new void Complete(T result) => base.Complete(result);

    public new void Fail() => base.Fail();

    protected override void DoBegin()
    {
        _begin.Invoke();
    }
}
}