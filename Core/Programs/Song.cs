using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Music;
using Note = MusicMachine.Music.Note;

namespace MusicMachine.Programs
{
public class Song
{
    private readonly TempoMapManager _tempoMapManager = new TempoMapManager();
    private readonly SortedList<SevenBitNumber, NoteTrack> _noteTracks = new SortedList<SevenBitNumber, NoteTrack>();
    public Track<MidiEvent> OtherTrack { get; } = new Track<MidiEvent>();

    private Song() { }

    public Song(MidiFile file) { }

    public void ReplaceTempoMap(TempoMap tempoMap) { _tempoMapManager.ReplaceTempoMap(tempoMap); }

    public NoteTrack GetProgramTrack(SevenBitNumber number)
    {
        if (_noteTracks.TryGetValue(number, out var track))
            return track;
        var noteTrack = new NoteTrack();
        _noteTracks.Add(number, noteTrack);
        return noteTrack;
    }

    public IEnumerable<SevenBitNumber> Programs => _noteTracks.Keys;
}

public class NoteTrack : Track<Note>
{
    public NoteTrack(string name)
        : base(name)
    { }

    public NoteTrack() { }

    public void AddNote(Melanchall.DryWetMidi.Smf.Interaction.Note note, Channel channel)
    {
        Add(note.Time, new Note(note, channel.PitchBend));
    }
}

public class SongBuilder
{
    private Channel[] channels = new Channel[16];
    public Song Song { get; }

    private SongBuilder(Song song) { Song = song; }

    public void ReadMidiFile(MidiFile file)
    {
        var tempoMap = file.GetTempoMap();
        if (!(tempoMap.TimeDivision is TicksPerQuarterNoteTimeDivision))
            throw new NotSupportedException();
        Song.ReplaceTempoMap(tempoMap);
        foreach (var timedObject in file.GetTimedEventsAndNotes())
        {
            if (timedObject is Melanchall.DryWetMidi.Smf.Interaction.Note note)
            {
                AddNote(note);
            } else
            {
                ProcessTimedObject(timedObject);
            }
        }
    }

    private void ProcessTimedObject(ITimedObject timedObject)
    {
        var timedEvent = (TimedEvent) timedObject;
        if (!(timedEvent.Event is ChannelEvent channelEvent))
            return;
        switch (channelEvent)
        {
        case PitchBendEvent pbe:
            channels[pbe.Channel].PitchBend = pbe.PitchValue;
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
        var channel = channels[note.Channel];
        var program = (SevenBitNumber) channel.Program;
        var track   = Song.GetProgramTrack(program);
        track.AddNote(note, channel);
    }
}
}