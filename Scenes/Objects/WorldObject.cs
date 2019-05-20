using Godot;

namespace MusicMachine.Scenes
{
public class WorldObject : RigidBody
{
    protected MeshInstance MeshInstance;

    [Export] public bool IsTrimesh { get; private set; }

    [Export(PropertyHint.Range, "0,300")] public float LifeTime { get; private set; } = 0;

    public override void _Ready()
    {
        TryCreateMesh();
    }
    private void TryCreateMesh()
    {
        var colShape = GetNode<CollisionShape>("CollisionShape");
        if (colShape.Shape != null)
            return;
        MeshInstance = GetNode<MeshInstance>("MeshInstance");
        if (MeshInstance.Mesh == null)
        {
            GD.PushWarning($"WorldObject ({Name}) instantiated with no mesh, deleting self");
            QueueFree();
            return;
        }
//        if (Mode == ModeEnum.Static && !IsTrimesh)
//        {
//            GD.PushWarning($"WorldObject ({Name}) static body is not trimesh, doing Trimesh anyways");
//            IsTrimesh = true;
//        }

        colShape.Transform = MeshInstance.Transform;
        colShape.Shape = MeshInstance.Mesh.GetCachedShape(IsTrimesh);
        if (LifeTime > 0)
            this.CreateAndConnectTimer(nameof(FreeSelf)).Start(LifeTime);
    }
    public override void _PhysicsProcess(float delta)
    {
    }
    public void LaunchFrom(
        Spatial spatial,
        Vector3 translation,
        Vector3 rotation,
        Vector3 velocity,
        Vector3 angularVelocity)
    {
        LaunchFrom(
            spatial.GetGlobalTransform(),
            translation,
            rotation,
            velocity,
            angularVelocity);
    }
    public void LaunchFrom(
        Transform transform,
        Vector3 translation,
        Vector3 rotation,
        Vector3 velocity,
        Vector3 angularVelocity)
    {
        transform = transform.Orthonormalized();
        transform.origin = transform.Xform(translation);
        Transform = transform; //copy rotation too!
        this.RotateBy(rotation);
        LinearVelocity = transform.basis.Xform(velocity);
        AngularVelocity = transform.basis.Xform(angularVelocity);
    }
    public void SimpleLaunchFrom(Spatial spatial, float offset, float velocity)
    {
        LaunchFrom(
            spatial,
            new Vector3(0, 0, offset),
            new Vector3(),
            new Vector3(0, 0, velocity),
            new Vector3());
    }
    public void SimpleLaunchFrom(Transform transform, float offset, float velocity)
    {
        LaunchFrom(
            transform,
            new Vector3(0, 0, offset),
            new Vector3(),
            new Vector3(0, 0, velocity),
            new Vector3());
    }
    private void FreeSelf()
    {
        QueueFree();
    }
}
}