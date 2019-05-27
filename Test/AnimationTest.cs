using Godot;
using NUnit.Framework;

namespace MusicMachine.Test
{
public class AnimationTest
{
    public static void WillDeletingMiddleTrackShiftTracks()
    {
        var animation = new Animation();
        var t1        = animation.AddTrack(Animation.TrackType.Animation);
        var t2        = animation.AddTrack(Animation.TrackType.Value);
        var t3        = animation.AddTrack(Animation.TrackType.Audio);
        Assert.That(animation.TrackGetType(t1) == Animation.TrackType.Animation);
        Assert.That(animation.TrackGetType(t2) == Animation.TrackType.Value);
        Assert.That(animation.TrackGetType(t3) == Animation.TrackType.Audio);
        animation.RemoveTrack(t1);
        t2--;
        t3--;
        Assert.That(animation.TrackGetType(t2) == Animation.TrackType.Value);
        Assert.That(animation.TrackGetType(t3) == Animation.TrackType.Audio);
    }

    public static void WhereDoesItInsert()
    {
        var animation = new Animation();
        var t1        = animation.AddTrack(Animation.TrackType.Animation);
        var t2        = animation.AddTrack(Animation.TrackType.Value);
        var t3        = animation.AddTrack(Animation.TrackType.Audio, 0);
        animation.TrackGetType(1);
    }
}
}