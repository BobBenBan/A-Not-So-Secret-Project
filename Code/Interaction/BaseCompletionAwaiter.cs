using System;
using System.Threading;
using Object = Godot.Object;

namespace MusicMachine.Interaction
{
/// <summary>
///     Concurrent Base implementation of ICompletionAwaiter.
/// </summary>
public abstract class BaseCompletionAwaiter : Object, ICompletionAwaiter
{
    //I'm solving concurrency issues!!!!
    //Aren't you proud of yourself?
    private const int NotDone = 0;
    private const int Succeeded = 1;
    private const int Failed = 2;
    private int _begun;
    private int _doneStatus = NotDone;

    public bool IsDone => _doneStatus != NotDone;

    public bool IsFailed => _doneStatus == Failed;

    public Action CompletionCallback { private get; set; }

    public Action FailureCallback { private get; set; }

    public bool Begun => _begun != 0;

    public void Begin()
    {
        if (Interlocked.Exchange(ref _begun, 1) == 1) return;
        DoBegin();
    }

    protected abstract void DoBegin();

    protected bool TrySuccess()
    {
        if (Interlocked.CompareExchange(ref _doneStatus, Succeeded, NotDone) != NotDone) return false;
        CompletionCallback?.Invoke();
        return true;
    }

    protected bool TryFail()
    {
        if (Interlocked.CompareExchange(ref _doneStatus, Failed, NotDone) != NotDone) return false;
        FailureCallback?.Invoke();
        return true;
    }
}
}