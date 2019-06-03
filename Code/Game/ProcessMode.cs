using System;

namespace MusicMachine.Game
{
public enum ProcessMode
{
    /// <summary>
    ///     Once per frame
    /// </summary>
    Process,

    /// <summary>
    ///     Once per physics step,
    /// </summary>
    Physics,

    /// <summary>
    ///     On the separate audio looping thread thing
    /// </summary>
    Audio,

    /// <summary>
    ///     Manually.
    /// </summary>
    Manual
}

public interface IProcessable
{
    void SetProcess(bool b);

    void SetPhysicsProcess(bool b);

    void StepTicks(long ticks);
}

public static class ProcessModeEx
{
    public static void RemoveProcess(this IProcessable node, ProcessMode mode)
    {
        switch (mode)
        {
        case ProcessMode.Process:
            node.SetProcess(false);
            break;
        case ProcessMode.Physics:
            node.SetPhysicsProcess(false);
            break;
        case ProcessMode.Audio:
            Loops.AudioProcess -= node.StepTicks;
            break;
        case ProcessMode.Manual: break;
        default:                 throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    public static void AddProcess(this IProcessable node, ProcessMode mode)
    {
        switch (mode)
        {
        case ProcessMode.Process:
            node.SetProcess(true);
            break;
        case ProcessMode.Physics:
            node.SetPhysicsProcess(true);
            break;
        case ProcessMode.Audio:
            Loops.AudioProcess += node.StepTicks;
            break;
        case ProcessMode.Manual: break;
        default:                 throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }
}
}