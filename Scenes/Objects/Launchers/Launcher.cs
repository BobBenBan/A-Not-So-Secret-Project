using System;
using Godot;

namespace MusicMachine.Scenes.Objects.Launchers
{
public class Launcher : Spatial
{
    //todo: pooling?
    public const string Dir = "res://Scenes/Objects/Launchers/" + nameof(Launcher) + ".cs";
    public const string DefaultProjectileDir = "res://Scenes/Objects/Launchers/MetalBall.tscn";
    private Spatial _projectileHolder;
    private PackedScene _projectileScene;

    protected RigidBody NextToBeLaunched { get; private set; }

//    [Export] public bool PoolProjectiles = false;
    [Export(PropertyHint.Dir)] public string ProjectileScene = DefaultProjectileDir;

    public override void _Ready()
    {
        _projectileScene = GD.Load(ProjectileScene) as PackedScene
                        ?? throw new InvalidOperationException("Provided scene not valid");
        NextToBeLaunched = _projectileScene.Instance() as RigidBody
                        ?? throw new InvalidOperationException("Provided scene is not a rigid body.");
        _projectileHolder = GetNode<Spatial>("Projectiles");
    }

    public void LaunchProjectile(Vector3 fromLocation, Vector3 velocity)

    {
        _projectileHolder.AddChild(NextToBeLaunched);
        var toTranslate = fromLocation - NextToBeLaunched.GlobalTransform.origin;
        NextToBeLaunched.GlobalTranslate(toTranslate);
        NextToBeLaunched.LinearVelocity = velocity;
        NextToBeLaunched = (RigidBody) _projectileScene.Instance();
    }
}
}