using System;
using System.Collections.Generic;
using Godot;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Interaction;
using MusicMachine.Tracks;
using MusicMachine.Tracks.Mappers;
using MusicMachine.Util;

namespace MusicMachine.Scenes.Objects.Launchers
{
public class CollisionTimingLaunchMapper : BaseAwaiter<bool>, IMapper
{
    private readonly CollisionTimingLauncher _launcher;
    private readonly Func<Pair<long, MusicEvent>, LocationVelocityPair?> _eventToLaunch;
    public long MicrosOffset = 0;

    public CollisionTimingLaunchMapper(
        CollisionTimingLauncher launcher,
        Func<Pair<long, MusicEvent>, LocationVelocityPair?> eventToLaunch)
    {
        _launcher = launcher;
        _eventToLaunch = eventToLaunch;
    }

    public IAwaiter<bool> GetAwaiter() => this;

    public void AnalyzeTrack(IEnumerable<Pair<long, MusicEvent>> track, MappingInfo info)
    {
        foreach (var pair in track)
        {
            var launch = _eventToLaunch(pair);
            if (launch != null)
                _launcher.GetOrMakeProjectileTiming(launch.Value); //ensure it exists.
        }
    }

    public void Prepare()
    {
        Begin();
    }

    public IEnumerable<Pair<long, Action>> MapTrack(IEnumerable<Pair<long, MusicEvent>> track, MappingInfo info)
    {
        foreach (var pair in track)
        {
            var launch = _eventToLaunch(pair);
            if (launch == null)
                continue;
            var elapsed = _launcher.GetOrMakeProjectileTiming(launch.Value).ElapsedMicros;
            if (elapsed == null)
                continue;
            var time = TimeConverter.ConvertTo<MetricTimeSpan>(pair.First, info.TempoMap).TotalMicroseconds;
            yield return new Pair<long, Action>(
                time - elapsed.Value - MicrosOffset,
                () => { _launcher.Launch(launch.Value); });
        }
    }

    protected override void DoBegin()
    {
        _launcher.StartAll(true, true);
    }
}
}