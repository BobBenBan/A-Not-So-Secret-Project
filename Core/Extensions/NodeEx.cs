using Godot;

namespace MusicMachine
{
public static class NodeEx
{
    public static Timer CreateAndConnectTimer(
        this Node node, string method,
        bool physics = true, bool oneShot = true
    )
    {
        var timer = new Timer
        {
            OneShot = oneShot,
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