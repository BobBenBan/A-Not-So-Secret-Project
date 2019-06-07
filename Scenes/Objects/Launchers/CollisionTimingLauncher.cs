using System;
using Godot;
using MusicMachine.Scenes.Functional.Timing;

namespace MusicMachine.Scenes.Objects.Launchers
{
public class CollisionTimingLauncher : Launcher
{
    private TimingRecorder _timingRecorder;

    public override void _Ready()
    {
        base._Ready();
        _timingRecorder = GetNode<TimingRecorder>("TimingRecorder");
    }

    public Timing GetProjectileTiming(Vector3 location, Vector3 velocity)
    {
        var key = new ProjectileKey(location, velocity);
        if (_timingRecorder.TryGetValue(key, out var timing))
            return timing;
        return _timingRecorder[key] = new ProjectileTiming(this, key);
    }

    private sealed class ProjectileTiming : Timing
    {
        private readonly ProjectileKey _projectileKey;
        private readonly CollisionTimingLauncher _launcher;
        private RigidBody _projectile;

        public ProjectileTiming(CollisionTimingLauncher launcher, ProjectileKey projectileKey)
        {
            _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
            _projectileKey = projectileKey;
        }

        protected override void OnStart()
        {
            _projectile = _launcher.NextToBeLaunched;
            _projectile.ContactMonitor = true;
            _projectile.ContactsReported = Math.Max(_projectile.ContactsReported, 1);
            _launcher.LaunchProjectile(_projectileKey.Location, _projectileKey.Velocity);
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

    private struct ProjectileKey : IEquatable<ProjectileKey>
    {
        public readonly Vector3 Location;
        public readonly Vector3 Velocity;

        public ProjectileKey(Vector3 location, Vector3 velocity)
        {
            Location = location;
            Velocity = velocity;
        }

        public bool Equals(ProjectileKey other) =>
            Location.Equals(other.Location)
         && Velocity.Equals(other.Velocity);

        public override bool Equals(object obj) => obj is ProjectileKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Location.GetHashCode() * 397) ^ Velocity.GetHashCode();
            }
        }
    }
}
}