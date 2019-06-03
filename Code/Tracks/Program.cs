using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Music;
using MusicMachine.Util;

namespace MusicMachine.Tracks
{
public partial class Program
{
    public const float MaxSemitonesPitchBend = 12;
    internal readonly List<Track<Action>> MappedTracks = new List<Track<Action>>();
    private readonly TempoMapManager _tempoMapManager = new TempoMapManager();
    public readonly List<MusicTrack> MusicTracks = new List<MusicTrack>();

    public TempoMap TempoMap => _tempoMapManager.TempoMap;

    public void ReplaceTempoMap(TempoMap tempoMap)
    {
        _tempoMapManager.ReplaceTempoMap(tempoMap);
    }

    public MusicTrack GetTrack(FTBN bank, SBN program)
    {
        foreach (var track in MusicTracks)
            if (track.Program == program && track.Bank == bank)
                return track;
        return null;
    }

    public void RemoveEmptyTracks()
    {
        MusicTracks.RemoveAll(track => !track.Events.Any(x => x is NoteOnEvent));
        //remove redundant channel events???
    }

    internal void LoadMappedTracks()
    {
        MappedTracks.Clear();
        foreach (var track in MusicTracks)
            MappedTracks.AddRange(track.GetMappedTracks(TempoMap));
    }
}
}