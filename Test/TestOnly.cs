using Godot;
using Melanchall.DryWetMidi.Smf;
using MusicMachine.Music;

namespace MusicMachine.Test
{
public class TestOnly : Node
{
    private MidiSong _midiSong;
    private MidiSongPlayer _midiPlayer;

    public override void _Ready()
    {
        var midiFile = MidiFile.Read(ProjectSettings.GlobalizePath("res://Resources/Midi/pink.mid"));
        _midiSong   = midiFile.ToMidiSong();
        _midiPlayer = new MidiSongPlayer(_midiSong) {SoundFontFile = "res://Resources/Midi/sf.sf2"};
        AddChild(_midiPlayer);
    }

    private void OnPlayPressed()
    {
        _midiPlayer.Play();
    }
}
}