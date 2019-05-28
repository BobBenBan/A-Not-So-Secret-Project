namespace MusicMachine.Music
{
//public class MidiConverter
//{
//    public const byte DrumChannel = 0x09;
//    public const int DrumBank = 128;
//
//    private readonly Dictionary<int, SortedList<FBN, InstrumentTrack>> _allTracks =
//        new Dictionary<int, SortedList<FBN, InstrumentTrack>>();
//
//    private readonly HashSet<NoteOn> _cachedSet = new HashSet<NoteOn>();
//    private readonly Channel[] _channels = new Channel[16];
//
//    public MidiConverter(MidiFile file)
//    {
//        for (var index = 0; index < _channels.Length; index++)
//            _channels[index] = new Channel();
//        _channels[DrumChannel].IsDrumTrack = true;
//        ReadMidiFile(file);
//    }
//
//    public Song Song { get; } = new Song();
//
//    private void ReadMidiFile(MidiFile file)
//    {
//        var tempoMap = file.GetTempoMap();
//        if (!(tempoMap.TimeDivision is TicksPerQuarterNoteTimeDivision))
//            throw new NotSupportedException("Only ticks per quarter note time division is supported.");
//        Song.ReplaceTempoMap(tempoMap);
//        ProcessMidiEvents(file.GetTimedEvents());
//
//
//        //TrySquash();
//        foreach (var trackSet in _allTracks.Values)
//        {
//            var number = 1;
//            foreach (var track in trackSet.Values)
//            {
//                track.Name = track.IsDrumTrack ? //
//                    $"Drum Track: Prog {track.Program}" :
//                    $"Instrument Track: Bank {track.Bank} Prog {track.Program} #{number}";
//                Song.Tracks.Add(track);
//                number++;
//            }
//        }
//        foreach (var track1 in Song.Tracks)
//            track1.Clear();
//        Song.RemoveEmptyTracks();
//    }
//
//    private void ProcessMidiEvents(IEnumerable<TimedEvent> events)
//    {
//        foreach (var timedEvent in events)
//        {
//            if (!(timedEvent.Event is ChannelEvent channelEvent))
//            {
//                Console.WriteLine($"Ignored non-channel event: {timedEvent}");
//                continue;
//            }
//            ProcessChannelEvent(channelEvent, timedEvent.Time);
//        }
//    }
//
//    private void ProcessChannelEvent(ChannelEvent @event, long time)
//    {
//        FBN channelNum = @event.Channel;
//        var channel    = _channels[channelNum];
//        switch (@event)
//        {
//        case Melanchall.DryWetMidi.Smf.PitchBendEvent pbe:
//            channel.PitchBend = pbe.PitchValue;
//            AddEventsToChannels(channelNum, time, new PitchBendEvent(pbe.PitchValue));
//            break;
//        case ProgramChangeEvent pce:
//            channel.Program = pce.ProgramNumber;
//            break;
//        case ControlChangeEvent cce:
//        {
//            var controlValue = cce.ControlValue;
//            var controlEnum  = (ControlNumbers) (byte) cce.ControlNumber;
//            switch (controlEnum)
//            {
//            case ControlNumbers.BankSelect:
//                channel.BankHead = controlValue;
//                break;
//            case ControlNumbers.BankSelectLsb:
//                channel.BankTail = controlValue;
//                break;
//            case ControlNumbers.Volume:
//                channel.Volume = controlValue;
//                AddEventsToChannels(channelNum, time, new VolumeChangeEvent(controlValue));
//                break;
//            case ControlNumbers.Expression:
//                channel.Expression = controlValue;
//                AddEventsToChannels(channelNum, time, new ExpressionChangeEvent(controlValue));
//                break;
//            //todo, maybe?
//            case ControlNumbers.Balance:
//            case ControlNumbers.Pan:
//            default:
//                Console.WriteLine($"Ignored control change {controlEnum}: [{channelNum}] ({controlValue})");
//                break;
//            }
//            break;
//        }
//        //TODO: Fix corner case note off on prev instrument after note on. PROBABLY switching to notes and events, not just timedEvents.
//        //And channel simulation, not just guessing. Which will be fun.
//        case Melanchall.DryWetMidi.Smf.NoteOffEvent noteOffEvent:
//            AddNoteOffEvent(channelNum, time, new NoteOffEvent(noteOffEvent.NoteNumber, noteOffEvent.Velocity));
//            break;
//        case Melanchall.DryWetMidi.Smf.NoteOnEvent noteOnEvent:
//        {
//            //release old note if exists.
//            AddNoteOffEvent(channelNum, time, new NoteOffEvent(noteOnEvent.NoteNumber, noteOnEvent.Velocity));
//            var shouldAddInitEvents = GetOrMakeTrack(channelNum, out var track) || channel.HasPresetChanged();
//            channel.ResetPresetChange();
//            if (shouldAddInitEvents)
//                AddInitEvents(track, time, channel);
//            track.Add(time, new NoteOnEvent(noteOnEvent.NoteNumber, noteOnEvent.Velocity));
//            channel.AddNoteOn(noteOnEvent.NoteNumber);
//            break;
//        }
//        //todo, maybe??
//        case NoteAftertouchEvent _:
//        case ChannelAftertouchEvent _:
//        default:
//            Console.WriteLine($"Ignored channel Event: {@event}");
//            break;
//        }
//    }
//
//    private void AddNoteOffEvent(FBN channelNum, long time, NoteOffEvent noteOffEvent)
//    {
//        var wasNoteOn = _channels[channelNum].NotesOn[noteOffEvent.NoteNumber];
//        _channels[channelNum].NotesOn[noteOffEvent.NoteNumber] = null;
//        if (wasNoteOn != null)
//        {
//            TryGetTrack(wasNoteOn.Value.CombinedPresetNum, channelNum)
//              ?.Add(time, noteOffEvent);
//        }
//    }
//
//    private void AddEventsToChannels(FBN channelNum, long time, IInstTrackEvent @event)
//    {
//        var channel = _channels[channelNum];
//        var set     = _cachedSet;
//        set.Clear();
//        set.Add(channel.GetNoteOn());
//        foreach (var noteOn in channel.NotesOn)
//            if (noteOn != null)
//                set.Add(noteOn.Value);
//        foreach (var noteOn in set)
//            TryGetTrack(noteOn.CombinedPresetNum, channelNum)?.Add(time, @event);
//    }
//
//    private InstrumentTrack TryGetTrack(int presetNum, FBN channelNum)
//    {
//        InstrumentTrack track = null;
//        if (_allTracks.TryGetValue(presetNum, out var instrTracks))
//            instrTracks.TryGetValue(channelNum, out track);
//        return track;
//    }
//
//    /// <summary>
//    /// </summary>
//    /// <param name="presetNum"></param>
//    /// <param name="channelNum"></param>
//    /// <param name="track"></param>
//    /// <returns>True if new track was made</returns>
//    private bool GetOrMakeTrack(FBN channelNum, out InstrumentTrack track)
//    {
//        var channel   = _channels[channelNum];
//        var presetNum = channel.CombinedPresetNum;
//        if (!_allTracks.TryGetValue(presetNum, out var instrTracks))
//            _allTracks[presetNum] = instrTracks = new SortedList<FBN, InstrumentTrack>();
//        if (instrTracks.TryGetValue(channelNum, out track))
//            return false;
//        instrTracks.Add(
//            channelNum,
//            track = new InstrumentTrack(channel.Bank, channel.Program, channel.IsDrumTrack));
//        return true;
//    }
//
//    private struct NoteOn
//    {
//        private readonly SBN _program;
//        private readonly FTBN _bank;
//
//        public int CombinedPresetNum => (_bank << 7) | _program;
//
//        public NoteOn(SBN program, FTBN bank)
//        {
//            _program = program;
//            _bank    = bank;
//        }
//
//        public bool Equals(NoteOn other) => CombinedPresetNum == other.CombinedPresetNum;
//
//        public override bool Equals(object obj) => obj is NoteOn other && Equals(other);
//
//        public override int GetHashCode() => CombinedPresetNum;
//    }
//
//    private class Channel
//    {
//        public readonly NoteOn?[] NotesOn = new NoteOn?[128];
//        private FTBN _bank;
//        private FTBN _lastBank;
//        private SBN _lastProgram;
//        public SBN Expression = SBN.MaxValue;
//        public bool IsDrumTrack;
//        public FTBN PitchBend = 8192;
//        public SBN Program;
//
//        //
//        public SBN Volume = SBN.MaxValue;
//
//        public SBN BankHead
//        {
//            get => _bank.Head;
//            set => _bank.Head = value;
//        }
//
//        public SBN BankTail
//        {
//            get => _bank.Tail;
//            set => _bank.Tail = value;
//        }
//
//        public FTBN Bank => IsDrumTrack ? (FTBN) DrumBank : _bank;
//
//        public int CombinedPresetNum => (Bank << 7) | Program;
//
//        public bool HasPresetChanged() => _lastProgram != Program || _lastBank != Bank;
//
//        public void ResetPresetChange()
//        {
//            _lastProgram = Program;
//            _lastBank    = Bank;
//        }
//
//        public NoteOn GetNoteOn() => new NoteOn(Program, Bank);
//
//        public void AddNoteOn(SBN noteNumber)
//        {
//            NotesOn[noteNumber] = GetNoteOn();
//        }
//    }
//
//    private static void AddInitEvents(InstrumentTrack track, long time, Channel channel)
//    {
//        track.Add(time, new VolumeChangeEvent(channel.Volume));
//        track.Add(time, new ExpressionChangeEvent(channel.Expression));
//        track.Add(time, new PitchBendEvent(channel.PitchBend));
//    }
//}
}