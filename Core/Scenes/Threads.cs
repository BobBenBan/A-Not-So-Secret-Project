using System;
using System.Diagnostics;
using System.Threading;
using Godot;
using Object = Godot.Object;
using Thread = System.Threading.Thread;

namespace MusicMachine.Scenes
{
public class Looper : Object
{
    [Signal]
    public delegate void Process(long micros);

    public const string SignalName = nameof(System.Diagnostics.Process);
    private const long MinTicks = 50000; //50 millisecond, or 200 times a second.
    public static readonly Looper Instance = new Looper();
    private readonly Thread _thread;
    public int Running;
    private Looper()
    {
        _thread = new Thread(ThreadStart);
    }
    internal void Start()
    {
        _thread.Start();
    }
    internal void Stop()
    {
        _thread.Interrupt();
    }
    private void ThreadStart()
    {
        try
        {
            Interlocked.Exchange(ref Running, 1);
            var stopwatch = new Stopwatch();
            //ticks are in 1/10 of a microsecond
            stopwatch.Start();
            while (true)
            {
                var elapsedTicks = stopwatch.ElapsedTicks;
                Thread.Sleep(new TimeSpan(Math.Max(0, MinTicks - elapsedTicks)));
                elapsedTicks = stopwatch.ElapsedTicks;
                stopwatch.Restart();
                EmitSignal(nameof(Process), elapsedTicks / 10);
            }
        }
        catch (ThreadInterruptedException)
        {
        }
        finally
        {
            Interlocked.Exchange(ref Running, 0);
        }
    }
}
}