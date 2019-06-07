namespace MusicMachine.Scenes.Functional.Timing
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