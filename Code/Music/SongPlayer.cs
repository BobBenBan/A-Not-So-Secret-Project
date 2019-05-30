using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Scenes;
using MusicMachine.ThirdParty.Midi;

namespace MusicMachine.Music
{
//this is more of a tester.
public class SongPlayer : Node
{
    private readonly PlayingState[] _playingState;
    private readonly Song _song;
    private readonly SongStepper _stepper = new SongStepper();
    private Bank _bank;
    private AdsrPlayer[] _players;
    private bool _ready;
    [Export] public float AmpDb = 20;
    [Export] public string Bus = "master";
    [Export] public int MaxPolyphony = 128;
    [Export] public AudioStreamPlayer.MixTargetEnum MixTarget = AudioStreamPlayer.MixTargetEnum.Stereo;
    [Export] public string SoundFontFile = "";
    [Export] public float VolumeDb = -10;

    public SongPlayer(Song song)
    {
        _song            =  song;
        _stepper.OnEvent += OnEvent;
        _playingState    =  new PlayingState[_song.Tracks.Count];
        for (var i = 0; i < _playingState.Length; i++)
            _playingState[i] = new PlayingState();
    }

    public bool Playing { get; private set; }

    public IReadOnlyList<PlayingState> PlayingState => _playingState;

    public event Action OnStop = delegate { };

    public override void _Ready()
    {
        SetProcess(false);
        CreatePlayers();
        PrepareBank();
        _ready = true;
    }

    private void CreatePlayers()
    {
        if (_players != null)
            return;
        _players = new AdsrPlayer[MaxPolyphony];
        for (var index = 0; index < _players.Length; index++)
        {
            var player = new AdsrPlayer {MixTarget = MixTarget, Bus = Bus};
            AddChild(player, true);
            _players[index] = player;
        }
    }

    private void PrepareBank()
    {
        if (_bank != null)
            return;
        _bank = new Bank(SoundFontFile, _song.Tracks.Select(track => track.CombinedPresetNum).ToGdArray());
    }

    public void Play<TTImeSpan>(TTImeSpan atTime)
        where TTImeSpan : ITimeSpan
    {
        Play(TimeConverter.ConvertFrom(atTime, _song.TempoMap));
    }

    public void Play(long startMidiTicks = 0)
    {
        if (!_ready)
            throw new InvalidOperationException();
        Stop();
        Playing = true;
        _stepper.BeginPlay(_song, startMidiTicks);
        AlternateProcess.Loop += DoProcess;
//        SetProcess(true);
    }

    public void Stop()
    {
        if (!Playing)
            return;
        Playing = false;
        _stepper.Clear();
        StopAllNotes();
        OnStop.Invoke();
        AlternateProcess.Loop -= DoProcess;
//        SetProcess(false);
    }

    public override void _Process(float delta)
    {
        DoProcess((delta * 10e6).RoundToLong());
    }

    private void DoProcess(long ticks)
    {
        if (_bank == null)
            return;
        if (!Playing)
            return;
        foreach (var channel in _playingState)
            channel.ClearNotPlaying();
        if (!_stepper.StepTicks(ticks))
            Stop();
        foreach (var player in _players)
            player.DoProcess(ticks);
    }

    public void StopAllNotes()
    {
        foreach (var player in _players)
            player.Stop();
        foreach (var channel in _playingState)
            channel.NotesOn.Clear();
    }

    private void OnEvent(int index, long time, IInstTrackEvent @event)
    {
        var state = _playingState[index];
        var track = _song.Tracks[index];
//        if (index >= 4)
//            Console.WriteLine("{0}: {1}", track, @event);
//        Console.WriteLine($"Event: {@event}");
        switch (@event)
        {
        case NoteOnEvent noteOnEvent:
        {
            var keyNum = noteOnEvent.NoteNumber;
            //update??
            var preset     = _bank.GetPreset(track.Program, track.Bank);
            var instrument = preset[keyNum];
            if (instrument == null)
                return;
            if (state.NotesOn.TryGetValue(keyNum, out var stopPlayer))
                stopPlayer.StartRelease();
            var player = GetIdlePlayer();
            if (player == null)
                return;
            player.Velocity = noteOnEvent.Velocity;
            player.UpdateChannelVolume(AmpDb, VolumeDb, state);
            player.PitchBend = state.PitchBend;
            player.SetInstrument(instrument);
            player.Play();
//            if (!track.IsDrumTrack)
            state.NotesOn[keyNum] = player;
            break;
        }
        case NoteOffEvent noteOffEvent:
        {
            var keyNum = noteOffEvent.NoteNumber;
            if (!track.IsDrumTrack && state.NotesOn.TryGetAndRemove(keyNum, out var player))
                player?.StartRelease();
            break;
        }
        case VolumeChangeEvent volumeChangeEvent:
            state.State.Volume = volumeChangeEvent.Volume;
            state.UpdateVolume(AmpDb, VolumeDb);
            break;
        case ExpressionChangeEvent expressionChangeEvent:
        {
            state.State.Expression = expressionChangeEvent.Expression;
            state.UpdateVolume(AmpDb, VolumeDb);
            break;
        }
        case PitchBendEvent pitchBendEvent:
        {
            state.State.PitchBend = pitchBendEvent.PitchValue;
            state.UpdatePitchBend();
            break;
        }
        default:
            Console.WriteLine($"Ignored Event: {@event}");
            break;
        }
    }

    private AdsrPlayer GetIdlePlayer()
    {
        var        minVol        = 100f;
        AdsrPlayer stoppedPlayer = null;
        var        oldestTime    = -1f;
        AdsrPlayer oldestPlayer  = null;
        foreach (var player in _players)
        {
            if (!player.Playing)
                return player;
            if (player.Releasing && player.CurrentVolume < minVol)
            {
                stoppedPlayer = player;
                minVol        = player.CurrentVolume;
            }
            if (player.UsingTimer > oldestTime)
            {
                oldestPlayer = player;
                oldestTime   = player.UsingTimer;
            }
        }
        return stoppedPlayer ?? oldestPlayer;
    }
}
}