using Godot;

namespace MusicMachine.Scenes.Mechanisms.MovingObject
{
[Tool]
public class MovingObject : Spatial
{
    private Spatial _objects;

    [Export] public PackedScene ObjectScene { get; set; }

    public override void _Ready()
    {
        base._Ready();
        AddChild(_objects = new Spatial {Name = "Objects"});
    }
}
}