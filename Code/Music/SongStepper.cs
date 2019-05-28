using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace MusicMachine.Music
{
public class SongStepper
{
    private readonly List<KeyValuePair<int, Track<IInstTrackEvent>.Stepper>> _steppers =
        new List<KeyValuePair<int, Track<IInstTrackEvent>.Stepper>>();

    private readonly List<KeyValuePair<long, IInstTrackEvent>> _cachedList =
        new List<KeyValuePair<long, IInstTrackEvent>>();

    private long? _curMidiTicks;
    private long _curTimeTicks;
    private bool _started;
    private TempoMap _tempoMap;

    private long CurTimeTicks
    {
        get => _curTimeTicks;
        set
        {
            _curMidiTicks = null;
            _curTimeTicks = value;
        }
    }

    public bool Playing => _steppers.Count > 0;

    private long CurMidiTicks
    {
        get
        {
            if (_curMidiTicks == null)
                _curMidiTicks = TimeConverter.ConvertFrom(new MetricTimeSpan((CurTimeTicks + 5) / 10), _tempoMap);
            // ReSharper disable once PossibleInvalidOperationException
            return _curMidiTicks.Value;
        }
    }

    /// <summary>
    ///     Will trigger on event steps.
    ///     Arguments are:
    ///     TRACK INDEX,
    /// </summary>
    public event Action<int, long, IInstTrackEvent> OnEvent;

    public void BeginPlay(Song song, long startMidiTicks = 0)
    {
//        Clear();

        _tempoMap    = song.TempoMap;
        _started     = false;
        CurTimeTicks = TimeConverter.ConvertTo<MetricTimeSpan>(startMidiTicks, _tempoMap).TotalMicroseconds;
        for (var index = 0; index < song.Tracks.Count; index++)
        {
            var track   = song.Tracks[index];
            var stepper = track.GetStepper(Math.Min(CurMidiTicks, track.Times.First()));
            _steppers.Add(new KeyValuePair<int, Track<IInstTrackEvent>.Stepper>(index, stepper));
        }
    }

    public void Clear()
    {
        _steppers.Clear();
        _tempoMap = null;
        _started  = false;
    }

    public bool Step(float seconds) => StepTicks((seconds * 10e6).RoundToLong());

    public bool StepTicks(long ticks)
    {
        if (ticks < 0)
            throw new InvalidOperationException("Attempted to step backwards...");
        if (_steppers.Count == 0)
            return false;
        if (!_started)
            _started = true;
        else
            CurTimeTicks += ticks;
        foreach (var stepperPair in _steppers)
        {
            var stepper = stepperPair.Value;
            if (!stepper.StepToInclusive(CurMidiTicks))
                continue;
            var theEvent = OnEvent;
            if (theEvent == null)
                continue;
            stepper.CopyCurrentTo(_cachedList);
            foreach (var eventPair in _cachedList)
                theEvent(stepperPair.Key, eventPair.Key, eventPair.Value);
        }
        _steppers.RemoveAll(x => x.Value.IsCurrentlyDone);
        return true;
    }
}
}