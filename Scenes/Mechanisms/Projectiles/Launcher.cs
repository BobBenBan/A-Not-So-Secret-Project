using Godot;
using MusicMachine.Mechanisms.Projectiles;

namespace MusicMachine.Scenes.Mechanisms.Projectiles
{
[Tool]
public class Launcher : Spatial, ILauncher
{
    private Spatial _projectileHolder;

//    [Export] public bool PoolProjectiles = false;
#pragma warning disable 649
    [Export] private PackedScene _projectileScene;
#pragma warning restore 649

    private RigidBody NextToLaunch { get; set; }

    RigidBody ILauncher.NextToLaunch => NextToLaunch;

    public void Launch(LaunchInfo launchInfo)
    {
        if (NextToLaunch == null) return;
        _projectileHolder.AddChild(NextToLaunch);
        var toTranslate = launchInfo.Location - NextToLaunch.GlobalTransform.origin;
        NextToLaunch.GlobalTranslate(toTranslate);
        NextToLaunch.LinearVelocity = launchInfo.Velocity;
        NextToLaunch                = (RigidBody) _projectileScene.Instance();
    }

    public override string _GetConfigurationWarning() => _projectileScene == null ? "Projectile Scene not set" : "";

    public override void _Ready()
    {
        if (_projectileScene == null) return;
        NextToLaunch = _projectileScene.Instance() as RigidBody;
        if (NextToLaunch == null)
        {
            GD.PushWarning("Provided scene is not a rigid body.");
            return;
        }
        if (!Engine.EditorHint)
            AddChild(_projectileHolder = new Spatial {Name = "Projectiles"});
    }
}
}