using System;
using System.Diagnostics;
using System.Threading;
using Object = Godot.Object;
using Thread = System.Threading.Thread;

namespace MusicMachine.Scenes
{
public class AlternateProcess : Object
{
    private const long MinTicks = 5 * 1000 * 10; //5 milliseconds, or 200 times a second.
    private static readonly AlternateProcess Instance = new AlternateProcess();
    private readonly Thread _thread;

    public static event Action<long> TickLoop
    {
        add => Instance.Processes += value;
        remove => Instance.Processes -= value;
    }

    private event Action<long> Processes = delegate { };

    private AlternateProcess()
    {
        _thread = new Thread(ThreadStart);
    }
    internal static void Start()
    {
        Instance._thread.Start();
    }
    internal static void Stop()
    {
        Instance._thread.Interrupt();
    }
    private void ThreadStart()
    {
        try
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                var elapsedTicks = stopwatch.ElapsedTicks;
                Thread.Sleep(new TimeSpan(Math.Max(0, MinTicks - elapsedTicks)));
                elapsedTicks = stopwatch.ElapsedTicks;
                stopwatch.Restart();
                var theProcesses = Processes;
                theProcesses?.Invoke(elapsedTicks);
            }
        }
        catch (ThreadInterruptedException)
        {
        }
    }
}
}