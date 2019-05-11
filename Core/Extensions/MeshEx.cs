using Godot;

namespace MusicMachine
{
public static class MeshEx
{
    public static Shape CreateShape(this Mesh mesh, bool isTrimesh) =>
        isTrimesh ? mesh.CreateTrimeshShape() : mesh.CreateConvexShape();
}
}