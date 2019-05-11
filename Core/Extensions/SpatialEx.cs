using Godot;

namespace MusicMachine
{
public static class SpatialEx
{
    public static void RotateBy(this Spatial spatial, Vector3 rot)
    {
        spatial.RotateX(rot.x);
        spatial.RotateY(rot.y);
        spatial.RotateZ(rot.z);
    }
}
}