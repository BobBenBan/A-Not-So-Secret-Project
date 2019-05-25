using System;
using System.Collections.Generic;
using Godot;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Scenes;
using MusicMachine.ThirdParty.Midi;

namespace MusicMachine.Music
{
//this is more of a tester.
public class MidiSongPlayer : Node
{
    private const byte DrumChannelNum = 0x09;
    private const int DrumBank = 128;
    public const int NumChannels = 16;
    private readonly Channel[] _channels = new Channel[NumChannels];
    private readonly MidiSong _song;
    private readonly MidiSongStepper _stepper = new MidiSongStepper();
    private Bank _bank;
    private AdsrPlayer[] _players;
    private bool _ready;
    [Export] public float AmpDB = 20;
    [Export] public string Bus = "master";
    [Export] public int MaxPolyphony = 128;
    [Export] public AudioStreamPlayer.MixTargetEnum MixTarget = AudioStreamPlayer.MixTargetEnum.Stereo;
    [Export] public string SoundFontFile = "";
    [Export] public float VolumeDB = -10;
    public MidiSongPlayer(MidiSong song)
    {
        _song            =  song;
        _stepper.OnEvent += OnEvent;
        for (var i = 0; i < _channels.Length; i++)
        {
            _channels[i] = new Channel();
            if (i == DrumChannelNum)
                _channels[i].Bank = DrumBank;
        }
    }

    public bool Playing { get; private set; }

    public IReadOnlyList<Channel> Channels => _channels;

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
        var usedProgNums = new HashSet<int>();
        foreach (var track in _song.Tracks)
        foreach (var bankEvent in track.Events)
        {
            var channelNum = track.Channel;
            var channel    = _channels[channelNum];
            switch (bankEvent)
            {
            case ProgramChangeEvent pce:
            {
                var progNum = pce.Program | (channel.Bank << 7);
                usedProgNums.Add(progNum);
                usedProgNums.Add(pce.Program);
                break;
            }
            case BankSelectEvent bse:
                if (channelNum != DrumChannelNum)
                    channel.Bank = bse.ApplyOn(channel.Bank);
                else
                    Console.WriteLine("Warning: Tried to change bank on drum channel, which makes no sense");
                break;
            }
        }
        _bank = new Bank(SoundFontFile, usedProgNums.ToGDArray());
    }
    public void Play<TTImeSpan>(TTImeSpan atTime)
        where TTImeSpan : ITimeSpan
    {
        Play(TimeConverter.ConvertTo<MidiTimeSpan>(atTime, _song.TempoMap).TimeSpan);
    }
    public void Play(long atMidiTimeSpan = 0)
    {
        if (!_ready)
            throw new InvalidOperationException();
        Stop();
        Playing = true;
        _stepper.BeginPlay(_song, atMidiTimeSpan);
        AlternateProcess.TickLoop += DoProcess;
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
        AlternateProcess.TickLoop -= DoProcess;
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
        foreach (var channel in _channels)
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
        foreach (var channel in _channels)
            channel.NotesOn.Clear();
    }
    private void OnEvent(long ignored, IMusicEvent @event)
    {
        //currently is verbatim. Will change.
        var channelNum = @event.Channel;
        var channel    = _channels[channelNum];
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
            player.UpdateChannelVolume(AmpDB, VolumeDB, channel);
            player.PitchBend = channel.PitchBend;
            player.SetInstrument(instrument);
            player.Play();
            if (channelNum != DrumChannelNum)
                channel.NotesOn[keyNum] = player;
            break;
        }
        case NoteOffEvent noteOffEvent:
        {
            var keyNum = noteOffEvent.NoteNumber;
            if (channel.NotesOn.TryGetAndRemove(keyNum, out var player))
                player?.StartRelease();
            break;
        }
        case VolumeChangeEvent volumeChangeEvent:
            channel.Volume = volumeChangeEvent.Volume / 127f;
            channel.UpdateVolume(AmpDB, VolumeDB);
            break;
        case ExpressionChangeEvent expressionChangeEvent:
        {
            channel.Expression = expressionChangeEvent.Expression / 127f;
            channel.UpdateVolume(AmpDB, VolumeDB);
            break;
        }
        case BankSelectEvent bankSelectEvent:
        {
            if (channelNum != DrumChannelNum)
                channel.Bank = bankSelectEvent.Bank;
            break;
        }
        case ProgramChangeEvent programChangeEvent:
        {
            channel.Program = programChangeEvent.Program;
            break;
        }
        case PitchBendEvent pitchBendEvent:
        {
            channel.PitchBend = pitchBendEvent.PitchValue / 8192f - 1;
            channel.UpdatePitchBend();
            break;
        }
        default:
            Console.WriteLine($"Unprocessed Event: {@event}");
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