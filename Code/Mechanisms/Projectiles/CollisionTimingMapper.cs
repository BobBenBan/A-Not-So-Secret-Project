using System;
using System.Collections.Generic;
using System.Linq;
using MusicMachine.Interaction;
using MusicMachine.Mechanisms.Timings;
using MusicMachine.Programs;
using MusicMachine.Programs.Mappers;
using MusicMachine.Util;

namespace MusicMachine.Mechanisms.Projectiles
{
//TODO: Consider if timingRecorder belongs in global node stuffs.
public class CollisionTimingMapper : BaseCompletionAwaiter, ITrackMapper
{
    private readonly ILauncher _launcher;
    private readonly TimingRecorder _timingRecorder;
    private readonly TrackToLaunchMapper _trackToLaunch;
    private List<Pair<long, LaunchInfo>> _launches;
    public long MicrosOffset = 0;

    public CollisionTimingMapper(ILauncher launcher, TimingRecorder timingRecorder, TrackToLaunchMapper trackToLaunch)
    {
        _launcher       = launcher ?? throw new ArgumentNullException(nameof(launcher));
        _timingRecorder = timingRecorder ?? throw new ArgumentNullException(nameof(timingRecorder));
        _trackToLaunch  = trackToLaunch ?? throw new ArgumentNullException(nameof(trackToLaunch));
    }

    public ICompletionAwaiter Prepare(AnyTrack track, MappingInfo info)
    {
        _launches = _trackToLaunch(track, info).ToList();
        foreach (var pair in _launches)
        {
            var key = pair.Second;
            if (!_timingRecorder.TryGetValue(key, out _))
                _timingRecorder[key] = new CollisionTiming(_launcher, key);
        }
        return this;
    }

    public IEnumerable<Pair<long, Action>> MapTrack(AnyTrack track, MappingInfo info)
    {
        foreach (var launch in _launches)
        {
            var launchInfo = launch.Second;
            var timing     = _timingRecorder[launchInfo];
            var elapsed    = timing.ElapsedMicros;
            if (elapsed == null) continue;
            yield return new Pair<long, Action>(
                launch.First - elapsed.Value - MicrosOffset,
                () => { _launcher.Launch(launchInfo); });
        }
    }

    protected override void DoBegin()
    {
        if (!_timingRecorder.IsInsideTree())
            throw new InvalidOperationException("Timing recorder not inside tree."); //todo: rethink this
        _timingRecorder.Connect(nameof(TimingRecorder.AllDone),   this, nameof(Complete));
        _timingRecorder.Connect(nameof(TimingRecorder.AnyCancel), this, nameof(Fail));
        _timingRecorder.StartAll(true, true);
    }

    private void Fail()
    {
        TryFail();
    }

    private void Complete()
    {
        TrySuccess();
    }
}
}