using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace MusicMachine.Music
{
public class MidiSongStepper
{
    private readonly List<KeyValuePair<FBN, Track<IMusicEvent>.Stepper>> _steppers =
        new List<KeyValuePair<FBN, Track<IMusicEvent>.Stepper>>();

    private readonly List<KeyValuePair<long, IMusicEvent>> _tempList = new List<KeyValuePair<long, IMusicEvent>>();
    private long? _curMidiTicks;
    private long _curTicks;
    private bool _started;
    private TempoMap _tempoMap;

    private long CurTicks
    {
        get => _curTicks;
        set
        {
            _curMidiTicks = null;
            _curTicks     = value;
        }
    }

    public bool Playing => _steppers.Count > 0;

    private long CurMidiTicks
    {
        get
        {
            if (_curMidiTicks == null)
            {
                _curMidiTicks = TimeConverter.ConvertTo<MidiTimeSpan>(
                    new MetricTimeSpan((CurTicks + 5) / 10),
                    _tempoMap);
            }
            // ReSharper disable once PossibleInvalidOperationException
            return _curMidiTicks.Value;
        }
    }

    public event Action<FBN, long, IMusicEvent> OnEvent;

    public void BeginPlay(MidiSong midiSong, long start = 0)
    {
//        Clear();

        _tempoMap = midiSong.TempoMap;
        _started  = false;
        CurTicks  = start;
        foreach (var track in midiSong.Tracks)
        {
            var stepper = track.GetStepper(start);
            stepper.CurTimeInclusive = CurMidiTicks;
            _steppers.Add(new KeyValuePair<FBN, Track<IMusicEvent>.Stepper>(track.Channel, stepper));
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
            CurTicks += ticks;

        foreach (var stepperPair in _steppers)
        {
            var stepper = stepperPair.Value;
            var channel = stepperPair.Key;
            if (!stepper.StepToInclusive(CurMidiTicks))
                continue;
            if (OnEvent == null)
                continue;
            stepper.CopyCurrentTo(_tempList);
            foreach (var eventPair in _tempList)
                OnEvent(channel, eventPair.Key, eventPair.Value);
        }
        _steppers.RemoveAll(x => x.Value.IsCurrentlyDone);
        return true;
    }
}
}