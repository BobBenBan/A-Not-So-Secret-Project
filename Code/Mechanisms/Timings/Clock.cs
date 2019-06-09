using MusicMachine.Scenes.Functional;

namespace MusicMachine.Mechanisms.Timings
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