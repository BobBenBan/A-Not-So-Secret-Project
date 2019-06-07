using Godot;
using System;
using MusicMachine.Util.Physics;

public class TestOnly : Node
{
    public override void _Ready()
    {
        Projectiles.CalculateVelocity(default, new Vector3(3, 4, 0), new Vector3(0, -10f, 0), 10, true);
        GetTree().Quit();
    }
}