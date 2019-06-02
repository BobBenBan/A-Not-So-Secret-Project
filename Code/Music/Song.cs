using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace MusicMachine.Music
{
public class Song
{
    public const float MaxSemitonesPitchBend = 12;
    public const byte DrumChannel = 0x09;
    private readonly TempoMapManager _tempoMapManager = new TempoMapManager();
    public readonly List<MidiInstTrack> Tracks = new List<MidiInstTrack>();

    public TempoMap TempoMap => _tempoMapManager.TempoMap;

    public void ReplaceTempoMap(TempoMap tempoMap)
    {
        _tempoMapManager.ReplaceTempoMap(tempoMap);
    }

    public void RemoveEmptyTracks()
    {
        Tracks.RemoveAll(track => !track.Events.Any(x => x is NoteOnEvent));
        //remove redundant channel events???
    }
}
}