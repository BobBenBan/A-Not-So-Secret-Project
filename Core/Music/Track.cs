using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Util;

namespace MusicMachine.Music
{
/// <summary>
/// Represents events on a track in time.
/// This can indexed with track time, and will return a LIST of events at that track time.
/// Track time can be any number (negative or positive).
/// Underlying data structure is a Dictionary of int to List of TEvent
/// </summary>
/// <remarks>
/// Do not use this for midi-accurate timing; this is meant as an intermediary between midi-files and actual
/// playback, where the track time is physics times.
/// Please do not store the obtained list. It may change.
/// </remarks>
public class Track<TTime, TEvent> : IEnumerable<Track<TTime, TEvent>.TrackElement>
    where TTime : IComparable<TTime>
{
    public struct TrackElement
    {
        public readonly KeyValuePair<TTime, List<TEvent>> Pair;
        public TTime Time => Pair.Key;
        public List<TEvent> Events => Pair.Value;

        private TrackElement(KeyValuePair<TTime, List<TEvent>> pair)
        {
            Pair = pair;
        }

        public static implicit operator TrackElement(KeyValuePair<TTime, List<TEvent>> pair) => new TrackElement(pair);
    }

    public struct TimeEventPair
    {
        public readonly KeyValuePair<TTime, TEvent> Pair;

        public TTime Time => Pair.Key;

        public TEvent Event => Pair.Value;

        public TimeEventPair(KeyValuePair<TTime, TEvent> pair)
        {
            Pair = pair;
        }

        public TimeEventPair(TTime time, TEvent @event)
        {
            Pair = new KeyValuePair<TTime, TEvent>(time, @event);
        }

        public static implicit operator TimeEventPair(KeyValuePair<TTime, TEvent> pair) => new TimeEventPair(pair);
    }

    private SortedDictionary<TTime, List<TEvent>> _track = new SortedDictionary<TTime, List<TEvent>>();

    public List<TEvent> this[TTime time]
    {
        get
        {
            if (!_track.ContainsKey(time)) _track.Add(time, new List<TEvent>());
            return _track[time];
        }
    }

    /// <summary>
    /// Optimizes slightly by removing lists of events from the track that are empty.
    /// </summary>
    public void RemoveEmpty()
    {
        var toRemove = new List<TTime>();
        foreach (var pair in _track)
        {
            if (pair.Value.Count == 0) toRemove.Add(pair.Key);
        }

        foreach (var key in toRemove) _track.Remove(key);
    }

    /// <summary>
    /// Gets the list of events at the current time; null if no events are found.
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public List<TEvent> GetOrNull(TTime time)
    {
        if (!_track.ContainsKey(time)) return null;
        var o = _track[time];
        if (o.Count != 0) return o;
        _track.Remove(time);
        return null;
    }

    /// <summary>
    /// Clears all events at the current time.
    /// </summary>
    /// <param name="time"></param>
    public void RemoveAt(TTime time) => _track.Remove(time);

    /// <summary>
    /// Clears the track.
    /// </summary>
    public void Clear() => _track.Clear();

    /// <summary>
    /// Transforms all the time times in this track through the specified function.
    /// </summary>
    /// <param name="times"></param>
    public void TransformTimes(Func<TTime, TTime> func)
    {
        if (_track.Count <= 0) return;
        var newTrack = new SortedDictionary<TTime, List<TEvent>>();
        foreach (var pair in _track)
        {
            if (pair.Value.Count != 0)
            {
                newTrack.Add(func(pair.Key), pair.Value);
            }
        }

        _track.Clear();
        _track = newTrack;
    }

    /// <summary>
    /// Transforms all keys in the track through the specified function.
    /// </summary>
    /// <param name="times"></param>
    public void TransformEvents(Func<TEvent, TEvent> func)
    {
        if (_track.Count <= 0) return;
        var newTrack = new SortedDictionary<TTime, List<TEvent>>();
        foreach (var pair in _track)
        {
            if (pair.Value.Count == 0) continue;
            for (var i = 0; i < pair.Value.Count; i++)
                pair.Value[i] = func(pair.Value[i]);
            newTrack.Add(pair.Key, pair.Value);
        }

        _track = newTrack;
    }

    /// <summary>
    /// Gets an enumerator of pairs of (track time, events at that time).
    /// Will also iterate through track times with no events, represented by a event list of of null.
    /// Will continue to iterate even if the track has ended,
    /// </summary>
    /// <returns>An enumerator that does the above</returns>
    public IEnumerable<List<TrackElement>> IterateTrack(TTime start, TTime end, Func<TTime, TTime> step)
    {
        if (start == null) throw new ArgumentNullException(nameof(start));
        if (end == null) throw new ArgumentNullException(nameof(end));
        if (step == null) throw new ArgumentNullException(nameof(step));
        if (end.CompareTo(start) < 0) throw new ArgumentException("to < from");

        using (var enumerator = _track.GetEnumerator())
        {
            TrackElement? curEnum;
            do
            {
                curEnum = enumerator.MoveNext() ? enumerator.Current : (KeyValuePair<TTime, List<TEvent>>?) null;
            } while (curEnum?.Time.CompareTo(start) < 0); //find first >= start;

            for (var curTime = start; curTime.CompareTo(end) < 0;)
            {
                if (curEnum?.Time.CompareTo(curTime) <= 0) //cur <= time;
                {
                    //add until cur > from; which means inclusive;
                    var o = new List<TrackElement>();
                    do
                    {
                        if (curEnum?.Events.Count != 0)
                            o.Add(curEnum.Value);
                        curEnum = enumerator.MoveNext()
                            ? enumerator.Current
                            : (KeyValuePair<TTime, List<TEvent>>?) null;
                    } while (curEnum?.Time.CompareTo(curTime) <= 0);

                    yield return o.Count != 0 ? o : null;
                }
                else yield return null;

                var newTime = step(curTime);
                if (!(newTime.CompareTo(curTime) > 0))
                    throw new OverflowException($"The stepped time {newTime} is not >= the past time {curTime}!");
                curTime = newTime;
            }
        }
    }


    /// <summary>
    /// Gets an enumerator of pairs of (track time, events at that time).
    /// Will also iterate through track times with no events, represented by a event list of of null.
    /// Enumeration will end when there is nothing left in the track.
    /// Starts at from, ends at to.
    /// </summary>
    /// <returns>An enumerator that does the above</returns>
    public IEnumerable<List<TrackElement>> IterateTrack(TTime start, Func<TTime, TTime> step)
    {
        if (start == null) throw new ArgumentNullException(nameof(start));
        if (step == null) throw new ArgumentNullException(nameof(step));
        RemoveEmpty();
        return TrackIterator(start, step);
    }

    /// <summary>
    /// Gets an enumerator for pairs of track time to events of that time.
    /// Will also iterate through track times with no events represented by a List of null.
    /// Starts at the lowest track time and ends with the highest.
    /// </summary>
    /// <returns>An enumerator that does the above</returns>
    public IEnumerable<List<TrackElement>> IterateTrack(Func<TTime, TTime> step)
    {
        RemoveEmpty();
        if (_track.Count == 0) return new List<TrackElement>[0];
        return TrackIterator(_track.First().Key, step);
    }

    private IEnumerable<List<TrackElement>> TrackIterator(TTime start, Func<TTime, TTime> step)
    {
        using (var enumerator = _track.GetEnumerator())
        {
            TrackElement curEnum;
            do
            {
                if (enumerator.MoveNext())
                    curEnum = enumerator.Current;
                else yield break;
            } while (curEnum.Time.CompareTo(start) < 0); //find first >= start, if exists.

            void TryStep(ref TTime curTime)
            {
            }

            for (var curTime = start;;)
            {
                if (curEnum.Time.CompareTo(curTime) <= 0) //cur <= time;
                {
                    //add until cur > from; which means inclusive;
                    var o = new List<TrackElement>();
                    do
                    {
                        o.Add(curEnum);
                        if (enumerator.MoveNext())
                            curEnum = enumerator.Current;
                        else
                        {
                            yield return o;
                            yield break;
                        }
                    } while (curEnum.Time.CompareTo(curTime) <= 0);

                    yield return o.Count != 0 ? o : null;
                }
                else yield return null;

                var newTime = step(curTime);
                if (!(newTime.CompareTo(curTime) > 0))
                    throw new OverflowException($"The stepped time {newTime} is not >= the past time {curTime}!");
                curTime = newTime;
            }
        }
    }

    /// <summary>
    /// Gets an enumerator that returns KeyValuePairs of (track time) to (a list of events) at that time,
    /// only returning track times that have an event.
    /// </summary>
    /// <returns>The above enumeartor</returns>
    public IEnumerable<TrackElement> IterateElements()
    {
        using (var enumerator = _track.GetEnumerator())
        {
            while (enumerator.MoveNext())
                if (enumerator.Current.Value.Count != 0)
                    yield return enumerator.Current;
        }
    }

    /// <summary>
    /// Gets an enumerator that returns KeyValuePairs of track time to Events, for every single event.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TimeEventPair> IterateEvents()
    {
        foreach (var pair in _track)
        foreach (var @event in pair.Value)
            yield return new TimeEventPair(pair.Key, @event);
    }

    IEnumerator<TrackElement> IEnumerable<TrackElement>.GetEnumerator() => IterateElements().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => IterateElements().GetEnumerator();

    /// <summary>
    /// Adds a range of events, using TimeEventPair's
    /// </summary>
    /// <param name="events"></param>
    public void AddRange(IEnumerable<TimeEventPair> events)
    {
        foreach (var pair in events)
            this[pair.Time].Add(pair.Event);
    }

    /// <summary>
    /// Adds a range of events, using TrackElements.
    /// </summary>
    /// <param name="events"></param>
    public void AddRange(IEnumerable<TrackElement> events)
    {
        foreach (var element in events)
            this[element.Time].AddRange(element.Events);
    }
}

public static class TrackBuildersEx
{
    [Obsolete]
    [Pure]
    public static Track<int, Note> MakeNoteTrack(this IEnumerable<Melanchall.DryWetMidi.Smf.Interaction.Note> notes,
        TempoMap tempoMap, int timesPerSecond = 60)
    {
        var track = new Track<int, Note>();
        var microsecondsPerTime = 1e6 / timesPerSecond;
        foreach (var note in notes)
        {
            var time = (note.TimeAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / microsecondsPerTime).RoundToInt();
            track[time].Add(new Note(note.NoteNumber, note.Velocity,
                (ushort) (note.LengthAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / microsecondsPerTime)
                .RoundToInt()));
        }

        return track;
    }
}
}