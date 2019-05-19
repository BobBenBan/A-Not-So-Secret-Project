using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Music;
using Note = MusicMachine.Music.Note;

namespace MusicMachine.Programs
{
public class Song
{
    private readonly SortedList<SevenBitNumber, NoteTrack> _noteTracks = new SortedList<SevenBitNumber, NoteTrack>();
    private readonly TempoMapManager _tempoMapManager = new TempoMapManager();
    private Song()
    {
    }
    public Song(MidiFile file)
    {
    }
    public Track<MidiEvent> OtherTrack { get; } = new Track<MidiEvent>();
    public IEnumerable<SevenBitNumber> Programs => _noteTracks.Keys;
    public void ReplaceTempoMap(TempoMap tempoMap)
    {
        _tempoMapManager.ReplaceTempoMap(tempoMap);
    }
    public NoteTrack GetProgramTrack(SevenBitNumber number)
    {
        if (_noteTracks.TryGetValue(number, out var track))
            return track;
        var noteTrack = new NoteTrack();
        _noteTracks.Add(number, noteTrack);
        return noteTrack;
    }
}

public class NoteTrack : Track<Note>
{
    public NoteTrack(string name)
        : base(name)
    {
    }
    public NoteTrack()
    {
    }
    public void AddNote(Melanchall.DryWetMidi.Smf.Interaction.Note note, Channel channel)
    {
        Add(note.Time, new Note(note, channel.PitchBend));
    }
}

public class SongBuilder
{
    private readonly Channel[] _channels = new Channel[16];
    private SongBuilder(Song song)
    {
        Song = song;
    }
    public Song Song { get; }
    public void ReadMidiFile(MidiFile file)
    {
        var tempoMap = file.GetTempoMap();
        if (!(tempoMap.TimeDivision is TicksPerQuarterNoteTimeDivision))
            throw new NotSupportedException();
        Song.ReplaceTempoMap(tempoMap);
        foreach (var timedObject in file.GetTimedEventsAndNotes())
            if (timedObject is Melanchall.DryWetMidi.Smf.Interaction.Note note)
                AddNote(note);
            else
                ProcessTimedObject(timedObject);
    }
    private void ProcessTimedObject(ITimedObject timedObject)
    {
        var timedEvent = (TimedEvent) timedObject;
        if (!(timedEvent.Event is ChannelEvent channelEvent))
            return;
        switch (channelEvent)
        {
        case PitchBendEvent pbe:
            _channels[pbe.Channel].PitchBend = pbe.PitchValue;
            break;
        case ControlChangeEvent controlChangeEvent:
            Console.WriteLine(controlChangeEvent.ControlNumber);
            break;
        default:
            Song.OtherTrack.Add(timedEvent.Time, timedEvent.Event);
            break;
        }
    }
    private void AddNote(Melanchall.DryWetMidi.Smf.Interaction.Note note)
    {
        var channel = _channels[note.Channel];
        var program = (SevenBitNumber) channel.Program;
        var track   = Song.GetProgramTrack(program);
        track.AddNote(note, channel);
    }
}
}