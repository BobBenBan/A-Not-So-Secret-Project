using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Interaction;
using MusicMachine.Programs;
using MusicMachine.Programs.Mappers;
using MusicMachine.Util;

namespace MusicMachine.Scenes.Mechanisms.LightBoard
{
public sealed class LightBoardMapper : ITrackMapper
{
    private readonly LightBoard _lightBoard;

    public LightBoardMapper(LightBoard lightBoard)
    {
        _lightBoard = lightBoard;
    }

    public ICompletionAwaiter Prepare(AnyTrack track, MappingInfo info) => new AlreadyDoneCompletionAwaiter();

    public IEnumerable<Pair<long, Action>> MapTrack(AnyTrack track, MappingInfo info)
    {
        if (!(track is MusicTrack musicTrack)) yield break;
//            var runningInfo = new RunningInfo();
        foreach (var pair in musicTrack.Track.EventPairs)
        {
//                runningInfo.ApplyEvent(pair.First, pair.Second);
            var @event     = pair.Second;
            var microsTime = TimeConverter.ConvertTo<MetricTimeSpan>(pair.First, info.TempoMap).TotalMicroseconds;
            switch (@event)
            {
            case NoteOnEvent onEvent:
            {
                var noteNumber = onEvent.NoteNumber;
                yield return new Pair<long, Action>(microsTime, () => _lightBoard.AddOnCount(noteNumber));
                break;
            }

            case NoteOffEvent offEvent:
            {
                var noteNumber = offEvent.NoteNumber;
                yield return new Pair<long, Action>(microsTime, () => _lightBoard.RemoveOnCount(noteNumber));
                break;
            }
            }
        }
    }
}
}