using System;
using System.IO;
using System.Reflection;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using NUnit.Framework;

namespace MusicMachine.Test
{
public class MidiFileTest
{
    public static readonly string FilePath =
        Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException(),
            "Midi.midi");

    [SetUp]
    public static void MakeMidiFile()
    {
        var pattern = new PatternBuilder()
                     .SetStep(MusicalTimeSpan.Quarter)
                     .SetVelocity((SevenBitNumber) 127)
                     .SetOctave(4)
                     .Note(NoteName.C)
                     .Note(NoteName.C)
                     .Note(NoteName.G)
                     .Note(NoteName.G)
                     .Note(NoteName.A)
                     .Note(NoteName.A)
                     .Note(NoteName.G)
                     .StepForward()
                     .Note(NoteName.F)
                     .Note(NoteName.F)
                     .Note(NoteName.E)
                     .Note(NoteName.E)
                     .Note(NoteName.D)
                     .Note(NoteName.D)
                     .Chord(new[] {Interval.Four, Interval.Seven}, NoteName.C)
                     .Build();
        var theFile = pattern.ToFile(TempoMap.Create(Tempo.FromBeatsPerMinute(120), new TimeSignature(4, 4)));
        theFile.Write(FilePath, true);
        Console.WriteLine($"Made file at {FilePath}");
    }

    public static MidiFile GetMidiFile() => MidiFile.Read(FilePath);

    [Test]
    public static void ReadMidiFile()
    {
        var midiFile = GetMidiFile();
        foreach (var trackChunk in midiFile.GetTrackChunks())
        foreach (var timedObject in trackChunk.Events)
            Console.WriteLine(timedObject);
    }
}
}