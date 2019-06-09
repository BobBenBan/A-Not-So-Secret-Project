using System;
using Godot;

namespace MusicMachine.Mechanisms.Projectiles
{
public struct LaunchInfo : IEquatable<LaunchInfo>
{
    public readonly Vector3 Location;
    public readonly Vector3 Velocity;

    public LaunchInfo(Vector3 location, Vector3 velocity)
    {
        Location = location;
        Velocity = velocity;
    }

    public bool Equals(LaunchInfo other) => Location.Equals(other.Location) && Velocity.Equals(other.Velocity);

    public override bool Equals(object obj) => obj is LaunchInfo other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            return (Location.GetHashCode() * 397) ^ Velocity.GetHashCode();
        }
    }
}
}