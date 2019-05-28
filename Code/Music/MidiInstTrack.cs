using System;
using System.Collections.Generic;

namespace MusicMachine.Music
{
/// <summary>
///     A label for a track that stores its time in units of MidiTicks.
/// </summary>
public class MidiInstTrack : Track<IInstTrackEvent>
{
    private readonly FTBN _bank;
    public readonly bool IsDrumTrack;
    public readonly SBN Program;

    public MidiInstTrack(FTBN bank, SBN program, bool isDrumTrack = false)
    {
        _bank       = bank;
        Program     = program;
        IsDrumTrack = isDrumTrack;
    }

    public FTBN Bank => IsDrumTrack ? (FTBN) MidiConstants.DrumBank : _bank;

    public int CombinedPresetNum => (Bank << 7) | Program;
}

/// <summary>
///     Just putting the instrument track pruning in another place.
/// </summary>
public static class InstrumentTrackCleaner
{
    /// <summary>
    ///     Removes redundant events from this track.
    /// </summary>
    /// <param name="midiInstTrack"></param>
    public static void Clean(this MidiInstTrack midiInstTrack)
    {
//        if (Program == 56)
//        {
//            Debugger.Break();
//        }
        var  notesOn    = new bool[128];
        byte numNotesOn = 0;
        var  pastEvents = new Dictionary<Type, EffectiveEventPair>();
        var  toRemove   = new List<KeyValuePair<long, IInstTrackEvent>>();
        foreach (var curEventPair in midiInstTrack.EventPairs)
        {
            var @event = curEventPair.Value;
            switch (@event)
            {
            case NoteOnEvent noteOn:
            {
                if (notesOn[noteOn.NoteNumber]) //already on
                    break;
                if (numNotesOn++ == 0) //a note played!
                {
                    foreach (var eventPair in pastEvents.Values)
                        eventPair.WasEffective = true; //it now affects a note.
                }
                notesOn[noteOn.NoteNumber] = true;
                break;
            }
            case NoteOffEvent noteOff:
            {
                if (!notesOn[noteOff.NoteNumber])
                    toRemove.Add(curEventPair); //redundant note off
                else
                {
                    numNotesOn--;
                    notesOn[noteOff.NoteNumber] = false;
                }
                break;
            }
            case IInstStateEvent _:
                var type = @event.GetType();
                if (pastEvents.TryGetValue(type, out var past))
                {
                    if (@event.Equals(past.EventPair.Value))
                    {
                        toRemove.Add(curEventPair); //current identical, redundant.
                        break;                      // and don't update past.
                    }
                    if (!past.WasEffective)
                        toRemove.Add(past.EventPair); //past was never effective, and now overriden.
                }
                pastEvents[type] = new EffectiveEventPair(curEventPair, numNotesOn != 0);
                break;
            }
        }
        if (numNotesOn != 0)
        {
            foreach (var past in pastEvents.Values)
            {
                var pair = past.EventPair;
                if (!(pair.Value is NoteOffEvent))
                    toRemove.Add(pair);
            }
        }
        foreach (var eventPair in toRemove) midiInstTrack.Remove(eventPair);
    }

    private class EffectiveEventPair
    {
        public readonly KeyValuePair<long, IInstTrackEvent> EventPair;
        public bool WasEffective;

        public EffectiveEventPair(KeyValuePair<long, IInstTrackEvent> eventPair, bool wasEffective)
        {
            EventPair    = eventPair;
            WasEffective = wasEffective;
        }
    }
}
}