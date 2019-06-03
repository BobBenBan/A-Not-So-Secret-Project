using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Game;
using MusicMachine.Util;

namespace MusicMachine.Tracks
{

/// <summary>
///     Unlike a <i>steppers</i> this one is delegate based, since we would be using this to PLAY, rather
///     than gather info.
/// </summary>
public class ProgramPlayer : Node, IProcessable
{
    private readonly List<Action> _cachedList = new List<Action>();
    private Program _program;

    private readonly List<Track<Action>.Stepper> _steppers =
        new List<Track<Action>.Stepper>();

    private long _curTimeTicks;
    private bool _playing;
    private ProcessMode _processMode;
    private bool _started;

    public ProgramPlayer(Program program = null, ProcessMode processMode = ProcessMode.Audio)
    {
        _program = program;
        ProcessMode = processMode;
    }

    public override void _PhysicsProcess(float delta)
    {
        Step(delta);
    }

    public override void _Process(float delta)
    {
        Step(delta);
    }

    public ProcessMode ProcessMode
    {
        get => _processMode;
        set
        {
            if (Playing && _processMode != value)
            {
                this.RemoveProcess(_processMode);
                this.AddProcess(value);
            }
            _processMode = value;
        }
    }

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
    }

    private TempoMap TempoMap => _program.TempoMap;

    private long CurMicros => (_curTimeTicks + 5) / 10;

    public bool Playing
    {
        get => _playing;
        private set
        {
            if (value) this.AddProcess(ProcessMode);
            else this.RemoveProcess(ProcessMode);
            _playing = value;
        }
    }

    public void StepTicks(long ticks)
    {
        if (ticks < 0)
            throw new InvalidOperationException("Attempted to step backwards...");
        if (!Playing) return;
        if (_steppers.Count == 0)
        {
            Stop();
            return;
        }
        if (!_started)
            _started = true;
        else
            _curTimeTicks += ticks;
        foreach (var stepper in _steppers)
        {
            if (!stepper.StepToInclusive((_curTimeTicks + 5) / 10))
                continue;
            stepper.CopyCurrentTo(_cachedList);
            foreach (var action in _cachedList)
            {
                if(action!=null)
                    Console.WriteLine($"invoking action... {action}");
                action?.Invoke();
            }
        }
        _steppers.RemoveAll(x => x.IsCurrentlyDone);
    }

    public void Play(long atMicros = 0)
    {
        if (_program == null)
        {
            GD.PushWarning("No program to play!");
            return;
        }
        _curTimeTicks = atMicros * 10;
        _started      = false;
        _program.LoadMappedTracks();
        foreach (var track in _program.MappedTracks)
        {
            var stepper = track.GetStepper(Math.Min(_curTimeTicks, track.Times.First()));
            _steppers.Add(stepper);
        }
        Playing = true;
    }

    public void Stop()
    {
        Playing = false;
        _steppers.Clear();
    }

    private void Step(float seconds) => StepTicks((seconds * 10e6).RoundToLong());
}
}