using System;
using System.Collections.Generic;
using Godot;
using MusicMachine.Programs;
using MusicMachine.Resources;

namespace MusicMachine.Scenes
{
[Obsolete]
public class SimpleTrackPlayer<TEvent> : Node
{
    [Signal]
    public delegate void EventAction(long time, Ref @event);

    public readonly Track<TEvent> Track;
    private bool _isPlaying;
    private IEnumerator<KeyValuePair<long, TEvent>?> _stepper;
    public SimpleTrackPlayer(Track<TEvent> track)
    {
        Track = track;
    }
    public SimpleTrackPlayer()
    {
    }
    public long Tick { get; private set; }
    [Export]
    public bool IsPlaying
    {
        get => _isPlaying;
        private set
        {
            if (_isPlaying == value)
                return;
            if (value)
            {
                if (HasPlayer)
                    Resume();
                else
                    Play();
            } else
                Pause();
        }
    }
    private bool HasPlayer => _stepper != null;
    public void Play(long atTick = 0)
    {
        if (Track == null)
        {
            GD.PushWarning("No track to play!");
            return;
        }

        _stepper?.Dispose();
        Tick = atTick;
        _stepper = Track.IterateTrackSingleNullSep(atTick, i => i + 1).GetEnumerator();
        IsPlaying = true;
    }
    public void Pause()
    {
        if (!HasPlayer)
            return;
        SetPhysicsProcess(false);
        _isPlaying = false;
    }
    private void Resume()
    {
        if (!HasPlayer)
            return;
        SetPhysicsProcess(true);
        _isPlaying = true;
    }
    public void Stop()
    {
        _stepper?.Dispose();
        _stepper = null;
        IsPlaying = false;
    }
    public override void _Ready()
    {
        SetPhysicsProcess(false);
    }
    public override void _PhysicsProcess(float delta)
    {
        if (!_stepper.MoveNext())
            Stop();
        while (_stepper.Current.HasValue)
        {
            var pair = _stepper.Current.Value;
            EmitSignal(nameof(EventAction), pair.Key, new Ref(pair.Value));
            if (!_stepper.MoveNext())
                Stop();
        }

        Tick++;
    }
}
}