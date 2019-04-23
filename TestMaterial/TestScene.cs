using Godot;
using System;
using MusicMachine.Extensions;
using MusicMachine.Scenes;
using MusicMachine.Scenes.Objects;
using Object = Godot.Object;

public class TestScene : Area
{
    private Player _player;
    private Node _objects;

    private PackedScene Obj =
        ResourceLoader.Load<PackedScene>("res://Scenes/Objects/Teapot.tscn");

    public override void _Ready()
    {
        _player = GetNode<Player>("Player");
        _player.PrimaryAction = OnAction;
        _player.SecondaryAction = OnSecondary;
        _objects = GetNode("Objects");
    }

    public void OnAction(float delta)
    {
        var obj = (WorldObject) Obj.Instance();
        obj.SimpleLaunchFrom(_player.CameraLocation,1,10);
        _objects.AddChild(obj, true);
    }

    private void OnSecondary(float delta) { }

    private void OnBodyExited(Node body)
    {
        if (!body.TryCall("OnWorldExit"))
            body.QueueFree();
    }
}