using Godot;

namespace MusicMachine.Mechanisms.Projectiles
{
public interface ILauncher
{
    RigidBody NextToLaunch { get; }

    void Launch(LaunchInfo launch);
}
}