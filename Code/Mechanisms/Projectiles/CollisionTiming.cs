using System;
using System.Threading;
using Godot;
using MusicMachine.Game.Constants;
using MusicMachine.Mechanisms.Timings;
using MusicMachine.Util;
using Array = Godot.Collections.Array;

namespace MusicMachine.Mechanisms.Projectiles
{
public class CollisionTiming : Timing
{
    private readonly ILauncher _launcher;
    private readonly LaunchInfo _launchInfo;
    private RigidBody _projectile;

    public CollisionTiming(ILauncher launcher, LaunchInfo launchInfo)
    {
        _launcher   = launcher ?? throw new ArgumentNullException(nameof(launcher));
        _launchInfo = launchInfo;
    }

    protected override void OnStart()
    {
        _projectile                  = _launcher.NextToLaunch;
        _projectile.ContactMonitor   = true;
        _projectile.ContactsReported = Math.Max(_projectile.ContactsReported, 2);
        _projectile.Connect(SignalNames.RigidBody.BodyEntered, this, nameof(OnBodyEntered));
        _projectile.Connect(SignalNames.Node.Exiting,          this, nameof(OnProjectileExit), new Array {_projectile});
        _launcher.Launch(new LaunchInfo(_launchInfo.Location, _launchInfo.Velocity));
    }

    private void OnBodyEntered(Node node)
    {
        var proj = _projectile;
        if (!proj.IsInsideTree())
        {
            Interlocked.CompareExchange(ref _projectile, null, proj);
            return;
        }
        if (!node.IsInsideTree() || !(node is PhysicsBody obj) || !proj.CollidableWith(obj))
            return;
        EndTiming();
    }

    private void OnProjectileExit(RigidBody which)
    {
        if (Interlocked.CompareExchange(ref _projectile, null, which) == which) CancelTiming();
    }

    protected override void OnCancel()
    {
        OnEnd();
    }

    protected override void OnEnd()
    {
        _projectile?.QueueFree();
        _projectile = null;
    }
}
}