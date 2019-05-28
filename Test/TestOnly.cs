using Godot;
using Melanchall.DryWetMidi.Smf;
using MusicMachine.Music;

namespace MusicMachine.Test
{
public class TestOnly : Node
{
    private SongPlayer _player;
    private Song _song;

    public override void _Ready()
    {
        var midiFile = MidiFile.Read(ProjectSettings.GlobalizePath("res://Resources/Midi/pink.mid"));
        _song   = midiFile.ToSong();
        _player = new SongPlayer(_song) {SoundFontFile = "res://Resources/Midi/sf.sf2"};
        AddChild(_player);
    }

    private void OnPlayPressed()
    {
        _player.Play();
    }
}
}