using System;
using System.Collections.Generic;
using Godot;
using Array = Godot.Collections.Array;

namespace MusicMachine.BetterAnimation
{
/// <summary>
///     A wrapper around Godot.Animation that allows interfacing with more OOP oriented animation tracks.
/// </summary>
public class AnimationWrapper
{
    private static readonly Dictionary<Type, Animation.TrackType> TypeToTrack =
        new Dictionary<Type, Animation.TrackType>
        {
            {typeof(ValueTrack), Animation.TrackType.Value},
            {typeof(TransformTrack), Animation.TrackType.Transform},
            {typeof(MethodTrack), Animation.TrackType.Method},
            {typeof(BezierTrack), Animation.TrackType.Bezier},
            {typeof(AudioTrack), Animation.TrackType.Audio},
            {typeof(AnimationTrack), Animation.TrackType.Animation}
        };

    private readonly List<Track> _tracks = new List<Track>();
    internal readonly Animation Animation;

    public AnimationWrapper()
    {
        if (Animation == null)
            Animation = new Animation();
    }

    public IReadOnlyList<Track> Tracks => _tracks;

    public float Length
    {
        get => Animation.Length;
        set => Animation.Length = value;
    }

    public bool Loop
    {
        get => Animation.Loop;
        set => Animation.Loop = value;
    }

    public float Step
    {
        get => Animation.Step;
        set => Animation.Step = value;
    }

    public void RemoveTrack(int trackIndex)
    {
        _tracks.RemoveAt(trackIndex);
        Animation.RemoveTrack(trackIndex);
    }

    internal void RemoveTrack(Track track)
    {
        var idx = IndexOf(track);
        if (idx != -1) RemoveTrack(idx);
    }

    public int GetTrackCount()
    {
        var trackCount = Animation.GetTrackCount();
        if (trackCount != _tracks.Count) throw new StateMalformedException();
        return trackCount;
    }

    public NodePath TrackGetPath(int idx)
    {
        throw new NotImplementedException();
    }

    public void TrackSetPath(int idx, NodePath path)
    {
        throw new NotImplementedException();
    }

    public int FindTrack(NodePath path)
    {
        return Animation.FindTrack(path);
    }

    public void TrackMoveUp(int idx)
    {
        _tracks.Swap(idx, idx + 1);
        Animation.TrackMoveUp(idx);
    }

    public void TrackMoveDown(int idx)
    {
        _tracks.Swap(idx, idx - 1);
        Animation.TrackMoveDown(idx);
    }

    public void TrackSwap(int from, int to)
    {
        Animation.TrackSwap(from, to);
        _tracks.Swap(from, to);
    }

    public void TrackSetImported(int idx, bool imported)
    {
        Animation.TrackSetImported(idx, imported);
    }

    public bool TrackIsImported(int idx)
    {
        return Animation.TrackIsImported(idx);
    }

    public void TrackSetEnabled(int idx, bool enabled)
    {
        Animation.TrackSetEnabled(idx, enabled);
    }

    public bool TrackIsEnabled(int idx)
    {
        return Animation.TrackIsEnabled(idx);
    }

    public int TransformTrackInsertKey(int idx, float time, Vector3 location, Quat rotation, Vector3 scale)
    {
        throw new NotImplementedException();
    }


    public Array TransformTrackInterpolate(int idx, float timeSec)
    {
        throw new NotImplementedException();
    }

    public void ValueTrackSetUpdateMode(int idx, Animation.UpdateMode mode)
    {
        throw new NotImplementedException();
    }

    public Animation.UpdateMode ValueTrackGetUpdateMode(int idx)
    {
        throw new NotImplementedException();
    }

    public int[] ValueTrackGetKeyIndices(int idx, float timeSec, float delta)
    {
        throw new NotImplementedException();
    }

    public int[] MethodTrackGetKeyIndices(int idx, float timeSec, float delta)
    {
        throw new NotImplementedException();
    }

    public string MethodTrackGetName(int idx, int keyIdx)
    {
        throw new NotImplementedException();
    }

    public Array MethodTrackGetParams(int idx, int keyIdx)
    {
        throw new NotImplementedException();
    }

    public int BezierTrackInsertKey(int track, float time, float value, Vector2? inHandle = null,
        Vector2? outHandle = null)
    {
        throw new NotImplementedException();
    }

    public void BezierTrackSetKeyValue(int idx, int keyIdx, float value)
    {
        throw new NotImplementedException();
    }

    public void BezierTrackSetKeyInHandle(int idx, int keyIdx, Vector2 inHandle)
    {
        throw new NotImplementedException();
    }

    public void BezierTrackSetKeyOutHandle(int idx, int keyIdx, Vector2 outHandle)
    {
        throw new NotImplementedException();
    }

    public float BezierTrackGetKeyValue(int idx, int keyIdx)
    {
        throw new NotImplementedException();
    }

    public Vector2 BezierTrackGetKeyInHandle(int idx, int keyIdx)
    {
        throw new NotImplementedException();
    }

    public Vector2 BezierTrackGetKeyOutHandle(int idx, int keyIdx)
    {
        throw new NotImplementedException();
    }

    public float BezierTrackInterpolate(int track, float time)
    {
        throw new NotImplementedException();
    }

    public int AudioTrackInsertKey(int track, float time, Resource stream, float startOffset = 0, float endOffset = 0)
    {
        throw new NotImplementedException();
    }

    public void AudioTrackSetKeyStream(int idx, int keyIdx, Resource stream)
    {
        throw new NotImplementedException();
    }

    public void AudioTrackSetKeyStartOffset(int idx, int keyIdx, float offset)
    {
        throw new NotImplementedException();
    }

    public void AudioTrackSetKeyEndOffset(int idx, int keyIdx, float offset)
    {
        throw new NotImplementedException();
    }

    public Resource AudioTrackGetKeyStream(int idx, int keyIdx)
    {
        throw new NotImplementedException();
    }

    public float AudioTrackGetKeyStartOffset(int idx, int keyIdx)
    {
        throw new NotImplementedException();
    }

    public float AudioTrackGetKeyEndOffset(int idx, int keyIdx)
    {
        throw new NotImplementedException();
    }

    public int AnimationTrackInsertKey(int track, float time, string animation)
    {
        throw new NotImplementedException();
    }

    public void AnimationTrackSetKeyAnimation(int idx, int keyIdx, string animation)
    {
        throw new NotImplementedException();
    }

    public string AnimationTrackGetKeyAnimation(int idx, int keyIdx)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        foreach (var track in _tracks)
        {
        }

        Animation.Clear();
    }

    public void CopyTrack(int track, Animation toAnimation)
    {
        throw new NotImplementedException();
    }


    public Track AddTrack(Animation.TrackType type, int idx = 0)
    {
        var track = AnimationTrackFactory(type);
        _tracks.Insert(idx, track);
        Animation.AddTrack(type, idx);
        return track;
    }

    public TTrack AddTrack<TTrack>(int idx = 0)
        where TTrack : Track
    {
        if (!TypeToTrack.ContainsKey(typeof(TTrack)))
            throw new ArgumentException("Track type not supported", nameof(TTrack));
        return (TTrack) AddTrack(TypeToTrack[typeof(TTrack)], idx);
    }


    private Track AnimationTrackFactory(Animation.TrackType type)
    {
        switch (type)
        {
            case Animation.TrackType.Value:
                return new ValueTrack(this);
            case Animation.TrackType.Transform:
                return new TransformTrack(this);
            case Animation.TrackType.Method:
                return new MethodTrack(this);
            case Animation.TrackType.Bezier:
                return new BezierTrack(this);
            case Animation.TrackType.Audio:
                return new AudioTrack(this);
            case Animation.TrackType.Animation:
                return new AnimationTrack(this);
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    internal int IndexOf(Track track)
    {
        return _tracks.IndexOf(track);
    }
}
}