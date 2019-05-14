using System;
using Godot;
using Melanchall.DryWetMidi.Common;
using MusicMachine.Util;

namespace MusicMachine.Music
{
/// <summary>
/// Represents a note with a note number with a pitch bend (following midi standards),
/// and a length in arbitrary units. (make sure to specify what length means in the actual thing!).
///
/// This, just so everyone is happy, assumes the standard that the maximum pitch bend is +/- two semitones.
/// It may get ugly for interpolating notes otherwise.
///
/// Set length to 0 to indicate no length (percussive note).
/// </summary>
//TODO: REFACTOR THE ush**t out of this
//                  (ushort)
public struct Note
{
    private const int SevenBitMask = 0xFF;
    private const int MaxFourteenBitNumber = 0x3FFF;
    public const int A4Number = 69;

    private short _pitchBendValue;
    public SevenBitNumber NoteNumber;
    public SevenBitNumber Velocity;
    public long Length;

    /// <summary>
    /// The MSB of the 14 bit pitch bend value.
    /// </summary>
    public SevenBitNumber PitchBendCoarse
    {
        get => (SevenBitNumber) (_pitchBendValue >> 7);
        set => _pitchBendValue = (short) (_pitchBendValue & SevenBitMask + value << 7);
    }

    /// <summary>
    /// The LSB of the 14 bit pitch bend value.
    /// </summary>
    public SevenBitNumber PitchBendFine
    {
        get => (SevenBitNumber) (_pitchBendValue & SevenBitMask);
        set => _pitchBendValue = (short) (_pitchBendValue & ~SevenBitMask + value);
    }

    /// <summary>
    /// The pitch bend value, as an 14 bit number.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">on set, if the supplied number does not fit in a 14 bit number</exception>
    public short PitchBendValue
    {
        get => _pitchBendValue;
        set
        {
            if (value < 0 || value > MaxFourteenBitNumber + 1) throw new ArgumentOutOfRangeException(nameof(value));
            if (value == MaxFourteenBitNumber + 1) value = MaxFourteenBitNumber; //corner case!
            _pitchBendValue = value;
        }
    }

    /// <summary>
    /// The bend in semitones as calculated from the pitch bend value
    /// </summary>
    public float PitchBendSemitones
    {
        get => (PitchBendValue - 8192) / 4096F;
        set => PitchBendValue = (short) (int) Math.Round(value * 4096F + 8192);
    }

    /// <summary>
    /// The note number with pitch bend taken into account.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">on set, if the supplied number is outside the range of possible note numbers + bend</exception>
    public float ActingNoteNumber
    {
        get => NoteNumber + PitchBendSemitones;

        set
        {
            //corner case if exactly 129. Great
            if (value >= 127 + 2 || value < -2) throw new ArgumentOutOfRangeException();
            NoteNumber = (SevenBitNumber) (
                value > 127 ? 127 :
                value < 0 ? 0 :
                value.RoundToInt()
            );
            var bend = value - NoteNumber;
            PitchBendSemitones = bend;
        }
    }

    /// <summary>
    /// Create a note based on midi 7-bit values.
    /// </summary>
    /// <param name="noteNumber">the note number</param>
    /// <param name="pitchBendCoarse">the MSB of the pitch bend</param>
    /// <param name="pitchBendFine">the LSB of the pitch bend</param>
    /// <param name="velocity">the velocity</param>
    /// <param name="length">the length (64-bit!)</param>
    public Note(byte noteNumber = 0, byte pitchBendCoarse = 64, byte pitchBendFine = 0,
        byte velocity = 0, long length = 0)
    {
        NoteNumber = (SevenBitNumber) noteNumber;
        Velocity = (SevenBitNumber) velocity;
        Length = length;
        _pitchBendValue = 0;
        PitchBendCoarse = (SevenBitNumber) pitchBendCoarse;
        PitchBendFine = (SevenBitNumber) pitchBendFine;
    }

    /// <summary>
    /// Create a note based on the <see cref="ActingNoteNumber"/>, velocity, and length
    /// </summary>
    /// <param name="actingNoteNumber"><see cref="ActingNoteNumber"/></param>
    /// <param name="velocity">the velocity, in 7-bit</param>
    /// <param name="length">the length (64-bit!)</param>
    public Note(float actingNoteNumber, byte velocity = 0, long length = 0)
    {
        Velocity = (SevenBitNumber) velocity;
        Length = length;
        NoteNumber = (SevenBitNumber) 0;
        _pitchBendValue = 0;
        ActingNoteNumber = actingNoteNumber;
    }

    /// <summary>
    /// Get the pitch of this note in HZ, where A4 (note 69) is 440HZ.
    /// </summary>
    public float Pitch => (float) (440 * Math.Pow(2, (ActingNoteNumber - A4Number) / 12));

    /// <summary>
    /// Linearly interpolate this note with another.
    /// </summary>
    /// <param name="that">the other note to interpolate to</param>
    /// <param name="weight">the weight of the note</param>
    /// <param name="length">the length of the resulting note</param>
    /// <returns>The interpolated note.</returns>
    public Note LerpTo(Note that, float weight, long length = 0) =>
        new Note(
            Mathf.Lerp(this.ActingNoteNumber, that.ActingNoteNumber, weight),
            Mathm.Lerpb(this.Velocity, that.Velocity, weight),
            length
        );

    public override string ToString()
    {
        return $"Note(n={ActingNoteNumber},v={Velocity},l={Length})";
    }
}
}