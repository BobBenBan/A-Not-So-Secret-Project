using Godot;
using Melanchall.DryWetMidi.Smf;
using MusicMachine.Music;
using MusicMachine.Scenes;

namespace MusicMachine.Test
{
public class TestScene : Area
{
    private Node _objects;
    private Player _player;
    private MidiPlayerDisplayer _displayer;
    public override void _Ready()
    {
        _player = GetNode<Player>("Player");
        _player.Primary = OnAction;
        _player.Secondary = OnSecondary;
        _objects = GetNode("Objects");
        var displayPoint = GetNode<Spatial>("DisplayPoint");

        const string midiLoc      = "res://Resources/Midi/starwars.mid";
        const string soundFontLoc = "res://Resources/Midi/sf.sf2";

        var midiSong = new MidiSong();
        midiSong.ReadMidiFile(MidiFile.Read(ProjectSettings.GlobalizePath(midiLoc)));
//        midiSong.SortByChannel();
        _displayer = new MidiPlayerDisplayer(displayPoint, midiSong, soundFontLoc);
        AddChild(_displayer);
    }
    public void OnAction(float delta)
    {
    }
    private void OnSecondary(float delta)
    {
        _displayer.Play();
    }
    private void OnBodyExited(Node body)
    {
        if (!body.TryCall("OnWorldExit"))
            body.QueueFree();
    }
}
}