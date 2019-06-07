using System;
using System.Collections.Generic;
using Godot;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Interaction;
using MusicMachine.Tracks;
using MusicMachine.Tracks.Mappers;
using MusicMachine.Util;

namespace MusicMachine.Scenes.Objects.LightBoard
{
public sealed class LightBoardMapper : IMapper
{
    private readonly LightBoard _lightBoard;

    public LightBoardMapper(LightBoard lightBoard)
    {
        _lightBoard = lightBoard;
    }

    public void AnalyzeTrack(IEnumerable<Pair<long, MusicEvent>> track, MappingInfo info)
    {
    }

    public void Prepare()
    {
    }

    public IEnumerable<Pair<long, Action>> MapTrack(IEnumerable<Pair<long, MusicEvent>> track, MappingInfo info)
    {
//            var runningInfo = new RunningInfo();
        foreach (var pair in track)
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

    public IAwaiter<bool> GetAwaiter() => new TrueAwaitable();
}
}