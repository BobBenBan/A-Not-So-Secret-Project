using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

// ReSharper disable RedundantCaseLabel
namespace MusicMachine.Music
{
public static class MidiConverterEx
{
    public static Song ToSong(this MidiFile midiFile) => new MidiConverter2(midiFile).Song;
}

public class MidiConverter2
{
    private readonly Track<IInstStateEvent> _stateChanges = new Track<IInstStateEvent>();
    private readonly Dictionary<int, MidiInstTrack> _tracks = new Dictionary<int, MidiInstTrack>();

    public MidiConverter2(MidiFile file)
    {
        ReadMidiFile(file);
    }

    public Song Song { get; } = new Song();

    private void ReadMidiFile(MidiFile file)
    {
        var tempoMap = file.GetTempoMap();
        if (!(tempoMap.TimeDivision is TicksPerQuarterNoteTimeDivision))
            throw new NotSupportedException("Only ticks per quarter note time division is supported.");
        Song.ReplaceTempoMap(tempoMap);
        var eventsByChannel = from timedObj in file.GetTimedEventsAndNotes()
                              let channel = timedObj is Note note ? note.Channel :
                                                timedObj is TimedEvent te && te.Event is ChannelEvent ce ?
                                                    ce.Channel : (FBN?) null
                              where channel != null
                              // ReSharper disable once PossibleInvalidOperationException
                              group timedObj by channel.Value;
        foreach (var channel in eventsByChannel) ProcessChannel(channel);
    }

    //todo, maybe: possible corner case note on only. But then, its an invalid midi file, so, yeaaah.
    private void ProcessChannel(IGrouping<FBN, ITimedObject> channel)
    {
        _stateChanges.Clear(); //cached
        _tracks.Clear();       // cached.
        var preset = new Preset {IsDrumTrack = channel.Key == MidiConstants.DrumChannel};
        foreach (var timedObject in channel)
        {
            if (timedObject is Note note) //GET TRACK, IF EXIST, ADD NOTE ON CURRENT PRESET.
            {
                if (!_tracks.TryGetValue(preset.CombinedPresetNum, out var track))
                {
                    _tracks[preset.CombinedPresetNum] = track = new MidiInstTrack(
                                                            preset.Bank,
                                                            preset.Program,
                                                            preset.IsDrumTrack);
                }
                track.Add(note.Time,               new NoteOnEvent(note.NoteNumber, note.Velocity));
                track.Add(note.Time + note.Length, new NoteOffEvent(note.NoteNumber, note.OffVelocity));
                continue;
            }
            var convertedObj = ConvertEvent(((TimedEvent) timedObject).Event);
            if (convertedObj == null) continue;
            switch (convertedObj)
            {
            case IPresetChange presetChange:
                presetChange.ApplyTo(ref preset);
                break;
            case IInstStateEvent stateEvent:
                _stateChanges.Add(timedObject.Time, stateEvent);
                break;
            default: throw new Exception("This shouldn't happen! yell at them developers!");
            }
        }
        foreach (var track in _tracks.Values)
        {
            track.AddRange(_stateChanges);
            track.Clean();
            track.Name = track.IsDrumTrack ? //
                             $"Drum Track: Prog {track.Program}" :
                             $"Instrument Track [{channel.Key}] Bank {track.Bank} Prog {track.Program} ";
            Song.Tracks.Add(track);
        }
    }

    private static object ConvertEvent(MidiEvent channelEvent)
    {
        if (!(channelEvent is ChannelEvent)) return null;
        switch (channelEvent)
        {
        case Melanchall.DryWetMidi.Smf.NoteEvent _: return null; //ignore!!
        case ProgramChangeEvent pce:
            return new ProgramChangeMidiEvent(pce.ProgramNumber);
        case Melanchall.DryWetMidi.Smf.PitchBendEvent pbe:
            return new PitchBendEvent(pbe.PitchValue);
        case ControlChangeEvent cce:
        {
            var controlEnum = (ControlNumbers) (byte) cce.ControlNumber;
            switch (controlEnum)
            {
            case ControlNumbers.BankSelect:
                return new BankMsbSelectEvent(cce.ControlValue);
            case ControlNumbers.BankSelectLsb:
                return new BankLsbSelectEvent(cce.ControlValue);
            case ControlNumbers.Volume:
                return new VolumeChangeEvent(cce.ControlValue);
            case ControlNumbers.Expression:
                return new ExpressionChangeEvent(cce.ControlValue);
            case ControlNumbers.Balance:
            case ControlNumbers.Pan:
            default:
                Console.WriteLine($"Ignored control change {controlEnum}: [{cce.Channel}] ({cce.ControlValue})");
                return null;
            }
        }
        //todo, maybe?
        case NoteAftertouchEvent _:
        case ChannelAftertouchEvent _:
        default:
            Console.WriteLine($"Ignored channel Event: {channelEvent}");
            return null;
        }
    }

    private struct Preset
    {
        public SBN Program;
        public FTBN Bank;
        public bool IsDrumTrack;
        private FTBN _bank;

        public int CombinedPresetNum => ((IsDrumTrack ? (FTBN) MidiConstants.DrumBank : _bank) << 7) | Program;
    }

    private interface IPresetChange
    {
        void ApplyTo(ref Preset preset);
    }

    private sealed class ProgramChangeMidiEvent : IPresetChange
    {
        public readonly SBN Program;

        public ProgramChangeMidiEvent(SBN program)
        {
            Program = program;
        }

        public void ApplyTo(ref Preset preset)
        {
            preset.Program = Program;
        }
    }

    private sealed class BankMsbSelectEvent : IPresetChange
    {
        public readonly SBN BankHead;

        public BankMsbSelectEvent(SBN bankHead)
        {
            BankHead = bankHead;
        }

        public void ApplyTo(ref Preset preset)
        {
            preset.Bank.Head = BankHead;
        }
    }

    private sealed class BankLsbSelectEvent : IPresetChange
    {
        public readonly SBN BankTail;

        public BankLsbSelectEvent(SBN bankTail)
        {
            BankTail = bankTail;
        }

        public void ApplyTo(ref Preset preset)
        {
            preset.Bank.Tail = BankTail;
        }
    }
}
}