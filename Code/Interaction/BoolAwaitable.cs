namespace MusicMachine.Interaction
{
public sealed class TrueAwaitable : BaseAwaiter<bool>
{
    public TrueAwaitable()
    {
    }

    protected override void DoBegin() => Complete(true);
}
}