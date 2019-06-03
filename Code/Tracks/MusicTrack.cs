using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Util;

namespace MusicMachine.Tracks
{
/// <summary>
///     A label for a track that stores its time in units of MidiTicks.
///     This is also the class that
/// </summary>
public partial class MusicTrack : Track<MusicEvent>
{
    private readonly FTBN _bank;
    public readonly bool IsDrumTrack;
    public readonly List<Mapper> Mappers = new List<Mapper>();
    public readonly SBN Program;
    public string Name;

    public MusicTrack(FTBN bank, SBN program, bool isDrumTrack = false)
    {
        _bank       = bank;
        Program     = program;
        IsDrumTrack = isDrumTrack;
    }

    public FTBN Bank => IsDrumTrack ? (FTBN) MidiConstants.DrumBank : _bank;

    public int CombinedPresetNum => (Bank << 7) | Program;

    public override string ToString() => Name;

    public IEnumerable<Track<Action>> GetMappedTracks(TempoMap tempoMap)
    {
        var o = new List<Track<Action>>(Mappers.Count);
        foreach (var mapping in Mappers)
        {
            var track = new Track<Action>();
            foreach (var pair in EventPairs)
            {
                var eventPair = mapping(pair.First, pair.Second, tempoMap);
                if (eventPair != null)
                    track.Add(eventPair.Value);
            }
            o.Add(track);
        }
        return o;
    }
}
}