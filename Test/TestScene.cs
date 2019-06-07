using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Melanchall.DryWetMidi.Smf;
using MusicMachine.Scenes;
using MusicMachine.Scenes.Functional;
using MusicMachine.Scenes.Functional.Synth;
using MusicMachine.Scenes.Functional.Timing;
using MusicMachine.Scenes.Objects.Launchers;
using MusicMachine.Scenes.Objects.LightBoard;
using MusicMachine.Tracks;
using MusicMachine.Util;
using MusicMachine.Util.Maths;

namespace MusicMachine.Test
{
public class TestScene : Area
{
    private CollisionTimingLauncher _launcher;
    private Player _player;               //as in like, the person, player
    private Program _program;             //as in song
    private ProgramPlayer _programPlayer; //as in song player. Not confusing at all.
    private List<Timing> _launchTimings = new List<Timing>();
    private List<LocationVelocityPair> _launches = new List<LocationVelocityPair>();
    private ActionPlayer _actionPlayer;

    public override void _Ready()
    {
        _player = GetNode<Player>("Player");
        _player.Primary = OnAction;
        _player.Secondary = OnSecondary;
        _launcher = GetNode<CollisionTimingLauncher>("Objects/Launcher");
//        ProgramTheProgram();
        PrepareTargets();
//        OnSecondary(0);
    }

    private void PrepareTargets()
    {
        var targets = GetNode<Spatial>("Targets");
        foreach (Spatial child in targets.GetChildren())
        {
            var t1 = GetLaunch1(child);
            var t2 = GetLaunch2(child);
            _launches.Add(t1);
            _launches.Add(t2);
            _launchTimings.Add(_launcher.GetProjectileTiming(t1));
            _launchTimings.Add(_launcher.GetProjectileTiming(t2));
        }
    }

    private LocationVelocityPair GetLaunch1(Spatial child)
    {
        var startLoc      = LaunchLoc;
        var endLoc        = child.GetGlobalTranslation();
        var gravity       = GravityVec * Gravity;
        var targetingInfo = new Projectiles.TargetingParams(startLoc, endLoc, gravity, 2, false);
        return LocationVelocityPair.Targeting(targetingInfo);
    }

    private LocationVelocityPair GetLaunch2(Spatial child)
    {
        var startLoc      = LaunchLoc;
        var endLoc        = child.GetGlobalTranslation();
        var gravity       = GravityVec * Gravity;
        var targetingInfo = new Projectiles.TargetingParams(startLoc, endLoc, gravity, 16, true);
        return LocationVelocityPair.Targeting(targetingInfo);
    }

    private void ProgramTheProgram()
    {
        const string midiFile      = "res://Resources/Midi/Fireflies.mid";
        const string soundFontFile = "res://Resources/Midi/sf.sf2";

        _program = MidiFile.Read(ProjectSettings.GlobalizePath(midiFile)).ToProgram();

        var lightBoard       = GetNode<LightBoard>("Objects/LightBoard");
        var lightBoardMapper = new LightBoardMapper(lightBoard);
        var lightBoardTrack1 = _program.GetTrack((byte) InstrumentNames.ElectricBass_pick);
        var lightBoardTrack2 = _program.GetTrack((byte) InstrumentNames.ElectricBass_finger);
        var lightBoardTrack3 = _program.GetTrack((byte) InstrumentNames.AcousticGrandPiano);
        lightBoardTrack1?.Mappers.Add(lightBoardMapper);
//        lightBoardTrack2?.Mappers.Add(lightBoardMapper);
//        lightBoardTrack3?.Mappers.Add(lightBoardMapper);

        var synth = new SortofVirtualSynth
        {
            NumChannels = _program.MusicTracks.Count,
            SoundFontFile = soundFontFile,
            UsedPresetNumbers = _program.MusicTracks.Select(track => track.CombinedPresetNum).ToGdArray(),
            ProcessMode = ProcessNode.Mode.Audio
        };
        AddChild(synth);
        synth.MatchProgram(_program);
        var synthMapper = synth.GetMapper();
        foreach (var track in _program.MusicTracks)
            track.Mappers.Add(synthMapper);

        _programPlayer = new ProgramPlayer(_program);
        AddChild(_programPlayer);

        foreach (var track in _program.MusicTracks)
            Console.WriteLine(track);
    }

    public Vector3 LaunchLoc => _launcher.GetGlobalTranslation();

    private void OnAction(float delta)
    {
        _launcher.TimingRecorder.StartAll(true, true);
        if (_actionPlayer != null)
        {
            RemoveChild(_actionPlayer);
            _actionPlayer.Stop();
        }
        _actionPlayer = null;
    }

    private void OnSecondary(float delta)
    {
        if (_actionPlayer != null)
        {
            _actionPlayer.Play();
            return;
        }
        var recorder = _launcher.TimingRecorder;
        if (recorder.Values.Any(timing => timing.Status != Timing.TimingStatus.Done)
        ) return;
        var track   = new Track<Action>();
        var maxTime = recorder.Values.Select(x => x.ElapsedMicros.Value).Max();
        foreach (var launch in _launches)
        {
            if (!recorder.TryGetValue(launch, out var timing))
                continue;
            var micros = timing.ElapsedMicros.Value;
            var time   = maxTime - micros;
            GD.Print($"Setting at time {time} micros.");
            track.Add(
                time,
                () => _launcher.Launch(launch));
        }
        _actionPlayer = new ActionPlayer(track);
        AddChild(_actionPlayer);
        _actionPlayer.Play();
    }

    private void OnBodyExited(Node body)
    {
        if (!body.TryCall("OnWorldExit"))
            body.QueueFree();
    }
}
}