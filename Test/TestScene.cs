using Godot;
using Melanchall.DryWetMidi.Smf;
using MusicMachine.Music;
using MusicMachine.Scenes;

namespace MusicMachine.Test
{
public class TestScene : Area
{
    private MidiPlayerDisplayer _displayer;
    private Player _player;

    public override void _Ready()
    {
        _player           = GetNode<Player>("Player");
        _player.Primary   = OnAction;
        _player.Secondary = OnSecondary;
        GetNode("Objects");
        var displayPoint = GetNode<Spatial>("DisplayPoint");

        const string midiLoc      = "res://Resources/Midi/starwars.mid";
        const string soundFontLoc = "res://Resources/Midi/sf.sf2";

        var midiSong = MidiFile.Read(ProjectSettings.GlobalizePath(midiLoc)).ToMidiSong();
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
        if (!body.TryCall("OnWorldExit")) body.QueueFree();
    }
}
}