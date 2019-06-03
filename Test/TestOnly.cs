using Godot;
using Melanchall.DryWetMidi.Smf;
using MusicMachine.Music;
using MusicMachine.Tracks;

namespace MusicMachine.Test
{
public class TestOnly : Node
{
    private Program _program;
    private SortofVirtualSynth _sortofVirtualSynth;

    public override void _Ready()
    {
        var midiFile = MidiFile.Read(ProjectSettings.GlobalizePath("res://Resources/Midi/pink.mid"));
        _program            = midiFile.ToSong();
        _sortofVirtualSynth = new SortofVirtualSynth {SoundFontFile = "res://Resources/Midi/sf.sf2"};
        AddChild(_sortofVirtualSynth);
    }

    private void OnPlayPressed()
    {
        _sortofVirtualSynth.Play();
    }
}
}