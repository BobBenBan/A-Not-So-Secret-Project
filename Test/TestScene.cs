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
    private long _startTime;

    public override void _Ready()
    {
        _player           = GetNode<Player>("Player");
        _player.Primary   = OnAction;
        _player.Secondary = OnSecondary;
        GetNode("Objects");
        var displayPoint = GetNode<Spatial>("DisplayPoint");

        const string midiLoc      = "res://Resources/Midi/starwars.mid";
        const string soundFontLoc = "res://Resources/Midi/sf.sf2";

        var song = MidiFile.Read(ProjectSettings.GlobalizePath(midiLoc)).ToSong();
        _startTime = song.Tracks.SelectMany(x => x.EventPairs).First(x => x.Value is NoteOnEvent).Key;
        _displayer = new PlayerDisplayer(displayPoint, song, soundFontLoc);
        AddChild(_displayer);
//        foreach (var track in song.Tracks)
//        {
//            Console.WriteLine(track);
//            Console.WriteLine($"  Num on events: {track.Events.OfType<NoteOffEvent>().Count()}");
//        }
//        //hardcode alert
//        var analyzeTrack = song.Tracks[9];
//        foreach (var pair in analyzeTrack.EventPairs)
//        {
//            Console.WriteLine(pair);
//        }
//        OnSecondary(0);
    }

    public void OnAction(float delta)
    {
    }

    private void OnSecondary(float delta)
    {
        _displayer.Play(_startTime);
    }

    private void OnBodyExited(Node body)
    {
        if (!body.TryCall("OnWorldExit"))
            body.QueueFree();
    }
}
}