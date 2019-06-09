using System;
using System.Diagnostics;
using System.Threading;
using Godot;
using Thread = System.Threading.Thread;

namespace MusicMachine.Scenes.Global
{
public sealed class AudioProcess : Node
{
    private const long BufferTime = 1000;
    private const long MinTicks = 5 * 1000 * 10; //5 milliseconds, or 200 times a second.
    private static AudioProcess _instance;
    private readonly Thread _audioLoopThread;

    private AudioProcess()
    {
        _audioLoopThread = new Thread(ThreadStart);
    }

    public static AudioProcess Instance
    {
        get => _instance ?? throw new InvalidOperationException($"No {nameof(AudioProcess)} node instantiated");
        private set
        {
            if (value != null && _instance != null)
                throw new InvalidOperationException($"Two {nameof(AudioProcess)} nodes instantiated!");
            _instance = value;
        }
    }

    public override void _EnterTree()
    {
        Instance = this;
        _audioLoopThread.Start();
    }

    public override void _ExitTree()
    {
        Instance = null;
        _audioLoopThread.Interrupt();
    }

    public static event Action<long> OnProcess
    {
        add => Instance.Processes += value;
        remove => Instance.Processes -= value;
    }

    private event Action<long> Processes;

    private void ThreadStart()
    {
        try
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                var elapsedTicks = stopwatch.ElapsedTicks;
                Thread.Sleep(new TimeSpan(Math.Max(0, MinTicks - elapsedTicks - BufferTime)));
                elapsedTicks = stopwatch.ElapsedTicks;
                stopwatch.Restart();
                try
                {
                    var processes = Processes;
                    processes?.Invoke(elapsedTicks);
                } catch (Exception e)
                {
                    GD.PrintErr(e);
                    throw;
                }
            }
        } catch (ThreadInterruptedException)
        {
        }
    }

    public override void _Notification(int what)
    {
        if (what == MainLoop.NotificationWmQuitRequest)
            OnQuit();
    }

    private void OnQuit()
    {
        _audioLoopThread.Interrupt();
    }
}
}