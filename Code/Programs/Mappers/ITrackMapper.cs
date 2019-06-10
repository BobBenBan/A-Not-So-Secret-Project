using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Util;

namespace MusicMachine.Programs.Mappers
{
public interface ITrackMapper
{
    IEnumerable<Pair<long, Action>> MapTrack(ProgramTrack track, MappingInfo info);

    void AnalyzeTrack(ProgramTrack track, MappingInfo info);
}

public sealed class MappingInfo
{
    public readonly TempoMap TempoMap;

    public MappingInfo(TempoMap tempoMap)
    {
        TempoMap = tempoMap ?? throw new ArgumentNullException(nameof(tempoMap));
    }

    public long GetMicros(long midiTime) =>
        TimeConverter.ConvertTo<MetricTimeSpan>(midiTime, TempoMap).TotalMicroseconds;
}
}