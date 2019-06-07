using System;
using Godot;
using MusicMachine.Util.Maths;

namespace MusicMachine.Scenes.Objects.Launchers
{
public struct LocationVelocityPair : IEquatable<LocationVelocityPair>
{
    public readonly Vector3 Location;
    public readonly Vector3 Velocity;

    public static LocationVelocityPair Targeting(Projectiles.TargetingParams @params)
    {
        return new LocationVelocityPair(@params.StartPos, Projectiles.CalculateVelocity(@params));
    }

    public LocationVelocityPair(Vector3 location, Vector3 velocity)
    {
        Location = location;
        Velocity = velocity;
    }

    public bool Equals(LocationVelocityPair other) =>
        Location.Equals(other.Location)
     && Velocity.Equals(other.Velocity);

    public override bool Equals(object obj) => obj is LocationVelocityPair other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            return (Location.GetHashCode() * 397) ^ Velocity.GetHashCode();
        }
    }
}
}