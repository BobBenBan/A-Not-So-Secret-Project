using System;

namespace MusicMachine
{
public static class Mathm
{
    public static int Lerpi(int from, int to, float step) => (from + (to - from) * step).RoundToInt();
    public static byte Lerpb(byte from, byte to, float step) => (from + (to - from) * step).RoundToByte();
    public static byte RoundToByte(this float d) => (byte) Math.Round(d);
    public static int RoundToInt(this double d) => (int) Math.Round(d);
    public static int RoundToInt(this float d) => (int) Math.Round(d);
    public static long RoundToLong(this double d) => (long) Math.Round(d);
}
}