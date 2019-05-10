using System;
using Godot;

namespace MusicMachine.Extensions
{
public static class MeshEx
{
    public static Shape CreateShape(this Mesh mesh, bool isTrimesh) => isTrimesh ? mesh.CreateTrimeshShape() : mesh.CreateConvexShape();
}
}