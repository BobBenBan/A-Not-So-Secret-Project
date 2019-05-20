using System;

namespace MusicMachine
{
public static class Mathm
{
    public static byte RoundToByte(this float d) => (byte) Math.Round(d);

    public static int RoundToInt(this double d) => (int) Math.Round(d);

    public static int RoundToInt(this float d) => (int) Math.Round(d);

    public static long RoundToLong(this double d) => (long) Math.Round(d);

    public static long SecondsToMicros(this float f) => (f * 1_000_000.0).RoundToLong();

    public static int Lerpi(int from, int to, float step) => (from + (to - from) * step).RoundToInt();
}
}