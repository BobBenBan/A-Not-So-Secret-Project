using System;

namespace MusicMachine.Interaction
{
/// <summary>
///     A very simple "promise/future".
///     MUST BE CONCURRENT, BY NATURE.
/// </summary>
public interface ICompletionAwaiter
{
    bool IsDone { get; }

    //if done, can either be fail or not.
    bool IsFailed { get; }

    Action CompletionCallback { set; }

    Action FailureCallback { set; }

    bool Begun { get; }

    void Begin();
}
}