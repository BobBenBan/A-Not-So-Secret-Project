//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using Godot;
//using Melanchall.DryWetMidi.Smf.Interaction;
//using MusicMachine.Programs;
//
//namespace MusicMachine.Scenes
//{
//public class ProgramPlayer : Node
//{
//    [Signal]
//    public delegate void TrackEvent(TimeEventPair<object> pair, TrackTypes.TrackType trackType);
//
//    public readonly Program Program;
//
//    private long _curTimeSpan;
//
//    private IEnumerator<IReadOnlyList<IReadOnlyTrackElement<object>>> _enumerator;
//
//    public ProgramPlayer(Program program)
//    {
//        Program = program;
//    }
//
//    public bool Playing { get; private set; }
//    public bool Loop { get; set; }
//
//    public void Play(long at = 0)
//    {
//        _curTimeSpan = at;
//        _enumerator = Program.MasterIterator(at, Step).GetEnumerator();
//        Playing = true;
//    }
//
//    public void Play(ITimeSpan at)
//    {
//        Play(TimeConverter.ConvertTo<MidiTimeSpan>(at, Program.TempoMap).TimeSpan);
//    }
//
//    public void Pause()
//    {
//        Playing = false;
//    }
//
//    public void Resume()
//    {
//        if (_enumerator == null) return;
//        Playing = true;
//    }
//
//    public void Stop()
//    {
//        _enumerator?.Dispose();
//        Playing = false;
//    }
//
//    public override void _Ready()
//    {
//        Looper.Instance.Connect(Looper.SignalName, this, nameof(AltProcess));
//    }
//
//    private long Step(long current)
//    {
//        return _curTimeSpan;
//    }
//
//    private void AltProcess(long micros)
//    {
//        if (!Playing) return;
//        if (!_enumerator.MoveNext())
//        {
//            Stop();
//            if (!Loop) Play();
//        }
//
//        Debug.Assert(_enumerator.Current != null, "_enumerator.Current != null");
//        foreach (var i in _enumerator.Current)
//        {
//            var type = TrackTypes.GetTypeOf(i.Events[0]);
//            foreach (var o in i.AsSingleEvents) EmitSignal(nameof(TrackEvent), o, type);
//        }
//
//        _curTimeSpan +=
//            LengthConverter.ConvertTo<MidiTimeSpan>(micros, _curTimeSpan, Program.TempoMap);
//    }
//}
//}
