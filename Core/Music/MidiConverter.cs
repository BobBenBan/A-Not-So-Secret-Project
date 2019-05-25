using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace MusicMachine.Music
{
public static class MidiConverter
{
    //TODO
    /// <summary>
    /// Creates a midi song from a midi, including most channel events, and tempo map.
    /// More events to be supported at a later date.
    /// </summary>
    /// <param name="file"></param>
    /// <exception cref="NotSupportedException"></exception>
    public static MidiSong ToMidiSong(this MidiFile file)
    {
        var song     = new MidiSong();
        var tempoMap = file.GetTempoMap();
        if (!(tempoMap.TimeDivision is TicksPerQuarterNoteTimeDivision))
            throw new NotSupportedException("Only ticks per quarter note time division is supported.");
        song.ReplaceTempoMap(tempoMap);
        var tracks = ChannelEventsToTracks(file.GetTimedEvents());
        song.Tracks.AddRange(tracks);
        return song;
    }
    public static IEnumerable<MidiTrack> ChannelEventsToTracks(IEnumerable<TimedEvent> events)
    {
        var tracks = new MidiTrack[16];
        foreach (var timedEvent in events)
        {
            if (!(timedEvent.Event is ChannelEvent channelEvent))
            {
                Console.WriteLine($"Ignored non-channel event: {timedEvent}");
                continue;
            }
            var musicEvent = TryConvertToMusicEvent(channelEvent);
            if (musicEvent == null)
                continue;
            var channel = channelEvent.Channel;
            var track   = tracks[channel] = tracks[channel] ?? new MidiTrack(channel);

            track.Add(timedEvent.Time, musicEvent);
        }
        return tracks.Where(x => x != null);
    }
    private static IMusicEvent TryConvertToMusicEvent(ChannelEvent @event)
    {
        switch (@event)
        {
        case ControlChangeEvent cce:                           return ConvertControlChange(cce);
        case Melanchall.DryWetMidi.Smf.NoteOnEvent noe:        return new NoteOnEvent(noe);
        case Melanchall.DryWetMidi.Smf.NoteOffEvent noe:       return new NoteOffEvent(noe);
        case Melanchall.DryWetMidi.Smf.PitchBendEvent pbe:     return new PitchBendEvent(pbe);
        case Melanchall.DryWetMidi.Smf.ProgramChangeEvent pce: return new ProgramChangeEvent(pce);
        //todo, maybe??
        case NoteAftertouchEvent _:
        case ChannelAftertouchEvent _:
        default:
            Console.WriteLine($"Ignored channel Event: {@event}");
            return null;
        }
    }
    private static IMusicEvent ConvertControlChange(ControlChangeEvent changeEvent)
    {
        var controlValue = changeEvent.ControlValue;
        switch ((ControlNumbers) (byte) changeEvent.ControlNumber)
        {
        case ControlNumbers.BankSelect:    return new BankSelectEvent {Head = controlValue};
        case ControlNumbers.BankSelectLsb: return new BankSelectEvent {Tail = controlValue};
        case ControlNumbers.Volume:        return new VolumeChangeEvent(controlValue);
        case ControlNumbers.Expression:    return new ExpressionChangeEvent(controlValue);
        //todo, maybe?
        case ControlNumbers.Balance:
        case ControlNumbers.Pan:
        default:
            Console.WriteLine($"Ignored control change: {changeEvent}");
            return null;
        }
    }
}
}