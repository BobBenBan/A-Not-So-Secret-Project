using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using MusicMachine.Programs;

namespace MusicMachine.Scenes.Functional.Tracks
{
/// <summary>
///     Unlike a <i>steppers</i> this one is delegate based, since we would be using this to PLAY, rather
///     than gather info.
/// </summary>
// TODO TO DO TO DO TO DO TO DO TO DO TO DO TO DO CONCURRENCY ISSUES
public class ActionPlayer : ProcessNode
{
    private readonly List<Action> _cachedList = new List<Action>();
    private long _curTimeTicks;
    private bool _started;
    private Track<Action>.Stepper _stepper;
    private Track<Action> _track;

    public ActionPlayer(Track<Action> track = null, Mode mode = Mode.Physics)
    {
        Track       = track;
        Enabled     = false;
        ProcessMode = mode;
    }

    public Track<Action> Track
    {
        get => _track;
        set
        {
            Stop();
            _track = value;
        }
    }

    private long CurMicros => (_curTimeTicks + 5) / 10;

    public bool Playing
    {
        get => Enabled;
        private set => Enabled = value;
    }

    protected override void StepTicks(long ticks)
    {
        if (ticks < 0)
            throw new InvalidOperationException("Attempted to step backwards...");
        Debug.Assert(Playing);
        if (!_started)
            _started = true;
        else
            _curTimeTicks += ticks;
        if (!_stepper.StepToInclusive(CurMicros))
        {
            Stop();
            return;
        }
        _stepper.CopyCurrentTo(_cachedList);
        foreach (var action in _cachedList)
            action?.Invoke();
    }

//todo: at micros. Thats a big task.
    public void Play()
    {
        if (Track == null)
        {
            GD.PushWarning("No program to play!");
            return;
        }
        _curTimeTicks = 0;
        _started      = false;
        _stepper      = Track.GetStepper();
        Playing       = true;
    }

    public void Stop()
    {
        _stepper = null;
        Playing  = false;
    }
}
}