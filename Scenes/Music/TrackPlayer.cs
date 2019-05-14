using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Godot;
using MusicMachine.Music;

namespace MusicMachine.Scenes.Music
{
public class TrackPlayer<TEvent> : Node
{
    public delegate void EventAction(int tick, TEvent @event);

    public Track<int, TEvent> Track { get; set; }
    public EventAction Action = (i, ev) => GD.Print($"{i}: {ev.ToString()}");
    public int Tick;

    private IEnumerator<List<Track<int, TEvent>.TrackElement>> _trackEnum;

    public bool Playing
    {
        get => IsPhysicsProcessing();
        private set
        {
            if (value == IsPhysicsProcessing() || value && _trackEnum == null) return;
            SetPhysicsProcess(value);
        }
    }

    public TrackPlayer(Track<int, TEvent> track)
    {
        Track = track;
    }

    public TrackPlayer()
    {
    }

    public override void _Ready()
    {
        SetPhysicsProcess(false);
    }

    public void Play(int tick = 0)
    {
        if (Track == null)
        {
            GD.PushWarning("No track to play!");
            return;
        }

        _trackEnum?.Dispose();
        Tick = tick;
        _trackEnum = Track.IterateTrack(tick, i => i + 1).GetEnumerator();
        Playing = true;
    }

    public void Resume()
    {
        Playing = true;
    }

    public void Pause()
    {
        Playing = false;
    }

    public void Toggle()
    {
        Playing = !Playing;
    }

    public override void _PhysicsProcess(float delta)
    {
        var current = _trackEnum.Current;
        if (current != null)
            foreach (var element in current)
            foreach (var @event in element.Events)
                Action(Tick, @event);
        Tick++;
        if (!_trackEnum.MoveNext()) Stop();
    }

    public void Stop()
    {
        _trackEnum?.Dispose();
        _trackEnum = null;
        Playing = false;
    }
}
}