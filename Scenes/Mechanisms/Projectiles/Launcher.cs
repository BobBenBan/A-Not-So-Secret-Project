using System;
using Godot;
using MusicMachine.Mechanisms.Projectiles;

namespace MusicMachine.Scenes.Mechanisms.Projectiles
{
public class Launcher : Spatial, ILauncher
{
    //todo: pooling?
    public const string Dir = "res://Scenes/Objects/Launchers/" + nameof(Launcher) + ".cs";
    public const string DefaultProjectileDir = "res://Scenes/Objects/Launchers/MetalBall.tscn";
    private Spatial _projectileHolder;
    private PackedScene _projectileScene;

//    [Export] public bool PoolProjectiles = false;
    [Export(PropertyHint.Dir)] public string ProjectileScene = DefaultProjectileDir;

    private RigidBody NextToLaunch { get; set; }

    RigidBody ILauncher.NextToLaunch => NextToLaunch;

    public void Launch(LaunchInfo launchInfo)
    {
        var pair = launchInfo;
        _projectileHolder.AddChild(NextToLaunch);
        var toTranslate = pair.Location - NextToLaunch.GlobalTransform.origin;
        NextToLaunch.GlobalTranslate(toTranslate);
        NextToLaunch.LinearVelocity = pair.Velocity;
        NextToLaunch                = (RigidBody) _projectileScene.Instance();
    }

    public override void _Ready()
    {
        _projectileScene = GD.Load(ProjectileScene) as PackedScene
                        ?? throw new InvalidOperationException("Provided resource not a scene");
        NextToLaunch = _projectileScene.Instance() as RigidBody
                    ?? throw new InvalidOperationException("Provided scene is not a rigid body.");
        AddChild(_projectileHolder = new Spatial {Name = "Projectiles"});
    }
}
}