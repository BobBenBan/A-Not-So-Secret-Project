using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Melanchall.DryWetMidi.Smf;
using MusicMachine.Mechanisms.Glowing;
using MusicMachine.Mechanisms.MovingObject;
using MusicMachine.Mechanisms.Projectiles;
using MusicMachine.Mechanisms.Timings;
using MusicMachine.Programs;
using MusicMachine.Programs.Mappers;
using MusicMachine.Scenes.Functional;
using MusicMachine.Scenes.Functional.Tracks;
using MusicMachine.Scenes.Global;
using MusicMachine.Scenes.Mechanisms.MovingObject;
using MusicMachine.Scenes.Mechanisms.Projectiles;
using MusicMachine.Scenes.Mechanisms.Synth;
using MusicMachine.Scenes.Objects.Drums;
using MusicMachine.Scenes.Objects.Xylophone;
using MusicMachine.Scenes.Player;
using MusicMachine.Util;
using NoteOnEvent = MusicMachine.Programs.NoteOnEvent;

namespace MusicMachine.Test
{
public class TestScene : Area
{
    private readonly List<LaunchInfo> _launches = new List<LaunchInfo>();
    private readonly Program _launchProgram = new Program();
    private readonly List<Timing> _launchTimings = new List<Timing>();
    private Launcher _airLauncher;
    private CollisionTimingMapper _airLaunchMapper;
    private Launcher _launcher;
    private CollisionTimingMapper _launchMapper;

//
    private ProgramPlayer _launchPlayer;
    private Player _player;               //as in like, the person, player
    private Program _program;             //as in song
    private ProgramPlayer _programPlayer; //as in song player. Not confusing at all.

    public Vector3 LaunchLoc => _launcher.GetGlobalTranslation();

    public override void _Ready()
    {
        _player   = GetNode<Player>("Player");
        _launcher = GetNode<Launcher>("Objects/Launcher");
        ProgramTheProgram();
//        PrepareTargets();
//        OnSecondary(0);
        _player.Primary   = OnAction;
        _player.Secondary = OnSecondary;

        OnSecondary(0);
    }

    private void ProgramTheProgram()
    {
        const string midiFile      = "res://Resources/Midi/Fireflies.mid";
        const string soundFontFile = "res://Resources/Midi/choriumreva.sf2";

        _program = MidiFile.Read(
                                ProjectSettings.GlobalizePath(midiFile),
                                new ReadingSettings {NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore})
                           .ToProgram();

        var lightBoard       = GetNode<IGlowingArray>("Objects/LightBoard");
        var lightBoardMapper = new PersistingGlowMapper(lightBoard);
        var lightBoardTrack1 = _program.GetMusicTrack((byte) InstrumentNames.ElectricBass_pick);
//        var lightBoardTrack2 = _program.GetMusicTrack((byte) InstrumentNames.ElectricBass_finger);
//        var lightBoardTrack3 = _program.GetMusicTrack((byte) InstrumentNames.AcousticGrandPiano);
        lightBoardTrack1?.Mappers.Add(lightBoardMapper);
//        lightBoardTrack2?.Mappers.Add(lightBoardMapper);
//        lightBoardTrack3?.Mappers.Add(lightBoardMapper);
//        var launcherMapper = new CollisionTimingMapper(_launcher, EventToLaunch);
        var xylophone       = GetNode<Xylophone>("Objects/Xylophone");
        var launcher        = GetNode<Launcher>("Objects/Launcher");
        var xylophoneMapper = xylophone.GetNoteToBarMapper();
        var targetMapper = TrackToLaunchMappers.FromEventToTargetMapper(
            xylophoneMapper,
            new Targeting.Params
            {
                StartPos          = launcher.GetGlobalTranslation(),
                Gravity           = GravityVec * Gravity,
                MinLaunchVelocity = 5,
                UseUpper          = true
            });
        var timingRecorder = new TimingRecorder {ProcessMode = ProcessNode.Mode.Physics};
        GlobalNode.Instance.AddChild(timingRecorder);
        _launchMapper = new CollisionTimingMapper(_launcher, timingRecorder, targetMapper);
        var xyloGlowMapper = new PersistingGlowMapper(xylophone);
        var xyloTrack      = _program.GetMusicTrack((byte) InstrumentNames.MusicBox);
        xyloTrack?.Mappers.Add(_launchMapper);
        xyloTrack?.Mappers.Add(xyloGlowMapper);

        var movingObject       = GetNode<MovingObject>("Objects/MovingObject");
        var movingObjectMapper = new MovingObjectMapper(movingObject);
        var movingObjectTrack  = _program.GetMusicTrack((byte) InstrumentNames.SynthVoice);
        movingObjectTrack?.Mappers.Add(movingObjectMapper);

        _airLauncher   = GetNode<Launcher>("Objects/AirLauncher");
        timingRecorder = new TimingRecorder {ProcessMode = ProcessNode.Mode.Physics};
        GlobalNode.Instance.AddChild(timingRecorder);
        _airLaunchMapper = new CollisionTimingMapper(_airLauncher, timingRecorder, AirMapper);
        var airLaunchTrack = _program.GetMusicTrack((byte) InstrumentNames.AcousticGrandPiano);
        airLaunchTrack?.Mappers.Add(_airLaunchMapper);

        var drums      = GetNode<Drums>("Objects/Drums");
        var drumMapper = new PulsingGlowMapper(drums, 100_000);
        var drumTrack  = _program.GetDrumTrack();
        drumTrack?.Mappers.Add(drumMapper);

        var synth = new SortofVirtualSynth
        {
            NumChannels   = _program.Tracks.Count,
            SoundFontFile = soundFontFile,
            UsedPresetNumbers =
                _program.Tracks.OfType<MusicTrack>().Select(track => track.CombinedPresetNum).ToGdArray(),
            ProcessMode = ProcessNode.Mode.Audio
        };
        AddChild(synth);
        synth.MatchProgram(_program);
        var synthMapper = synth.GetMapper();
        foreach (var track in _program.Tracks)
            track.Mappers.Add(synthMapper);

        _programPlayer = new ProgramPlayer(_program);
        AddChild(_programPlayer);

        foreach (var track in _program.Tracks)
            Console.WriteLine(track);
    }

    private IEnumerable<Pair<long, LaunchInfo>> AirMapper(ProgramTrack track, MappingInfo info)
    {
        foreach (var pair in track.EventPairs)
            if (pair.Second is NoteOnEvent noe)
            {
                yield return new Pair<long, LaunchInfo>(
                    info.GetMicros(pair.First),
                    new LaunchInfo(
                        _airLauncher.GetGlobalTranslation() + Vector3.Right * (0.2f * noe.NoteNumber),
                        Vector3.Back * 2 + Vector3.Down));
            }
    }

    private void OnAction(float delta)
    {
        _programPlayer.CreateTrack();
        _programPlayer.Play();
    }

    private void PrepBallLaunch()
    {
        _launchPlayer.AnalyzeTracks();
        _launchMapper.EnsureConnect(nameof(CollisionTimingMapper.TimingReady), this, nameof(CreateTrack));
    }

    private void CreateTrack()
    {
        _launchPlayer.CreateTrack();
    }

    private void OnSecondary(float delta)
    {
        _programPlayer.AnalyzeTracks();
        _launchMapper.TimeAll();
        _airLaunchMapper.TimeAll();
    }

    private void OnBodyExited(Node body)
    {
        if (!body.TryCall("OnWorldExit"))
            body.QueueFree();
    }
}
}
