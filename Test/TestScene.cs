using System;
using System.Linq;
using Godot;
using Melanchall.DryWetMidi.Smf;
using MusicMachine.Scenes;
using MusicMachine.Scenes.Functional;
using MusicMachine.Scenes.Functional.Synth;
using MusicMachine.Scenes.Objects.Launchers;
using MusicMachine.Scenes.Objects.LightBoard;
using MusicMachine.Tracks;
using MusicMachine.Util;
using MusicMachine.Util.Physics;

namespace MusicMachine.Test
{
public class TestScene : Area
{
    private CollisionTimingLauncher _collisionTimingLauncher;
    private Vector3 _launchLoc;
    private Player _player;               //as in like, the person, player
    private Program _program;             //as in song
    private ProgramPlayer _programPlayer; //as in song player. Not confusing at all.

    public override void _Ready()
    {
        _player = GetNode<Player>("Player");
        _player.Primary = OnAction;
        _player.Secondary = OnSecondary;

//        _collisionTimingLauncher = GetNode<CollisionTimingLauncher>("Launcher");
        const string midiFile      = "res://Resources/Midi/Fireflies.mid";
        const string soundFontFile = "res://Resources/Midi/choriumreva.sf2";

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
            ProcessMode = ProcessOnlyNode.Mode.Audio
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

//        OnSecondary(0);
    }

    private static readonly Vector3 LaunchLoc = new Vector3(0, 2, 0);

    private void OnAction(float delta)
    {
        _collisionTimingLauncher.LaunchProjectile(
            LaunchLoc,
            Projectiles.CalculateVelocity(
                LaunchLoc,
                _player.GetGlobalTransform().origin + 2*Vector3.Up,
                Vector3.Down * 9.8f,
                0.5f,
                true));

//        _collisionTimingLauncher.AddTimingProjectile(_loc, Vector3.Left * 2);
    }

    private void OnSecondary(float delta)
    {
        _programPlayer.Play();
//        GD.Print(_song.Tracks.SelectMany(x=>x.Events).Any(x=>x is PitchBendEvent));
    }

    private void OnBodyExited(Node body)
    {
        if (!body.TryCall("OnWorldExit"))
            body.QueueFree();
    }
}
}