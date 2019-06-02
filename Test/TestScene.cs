using System;
using System.Linq;
using Godot;
using Melanchall.DryWetMidi.Smf;
using MusicMachine.Music;
using MusicMachine.Scenes;
using NoteOnEvent = MusicMachine.Music.NoteOnEvent;

namespace MusicMachine.Test
{
public class TestScene : Area
{
    private PlayerDisplayer _displayer;
    private Player _player;
    private Song _song;

    public override void _Ready()
    {
        _player           = GetNode<Player>("Player");
        _player.Primary   = OnAction;
        _player.Secondary = OnSecondary;
        GetNode("Objects");
        var displayPoint = GetNode<Spatial>("DisplayPoint");

        const string midiLoc      = "res://Resources/Midi/Fireflies.mid";
        const string soundFontLoc = "res://Resources/Midi/Timbres Of Heaven GM_GS_XG_SFX V 3.4 Final.sf2";

        _song      = MidiFile.Read(ProjectSettings.GlobalizePath(midiLoc)).ToSong();
        _displayer = new PlayerDisplayer(displayPoint, _song, soundFontLoc);
        AddChild(_displayer);
        foreach (var track in _song.Tracks)
        {
            Console.WriteLine(track);
        }
    }

    public void OnAction(float delta)
    {
    }

    private void OnSecondary(float delta)
    {
        _displayer.Play(
            _song.Tracks.SelectMany(x => x.EventPairs)
                 .First(x => x.Value is NoteOnEvent)
                 .Key);
//        GD.Print(_song.Tracks.SelectMany(x=>x.Events).Any(x=>x is PitchBendEvent));
    }

    private void OnBodyExited(Node body)
    {
        if (!body.TryCall("OnWorldExit"))
            body.QueueFree();
    }
}
}