using Godot;

namespace MusicMachine.Scenes.Objects
{
public class Pointer : Spatial
{
    private static readonly SpatialMaterial _material = GD.Load<SpatialMaterial>("res://Scenes/Objects/Material.tres");
    private MeshInstance _meshInstance;

    public SpatialMaterial SpatialMaterial { get; private set; }

    public override void _Ready()
    {
        _meshInstance   = GetNode<MeshInstance>("Mesh");
        SpatialMaterial = (SpatialMaterial) _material.Duplicate();
        _meshInstance.SetSurfaceMaterial(0, SpatialMaterial);
    }
}
}