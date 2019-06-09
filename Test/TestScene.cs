using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Melanchall.DryWetMidi.Smf;
using MusicMachine.Mechanisms.Projectiles;
using MusicMachine.Mechanisms.Timings;
using MusicMachine.Programs;
using MusicMachine.Programs.Mappers;
using MusicMachine.Scenes;
using MusicMachine.Scenes.Functional;
using MusicMachine.Scenes.Functional.Tracks;
using MusicMachine.Scenes.Global;
using MusicMachine.Scenes.Mechanisms;
using MusicMachine.Scenes.Mechanisms.LightBoard;
using MusicMachine.Scenes.Mechanisms.Projectiles;
using MusicMachine.Util;

namespace MusicMachine.Test
{
public class TestScene : Area
{
    private readonly List<LaunchInfo> _launches = new List<LaunchInfo>();
    private readonly Program _launchProgram = new Program();
    private readonly List<Timing> _launchTimings = new List<Timing>();
    private ActionPlayer _actionPlayer;
    private Launcher _launcher;
    private CollisionTimingMapper _launchMapper;
    private ProgramPlayer _launchPlayer;
    private Player _player;               //as in like, the person, player
    private Program _program;             //as in song
    private ProgramPlayer _programPlayer; //as in song player. Not confusing at all.

    public Vector3 LaunchLoc => _launcher.GetGlobalTranslation();

    public override void _Ready()
    {
        _player           = GetNode<Player>("Player");
        _player.Primary   = OnAction;
        _player.Secondary = OnSecondary;
        _launcher         = GetNode<Launcher>("Objects/Launcher");
//        ProgramTheProgram();
        PrepareTargets();
//        OnSecondary(0);
    }

    private void PrepareTargets()
    {
        var timingRecorder = new TimingRecorder();
        GlobalNode.Instance.AddChild(timingRecorder);
        var targets = GetNode<Spatial>("Targets");
        foreach (Spatial child in targets.GetChildren())
        {
            var t1 = GetLaunch1(child);
            var t2 = GetLaunch2(child);
            _launches.Add(t1);
            _launches.Add(t2);
        }

        IEnumerable<Pair<long, LaunchInfo>> TrackToLaunch(AnyTrack track, MappingInfo info)
        {
            foreach (var launch in _launches) yield return new Pair<long, LaunchInfo>(0, launch);
        }

        _launchMapper = new CollisionTimingMapper(_launcher, timingRecorder, TrackToLaunch);
        var emptyTestTrack = new EmptyTestTrack();
        _launchProgram.Tracks.Add(emptyTestTrack);
        emptyTestTrack.Mappers.Add(_launchMapper);
        _launchPlayer = new ProgramPlayer(_launchProgram);
    }

    private LaunchInfo GetLaunch1(Spatial child)
    {
        var startLoc      = LaunchLoc;
        var endLoc        = child.GetGlobalTranslation();
        var gravity       = GravityVec * Gravity;
        var targetingInfo = new Targeting.TargetingParams(startLoc, endLoc, gravity, 2, false);
        var @params       = targetingInfo;
        return @params.ToLaunch();
    }

    private LaunchInfo GetLaunch2(Spatial child)
    {
        var startLoc      = LaunchLoc;
        var endLoc        = child.GetGlobalTranslation();
        var gravity       = GravityVec * Gravity;
        var targetingInfo = new Targeting.TargetingParams(startLoc, endLoc, gravity, 16, true);
        var @params       = targetingInfo;
        return @params.ToLaunch();
    }

    private void ProgramTheProgram()
    {
        const string midiFile      = "res://Resources/Midi/Fireflies.mid";
        const string soundFontFile = "res://Resources/Midi/sf.sf2";

        _program = MidiFile.Read(ProjectSettings.GlobalizePath(midiFile)).ToProgram();

        var lightBoard       = GetNode<LightBoard>("Objects/LightBoard");
        var lightBoardMapper = new LightBoardMapper(lightBoard);
        var lightBoardTrack1 = _program.GetMusicTrack((byte) InstrumentNames.ElectricBass_pick);
        var lightBoardTrack2 = _program.GetMusicTrack((byte) InstrumentNames.ElectricBass_finger);
        var lightBoardTrack3 = _program.GetMusicTrack((byte) InstrumentNames.AcousticGrandPiano);
        lightBoardTrack1?.Mappers.Add(lightBoardMapper);
//        lightBoardTrack2?.Mappers.Add(lightBoardMapper);
//        lightBoardTrack3?.Mappers.Add(lightBoardMapper);
//        var launcherMapper = new CollisionTimingMapper(_launcher, EventToLaunch);
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

    private void OnAction(float delta)
    {
        PrepBallLaunch();
    }

    private void PrepBallLaunch()
    {
        if (_actionPlayer != null)
        {
            RemoveChild(_actionPlayer);
            _actionPlayer.Stop();
        }
        _actionPlayer = null;
    }

    private void OnSecondary(float delta)
    {
//        DoBallLaunch();
    }
//
//    private void DoBallLaunch()
//    {
//        if (_actionPlayer != null)
//        {
//            _actionPlayer.Play();
//            return;
//        }
//        var recorder = _launcher.TimingRecorder;
//        if (recorder.Values.Any(timing => timing.Status != Timing.TimingStatus.Done)) return;
//        var track   = new Track<Action>();
//        var maxTime = recorder.Values.Select(x => x.ElapsedMicros.Value).Max();
//        foreach (var launch in _launches)
//        {
//            if (!recorder.TryGetValue(launch, out var timing))
//                continue;
//            var micros = timing.ElapsedMicros.Value;
//            var time   = maxTime - micros;
//            track.Add(time, () => _launcher.Launch(launch));
//        }
//        _actionPlayer = new ActionPlayer(track);
//        AddChild(_actionPlayer);
//        _actionPlayer.Play();
//    }

    private void OnBodyExited(Node body)
    {
        if (!body.TryCall("OnWorldExit"))
            body.QueueFree();
    }
}
}