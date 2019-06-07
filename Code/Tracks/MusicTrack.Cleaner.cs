using System;
using System.Collections.Generic;
using MusicMachine.Util;

namespace MusicMachine.Tracks
{
public partial class MusicTrack
{
    /// <summary>
    ///     Removes redundant events from this track (events that have no effect on output).
    ///     This includes identical change events and events played during silence.
    ///     Only do this when you're sure you don't want to preserve changes.
    /// </summary>
    public void Clean()
    {
        //just putting the giant aaaah in a separate place.
        //        if (Program == 56)
//        {
//            Debugger.Break();
//        }
        var  notesOn    = new bool[128];
        byte numNotesOn = 0;
        var  pastEvents = new Dictionary<Type, EffectiveEventPair>();
        var  toRemove   = new List<Pair<long, MusicEvent>>();
        foreach (var curEventPair in Track.EventPairs)
        {
            var @event = curEventPair.Second;
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
            case MusicStateEvent _:
                var type = @event.GetType();
                if (pastEvents.TryGetValue(type, out var past))
                {
                    if (@event.Equals(past.EventPair.Second))
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
                if (!(pair.Second is NoteOffEvent))
                    toRemove.Add(pair);
            }
        }
        foreach (var eventPair in toRemove) Track.Remove(eventPair);
    }

    private class EffectiveEventPair
    {
        public readonly Pair<long, MusicEvent> EventPair;
        public bool WasEffective;

        public EffectiveEventPair(Pair<long, MusicEvent> eventPair, bool wasEffective)
        {
            EventPair = eventPair;
            WasEffective = wasEffective;
        }
    }
}
}