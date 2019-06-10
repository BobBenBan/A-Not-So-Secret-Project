using System;
using System.Collections.Generic;
using Godot;
using MusicMachine.Programs;
using MusicMachine.Programs.Mappers;
using MusicMachine.Util;

namespace MusicMachine.Mechanisms.Projectiles
{
/// <summary>
///     Midi time to absolute time.
/// </summary>
/// <param name="track"></param>
/// <param name="mappingInfo"></param>
public delegate IEnumerable<Pair<long, LaunchInfo>> TrackToLaunchMapper(ProgramTrack track, MappingInfo mappingInfo);

public static class TrackToLaunchMappers
{
    public static TrackToLaunchMapper FromEventToTargetMapper(
        Func<BaseEvent, Vector3?> eventToTarget,
        Targeting.Params @params)
    {
        IEnumerable<Pair<long, LaunchInfo>> ToTargetMapper(ProgramTrack track, MappingInfo info)
        {
            foreach (var pair in track.EventPairs)
            {
                var target = eventToTarget(pair.Second);
                if (target == null) continue;
                @params.EndPos = target.Value;
                yield return new Pair<long, LaunchInfo>(info.GetMicros(pair.First), @params.ToLaunch());
            }
        }

        return ToTargetMapper;
    }
}
}