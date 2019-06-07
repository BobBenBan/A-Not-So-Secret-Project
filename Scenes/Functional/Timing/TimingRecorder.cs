using System;
using System.Collections;
using System.Collections.Generic;

namespace MusicMachine.Scenes.Functional.Timing
{
/// <summary>
///     Utility for timing an action and maintaining of timings.
///     One day we will put a TimingInfo resource or something.
/// </summary>
public class TimingRecorder : Clock, IDictionary<object, Timing>
{
//    public const string Dir = "res://Scenes/Functional/Timer/" + nameof(TimingRecorder) + ".cs";
    private readonly IDictionary<object, Timing> _timings = new Dictionary<object, Timing>();

    public void StartAll(bool cancel, bool restart)
    {
        foreach (var timing in _timings.Values)
            timing.Start(cancel, restart);
    }

    public void CancelAll(bool ifDone)
    {
        foreach (var timing in _timings.Values)
            timing.Cancel(ifDone);
    }

    public void Start(object key, bool cancel, bool restart)
    {
        if (!_timings.TryGetValue(key, out var timing)) throw new KeyNotFoundException();
        timing.Start(cancel, restart);
    }

    public void Cancel(object key, bool ifDone)
    {
        if (!_timings.TryGetValue(key, out var timing)) throw new KeyNotFoundException();
        timing.Cancel(ifDone);
    }

    public void End(object key)
    {
        if (!_timings.TryGetValue(key, out var timing)) throw new KeyNotFoundException();
        timing.End();
    }

    public IEnumerator<KeyValuePair<object, Timing>> GetEnumerator() => _timings.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _timings).GetEnumerator();

    void ICollection<KeyValuePair<object, Timing>>.Add(KeyValuePair<object, Timing> item) => Add(item.Key, item.Value);

    public void Clear()
    {
        foreach (var timing in Values)
            timing.TimingRecorder = null;
        _timings.Clear();
    }

    bool ICollection<KeyValuePair<object, Timing>>.Contains(KeyValuePair<object, Timing> item) =>
        _timings.Contains(item);

    void ICollection<KeyValuePair<object, Timing>>.CopyTo(KeyValuePair<object, Timing>[] array, int arrayIndex) =>
        _timings.CopyTo(array, arrayIndex);

    bool ICollection<KeyValuePair<object, Timing>>.Remove(KeyValuePair<object, Timing> item)
    {
        if (!_timings.TryGetValue(item.Key, out var value) || !value.Equals(item.Value))
            return false;
        Remove(item.Key);
        return true;
    }

    public int Count => _timings.Count;

    bool ICollection<KeyValuePair<object, Timing>>.IsReadOnly => false;

    public bool ContainsKey(object key) => _timings.ContainsKey(key);

    public void Add(object key, Timing value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        value.TimingRecorder = this;
        _timings.Add(key, value);
    }

    public bool Remove(object key)
    {
        if (!_timings.TryGetValue(key, out var timing))
            return false;
        timing.TimingRecorder = null;
        return true;
    }

    public bool TryGetValue(object key, out Timing value) => _timings.TryGetValue(key, out value);

    public Timing this[object key]
    {
        get => _timings[key];
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            value.TimingRecorder = this;
            _timings[key] = value;
        }
    }

    public ICollection<object> Keys => _timings.Keys;

    public ICollection<Timing> Values => _timings.Values;
}
}