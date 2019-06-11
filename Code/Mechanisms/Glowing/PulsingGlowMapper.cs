using System;
using System.Collections.Generic;
using MusicMachine.Programs;
using MusicMachine.Programs.Mappers;
using MusicMachine.Util;

namespace MusicMachine.Mechanisms.Glowing
{
public sealed class PulsingGlowMapper : ITrackMapper
{
    private readonly IGlowingArray _glowingArray;
    private readonly long _pulseMicros;
    private readonly int[] _onCount = new int[128];

    public PulsingGlowMapper(IGlowingArray glowingArray, long pulseMicros)
    {
        if (pulseMicros <= 0) throw new ArgumentException(nameof(pulseMicros));
        _pulseMicros  = pulseMicros;
        _glowingArray = glowingArray;
    }

    public IEnumerable<Pair<long, Action>> MapTrack(ProgramTrack track, MappingInfo info)
    {
        foreach (var pair in track.EventPairs)
        {
//                runningInfo.ApplyEvent(pair.First, pair.Second);
            var @event = pair.Second;
            switch (@event)
            {
            case NoteOnEvent onEvent:
            {
                var microsTime = info.GetMicros(pair.First);
                var noteNumber = onEvent.NoteNumber;

                void OnAction()
                {
                    if (_onCount[noteNumber]++ != 0) return;
                    var glowingObject = _glowingArray.GetGlowingForNote(noteNumber)?.Glowing;
                    if (glowingObject != null)
                        glowingObject.IsGlowing = true;
                }

                void OffAction()
                {
                    if (--_onCount[noteNumber] != 0) return;
                    var glowingObject = _glowingArray.GetGlowingForNote(noteNumber)?.Glowing;
                    if (glowingObject != null)
                        glowingObject.IsGlowing = false;
                }

                yield return new Pair<long, Action>(microsTime,                OnAction);
                yield return new Pair<long, Action>(microsTime + _pulseMicros, OffAction);
                break;
            }
            }
        }
    }

    public void AnalyzeTrack(ProgramTrack track, MappingInfo info)
    {
    }
}
}