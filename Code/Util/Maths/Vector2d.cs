using Godot;

namespace MusicMachine.Util.Maths
{
public struct Vector2d
{
    public double x;
    public double y;

    public Vector2d(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    public static explicit operator Vector2(Vector2d vector2d)
    {
        return new Vector2((float) vector2d.x, (float) vector2d.y);
    }
}
}