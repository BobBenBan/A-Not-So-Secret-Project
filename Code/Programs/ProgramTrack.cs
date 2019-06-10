using System;
using System.Collections.Generic;
using System.Linq;
using MusicMachine.Programs.Mappers;
using MusicMachine.Util;

namespace MusicMachine.Programs
{
public abstract class ProgramTrack
{
    public readonly List<ITrackMapper> Mappers = new List<ITrackMapper>();
    public string Name;

    public abstract IEnumerable<Pair<long, BaseEvent>> EventPairs { get; }

    public override string ToString() => Name;

    public IEnumerable<Pair<long, Action>> MapThis(MappingInfo info)
    {
        foreach (var mapper in Mappers)
        foreach (var pair in mapper.MapTrack(this, info))
            yield return pair;
    }

    public void AnalyzeThis(MappingInfo info)
    {
        foreach (var mapper in Mappers)
            mapper.AnalyzeTrack(this, info);
    }
}

public class BasicTrack : ProgramTrack
{
    public readonly Track<BaseEvent> Track = new Track<BaseEvent>();

    public override IEnumerable<Pair<long, BaseEvent>> EventPairs => Track.EventPairs;
}

public sealed class EmptyTestTrack : ProgramTrack
{
    public override IEnumerable<Pair<long, BaseEvent>> EventPairs => Enumerable.Empty<Pair<long, BaseEvent>>();
}
}