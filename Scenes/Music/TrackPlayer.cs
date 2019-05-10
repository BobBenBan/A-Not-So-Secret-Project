using System.Collections.Generic;
using Godot;
using MusicMachine.Extensions;
using MusicMachine.Music;

namespace MusicMachine.Scenes.Music
{
public class TrackPlayer<TEvent> : Node
{
    public delegate void EventAction(int tick, TEvent @event);

    public Track<TEvent> Track { get; set; }
    public EventAction action = (i, ev) => GD.Print($"{i}: {ev.ToString()}");
    public int Tick;

    private IEnumerator<KeyValuePair<int, List<TEvent>>> _trackEnum;
    private Timer _timer;
    private bool _playing;

    public bool Playing
    {
        get => _playing;
        private set
        {
            if (value == _playing) return;
            if (value)
            {
                if (_trackEnum == null) return;
                _timer.Start();
            }
            else _timer.Stop();

            _playing = value;
        }
    }

    public TrackPlayer(Track<TEvent> track)
    {
        Track = track;
    }

    public TrackPlayer()
    {

    }
    public override void _Ready()
    {
        _timer = this.CreateAndConnectTimer(nameof(StepTick), true, false);
        _timer.SetWaitTime(1f / 60);
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
        _trackEnum = Track.Iterate(tick).GetEnumerator();
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

    private void StepTick()
    {
        if (!_playing) _timer.Stop();
            Tick = _trackEnum.Current.Key;
        if (_trackEnum.Current.Value != null)
            foreach (var @event in _trackEnum.Current.Value) //do stuff to events
                action(Tick, @event);

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