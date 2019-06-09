using System;
using System.Collections.Generic;
using System.Linq;
using MusicMachine.Programs.Mappers;
using MusicMachine.Util;

namespace MusicMachine.Programs
{
public abstract class AnyTrack
{
    public readonly List<ITrackMapper> Mappers = new List<ITrackMapper>();
    public string Name;

    protected abstract IEnumerable<Pair<long, BaseEvent>> EventPairs { get; }

    public override string ToString() => Name;

    public IEnumerable<Pair<long, Action>> GetMappedEvents(MappingInfo mappingInfo)
    {
        if (!mappingInfo.IsValid)
            throw new ArgumentException(nameof(mappingInfo));
        foreach (var mapper in Mappers)
        foreach (var pair in mapper.MapTrack(this, mappingInfo))
            yield return pair;
    }
}

public class BaseTrack : AnyTrack
{
    public readonly Track<BaseEvent> Track = new Track<BaseEvent>();

    protected override IEnumerable<Pair<long, BaseEvent>> EventPairs => Track.EventPairs;
}

public sealed class EmptyTestTrack : AnyTrack
{
    protected override IEnumerable<Pair<long, BaseEvent>> EventPairs => Enumerable.Empty<Pair<long, BaseEvent>>();
}
}