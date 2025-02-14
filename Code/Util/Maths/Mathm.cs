using System;

namespace MusicMachine.Util.Maths
{
public static class Mathm
{
    public static byte RoundToByte(this float d) => (byte) Math.Round(d);

    public static int RoundToInt(this double d) => (int) Math.Round(d);

    public static int RoundToInt(this float d) => (int) Math.Round(d);

    public static long RoundToLong(this double d) => (long) Math.Round(d);

    public static int Lerpi(int from, int to, float step) => (from + (to - from) * step).RoundToInt();

    public static void Constrain(this ref long value, long min, long max)
    {
        if (value < min)
            value = min;
        else if (value > max)
            value = max;
    }

    public static void Constrain(this ref int value, int min, int max)
    {
        if (value < min)
            value = min;
        else if (value > max)
            value = max;
    }

    public static void Constrain(this ref float value, float min, float max)
    {
        if (value < min)
            value = min;
        else if (value > max)
            value = max;
    }

    public static void MinWith(this ref long value, long min)
    {
        value = Math.Min(value, min);
    }

    public static void MinWith(this ref int value, int min)
    {
        value = Math.Min(value, min);
    }

    public static void MinWith(this ref double value, double min)
    {
        value = Math.Min(value, min);
    }

    public static void MaxWith(this ref long value, long max)
    {
        value = Math.Max(value, max);
    }

    public static void MaxWith(this ref int value, int max)
    {
        value = Math.Max(value, max);
    }

    public static void MaxWith(this ref double value, double max)
    {
        value = Math.Max(value, max);
    }

    public static bool InRange(this int value, int min, int maxExclusive) => value >= min && value < maxExclusive;

    public static double Hypot(double x, double y)
    {
        return Math.Sqrt(x * x + y * y);
    }
}
}