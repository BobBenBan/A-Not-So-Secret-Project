using Godot;

namespace MusicMachine.Extensions
{
public static class NodeEx
{
    public static Timer CreateAndConnectTimer(
        this Node node, string method,
        bool physics = true, bool oneshot = true
    )
    {
        var timer = new Timer
        {
            OneShot = oneshot,
            ProcessMode = physics
                ? Timer.TimerProcessMode.Physics
                : Timer.TimerProcessMode.Idle
        };
        node.AddChild(timer);
        timer.Connect("timeout", node, method);
        return timer;
    }
}
}