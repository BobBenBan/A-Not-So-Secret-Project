using Godot;

//#if TOOLS

namespace MusicMachine
{
[Tool]
public class Plugin : EditorPlugin
{
    public override void _EnterTree()
    {
        GD.Print("Plugin Enter Tree");
//        AddCustomType(nameof(TimingRecorder), "Node", GD.Load<Script>(TimingRecorder.Dir), null);
    }

    public override void _ExitTree()
    {
        GD.Print("Plugin Exit Tree");
//        RemoveCustomType(nameof(TimingRecorder));
    }
}
}

//#endif