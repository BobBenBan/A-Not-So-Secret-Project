using System;
using Godot;

namespace MusicMachine.Util.Maths
{
public static class Projectiles
{
    public static Vector3 CalculateVelocity(
        TargetingParams i)
    {
        var diff = i.EndPos - i.StartPos;
        if (i.Gravity.Length() < 1e-6)
            return diff.Normalized() * i.MinLaunchVelocity;
        var    rise = diff.Project(i.Gravity);
        var    run  = diff - rise;
        double x    = run.Length();
        double y    = rise.Length() * -Math.Sign(rise.Dot(i.Gravity));
        double g    = i.Gravity.Length();
        double v    = i.MinLaunchVelocity;
        var    vv   = (Vector2) CalcVelocity2d(x, y, g, v, i.UseUpper);
        Console.WriteLine($"x: {x}, y: {y}, vx: {vv.x}, vy: {vv.y}, v: {v}");

        return run.Normalized() * vv.x + -i.Gravity.Normalized() * vv.y;
    }

    private static Vector2d CalcVelocity2d(double x, double y, double g, double v, bool useUpper)
    {
        var m    = useUpper ? 1 : -1;
        var minV = Math.Sqrt(g * (y + Mathm.Hypot(x, y))) * 1.0001;
        v.MaxWith(minV);
        var c    = g * x / v / v;
        var sqrt = Math.Sqrt(1 - c * c - 2 * g * y / v / v);
        var a  = Math.Atan((1 + m * sqrt) / c);
        var vx = v * Math.Cos(a); //backwards.
        var vy = v * Math.Sin(a);
        return new Vector2d(vx, vy);
    }

    public struct TargetingParams
    {
        public Vector3 StartPos;
        public Vector3 EndPos;
        public Vector3 Gravity;
        public float MinLaunchVelocity;
        public bool UseUpper;

        public TargetingParams(Vector3 startPos, Vector3 endPos, Vector3 gravity, float minLaunchVelocity, bool useUpper)
        {
            StartPos = startPos;
            EndPos = endPos;
            Gravity = gravity;
            MinLaunchVelocity = minLaunchVelocity;
            UseUpper = useUpper;
        }
    }
}
}