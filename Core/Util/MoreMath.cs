using System;

namespace MusicMachine.Util
{
public static class MoreMath
{
    public static int Lerpi(int from, int to, double step)
    {
        return (int) (from + (to - from) * step + 0.5);
    }

    public static int RoundToInt(this double d)
    {
        return (int)Math.Round(d);
    }
}
}