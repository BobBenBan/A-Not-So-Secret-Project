using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Util;

namespace MusicMachine.Tracks
{
public partial class Program
{
    public const float MaxSemitonesPitchBend = 12;
    internal readonly List<Track<object>> _mappedTracks = new List<Track<object>>();
    private readonly TempoMapManager _tempoMapManager = new TempoMapManager();
    public readonly List<InstrumentTrack> InstrumentTracks = new List<InstrumentTrack>();

    public TempoMap TempoMap => _tempoMapManager.TempoMap;

    public void ReplaceTempoMap(TempoMap tempoMap)
    {
        _tempoMapManager.ReplaceTempoMap(tempoMap);
    }

    public InstrumentTrack GetTrack(FTBN Bank, SBN Program)
    {
        foreach (var track in InstrumentTracks)
            if (track.Program == Program && track.Bank == Bank)
                return track;
        return null;
    }

    public void RemoveEmptyTracks()
    {
        InstrumentTracks.RemoveAll(track => !track.Events.Any(x => x is NoteOnEvent));
        //remove redundant channel events???
    }

    public void LoadMappedTracks()
    {
        _mappedTracks.Clear();
        foreach (var track in InstrumentTracks) _mappedTracks.AddRange(track.GetMappedTracks(TempoMap));
    }
}
}