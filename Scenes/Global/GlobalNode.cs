using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Godot;
using Thread = System.Threading.Thread;

namespace MusicMachine.Scenes
{
public class GlobalNode : Node
{
    private const long MinTicks = 5 * 1000 * 10; //5 milliseconds, or 200 times a second.
    private const long BufferTime = 1000;
    private readonly Thread _audioLoopThread;
    private GlobalNode _instance;

    private GlobalNode()
    {
        _audioLoopThread = new Thread(ThreadStart);
    }

    public GlobalNode Instance
    {
        get => _instance ?? throw new InvalidOperationException("No global node instantiated...");
        private set
        {
            if (_instance != null) throw new InvalidOperationException("Two singletons created. That's a no-no.");
            _instance = value;
        }
    }

    private static readonly ConcurrentDictionary<Action<long>, bool> AudioProcesses =
        new ConcurrentDictionary<Action<long>, bool>();

    public static event Action<long> AudioProcess
    {
        add
        {
            if (value != null)
                AudioProcesses.TryAdd(value, true);
        }
        remove => AudioProcesses.TryRemove(value, out _);
    }

    private readonly List<Action<long>> _processesList = new List<Action<long>>();

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
                _processesList.AddRange(AudioProcesses.Keys);
                foreach (var action in _processesList)
                    try
                    {
                        action(elapsedTicks);
                    }
                    catch (Exception e)
                    {
                        GD.PrintErr(e);
                        throw;
                    }
                _processesList.Clear();
            }
        }
        catch (ThreadInterruptedException)
        {
        }
        finally
        {
            _processesList.Clear();
        }
    }

    public override void _Ready()
    {
        Instance = this;
        _audioLoopThread.Start();
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