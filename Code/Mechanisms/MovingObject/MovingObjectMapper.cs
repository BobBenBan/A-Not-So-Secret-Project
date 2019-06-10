using System;
using System.Collections.Generic;
using MusicMachine.Programs;
using MusicMachine.Programs.Mappers;
using MusicMachine.Util;

namespace MusicMachine.Mechanisms.MovingObject
{
public sealed class MovingObjectMapper : ITrackMapper
{
    private readonly Scenes.Mechanisms.MovingObject.MovingObject _movingObject;

    public MovingObjectMapper(Scenes.Mechanisms.MovingObject.MovingObject movingObject)
    {
        _movingObject = movingObject;
    }

    public IEnumerable<Pair<long, Action>> MapTrack(ProgramTrack track, MappingInfo info)
    {
        foreach (var pair in track.EventPairs)
            switch (pair.Second)
            {
            case NoteOnEvent noe:
                yield return new Pair<long, Action>(info.GetMicros(pair.First), () => _movingObject.AddObject(noe));
                break;
            case NoteOffEvent noe:
                yield return new Pair<long, Action>(info.GetMicros(pair.First), () => _movingObject.RemoveObject(noe));
                break;
            }
    }

    public void AnalyzeTrack(ProgramTrack track, MappingInfo info)
    {
    }
}
}