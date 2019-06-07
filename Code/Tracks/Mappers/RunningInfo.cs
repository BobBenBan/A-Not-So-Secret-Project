using System.Collections.Generic;
using MusicMachine.Util;
using MusicMachine.Util.Maths;

namespace MusicMachine.Tracks.Mappers
{
public sealed class RunningInfo
{
    public PlayingState PlayingState;
    private readonly Dictionary<SBN, Pair<long, SBN>> _notesOn;

    public RunningInfo()
    {
        _notesOn = new Dictionary<SBN, Pair<long, SBN>>();
    }

    public IReadOnlyDictionary<SBN, Pair<long, SBN>> NotesOn => _notesOn;

    public void ApplyEvent(long midiTime, MusicEvent @event)
    {
        switch (@event)
        {
        case NoteOnEvent noteOnEvent:
            _notesOn[noteOnEvent.NoteNumber] = new Pair<long, SBN>(midiTime, noteOnEvent.Velocity);
            break;
        case NoteOffEvent noteOffEvent:
            _notesOn.Remove(noteOffEvent.NoteNumber);
            break;
        case MusicStateEvent musicEvent:
            musicEvent.ApplyToState(ref PlayingState);
            break;
        }
    }

    public void ApplyEvent(Pair<long, MusicEvent> pair)
    {
        ApplyEvent(pair.First, pair.Second);
    }
}
}