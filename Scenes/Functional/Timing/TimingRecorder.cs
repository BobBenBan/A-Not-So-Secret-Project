using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    //todo: signal on all done/ record status.
    public int StartAll(bool cancel, bool restart)
    {
        return _timings.Values.Count(timing => timing.StartTiming(cancel, restart));
    }

    public int CancelAll(bool evenIfDone)
    {
        return _timings.Values.Count(timing => timing.CancelTiming(evenIfDone));
    }

    public void Start(object key, bool cancel, bool restart)
    {
        if (!_timings.TryGetValue(key, out var timing))
            throw new KeyNotFoundException($"No timing for key ({key}) ");
        timing.StartTiming(cancel, restart);
    }

    public void Cancel(object key, bool ifDone)
    {
        if (!_timings.TryGetValue(key, out var timing))
            throw new KeyNotFoundException($"No timing for key ({key}) ");
        timing.CancelTiming(ifDone);
    }

    public void End(object key)
    {
        if (!_timings.TryGetValue(key, out var timing))
            throw new KeyNotFoundException($"No timing for key ({key}) ");
        timing.EndTiming();
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
//
//    private class Obj : Object
//    {
//        public void OnThing()
//        {
//            GD.Print("AAAAh");
//        }
//    }
//    public override void _EnterTree()
//    {
//        var obj = new Obj();
//        this.Connect(SignalNames.NodeReady, obj, nameof(obj.OnThing));
//    }

    public ICollection<object> Keys => _timings.Keys;

    public ICollection<Timing> Values => _timings.Values;
}
}