using System;
using Godot;

namespace MusicMachine.Util.Physics
{
public static class Projectiles
{
    public static Vector3 CalculateVelocity(
        Vector3 startPos,
        Vector3 endPos,
        Vector3 gravity,
        float targetVelocity,
        bool useUpper)
    {
        var diff = endPos - startPos;
        if (gravity.Length() < 1e-6) return diff.Normalized() * targetVelocity;
        var    rise  = diff.Project(gravity);
        var    trans = diff - rise;
        double x     = trans.Length();
        double y     = rise.Length() * Math.Sign(rise.Dot(gravity)); //negative
        double g = gravity.Length();
        double v = targetVelocity;
        var    c = g * x / v / v;
        var    m = useUpper ? 1 : -1;
        double a;
        var    minV = g * (y + diff.Length());
        if (v * v * 0.995 <= minV)
        {
            a = Math.Atan(1 / c);
            v = minV;
            GD.Print("Minified");
        } else
            a = Math.Atan((1 + m * Math.Sqrt(1 - c * c - 2 * g * y / v / v)) / c);
        var vx = v * Math.Cos(a); //backwards.
        var vy = -(v * Math.Sin(a) - x / vx * g);
        return trans.Normalized() * (float) vx + -gravity.Normalized() * (float) vy;
    }
}
}