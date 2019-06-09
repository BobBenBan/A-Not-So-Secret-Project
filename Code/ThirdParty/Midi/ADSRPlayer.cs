using Godot;
using Godot.Collections;
using Melanchall.DryWetMidi.Common;
using MusicMachine.Programs;
using MusicMachine.Util.Maths;
using GDObject = Godot.Object;

namespace MusicMachine.ThirdParty.Midi
{
public class AdsrPlayer : AudioStreamPlayer
{
//    private bool _debug;
    private Instrument _instrument;
    private bool _requestRelease;
    public float AmpDb;
    public float BaseVolumeDb;

    public Instrument Instrument
    {
        private get => _instrument;
        set
        {
            if (_instrument == value && value == null)
                return;
            Stream      = (AudioStreamSample) value.Stream.Duplicate();
            _instrument = value;
        }
    }

    private new AudioStreamSample Stream
    {
        get => (AudioStreamSample) base.Stream;
        set => base.Stream = value;
    }

    public float MaximumVolumeDb { get; set; } = -8;

    public SevenBitNumber Velocity { get; set; }

    private float MinimumVolumeDb { get; } = -108;

    public bool Releasing { get; private set; }

    public float PitchBend
    {
        set => PitchScale = Mathf.Pow(2, value * Program.MaxSemitonesPitchBend / 12);
    }

    private float MixRate => Instrument.MixRate;

    public float UsingTimer { get; private set; }

    private float Timer { get; set; }

    public float CurrentVolumeRange { get; private set; }

    private State[] AdsState => Instrument.AdsState;

    private State[] ReleaseState => Instrument.ReleaseState;

    public float VolumeRange
    {
        set
        {
            var noteVol = value * Velocity / 127f;
            MaximumVolumeDb = (noteVol - 1) * AmpDb + BaseVolumeDb;
        }
    }

    public new void Play(float fromPosition = 0)
    {
        Releasing          = false;
        _requestRelease    = false;
        Timer              = 0;
        UsingTimer         = 0;
        CurrentVolumeRange = AdsState[0].Volume;
        Stream.MixRate     = MixRate.RoundToInt();
        base.Play(fromPosition);
        UpdateVolume();
//        _debug = false;
    }

//    private int CalcMixRate() => (MixRate * (1 + PitchBend / 2)).RoundToInt();

    public override void _Ready()
    {
        SetProcess(false);
    }

    public void StartRelease() => _requestRelease = true;

    public override void _Process(float delta)
    {
        DoProcess((delta * 10e6).RoundToLong());
    }

    public void DoProcess(long ticks)
    {
        if (!Playing)
            return;
        Timer      += ticks / 10e6f;
        UsingTimer += ticks / 10e6f;
        var useState  = Releasing ? ReleaseState : AdsState;
        var allStates = useState.Length;
        var lastState = allStates - 1;
        if (useState[lastState].Time <= Timer)
        {
            CurrentVolumeRange = useState[lastState].Volume;
            if (Releasing)
                Stop();
        }
        else
        {
            for (var i = 1; i < allStates; i++)
            {
                var state = useState[i];
                if (Timer < state.Time)
                {
                    var prevState = useState[i - 1];
                    var s         = Mathf.InverseLerp(prevState.Time, state.Time, Timer);
                    CurrentVolumeRange = Mathf.Lerp(prevState.Volume, state.Volume, s);
                    break;
                }
            }
        }
        UpdateVolume();
        if (_requestRelease && !Releasing)
        {
            Releasing          = true;
            CurrentVolumeRange = ReleaseState[0].Volume;
            Timer              = 0;
        }
    }

    private void UpdateVolume()
    {
        VolumeDb = Mathf.Lerp(MinimumVolumeDb, MaximumVolumeDb, CurrentVolumeRange);
    }
}

public struct State
{
    public float Time;
    public float Volume;

    internal static State[] Create(object obj)
    {
        var arr = (Array) obj;
        var o   = new State[arr.Count];
        for (var index = 0; index < arr.Count; index++)
        {
            var point = (Dictionary) arr[index];
            o[index] = new State {Time = (float) point["time"], Volume = (float) point["volume"]};
        }
        return o;
    }

    public override string ToString() => $"{nameof(Time)}: {Time}, {nameof(Volume)}: {Volume}";
}
}