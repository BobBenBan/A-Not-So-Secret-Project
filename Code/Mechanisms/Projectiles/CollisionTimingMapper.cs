using System;
using System.Collections.Generic;
using Godot;
using MusicMachine.Mechanisms.Timings;
using MusicMachine.Programs;
using MusicMachine.Programs.Mappers;
using MusicMachine.Util;
using Object = Godot.Object;

namespace MusicMachine.Mechanisms.Projectiles
{
//TODO: Consider if timingRecorder belongs in global node stuffs.
public class CollisionTimingMapper : Object, ITrackMapper
{
    [Signal] public delegate void TimingReady();

    private readonly ILauncher _launcher;
    private readonly HashSet<Pair<long, LaunchInfo>> _launches = new HashSet<Pair<long, LaunchInfo>>();
    private readonly TimingRecorder _timingRecorder;
    private readonly TrackToLaunchMapper _trackToLaunch;
    private bool _isReady;
    public long MicrosOffset = 0;

    public CollisionTimingMapper(ILauncher launcher, TimingRecorder timingRecorder, TrackToLaunchMapper trackToLaunch)
    {
        _launcher       = launcher ?? throw new ArgumentNullException(nameof(launcher));
        _timingRecorder = timingRecorder ?? throw new ArgumentNullException(nameof(timingRecorder));
        _trackToLaunch  = trackToLaunch ?? throw new ArgumentNullException(nameof(trackToLaunch));
    }

    public bool IsReady
    {
        get => _isReady;
        private set
        {
            if (!_isReady && value) EmitSignal(nameof(TimingReady));
            _isReady = value;
        }
    }

    IEnumerable<Pair<long, Action>> ITrackMapper.MapTrack(ProgramTrack track, MappingInfo info)
    {
        foreach (var launch in _launches)
        {
            var launchInfo = launch.Second;
            if (!_timingRecorder.TryGetValue(launchInfo, out var timing)) continue;
            var elapsed = timing.ElapsedMicros;
            if (elapsed == null) continue;
            yield return new Pair<long, Action>(
                launch.First - elapsed.Value - MicrosOffset,
                () => { _launcher.Launch(launchInfo); });
        }
    }

    void ITrackMapper.AnalyzeTrack(ProgramTrack track, MappingInfo info)
    {
        foreach (var pair in _trackToLaunch(track, info))
        {
            _launches.Add(pair);
            var launchInfo = pair.Second;
            if (!_timingRecorder.ContainsKey(launchInfo))
                _timingRecorder[launchInfo] = new CollisionTiming(_launcher, launchInfo);
        }
    }

    public void TimeAll()
    {
        IsReady = false;
        if (!_timingRecorder.IsInsideTree())
            throw new InvalidOperationException("Timing recorder not inside tree."); //todo: rethink this
        _timingRecorder.EnsureConnect(nameof(TimingRecorder.AllDone), this, nameof(OnReady));
        _timingRecorder.StartAll(true, true);
    }

    private void OnReady()
    {
        IsReady = true;
    }
}
}