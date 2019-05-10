using System;
using System.Windows.Forms;
using Godot;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Util;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace MusicMachine.Music
{
/// <summary>
/// Represents a note with a NoteNumber, pitch that is bendable from 0-1, velocity, and Length in time ticks.
/// Set length to 0 to indicate no length (percussive note).
/// </summary>
public class PitchedNote
{
    public const byte DefaultVelocity = 64;
    public const int A4Number = 69;

    public ushort NoteNumberTimes512;
    public ushort VelocityTimes512;
    public ushort Length;

    public SevenBitNumber NoteNumber
    {
        get => (SevenBitNumber) ((NoteNumberTimes512 + 256) / 512);
        set => NoteNumberTimes512 = (ushort) (value * 512);
    }

    public SevenBitNumber Velocity
    {
        get => (SevenBitNumber) ((VelocityTimes512 + 256) / 512);
        set => VelocityTimes512 = (ushort) (value * 512);
    }

    public PitchedNote(ushort noteNumberTimes512, ushort velocityTimes512 = DefaultVelocity * 512, ushort length = 0)
    {
        NoteNumberTimes512 = noteNumberTimes512;
        VelocityTimes512 = velocityTimes512;
        Length = length;
    }

    public PitchedNote(SevenBitNumber noteNumber, SevenBitNumber velocity, ushort length = 0)
        : this(0, 0, length)
    {
        NoteNumber = noteNumber;
        Velocity = velocity;
    }

    public PitchedNote(SevenBitNumber noteNumber, ushort length = 0)
        : this(0, 0, length)
    {
        NoteNumber = noteNumber;
        Velocity = (SevenBitNumber) DefaultVelocity;
    }

    public short PitchSharpBend
    {
        get => (short) (NoteNumberTimes512 % 512);
        set => NoteNumberTimes512 = (ushort) (NoteNumberTimes512 / 512 * 512 + value);
    }

    public Pitch Pitch => GetPitch();

    public Pitch GetPitch(float a4Pitch = 440) =>
        (float) (a4Pitch * Math.Pow(2, (NoteNumberTimes512 / 512.0 - A4Number) / 12));

    public PitchedNote Lerp(PitchedNote that, float weight)
    {
        var o = new PitchedNote
        (
            NoteNumberTimes512 = (ushort) MoreMath.Lerpi(this.NoteNumberTimes512, that.NoteNumberTimes512, weight),
            VelocityTimes512 = (ushort) MoreMath.Lerpi(this.VelocityTimes512, that.VelocityTimes512, weight),
            Length = 0
        );
        return o;
    }

    public override string ToString()
    {
        return $"Note(n={NoteNumberTimes512},v={VelocityTimes512},l={Length})";
    }
}

public struct Pitch
{
    public float Hertz;

    public Pitch(float hertz)
    {
        Hertz = hertz;
    }

    public static implicit operator float(Pitch pitch)
    {
        return pitch.Hertz;
    }

    public static implicit operator Pitch(float hertz)
    {
        return new Pitch(hertz);
    }
}
}