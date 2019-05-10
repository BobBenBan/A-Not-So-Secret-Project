using Godot;

namespace MusicMachine.Test
{
public class TestOnly : Node
{
    public override void _EnterTree()
    {
        AnimationTest.WhereDoesItInsert();
        GetTree().Quit();
    }
}
}