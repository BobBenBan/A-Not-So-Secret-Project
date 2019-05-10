using Godot;
using MusicMachine.Extensions;
using MusicMachine.Scenes;

namespace MusicMachine.Test
{
public class TestScene : Area
{
    private Player _player;
    private Node _objects;

    private readonly PackedScene _obj =
        ResourceLoader.Load<PackedScene>("res://Scenes/Objects/Teapot.tscn");

    public override void _Ready()
    {
        _player = GetNode<Player>("Player");
        _player.Primary = OnAction;
        _player.Secondary = OnSecondary;
        _objects = GetNode("Objects");
    }

    public void OnAction(float delta)
    {
        var obj = (WorldObject) _obj.Instance();
        obj.SimpleLaunchFrom(_player.CameraLocation, 1, 10);
        _objects.AddChild(obj, true);
    }

    private void OnSecondary(float delta)
    {
    }

    private void OnBodyExited(Node body)
    {
        if (!body.TryCall("OnWorldExit"))
            body.QueueFree();
    }
}
}