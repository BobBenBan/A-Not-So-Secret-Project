namespace MusicMachine.Interaction
{
public sealed class AlreadyDoneCompletionAwaiter : BaseCompletionAwaiter
{
    protected override void DoBegin() => TrySuccess();
}
}