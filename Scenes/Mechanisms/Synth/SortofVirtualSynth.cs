using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Interaction;
using MusicMachine.Programs;
using MusicMachine.Programs.Mappers;
using MusicMachine.Scenes.Functional;
using MusicMachine.ThirdParty.Midi;
using MusicMachine.Util;

namespace MusicMachine.Scenes.Mechanisms
{
//this is more of a tester.

//TODO: FIX CONCURRENCY
public partial class SortofVirtualSynth : ProcessNode
{
    private Bank _bank;
    private ChannelState[] _channelStates;
    private AdsrPlayer[] _players;
    private bool _ready;
    private Array<int> _usedPresetNumbers = new Array<int>();
    [Export] public int NumChannels = 24;
    [Export] public string SoundFontFile = "";

    public SortofVirtualSynth(Mode mode = Mode.Audio)
    {
        Enabled     = true;
        ProcessMode = mode;
    }

    [Export] public float AmpDb { get; private set; } = 20;

    [Export] public string Bus { get; private set; } = "master";

    [Export] public int MaxPolyphony { get; private set; } = 150;

    [Export]
    public AudioStreamPlayer.MixTargetEnum MixTarget { get; private set; } = AudioStreamPlayer.MixTargetEnum.Stereo;

    [Export] public float VolumeDb { get; private set; } = -10;

    [Export]
    public Array<int> UsedPresetNumbers
    {
        get => _usedPresetNumbers;
        set
        {
            var diff = value != _usedPresetNumbers;
            _usedPresetNumbers = value;
            if (diff && _ready)
            {
                StopAllNotes();
                PrepareBank();
                _bank.UpdateUsedProgNums(value);
            }
        }
    }

    protected override void Ready()
    {
        InitChannels();
        CreatePlayers();
        PrepareBank();
        SetProcess(false);
        _ready = true;
    }

    private void InitChannels()
    {
        _channelStates = new ChannelState[NumChannels];
        for (var i = 0; i < _channelStates.Length; i++)
            _channelStates[i] = new ChannelState(AmpDb, VolumeDb);
    }

    public override void _ExitTree()
    {
    }

    public void MatchProgram(Program program)
    {
        for (var index = 0; index < Math.Min(program.Tracks.Count, _channelStates.Length); index++)
        {
            var track = program.Tracks[index] as MusicTrack;
            if (track == null) continue;
            _channelStates[index].Bank        = track.Bank;
            _channelStates[index].Program     = track.Program;
            _channelStates[index].IsDrumTrack = track.IsDrumTrack;
        }
    }

    private void CreatePlayers()
    {
        if (_players != null)
            return;
        _players = new AdsrPlayer[MaxPolyphony];
        for (var index = 0; index < _players.Length; index++)
        {
            var player = new AdsrPlayer
            {
                MixTarget = MixTarget,
                Bus       = Bus,
                AmpDb     = AmpDb,
                VolumeDb  = VolumeDb
            };
            AddChild(player, true);
            _players[index] = player;
        }
    }

    private void PrepareBank()
    {
        if (_bank != null)
            return;
        if (SoundFontFile != null)
            _bank = new Bank(SoundFontFile, UsedPresetNumbers);
        else
            GD.PushWarning("No soundfont supplied!");
    }

    protected override void StepTicks(long ticks)
    {
        if (_bank == null)
            return;
        foreach (var channel in _channelStates)
            channel.ClearNotPlaying();
        foreach (var player in _players)
            player.DoProcess(ticks);
    }

    public void StopAllNotes()
    {
        foreach (var playingState in _channelStates)
        {
            foreach (var player in playingState.NotesOn.Values)
                player.Stop();
            playingState.NotesOn.Clear();
        }
        foreach (var player in _players)
            player.Stop();
    }

    public void Reset()
    {
        foreach (var state in _channelStates)
            state.ResetState();
    }

    public void SendEvent(int channelNum, MusicEvent @event)
    {
        if (_bank == null)
            return;
//        Console.WriteLine("Recieved event" + @event);
        var channel = _channelStates[channelNum];
//        if (index >= 4)
//            Console.WriteLine("{0}: {1}", track, @event);
//        Console.WriteLine($"Event: {@event}");
        switch (@event)
        {
        case NoteOnEvent noteOnEvent:
        {
            ProcessNoteOn(noteOnEvent, channel);
            break;
        }
        case NoteOffEvent noteOffEvent:
        {
            var keyNum = noteOffEvent.NoteNumber;
            if (!channel.IsDrumTrack && channel.NotesOn.TryGetAndRemove(keyNum, out var player))
                player?.StartRelease();
            break;
        }
        case MusicStateEvent musicStateEvent:
            musicStateEvent.ApplyToState(ref channel.PlayingState);
            break;
        default:
            Console.WriteLine($"Unprocessed MusicEvent: {@event}");
            break;
        }
    }

    private void ProcessNoteOn(NoteOnEvent noteOnEvent, ChannelState channel)
    {
        var keyNum = noteOnEvent.NoteNumber;
        //update??
        var preset     = _bank.GetPreset(channel.Program, channel.Bank);
        var instrument = preset[keyNum];
        if (instrument == null)
            return;

        if (channel.NotesOn.TryGetValue(keyNum, out var stopPlayer))
            stopPlayer.StartRelease();
        var player = GetIdlePlayer();
        if (player == null)
            return;
        player.Instrument  = instrument;
        player.Velocity    = noteOnEvent.Velocity;
        player.VolumeRange = channel.VolumeRange * channel.ExpressionRange;
        player.PitchBend   = channel.PitchBendRange;
        player.Play();
//            if (!track.IsDrumTrack)
        channel.NotesOn[keyNum] = player;
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
            if (player.Releasing && player.CurrentVolumeRange < minVol)
            {
                stoppedPlayer = player;
                minVol        = player.CurrentVolumeRange;
            }
            if (player.UsingTimer > oldestTime)
            {
                oldestPlayer = player;
                oldestTime   = player.UsingTimer;
            }
        }
        return stoppedPlayer ?? oldestPlayer;
    }

    public ITrackMapper GetMapper() => new SynthTrackMapper(this);

    private sealed class SynthTrackMapper : ITrackMapper
    {
        private readonly SortofVirtualSynth _synth;

        public SynthTrackMapper(SortofVirtualSynth synth)
        {
            _synth = synth;
        }

        public ICompletionAwaiter Prepare(AnyTrack track, MappingInfo info) => new AlreadyDoneCompletionAwaiter();

        public IEnumerable<Pair<long, Action>> MapTrack(AnyTrack track, MappingInfo info)
        {
            if (!(track is MusicTrack musicTrack)) return Enumerable.Empty<Pair<long, Action>>();
            for (var index = 0; index < _synth._channelStates.Length; index++)
            {
                var channel = _synth._channelStates[index];
                if (channel.Program == musicTrack.Program && channel.Bank == musicTrack.Bank)
                    return DoMapTrack(musicTrack, index, info.TempoMap);
            }
            return Enumerable.Empty<Pair<long, Action>>();
        }

        private IEnumerable<Pair<long, Action>> DoMapTrack(MusicTrack track, int channelNum, TempoMap map)
        {
            foreach (var pair in track.Track.EventPairs)
            {
                var @event = pair.Second;
                if (!(@event is MusicEvent musicEvent)) continue;
                var microTime = TimeConverter.ConvertTo<MetricTimeSpan>(pair.First, map).TotalMicroseconds;
                yield return new Pair<long, Action>(microTime, () => _synth.SendEvent(channelNum, musicEvent));
            }
        }
    }
}
}