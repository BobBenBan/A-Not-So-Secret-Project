namespace MusicMachine.Scenes.Functional
{
public class Clock : ProcessNode
{
    public long CurTicks { get; private set; }

    protected override void StepTicks(long ticks)
    {
        CurTicks += ticks;
    }
}
}