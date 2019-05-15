using System;
using Godot;
using Godot.Collections;

namespace MusicMachine.BetterAnimation
{
public class ValueKey : Key
{
    internal ValueKey(Track owner) : base(owner)
    {
    }
}

public sealed class ValueTrack : Track
{
    internal ValueTrack(AnimationWrapper owner) : base(owner)
    {
    }

    public override Animation.TrackType Type { get; } = Animation.TrackType.Value;
}

public sealed class TransformTrack : Track
{
    internal TransformTrack(AnimationWrapper owner) : base(owner)
    {
    }

    public override Animation.TrackType Type { get; } = Animation.TrackType.Transform;
}

public struct MethodCallKey
{
    public String name;
    public object[] args;

    internal Dictionary ToDictionary() =>
        new Dictionary
        {
            {"name", name},
            {"args", args}
        };
}

public sealed class MethodTrack : Track
{
    public void AddKey(float time, MethodCallKey key)
    {
        AddKey(time, key.ToDictionary());
    }

    public override Animation.TrackType Type { get; } = Animation.TrackType.Method;

    internal MethodTrack(AnimationWrapper owner) : base(owner)
    {
    }
}

public sealed class BezierTrack : Track
{
    public override Animation.TrackType Type { get; } = Animation.TrackType.Bezier;

    internal BezierTrack(AnimationWrapper owner) : base(owner)
    {
    }
}


public sealed class AudioTrack : Track
{
    public override Animation.TrackType Type { get; } = Animation.TrackType.Audio;

    internal AudioTrack(AnimationWrapper owner) : base(owner)
    {
    }
}

public sealed class AnimationTrack : Track
{
    public override Animation.TrackType Type { get; } = Animation.TrackType.Animation;

    internal AnimationTrack(AnimationWrapper owner) : base(owner)
    {
    }
}
}