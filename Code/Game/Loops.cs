using System;
using System.Diagnostics;
using System.Threading;
using Godot;
using Thread = System.Threading.Thread;

namespace MusicMachine.Game
{
public class Loops
{
    private const long MinTicks = 5 * 1000 * 10; //5 milliseconds, or 200 times a second.
    private static readonly Loops Instance = new Loops();
    private readonly Thread _thread;

    private Loops()
    {
        _thread = new Thread(ThreadStart);
    }

    public static event Action<long> AudioProcess = delegate { };

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
                var theProcesses = AudioProcess;
                try
                {
                    theProcesses?.Invoke(elapsedTicks);
                } catch (ThreadInterruptedException)
                {
                } catch (ThreadAbortException)
                {
                    break;
                } catch (Exception e)
                {
                    GD.PrintErr(e);
                }
            }
        } catch (ThreadInterruptedException)
        {
        }
    }
}
}