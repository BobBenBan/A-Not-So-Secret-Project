using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace MusicMachine.Music
{
public class MidiSong
{
    public readonly List<MidiTrack> Tracks =
        new List<MidiTrack>();

    private readonly TempoMapManager _tempoMapManager = new TempoMapManager();

    public TempoMap TempoMap => _tempoMapManager.TempoMap;

    public void ReplaceTempoMap(TempoMap tempoMap)
    {
        _tempoMapManager.ReplaceTempoMap(tempoMap);
    }
    /// <summary>
    /// Reads a midifile and records (a limited variety of) its events into a single MusicTrack, and adds said track to the end
    /// of the list of Instrument tracks.
    /// </summary>
    /// <param name="file"></param>
    /// <exception cref="NotSupportedException"></exception>
    public void ReadMidiFile(MidiFile file)
    {
        var tempoMap = file.GetTempoMap();
        if (!(tempoMap.TimeDivision is TicksPerQuarterNoteTimeDivision))
            throw new NotSupportedException();
        ReplaceTempoMap(tempoMap);
        var track = TrackBuilders.TimedEventsToTrack(file.GetTimedEvents());
        Tracks.Add(track);
    }
    /// <summary>
    /// Sorts all events into new tracks by channel.
    /// Tracks with no events are not added.
    /// Channel info is retained in events.
    /// </summary>
    public void SortByChannel()
    {
        var newTracks = new MidiTrack[16];
        foreach (var track in Tracks)
        foreach (var pair in track.EventPairs)
        {
            var ev       = pair.Value;
            var curTrack = newTracks[ev.Channel] = newTracks[ev.Channel] ?? new MidiTrack();
            curTrack.Add(pair);
        }
        Tracks.Clear();
        Tracks.AddRange(newTracks.Where(x => x != null));
    }
}

/// <summary>
/// A label for a track that stores its time in units of MidiTicks.
/// </summary>
public class MidiTrack : Track<ChannelEvent>
{
    public MidiTrack()
        : base("MidiTrack")
    {
    }
}
}