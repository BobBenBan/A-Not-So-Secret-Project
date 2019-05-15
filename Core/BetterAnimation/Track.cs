using System;
using System.Collections.Generic;
using Godot;

namespace MusicMachine.BetterAnimation
{
public abstract class Track
{
    protected AnimationWrapper _owner;

    private protected Track(AnimationWrapper owner)
    {
        _owner = owner;
    }

    private Animation Animation
    {
        get
        {
            if (_owner.Animation == null)
                throw new InvalidOperationException("This track has been removed.");
            return _owner.Animation;
        }
    }

    public abstract Animation.TrackType Type { get; }

    public Animation.InterpolationType InterpolationType
    {
        get => Animation.TrackGetInterpolationType(idx);
        set => Animation.TrackSetInterpolationType(idx, value);
    }

    public bool InterpolationLoopWrap
    {
        get => Animation.TrackGetInterpolationLoopWrap(idx);
        set => Animation.TrackSetInterpolationLoopWrap(idx, value);
    }

    private int idx
    {
        get
        {
            var index = _owner.IndexOf(this);
            if (index == -1)
                throw new ShouldNotHappenException();
            return index;
        }
    }

    public bool IsValid() => _owner.Animation != null;
    internal void Remove() => _owner = null;

    protected void AddKey(float time, object key, float transition = 1) =>
        Animation.TrackInsertKey(idx, time, key, transition);

    protected void InsertKey(float time, object key, float transition = 1) =>
        Animation.TrackInsertKey(idx, time, key, transition);

    public void RemoveKey(int keyIdx) => Animation.TrackRemoveKey(idx, keyIdx);

    public void RemoveKeyAtPosition(float position) =>
        Animation.TrackRemoveKeyAtPosition(idx, position);

    protected void SetKeyValue(int key, object value) =>
        Animation.TrackSetKeyValue(idx, key, value);

    public void SetKeyTransition(int keyIdx, float transition) =>
        Animation.TrackSetKeyTransition(idx, keyIdx, transition);

    public float GetKeyTransition(int keyIdx) =>
        Animation.TrackGetKeyTransition(idx, keyIdx);

    public int GetKeyCount() => Animation.TrackGetKeyCount(idx);
    protected object GetKeyValue(int keyIdx) => Animation.TrackGetKeyValue(idx, keyIdx);
    public float GetKeyTime(int keyIdx) => Animation.TrackGetKeyTime(idx, keyIdx);
    public int FindKey(float time, bool exact = false) => Animation.TrackFindKey(idx, time, exact);
}

public abstract class Track<TKey> : Track
{
//    protected bool Enabled = true;
//    protected bool Imported = false;
//    protected bool LoopWrap = true;
//    protected NodePath Path;

    protected Track(AnimationWrapper owner) : base(owner)
    {
    }

    protected abstract IList<Key> keys { get; }


    // ReSharper disable once InconsistentNaming
}
}