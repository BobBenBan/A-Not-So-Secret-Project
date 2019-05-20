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
        var midiFile = MidiFile.Read(ProjectSettings.GlobalizePath("res://Resources/midi.mid"));
        _midiSong = new MidiSong();
        _midiSong.ReadMidiFile(midiFile);
        _midiPlayer = new MidiSongPlayer(_midiSong) {SoundFontFile = "res://Resources/sf.sf2"};
        AddChild(_midiPlayer);
    }
    private void OnPlayPressed()
    {
        _midiPlayer.Play();
    }
}
}