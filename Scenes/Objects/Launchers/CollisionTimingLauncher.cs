using System;
using System.Threading;
using Godot;
using MusicMachine.Scenes.Functional.Timing;
using MusicMachine.Util;
using Array = Godot.Collections.Array;

namespace MusicMachine.Scenes.Objects.Launchers
{
public class CollisionTimingLauncher : Launcher
{
    public TimingRecorder TimingRecorder { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        TimingRecorder = GetNode<TimingRecorder>("TimingRecorder");
    }

    public Timing GetProjectileTiming(LocationVelocityPair locationVelocityPair)
    {
        var key = locationVelocityPair;
        if (TimingRecorder.TryGetValue(key, out var timing))
            return timing;
        return TimingRecorder[key] = new ProjectileTiming(this, key);
    }

    private sealed class ProjectileTiming : Timing
    {
        private readonly LocationVelocityPair _locationVelocityPair;
        private readonly CollisionTimingLauncher _launcher;
        private RigidBody _projectile;

        public ProjectileTiming(CollisionTimingLauncher launcher, LocationVelocityPair locationVelocityPair)
        {
            _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
            _locationVelocityPair = locationVelocityPair;
        }

        protected override void OnStart()
        {
            _projectile = _launcher.NextToBeLaunched;
            _projectile.ContactMonitor = true;
            _projectile.ContactsReported = Math.Max(_projectile.ContactsReported, 2);
            _projectile.Connect(
                SignalNames.RigidBody.BodyEntered,
                this,
                nameof(OnBodyEntered));
            _projectile.Connect(SignalNames.Node.Exiting, this, nameof(OnProjectileExit), new Array {_projectile});
            _launcher.Launch(new LocationVelocityPair(_locationVelocityPair.Location, _locationVelocityPair.Velocity));
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
            if (Interlocked.CompareExchange(ref _projectile, null, which) == which)
                CancelTiming(false);
        }

        protected override void OnCancel(bool isDone)
        {
            if (!isDone)
                OnEnd();
        }

        protected override void OnEnd()
        {
            _projectile?.QueueFree();
        }
    }
}
}