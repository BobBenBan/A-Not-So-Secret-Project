using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MusicMachine.Music
{
/// <summary>
///     Represents events on a track in time.
///     This can indexed with track time, and will return a LIST of events at that track time.
///     Track time can be any number (negative or positive).
/// </summary>
public class Track<TEvent>
    : IReadOnlyDictionary<long, IEnumerable<TEvent>>
{
    private SortedList<long, List<TEvent>> _track = new SortedList<long, List<TEvent>>();
    public string Name;

    public Track(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public Track()
        : this($"Track of ({typeof(long).Name}, {typeof(TEvent).Name})")
    {
    }

    public IEnumerable<long> Times
    {
        get { return _track.Keys; }
    }

    /// <summary>
    ///     Iterate through all the events in a track, in order of time, grouped in lists by the time they were at.
    /// </summary>
    public IEnumerable<IReadOnlyList<TEvent>> EventLists
    {
        get { return _track.Values; }
    }

    /// <summary>
    ///     Iterate through all events in the track in order of time.
    /// </summary>
    public IEnumerable<TEvent> Events
    {
        get { return _track.Values.SelectMany(x => x); }
    }

    /// <summary>
    /// Iterate elements in reverse of time.
    /// </summary>
    public IEnumerable<TEvent> EventsReversed
    {
        get { return _track.Values.ListReverse().SelectMany(x => x); }
    }

    public IEnumerable<KeyValuePair<long, IEnumerable<TEvent>>> Elements
    {
        get { return _track.Select(x => new KeyValuePair<long, IEnumerable<TEvent>>(x.Key, x.Value)); }
    }

    public IEnumerable<KeyValuePair<long, TEvent>> EventPairs
    {
        get
        {
            return _track.SelectMany(
                pair => pair.Value,
                (pair, @event) => new KeyValuePair<long, TEvent>(pair.Key, @event));
        }
    }

    IEnumerable<long> IReadOnlyDictionary<long, IEnumerable<TEvent>>.Keys
    {
        get { return _track.Keys; }
    }

    IEnumerable<IEnumerable<TEvent>> IReadOnlyDictionary<long, IEnumerable<TEvent>>.Values
    {
        get { return _track.Values; }
    }

    IEnumerator<KeyValuePair<long, IEnumerable<TEvent>>> IEnumerable<KeyValuePair<long, IEnumerable<TEvent>>>.
        GetEnumerator()
    {
        return Elements.GetEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
        return Elements.GetEnumerator();
    }

    bool IReadOnlyDictionary<long, IEnumerable<TEvent>>.ContainsKey(long time)
    {
        return _track.ContainsKey(time);
    }

    /// <summary>
    ///     Gets a list of all elements at the specified time.
    ///     May become invalidated later if elements at this time reach 0 at any point.
    /// </summary>
    /// <param name="time"></param>
    public IEnumerable<TEvent> this[long time]
    {
        get { return _track[time]; }
    }

    bool IReadOnlyDictionary<long, IEnumerable<TEvent>>.TryGetValue(long key, out IEnumerable<TEvent> value)
    {
        var b = _track.TryGetValue(key, out var v);
        value = v;
        return b;
    }

    /// <summary>
    ///     The number of separate times that have events.
    /// </summary>
    public int Count
    {
        get { return _track.Count; }
    }

    private bool ContainsTime(long time)
    {
        return _track.ContainsKey(time);
    }

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
    public void Add(KeyValuePair<long, TEvent> eventPair)
    {
        Add(eventPair.Key, eventPair.Value);
    }

    /// <summary>
    ///     Adds a series of events at a time.
    /// </summary>
    /// <param name="time">The time to add at</param>
    /// <param name="events">The events</param>
    public void AddRange(long time, IEnumerable<TEvent> events)
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

    /// <summary>
    ///     Adds a range of events via a series of KeyValuePairs of (time, to, a list of events).
    /// </summary>
    /// <param name="events"> a series of KeyValuePairs of (time, to, a list of events).</param>
    public void AddRange(IEnumerable<KeyValuePair<long, IEnumerable<TEvent>>> events)
    {
        foreach (var pair in events)
            AddRange(pair.Key, pair.Value);
    }

    private List<TEvent> GetListForAdd(long time)
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
        if (b && list.IsEmpty())
            _track.RemoveAt(index);
        return b;
    }

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
            if (!list.Remove(@event))
                continue;
            if (list.IsEmpty())
                _track.RemoveAt(index);
            return true;
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
        for (var index = 0; index < _track.Values.Count; index++)
        {
            var list = _track.Values[index];
            total += list.RemoveAll(match);
            if (list.IsEmpty())
                _track.RemoveAt(index);
        }

        return total;
    }

    /// <summary>
    ///     Clears all events at the current time.
    /// </summary>
    /// <param name="time">The time to remove.</param>
    /// <returns>If any events were removed.</returns>
    public bool RemoveAt(long time)
    {
        return _track.Remove(time);
    }

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

    public IEnumerable<KeyValuePair<long, IReadOnlyList<TEvent>>?> IterateTrackNullSep(
        long startInclusive,
        long endInclusive,
        Func<long, long> step)
    {
        if (step == null)
            throw new ArgumentNullException(nameof(step));
        if (endInclusive < startInclusive)
            throw new ArgumentException();
        return DoNullSeparated(startInclusive, endInclusive, step);
    }

    public IEnumerable<KeyValuePair<long, IReadOnlyList<TEvent>>?> IterateTrackNullSep(
        long startInclusive,
        Func<long, long> step)
    {
        return IterateTrackNullSep(startInclusive, LastTime, step);
    }

    public IEnumerable<KeyValuePair<long, TEvent>?> IterateTrackSingleNullSep(
        long startInclusive,
        long endInclusive,
        Func<long, long> step)
    {
        if (step == null)
            throw new ArgumentNullException(nameof(step));
        if (endInclusive < startInclusive)
            throw new ArgumentException();
        return FlattenNullSeps(DoNullSeparated(startInclusive, endInclusive, step));
    }

    public IEnumerable<KeyValuePair<long, TEvent>?> IterateTrackSingleNullSep(
        long startInclusive,
        Func<long, long> step)
    {
        return IterateTrackSingleNullSep(startInclusive, LastTime, step);
    }

    IEnumerable<IList<KeyValuePair<long, IReadOnlyList<TEvent>>>> IterateTrackLists(
        long startInclusive,
        long endInclusive,
        Func<long, long> step)
    {
        return CombineNullSep(IterateTrackNullSep(startInclusive, endInclusive, step));
    }

    IEnumerable<IList<KeyValuePair<long, IReadOnlyList<TEvent>>>> IterateTrackLists(
        long startInclusive,
        Func<long, long> step)
    {
        return CombineNullSep(IterateTrackNullSep(startInclusive, LastTime, step));
    }

    public IEnumerable<IList<KeyValuePair<long, TEvent>>> IterateTrackSingleLists(
        long startInclusive,
        long endInclusive,
        Func<long, long> step)
    {
        return CombineNullSep(IterateTrackSingleNullSep(startInclusive, endInclusive, step));
    }

    public IEnumerable<IList<KeyValuePair<long, TEvent>>> IterateTrackSingleLists(
        long startInclusive,
        Func<long, long> step)
    {
        return IterateTrackSingleLists(startInclusive, LastTime, step);
    }

    private static IEnumerable<IList<T>> CombineNullSep<T>(IEnumerable<T?> nullSeparated)
        where T : struct
    {
        var curList = new List<T>();
        foreach (var pair in nullSeparated)
            if (pair.HasValue)
            {
                curList.Add(pair.Value);
            } else
            {
                yield return curList;
                curList.Clear();
            }
//        if (curList.NotEmpty())
//            yield return curList;
    }

    private static IEnumerable<KeyValuePair<long, TEvent>?> FlattenNullSeps(
        IEnumerable<KeyValuePair<long, IReadOnlyList<TEvent>>?> doNullSeparated)
    {
        foreach (var pair in doNullSeparated)
            if (pair.HasValue)
            {
                foreach (var @event in pair.Value.Value)
                    yield return new KeyValuePair<long, TEvent>(pair.Value.Key, @event);
            } else
                yield return null;
    }

    private IEnumerable<KeyValuePair<long, IReadOnlyList<TEvent>>?> DoNullSeparated(
        long start,
        long end,
        Func<long, long> step)
    {
        var idx = _track.Keys.BinarySearchIndexOf(start);
        if (idx >= 0)
            yield return new KeyValuePair<long, IReadOnlyList<TEvent>>(start, _track.Values[idx]);
        yield return null;
        idx = idx < 0 ? ~idx : idx + 1;
        var target = start;
        while (true)
        {
            var next = step(target);
            if (next < target)
                throw new InvalidOperationException($"next value {next} is less than target {target}");
            target = Math.Min(next, end);

            for (; idx != _track.Count && _track.Keys[idx] <= target; idx++)
                yield return new KeyValuePair<long, IReadOnlyList<TEvent>>(_track.Keys[idx], _track.Values[idx]);
            yield return null;
            if (target == end)
                yield break;
        }
    }

    public long LastTime => _track.Keys.ListLast();
}
}