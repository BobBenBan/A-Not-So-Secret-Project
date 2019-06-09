using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Interaction;
using MusicMachine.Util;

namespace MusicMachine.Programs.Mappers
{
public interface ITrackMapper
{
    ICompletionAwaiter Prepare(AnyTrack track, MappingInfo info);

    IEnumerable<Pair<long, Action>> MapTrack(AnyTrack track, MappingInfo info);
}

public struct MappingInfo
{
    public readonly bool IsValid;
    public readonly TempoMap TempoMap;

    public MappingInfo(TempoMap tempoMap)
    {
        TempoMap = tempoMap ?? throw new ArgumentNullException(nameof(tempoMap));
        IsValid  = true;
    }
}
}