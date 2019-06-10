using Godot;

namespace MusicMachine.Util.Maths
{
public struct Vector2d
{
    public double X;
    public double Y;

    public Vector2d(double x, double y)
    {
        X = x;
        Y = y;
    }

    public static explicit operator Vector2(Vector2d vector2d) => new Vector2((float) vector2d.X, (float) vector2d.Y);
}
}