using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace MusicMachine.Interaction
{
/// <summary>
///     An ICompletionAwaiter that combines multiple ICompletionAwaiters.
///     Will fail if any one operation fails, and completes when all complete.
///     The list of Awaiters is not yet finalized until this ICompletionAwaiter has begun.
/// </summary>
public sealed class CombinedCompletionAwaiter : BaseCompletionAwaiter
{
    private ICompletionAwaiter[] _finalAwaiters;
    private int _numDone;
    public IEnumerable<ICompletionAwaiter> Awaiters;

    protected override void DoBegin()
    {
        _finalAwaiters = Awaiters.ToArray();

        void AwaiterCompletionCallback()
        {
            Debug.Assert(!IsDone);
            if (Interlocked.Increment(ref _numDone) == _finalAwaiters.Length) TrySuccess();
        }

        void AwaiterFailureCallback() => TryFail();

        foreach (var awaiter in _finalAwaiters)
        {
            awaiter.CompletionCallback = AwaiterCompletionCallback;
            awaiter.FailureCallback    = AwaiterFailureCallback;
        }
        foreach (var awaiter in _finalAwaiters) awaiter.Begin();
    }
}
}