using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MusicMachine.Util;
using MusicMachine.Util.Maths;

namespace MusicMachine.Tracks
{
/// <summary>
///     Represents events on a track in time.
///     This can indexed with track time, and will return a LIST of events at that track time.
///     Track time can be any number (negative or positive).
///
///     Please be nice and don't try to cast the IReadOnlyLists into Lists to modify them, it will break things
/// </summary>
public class Track<TEvent> : IEnumerable<Pair<long, IEnumerable<TEvent>>>
{
    private SortedList<long, List<TEvent>> _track = new SortedList<long, List<TEvent>>();

    public IEnumerable<long> Times => _track.Keys;

    /// <summary>
    ///     Iterate through all the events in a track, in order of time, grouped in lists by the time they were at.
    /// </summary>
    public IEnumerable<IReadOnlyList<TEvent>> EventLists => _track.Values;

    /// <summary>
    ///     Iterate through all events in the track in order of time.
    /// </summary>
    public IEnumerable<TEvent> Events => _track.Values.SelectMany(x => x);

    public IEnumerable<Pair<long, IEnumerable<TEvent>>> Elements =>
        _track.Select(x => new Pair<long, IEnumerable<TEvent>>(x.Key, x.Value));

    public IEnumerable<Pair<long, TEvent>> EventPairs =>
        _track.SelectMany(pair => pair.Value, (pair, @event) => new Pair<long, TEvent>(pair.Key, @event));

    /// <summary>
    ///     Gets a list of all elements at the specified time.
    ///     May become invalidated later if elements at this time reach 0 at any point.
    /// </summary>
    /// <param name="time"></param>
    public IReadOnlyList<TEvent> this[long time] => _track[time];

    /// <summary>
    ///     The number of separate times that have events.
    /// </summary>
    public int Count => _track.Count;

    public IEnumerator GetEnumerator() => Elements.GetEnumerator();

    IEnumerator<Pair<long, IEnumerable<TEvent>>> IEnumerable<Pair<long, IEnumerable<TEvent>>>.GetEnumerator() =>
        Elements.GetEnumerator();

    public bool TryGetValue(long key, out IEnumerable<TEvent> value)
    {
        var b = _track.TryGetValue(key, out var v);
        value = v;
        return b;
    }

    public bool ContainsTime(long time) => _track.ContainsKey(time);

    /// <summary>
    ///     Adds an element to this track.
    /// </summary>
    /// <param name="time">The time to add at</param>
    /// <param name="event">The event</param>
    public void Add(long time, TEvent @event)
    {
        GetListForAdd(time).Add(@event);
    }

    /// <summary>
    ///     Adds an element to this track.
    /// </summary>
    /// <param name="eventPair">The time and event to add at.</param>
    public void Add<TOEvent>(Pair<long, TOEvent> eventPair)
        where TOEvent : TEvent
    {
        Add(eventPair.First, eventPair.Second);
    }

    /// <summary>
    ///     Adds a series of events at a time.
    /// </summary>
    /// <param name="time">The time to add at</param>
    /// <param name="events">The events</param>
    public void AddRange<OEvent>(long time, IEnumerable<OEvent> events)
        where OEvent : TEvent
    {
        using (var enumerator = events.GetEnumerator())
        {
            if (!enumerator.MoveNext())
                return;
            var list = GetListForAdd(time);
            do
            {
                list.Add(enumerator.Current);
            } while (enumerator.MoveNext());
        }
    }

    public void AddRange(IEnumerable<Pair<long, TEvent>> events)
    {
        foreach (var pair in events)
            Add(pair);
    }

    /// <summary>
    ///     Adds a range of events via a series of Pairs of (time, to, a list of events).
    /// </summary>
    /// <param name="events"> a series of Pairs of (time, to, a list of events).</param>
    public void AddRange<OEvent>(IEnumerable<Pair<long, IEnumerable<OEvent>>> events)
        where OEvent : TEvent
    {
        foreach (var pair in events)
            AddRange(pair.First, pair.Second);
    }

    public List<TEvent> GetListForAdd(long time)
    {
        if (_track.TryGetValue(time, out var list))
            return list;
        list = new List<TEvent>();
        _track.Add(time, list);
        return list;
    }

    /// <summary>
    ///     Removes an event at the current time, if it exists.
    /// </summary>
    /// <param name="time">the time to search for the event.</param>
    /// <param name="event">the event</param>
    /// <returns>If the event was removed or not</returns>
    public bool Remove(long time, TEvent @event)
    {
        var index = _track.IndexOfKey(time);
        if (index < 0)
            return false;
        var list = _track.Values[index];
        var b    = list.Remove(@event);
        if (b && list.Count == 0)
            _track.RemoveAt(index);
        return b;
    }

    public bool Remove<TOEvent>(Pair<long, TOEvent> pair)
        where TOEvent : TEvent =>
        Remove(pair.First, pair.Second);

    /// <summary>
    ///     Searches for and removes the first event that equals the current event, if it exists.
    /// </summary>
    /// <param name="event">The event to search for</param>
    /// <returns>If the event was removed or not.</returns>
    public bool FindRemove(TEvent @event)
    {
        for (var index = 0; index < _track.Values.Count; index++)
        {
            var list = _track.Values[index];
            if (list.Remove(@event))
            {
                if (list.Count == 0)
                    _track.RemoveAt(index);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Removes all elements that satisfy the given predicate.
    /// </summary>
    /// <param name="match">the predicate to check for.</param>
    /// <returns>The number of events removed.</returns>
    public int RemoveAll(Predicate<TEvent> match)
    {
        var total = 0;
        for (var index = 0; index < _track.Values.Count;)
        {
            var list = _track.Values[index];
            total += list.RemoveAll(match);
            if (list.Count == 0)
                _track.RemoveAt(index);
            else
                index++;
        }
        return total;
    }

    /// <summary>
    ///     Clears all events at the current time.
    /// </summary>
    /// <param name="time">The time to remove.</param>
    /// <returns>If any events were removed.</returns>
    public bool RemoveAt(long time) => _track.Remove(time);

    /// <summary>
    ///     Clears the track of all events.
    /// </summary>
    public void Clear()
    {
        _track.Clear();
    }

    /// <summary>
    ///     Transforms all the time times in this track through the specified function.
    /// </summary>
    /// <param name="func">The function to transform times.</param>
    public void TransformTimes(Func<long, long> func)
    {
        if (_track.Count <= 0)
            return;
        var newTrack = new SortedList<long, List<TEvent>>();
        foreach (var pair in _track)
            newTrack.Add(func(pair.Key), pair.Value);
        _track = newTrack;
    }

    /// <summary>
    ///     Transforms all events in the track through the specified function.
    /// </summary>
    /// <param name="func">The function to transform events</param>
    public void TransformEvents(Func<TEvent, TEvent> func)
    {
        foreach (var pair in _track)
        {
            var list = pair.Value;
            for (var i = 0; i < list.Count; i++)
                list[i] = func(list[i]);
        }
    }

    public Stepper GetStepper(long startTime = long.MinValue) => new Stepper(this, startTime);

    public class Stepper
    {
        private readonly List<Pair<long, List<TEvent>>> _current = new List<Pair<long, List<TEvent>>>();
        private readonly Track<TEvent> _owner;
        private long _curTimeInclusive;
        private int _idx = -1;

        internal Stepper(Track<TEvent> owner, long startTimeInclusive)
        {
            _owner = owner;
            _curTimeInclusive = startTimeInclusive;
        }

        private SortedList<long, List<TEvent>> Track => _owner._track;

        public long CurTimeInclusive
        {
            get => _curTimeInclusive;
            set
            {
                _idx = -1;
                _curTimeInclusive = value;
            }
        }

        public long CurTimeExclusive
        {
            get => _curTimeInclusive - 1;
            set
            {
                _idx = -1;
                _curTimeInclusive = value + 1;
            }
        }

        public bool IsCurrentlyDone => _idx >= Track.Count;

        public bool StepToInclusive(long nextTimeInclusive)
        {
            if (nextTimeInclusive < _curTimeInclusive)
                throw new ArgumentException("Attempted to step backwards", nameof(nextTimeInclusive));
            if (_idx == -1)
            {
                _idx = Track.Keys.BinarySearchIndexOf(_curTimeInclusive);
                if (_idx < 0)
                    _idx = ~_idx;
            } else
            {
                _idx.Constrain(0, Track.Count - 1);
                //linear search
                if (Track.Keys[_idx] < _curTimeInclusive)
                {
                    do
                    {
                        _idx++;
                    } while (_idx < Track.Count && Track.Keys[_idx] < _curTimeInclusive);
                } else
                {
                    while (_idx > 0 && Track.Keys[_idx - 1] > _curTimeInclusive)
                        _idx--;
                }
            }
            _current.Clear();
            if (_idx >= Track.Count)
                return false;
            while (_idx < Track.Keys.Count && Track.Keys[_idx] <= nextTimeInclusive)
            {
                _current.Add(new Pair<long, List<TEvent>>(Track.Keys[_idx], Track.Values[_idx]));
                _idx++;
            }
            _curTimeInclusive = nextTimeInclusive;
            return true;
        }

        public bool StepTo(long nextTimeExclusive) => StepToInclusive(nextTimeExclusive - 1);

        public void CopyCurrentTo(List<Pair<long, IReadOnlyList<TEvent>>> list)
        {
            list.Clear();
            foreach (var pair in _current)
                list.Add(new Pair<long, IReadOnlyList<TEvent>>(pair.First, pair.Second));
        }

        public void CopyCurrentTo(List<Pair<long, TEvent>> list)
        {
            list.Clear();
            foreach (var pair in _current)
            foreach (var @event in pair.Second)
                list.Add(new Pair<long, TEvent>(pair.First, @event));
        }

        public void CopyCurrentTo(List<TEvent> list)
        {
            list.Clear();
            foreach (var pair in _current)
                list.AddRange(pair.Second);
        }
    }
}
}