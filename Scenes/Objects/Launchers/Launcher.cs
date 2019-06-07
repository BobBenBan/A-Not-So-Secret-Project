using System;
using Godot;
using MusicMachine.Util.Maths;

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

    //todo: launchProjectileTarget
    public void LaunchProjectileTarget(Projectiles.TargetingParams targetingParams)
    {
        Launch(LocationVelocityPair.Targeting(targetingParams));
    }

    public void Launch(LocationVelocityPair locationVelocityPair)
    {
        var pair = locationVelocityPair;
        _projectileHolder.AddChild(NextToBeLaunched);
        var toTranslate = pair.Location - NextToBeLaunched.GlobalTransform.origin;
        NextToBeLaunched.GlobalTranslate(toTranslate);
        NextToBeLaunched.LinearVelocity = pair.Velocity;
        NextToBeLaunched = (RigidBody) _projectileScene.Instance();
    }
}
}