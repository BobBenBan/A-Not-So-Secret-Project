using System.Collections.Generic;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Util;

namespace MusicMachine.Tracks
{
/// <summary>
///     A label for a track that stores its time in units of MidiTicks.
///     This is also the class that
/// </summary>
public partial class InstrumentTrack : Track<InstrumentEvent>
{
    public delegate Pair<long, object>? Mapper(Pair<long, InstrumentEvent> instPair, TempoMap tempoMap);

    public static readonly Mapper DefaultMapper = (input, tempomap) => new Pair<long, object>(
        TimeConverter.ConvertTo<MetricTimeSpan>(input.First, tempomap).TotalMicroseconds,
        input.Second);

    private readonly FTBN _bank;
    public readonly bool IsDrumTrack;
    public readonly List<Mapper> Mappers = new List<Mapper>();
    public readonly SBN Program;
    public string Name;

    public InstrumentTrack(FTBN bank, SBN program, bool isDrumTrack = false)
    {
        _bank       = bank;
        Program     = program;
        IsDrumTrack = isDrumTrack;
        Mappers.Add(DefaultMapper);
    }

    public FTBN Bank => IsDrumTrack ? (FTBN) MidiConstants.DrumBank : _bank;

    public int CombinedPresetNum => (Bank << 7) | Program;

    public override string ToString() => Name;

    public List<Track<object>> GetMappedTracks(TempoMap tempoMap)
    {
        var o = new List<Track<object>>();
        foreach (var mapping in Mappers)
        {
            var track = new Track<object>();
            foreach (var pair in EventPairs)
            {
                var eventPair = mapping(pair, tempoMap);
                if (eventPair != null)
                    track.Add(eventPair.Value);
            }
            o.Add(track);
        }
        return o;
    }
}
}