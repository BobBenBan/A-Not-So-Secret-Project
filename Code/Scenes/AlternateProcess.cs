using System;
using System.Diagnostics;
using System.Threading;
using Godot;
using Object = Godot.Object;
using Thread = System.Threading.Thread;

namespace MusicMachine.Scenes
{
public class AlternateProcess : Object
{
    private const long MinTicks = 5 * 1000 * 10; //5 milliseconds, or 200 times a second.
    private static readonly AlternateProcess Instance = new AlternateProcess();
    private readonly Thread _thread;

    private AlternateProcess()
    {
        _thread = new Thread(ThreadStart);
    }

    public static event Action<long> Loop = delegate { };

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
                var theProcesses = Loop;
                try
                {
                    theProcesses?.Invoke(elapsedTicks);
                }
                catch (ThreadInterruptedException)
                {
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception e)
                {
                    GD.PrintErr(e);
                }
            }
        }
        catch (ThreadInterruptedException)
        {
        }
    }
}
}