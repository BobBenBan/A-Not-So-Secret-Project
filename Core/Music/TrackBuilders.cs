using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace MusicMachine.Music
{
public class TrackBuilders
{
    public static MidiTrack TimedEventsToTrack(IEnumerable<TimedEvent> events)
    {
        var track = new MidiTrack();

        foreach (var timedEvent in events)
        {
            if (!(timedEvent.Event is ChannelEvent channelEvent))
            {
            } else
            {
                switch (channelEvent)
                {
                case ControlChangeEvent cce:
                    ProcessControlChange(timedEvent.Time, cce, track);
                    break;
                case NoteEvent _:
                case PitchBendEvent _:
                case ProgramChangeEvent _:
                    track.Add(timedEvent.Time, channelEvent);
                    break;
                }
            }
        }

        return track;
    }
    private static void ProcessControlChange(long time, ControlChangeEvent changeEvent, MidiTrack track)
    {
        var value   = changeEvent.ControlValue;
        var channel = changeEvent.Channel;
        switch ((ControlNumbers) (byte) changeEvent.ControlNumber)
        {
        case ControlNumbers.BankSelect:
            Add(new BankSelectEvent(channel, value));
            break;
        case ControlNumbers.BankSelectLsb:
            var bse = ((BankSelectEvent) track.EventsReversed.FirstOrDefault(x => x is BankSelectEvent b && !b.LSBset));
            if (bse != null)
                bse.LSB = value;
            break;
        case ControlNumbers.Volume:
            Add(new VolumeChangeEvent(channel, value));
            break;
        case ControlNumbers.Expression:
            Add(new ExpressionChangeEvent(channel, value));
            break;
        default:
            Console.WriteLine($"Ignored control change: {changeEvent}");
            break;
        }

        void Add(ChannelEvent @event)
        {
            track.Add(time, @event);
        }
    }
}
}