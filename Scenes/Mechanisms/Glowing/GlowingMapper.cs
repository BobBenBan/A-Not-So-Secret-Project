using System;
using System.Collections.Generic;
using MusicMachine.Programs;
using MusicMachine.Programs.Mappers;
using MusicMachine.Util;

namespace MusicMachine.Scenes.Mechanisms.Glowing
{
public sealed class GlowingMapper : ITrackMapper
{
    private readonly IGlowingArray _glowingArray;
    private readonly int[] onCount = new int[128];

    public GlowingMapper(IGlowingArray glowingArray)
    {
        _glowingArray = glowingArray;
    }

    public IEnumerable<Pair<long, Action>> MapTrack(ProgramTrack track, MappingInfo info)
    {
        if (!(track is MusicTrack musicTrack)) yield break;
//            var runningInfo = new RunningInfo();
        foreach (var pair in musicTrack.Track.EventPairs)
        {
//                runningInfo.ApplyEvent(pair.First, pair.Second);
            var @event     = pair.Second;
            var microsTime = info.GetMicros(pair.First);
            switch (@event)
            {
            case NoteOnEvent onEvent:
            {
                var noteNumber = onEvent.NoteNumber;

                void OnAction()
                {
                    if (onCount[noteNumber]++ != 0) return;
                    var glowingObject = _glowingArray.GetGlowingForNote(noteNumber)?.Glowing;
                    if (glowingObject != null)
                        glowingObject.IsGlowing = true;
                }

                yield return new Pair<long, Action>(microsTime, OnAction);
                break;
            }

            case NoteOffEvent offEvent:
            {
                var noteNumber = offEvent.NoteNumber;

                void OffAction()
                {
                    if (--onCount[noteNumber] != 0) return;
                    var glowingObject = _glowingArray.GetGlowingForNote(noteNumber)?.Glowing;
                    if (glowingObject != null)
                        glowingObject.IsGlowing = false;
                }

                yield return new Pair<long, Action>(microsTime, OffAction);
                break;
            }
            }
        }
    }

    public void AnalyzeTrack(ProgramTrack track, MappingInfo info)
    {
        //do nothing
    }
}
}