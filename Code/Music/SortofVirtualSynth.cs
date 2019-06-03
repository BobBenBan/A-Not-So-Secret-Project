using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using MusicMachine.Game;
using MusicMachine.ThirdParty.Midi;
using MusicMachine.Tracks;
using MusicMachine.Util;

namespace MusicMachine.Music
{
//this is more of a tester.
public partial class SortofVirtualSynth : Node
{
    private readonly PlayingState[] _playingStates;
    private Bank _bank;
    private AdsrPlayer[] _players;
    private bool _ready;
    private Array<int> _usedPresetNumbers = new Array<int>();
    [Export] public float AmpDb = 20;
    [Export] public string Bus = "master";
    [Export] public int MaxPolyphony = 150;
    [Export] public AudioStreamPlayer.MixTargetEnum MixTarget = AudioStreamPlayer.MixTargetEnum.Stereo;
    [Export] public string SoundFontFile = "";
    [Export] public float VolumeDb = -10;

    public SortofVirtualSynth(int numChannels = 24)
    {
        _playingStates = new PlayingState[numChannels];
        for (var i = 0; i < _playingStates.Length; i++)
            _playingStates[i] = new PlayingState();
    }

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
    public IReadOnlyList<PlayingState> PlayingStates => _playingStates;

    public override void _Ready()
    {
        SetProcess(false);
        CreatePlayers();
        PrepareBank();
        Loops.AudioProcess += DoProcess;
        _ready             =  true;
    }

    public override void _ExitTree()
    {
        Loops.AudioProcess -= DoProcess;
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
        if (SoundFontFile != null)
            _bank = new Bank(SoundFontFile, UsedPresetNumbers);
        else
            GD.PushWarning("No soundfont supplied!");
    }

    public override void _Process(float delta)
    {
        DoProcess((delta * 10e6).RoundToLong());
    }

    private void DoProcess(long ticks)
    {
        if (_bank == null)
            return;
        foreach (var channel in _playingStates)
            channel.ClearNotPlaying();
        foreach (var player in _players)
            player.DoProcess(ticks);
    }

    public void StopAllNotes()
    {
        foreach (var playingState in _playingStates)
            playingState.StopAllNotes();
        foreach (var player in _players)
            player.Stop();
    }

    public void Reset()
    {
        foreach (var state in _playingStates)
            state.Reset();
    }

    public void SendEvent(int channelNum, MusicEvent @event)
    {
        if (_bank == null)
            return;
        Console.WriteLine("Recieved event" + @event);
        var channel = _playingStates[channelNum];
//        if (index >= 4)
//            Console.WriteLine("{0}: {1}", track, @event);
//        Console.WriteLine($"Event: {@event}");
        switch (@event)
        {
        case NoteOnEvent noteOnEvent:
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
            player.Velocity = noteOnEvent.Velocity;
            player.UpdateChannelVolume(AmpDb, VolumeDb, channel);
            player.PitchBend = channel.PitchBend;
            player.SetInstrument(instrument);
            player.Play();
//            if (!track.IsDrumTrack)
            channel.NotesOn[keyNum] = player;
            break;
        }
        case NoteOffEvent noteOffEvent:
        {
            var keyNum = noteOffEvent.NoteNumber;
            if (!channel.IsDrumTrack && channel.NotesOn.TryGetAndRemove(keyNum, out var player))
                player?.StartRelease();
            break;
        }
        case VolumeEventEvent volumeChangeEvent:
            channel.State.Volume = volumeChangeEvent.Volume;
            channel.UpdateVolume(AmpDb, VolumeDb);
            break;
        case ExpressionEventEvent expressionChangeEvent:
        {
            channel.State.Expression = expressionChangeEvent.Expression;
            channel.UpdateVolume(AmpDb, VolumeDb);
            break;
        }
        case PitchBendEvent pitchBendEvent:
        {
            channel.State.PitchBend = pitchBendEvent.PitchValue;
            channel.UpdatePitchBend();
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