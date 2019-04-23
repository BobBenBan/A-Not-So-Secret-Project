using Godot;
using MusicMachine.Extensions;

namespace MusicMachine.Scenes.Objects
{
public class WorldObject : RigidBody
{
    [Export] public bool Trimesh { get; set; } = false;
    [Export] public float LifeTime = 0;

    public override void _Ready()
    {
        GetNode<Spatial>("CollisionShape").Transform =
            GetNode<Spatial>("MeshInstance").Transform;
        var timer = GetNode<Timer>("LifeTimer");
        if (LifeTime > 0)
        {
            timer.Start(LifeTime);
        }


    }

    public void LaunchFrom(Spatial spatial, Vector3 translation,
        Vector3 rotation, Vector3 velocity, Vector3 angularVelocity)
    {
        LaunchFrom(spatial.GetGlobalTransform(), translation, rotation,
            velocity, angularVelocity);
    }

    public void LaunchFrom(Transform transform, Vector3 translation,
        Vector3 rotation, Vector3 velocity, Vector3 angularVelocity)
    {
        transform = transform.Orthonormalized();
        transform.origin = transform.Xform(translation);
        this.Transform = transform; //copy rotation too!
        this.RotateBy(rotation);
        LinearVelocity = transform.basis.Xform(velocity);
        AngularVelocity = transform.basis.Xform(angularVelocity);
    }

    public void SimpleLaunchFrom(Spatial spatial, float offset, float velocity)
    {
        LaunchFrom(spatial, new Vector3(0, 0, offset), new Vector3(),
            new Vector3(0, 0, velocity), new Vector3());
    }

    public void SimpleLaunchFrom(Transform transform, float offset,
        float velocity)
    {
        LaunchFrom(transform, new Vector3(0, 0, offset), new Vector3(),
            new Vector3(0, 0, velocity), new Vector3());
    }
    private void OnTimeout(){QueueFree();}
}
}