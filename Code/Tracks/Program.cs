using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Tracks.Mappers;
using MusicMachine.Util.Maths;

namespace MusicMachine.Tracks
{
public class Program
{
    public const float MaxSemitonesPitchBend = 12;
    private readonly TempoMapManager _tempoMapManager = new TempoMapManager();
    public readonly List<MusicTrack> MusicTracks = new List<MusicTrack>();

    public TempoMap TempoMap => _tempoMapManager.TempoMap;

    public void ReplaceTempoMap(TempoMap tempoMap)
    {
        _tempoMapManager.ReplaceTempoMap(tempoMap);
    }

    public MusicTrack GetTrack(SBN program, FTBN bank)
    {
        foreach (var track in MusicTracks)
            if (track.Program == program && track.Bank == bank)
                return track;
        return null;
    }

    public MusicTrack GetTrack(SBN program) => GetTrack(program, 0);

    public void RemoveEmptyTracks()
    {
        MusicTracks.RemoveAll(track => !track.Track.Events.Any(x => x is NoteOnEvent));
        //remove redundant channel events???
    }

    public Track<Action> GetMappedTrack()
    {
        var actionTrack = new Track<Action>();
        foreach (var track in MusicTracks)
        {
            var mappingInfo = new MappingInfo(TempoMap, track.Bank, track.Program);
            actionTrack.AddRange(track.GetMappedEvents(mappingInfo));
        }
        return actionTrack;
    }
}
}