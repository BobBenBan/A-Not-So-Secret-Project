using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Game;
using MusicMachine.Util;

namespace MusicMachine.Tracks
{
public partial class Program
{
}

/// <summary>
///     Unlike a <i>steppers</i> this one is delegate based, since we would be using this to PLAY, rather
///     than gather info.
/// </summary>
public class ProgramPlayer : Node, IProcessable
{
    private readonly List<Pair<long, object>> _cachedList = new List<Pair<long, object>>();
    private readonly Program _program;

    private readonly List<Pair<int, Track<object>.Stepper>> _steppers =
        new List<Pair<int, Track<object>.Stepper>>(); //colors!!!

    private long _curTimeTicks;
    private bool _playing;
    private ProcessMode _processMode;
    private bool _started;

    /// <summary>
    ///     Will trigger on event steps.
    ///     Arguments are:
    ///     track index, Time, Event.
    /// </summary>
    public Action<int, long, object> OnEvent;

    internal ProgramPlayer(Program program, ProcessMode processMode = ProcessMode.Physics)
    {
        _program = program;
        program.LoadMappedTracks();

        ProcessMode = processMode;
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

    private TempoMap TempoMap => _program.TempoMap;

    private long CurMicros => (_curTimeTicks + 5) / 10;

    public bool Playing
    {
        get => _playing;
        private set
        {
            if (Playing) this.AddProcess(ProcessMode);
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
        foreach (var stepperPair in _steppers)
        {
            var stepper = stepperPair.Second;
            if (!stepper.StepToInclusive((_curTimeTicks + 5) / 10))
                continue;
            var theEvent = OnEvent;
            if (theEvent == null)
                continue;
            stepper.CopyCurrentTo(_cachedList);
            foreach (var eventPair in _cachedList)
                theEvent(stepperPair.First, eventPair.First, eventPair.Second);
        }
        _steppers.RemoveAll(x => x.Second.IsCurrentlyDone);
    }

    public void Play(long at = 0)
    {
        _curTimeTicks = at * 10;
        _started      = false;
        for (var index = 0; index < _program._mappedTracks.Count; index++)
        {
            var track   = _program._mappedTracks[index];
            var stepper = track.GetStepper(Math.Min(_curTimeTicks, track.Times.First()));
            _steppers.Add(new Pair<int, Track<object>.Stepper>(index, stepper));
        }
        Playing = true;
    }

    public void Stop()
    {
        Playing = false;
        _steppers.Clear();
    }

    public void Step(float seconds) => StepTicks((seconds * 10e6).RoundToLong());
}
}