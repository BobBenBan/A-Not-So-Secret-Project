using Godot;
using MusicMachine.Util;

namespace MusicMachine.Scenes.Objects
{
public class WorldObject : RigidBody
{
    private MeshInstance _meshInstance;

    [Export] public bool IsTrimesh { get; private set; }

    [Export(PropertyHint.Range, "0,300")] public float LifeTime { get; private set; } = 0;

    /// <summary>
    /// Optional mesh instance to automatically generate collision shape from.
    /// </summary>
    [Export]
    public NodePath AutoShapeMesh { get; private set; }

    public override void _Ready()
    {
        if (Engine.EditorHint) return;
        TryCreateMesh();
    }

    private void TryCreateMesh()
    {
        var colShape = GetNode<CollisionShape>("CollisionShape");
        if (colShape.Shape != null)
            return;
        if (AutoShapeMesh != null)
            _meshInstance = GetNode(AutoShapeMesh) as MeshInstance;
        if (_meshInstance?.Mesh == null)
        {
            GD.PushWarning($"WorldObject ({Name}) instantiated with no mesh or shape");
            return;
        }
//        if (Mode == ModeEnum.Static && !IsTrimesh)
//        {
//            GD.PushWarning($"WorldObject ({Name}) static body is not trimesh, doing Trimesh anyways");
//            IsTrimesh = true;
//        }

        colShape.Transform = _meshInstance.Transform;
        colShape.Shape = _meshInstance.Mesh.GetCachedShape(IsTrimesh);
        if (LifeTime > 0)
            this.CreateAndConnectTimer(nameof(FreeSelf)).Start(LifeTime);
    }

    private void FreeSelf()
    {
        QueueFree();
    }
}
}