using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Util.Maths;

// ReSharper disable RedundantCaseLabel
namespace MusicMachine.Programs
{
public class MidiConverter
{
    private readonly Track<MusicStateEvent> _stateChanges = new Track<MusicStateEvent>();
    private readonly Dictionary<int, MusicTrack> _tracks = new Dictionary<int, MusicTrack>();

    public MidiConverter(MidiFile file)
    {
        ReadMidiFile(file);
    }

    public Program Program { get; } = new Program();

    private void ReadMidiFile(MidiFile file)
    {
        var tempoMap = file.GetTempoMap();
        if (!(tempoMap.TimeDivision is TicksPerQuarterNoteTimeDivision))
            throw new NotSupportedException("Only ticks per quarter note time division is supported.");
        Program.ReplaceTempoMap(tempoMap);
        var eventsByChannel = from timedObj in file.GetTimedEventsAndNotes()
                              let channel = timedObj is Note note ? note.Channel :
                                                timedObj is TimedEvent te && te.Event is ChannelEvent ce ?
                                                    ce.Channel : (FBN?) null
                              where channel != null
                              // ReSharper disable once PossibleInvalidOperationException
                              group timedObj by channel.Value;
        foreach (var channel in eventsByChannel)
            ProcessChannel(channel);
    }

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
                    _tracks[preset.CombinedPresetNum] = track = new MusicTrack(
                                                            preset.Bank,
                                                            preset.Program,
                                                            preset.IsDrumTrack);
                }
                track.Track.Add(note.Time,               new NoteOnEvent(note.NoteNumber, note.Velocity));
                track.Track.Add(note.Time + note.Length, new NoteOffEvent(note.NoteNumber, note.OffVelocity));
                continue;
            }
            var convertedObj = ConvertEvent(((TimedEvent) timedObject).Event);
            if (convertedObj == null)
                continue;
            switch (convertedObj)
            {
            case IPresetChange presetChange:
                presetChange.ApplyTo(ref preset);
                break;
            case MusicStateEvent stateEvent:
                _stateChanges.Add(timedObject.Time, stateEvent);
                break;
            default: throw new Exception("Shouldn't happen");
            }
        }
        foreach (var track in _tracks.Values)
        {
            track.Track.AddRange(_stateChanges);
            track.Clean();
            Program.Tracks.Add(track);
            track.Name = track.IsDrumTrack ? $"Drum Track: Prog {track.Program}" :
                             typeof(InstrumentNames).IsEnumDefined(track.CombinedPresetNum) ?
                                 $"Instrument Track: {(InstrumentNames) track.CombinedPresetNum}" :
                                 $"Instrument Track {(InstrumentNames) (int) track.Program}), Bank {track.Bank}";
        }
    }

    private static object ConvertEvent(MidiEvent channelEvent)
    {
        if (!(channelEvent is ChannelEvent))
            return null;
        switch (channelEvent)
        {
        case Melanchall.DryWetMidi.Smf.NoteEvent _:        return null; //ignore!!
        case ProgramChangeEvent pce:                       return new ProgramChangeMidiEvent(pce.ProgramNumber);
        case Melanchall.DryWetMidi.Smf.PitchBendEvent pbe: return new PitchBendEvent(pbe.PitchValue);
        case ControlChangeEvent cce:
        {
            var controlEnum = (ControlNumbers) (byte) cce.ControlNumber;
            switch (controlEnum)
            {
            case ControlNumbers.BankSelect:                      return new BankMsbSelectEvent(cce.ControlValue);
            case ControlNumbers.BankSelectLsb:                   return new BankLsbSelectEvent(cce.ControlValue);
            case ControlNumbers.Volume:                          return new VolumeChangeEvent(cce.ControlValue);
            case ControlNumbers.Expression:                      return new ExpressionChangeEvent(cce.ControlValue);
            case ControlNumbers.Modulation:                      break;
            case ControlNumbers.BreathController:                break;
            case ControlNumbers.FootController:                  break;
            case ControlNumbers.PortamentoTime:                  break;
            case ControlNumbers.DataEntryMSB:                    break;
            case ControlNumbers.EffectController1:               break;
            case ControlNumbers.EffectController2:               break;
            case ControlNumbers.DataEntryLSB:                    break;
            case ControlNumbers.DamperPedal:                     break;
            case ControlNumbers.Portamento:                      break;
            case ControlNumbers.Sostenuto:                       break;
            case ControlNumbers.SoftPedal:                       break;
            case ControlNumbers.Legato:                          break;
            case ControlNumbers.Hold2:                           break;
            case ControlNumbers.SoundController1:                break;
            case ControlNumbers.SoundController2:                break;
            case ControlNumbers.SoundController3:                break;
            case ControlNumbers.SoundController4:                break;
            case ControlNumbers.SoundController5:                break;
            case ControlNumbers.SoundController6:                break;
            case ControlNumbers.SoundController7:                break;
            case ControlNumbers.SoundController8:                break;
            case ControlNumbers.SoundController9:                break;
            case ControlNumbers.SoundController10:               break;
            case ControlNumbers.GeneralPurpose1:                 break;
            case ControlNumbers.GeneralPurpose2:                 break;
            case ControlNumbers.GeneralPurpose3:                 break;
            case ControlNumbers.GeneralPurpose4:                 break;
            case ControlNumbers.PortamentoCC:                    break;
            case ControlNumbers.Effect1Depth:                    break;
            case ControlNumbers.Effect2Depth:                    break;
            case ControlNumbers.Effect3Depth:                    break;
            case ControlNumbers.Effect4Depth:                    break;
            case ControlNumbers.Effect5Depth:                    break;
            case ControlNumbers.DataIncrement:                   break;
            case ControlNumbers.DataDecrement:                   break;
            case ControlNumbers.NonRegisteredParameterNumberLSB: break;
            case ControlNumbers.NonRegisteredParameterNumberMSB: break;
            case ControlNumbers.RegisteredParameterNumberLSB:    break;
            case ControlNumbers.RegisteredParameterNumberMSB:    break;
            case ControlNumbers.AllSoundOff:                     break;
            case ControlNumbers.ResetAllControllers:             break;
            case ControlNumbers.LocalSwitch:                     break;
            case ControlNumbers.AllNotesOff:                     break;
            case ControlNumbers.OmniModeOff:                     break;
            case ControlNumbers.OmniModeOn:                      break;
            case ControlNumbers.MonoMode:                        break;
            case ControlNumbers.PolyMode:                        break;
            case ControlNumbers.Balance:                         break;
            case ControlNumbers.Pan:                             break;
            }
            Console.WriteLine($"Ignored control change {controlEnum}: [{cce.Channel}] ({cce.ControlValue})");
            return null;
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

        public int CombinedPresetNum => ((IsDrumTrack ? (FTBN) MidiConstants.DrumBank : Bank) << 7) | Program;
    }

    private interface IPresetChange
    {
        void ApplyTo(ref Preset preset);
    }

    private sealed class ProgramChangeMidiEvent : IPresetChange
    {
        private readonly SBN _program;

        public ProgramChangeMidiEvent(SBN program)
        {
            _program = program;
        }

        public void ApplyTo(ref Preset preset)
        {
            preset.Program = _program;
        }
    }

    private sealed class BankMsbSelectEvent : IPresetChange
    {
        private readonly SBN _bankHead;

        public BankMsbSelectEvent(SBN bankHead)
        {
            _bankHead = bankHead;
        }

        public void ApplyTo(ref Preset preset)
        {
            preset.Bank.Head = _bankHead;
        }
    }

    private sealed class BankLsbSelectEvent : IPresetChange
    {
        private readonly SBN _bankTail;

        public BankLsbSelectEvent(SBN bankTail)
        {
            _bankTail = bankTail;
        }

        public void ApplyTo(ref Preset preset)
        {
            preset.Bank.Tail = _bankTail;
        }
    }
}

public static class MidiConverterEx
{
    public static Program ToProgram(this MidiFile midiFile) => new MidiConverter(midiFile).Program;
}
}