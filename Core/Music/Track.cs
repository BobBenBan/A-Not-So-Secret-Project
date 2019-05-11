using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
/// playback, where the track time is physics ticks.
/// Please do not store the obtained list. It may change.
/// </remarks>
[Obsolete]
public class Track<TEvent>
{
    private SortedDictionary<int, List<TEvent>> _track = new SortedDictionary<int, List<TEvent>>();

    public List<TEvent> this[int tick]
    {
        get
        {
            if (!_track.ContainsKey(tick)) _track.Add(tick, new List<TEvent>());
            return _track[tick];
        }
    }

    /// <summary>
    /// Optimizes slightly by removing lists from the track that are empty.
    /// </summary>
    public void DeleteEmpty()
    {
        var toRemove = new List<int>();
        foreach (var pair in _track)
        {
            if (pair.Value.Count == 0) toRemove.Add(pair.Key);
        }

        foreach (var key in toRemove) _track.Remove(key);
    }

    /// <summary>
    /// Gets the list of events at the current time; null if no events are found.
    /// </summary>
    /// <param name="tick"></param>
    /// <returns></returns>
    public IList<TEvent> GetOrNull(int tick)
    {
        if (!_track.ContainsKey(tick)) return null;
        var o = _track[tick];
        if (o.Count != 0) return o;
        _track.Remove(tick);
        return null;
    }

    /// <summary>
    /// Clears all events at the current time.
    /// </summary>
    /// <param name="tick"></param>
    public void ClearAt(int tick) => _track.Remove(tick);

    /// <summary>
    /// Clears the track.
    /// </summary>
    public void Clear() => _track.Clear();

    /// <summary>
    /// Shifts all events in this track by [ticks] ticks.
    /// Also removes empty Lists of events.
    /// </summary>
    /// <param name="ticks"></param>
    public void Shift(int ticks)
    {
        if (_track.Count <= 0) return;
        var newTrack = new SortedDictionary<int, List<TEvent>>();
        foreach (var el in _track)
        {
            if (el.Value.Count != 0)
            {
                newTrack.Add(el.Key + ticks, el.Value);
            }
        }

        _track.Clear();
        _track = newTrack;
    }

    /// <summary>
    /// Gets an enumerator of pairs of (track time, events at that time).
    /// Will also iterate through track times with no events, represented by a event list of of null.
    /// Starts at from, ends at to.
    /// </summary>
    /// <returns>An enumerator that does the above</returns>
    public IEnumerable<KeyValuePair<int, List<TEvent>>> Iterate(int from, int to)
    {
        if (to < from) throw new ArgumentException("to < from");
        var enumerator = _track.GetEnumerator();
        KeyValuePair<int, List<TEvent>> cur;
        do
        {
            cur = !enumerator.MoveNext() ? new KeyValuePair<int, List<TEvent>>(int.MaxValue, null) : enumerator.Current;
        } while (cur.Key < from);

        //cur.Key >= from;
        for (var i = from; i < to; i++)
        {
            if (i < cur.Key) yield return new KeyValuePair<int, List<TEvent>>(i, null);
            else
            {
                yield return cur;
                cur = !enumerator.MoveNext()
                    ? new KeyValuePair<int, List<TEvent>>(int.MaxValue, null)
                    : enumerator.Current;
            }
        }

        enumerator.Dispose();
    }

    /// <summary>
    /// Gets an enumerator of pairs of (track time, events at that time).
    /// Will also iterate through track times with no events, represented by a event list of of null.
    /// Starts at from, ends at to.
    /// </summary>
    /// <returns>An enumerator that does the above</returns>
    public IEnumerable<KeyValuePair<int, List<TEvent>>> Iterate(int from)
    {
        using (var enumerator = _track.GetEnumerator())
        {
            do
                if (!enumerator.MoveNext())
                    yield break;
            while (enumerator.Current.Key < from);

            //cur.Key >= from;
            for (var i = from;; i++)
            {
                if (i < enumerator.Current.Key) yield return new KeyValuePair<int, List<TEvent>>(i, null);
                else
                {
                    yield return enumerator.Current;
                    if (!enumerator.MoveNext()) break;
                }
            }
        }
    }


    /// <summary>
    /// Gets an enumerator for pairs of track time to events of that time.
    /// Will also iterate through track times with no events represented by a List of null.
    /// Starts at the lowest track time and ends with the highest.
    /// </summary>
    /// <returns>An enumerator that does the above</returns>
    public IEnumerable<KeyValuePair<int, List<TEvent>>> Iterate()
    {
        using (var enumerator = _track.GetEnumerator())
        {
            if (!enumerator.MoveNext()) yield break;
            var i = enumerator.Current.Key;
            while (true)
            {
                if (i < enumerator.Current.Key) yield return new KeyValuePair<int, List<TEvent>>(i, null);
                else
                {
                    yield return enumerator.Current;
                    if (!enumerator.MoveNext()) break;
                }

                i++;
            }
        }
    }

    /// <summary>
    /// Gets an enumerator that returns KeyValuePairs of track time to a list of events at that time,
    /// only returning track times that have an event.
    /// </summary>
    /// <returns>The above enumeartor</returns>
    public IEnumerable<KeyValuePair<int, List<TEvent>>> ContentIterate()
    {
        var enumerator = _track.GetEnumerator();
        while (enumerator.MoveNext())
            if (enumerator.Current.Value.Count != 0)
                yield return enumerator.Current;

        enumerator.Dispose();
    }

    /// <summary>
    /// Gets an enumerator that returns KeyValuePairs of track time to Events, for every single event.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<KeyValuePair<int, TEvent>> Events()
    {
        foreach (var pair in _track)
        foreach (var @event in pair.Value)
            yield return new KeyValuePair<int, TEvent>(pair.Key, @event);
    }

    public void AddAll(IEnumerable<KeyValuePair<int, TEvent>> events)
    {
        foreach (var pair in events)
        {
            this[pair.Key].Add(pair.Value);
        }
    }

    public void AddAll(IEnumerable<KeyValuePair<int, List<TEvent>>> events)
    {
        foreach (var pair in events)
        {
            foreach (var @event in pair.Value)
            {
                this[pair.Key].Add(@event);
            }
        }
    }
}

public class NoteTrack : Track<PitchedNote>
{
}

public static class TrackBuildersEx
{
    [Pure]
    public static NoteTrack MakeNoteTrack(this IEnumerable<Note> notes, TempoMap tempoMap, int ticksPerSecond = 60)
    {
        var track = new NoteTrack();
        var microsecondsPerTick = 1e6 / ticksPerSecond;
        foreach (var note in notes)
        {
            var tick = (note.TimeAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / microsecondsPerTick).RoundToInt();
            track[tick].Add(new PitchedNote(note.NoteNumber, note.Velocity,
                (ushort) (note.LengthAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / microsecondsPerTick)
                .RoundToInt()));
        }

        return track;
    }
}
}