using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using MusicMachine.Scenes.Functional;
using MusicMachine.Util;

namespace MusicMachine.Tracks
{
/// <summary>
///     Unlike a <i>steppers</i> this one is delegate based, since we would be using this to PLAY, rather
///     than gather info.
/// </summary>
public class ProgramPlayer : ProcessOnlyNode
{
    private readonly List<Action> _cachedList = new List<Action>();
    private Program _program;
    private Track<Action> _mappedTrack = null;

    public Program Program
    {
        get => _program;
        set
        {
            _mappedTrack = null;
            _program = value;
        }
    }

    private Track<Action>.Stepper _stepper = null;
    private long _curTimeTicks;
    private bool _started;

    public ProgramPlayer(Program program = null, Mode mode = Mode.Physics)
    {
        Enabled = false;
        _program = program;
        ProcessMode = mode;
    }

    protected override void Ready()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
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
        Debug.Assert(!Playing);
        if (!_started)
            _started = true;
        else
            _curTimeTicks += ticks;
        if (!_stepper.StepToInclusive((_curTimeTicks + 5) / 10))
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
        if (_program == null)
        {
            GD.PushWarning("No program to play!");
            return;
        }
        _curTimeTicks = 0;
        _started = false;
        _mappedTrack = _program.GetMappedTrack(); //perhaps refactor this out one day in the future.
        _stepper = _mappedTrack.GetStepper();

        Playing = true;
    }

    public void Stop()
    {
        _stepper = null;
        Playing = false;
    }

    private void Step(float seconds) => StepTicks((seconds * 10e6).RoundToLong());
}
}