using MusicMachine.Music;
using MusicMachine.Util;

namespace MusicMachine.Tracks
{
public abstract class InstrumentEvent : BaseEvent
{
    internal InstrumentEvent()
    {
    }
}

public abstract class InstrumentStateEvent : InstrumentEvent
{
    internal InstrumentStateEvent()
    {
    }
}

public abstract class NoteEvent : InstrumentEvent
{
    public SBN NoteNumber;
    public SBN Velocity;

    protected NoteEvent()
    {
    }

    protected NoteEvent(SBN noteNumber, SBN velocity)
    {
        NoteNumber = noteNumber;
        Velocity   = velocity;
    }

    protected bool Equals(NoteEvent other) => NoteNumber.Equals(other.NoteNumber) && Velocity.Equals(other.Velocity);

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        if (obj == this)
            return true;
        return obj.GetType() == GetType() && Equals((NoteEvent) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (NoteNumber.GetHashCode() * 397) ^ Velocity.GetHashCode();
        }
    }
}

public class NoteOnEvent : NoteEvent
{
//    public new NoteOffEvent Corresponding
//    {
//        get => (NoteOffEvent) base.Corresponding;
//        internal set => base.Corresponding = value;
//    }
    public NoteOnEvent()
    {
    }

    public NoteOnEvent(SBN noteNumber, SBN velocity)
        : base(noteNumber, velocity)
    {
    }

    public override string ToString() => $"Note On (Num: {NoteNumber}, Vel: {Velocity})";
}

public class NoteOffEvent : NoteEvent
{
//    public new NoteOnEvent Corresponding
//    {
//        get => (NoteOnEvent) base.Corresponding;
//        internal set
//        {
//            if(value == null) throw new ArgumentNullException(nameof(value));
//            base.Corresponding = value;
//        }
//    }

    public NoteOffEvent(SBN noteNumber, SBN velocity)
        : base(noteNumber, velocity)
    {
    }

    public override string ToString() => $"Note Off (Num: {NoteNumber}, Vel: {Velocity})";
}

public sealed class PitchBendEvent : InstrumentStateEvent
{
    public FTBN PitchValue;

    public PitchBendEvent(FTBN pitchValue)
    {
        PitchValue = pitchValue;
    }

    public float AsRange => PitchValue / 8192 - 1;

    private bool Equals(PitchBendEvent other) => PitchValue.Equals(other.PitchValue);

    public override bool Equals(object obj) =>
        ReferenceEquals(this, obj) || obj is PitchBendEvent other && Equals(other);

    public override int GetHashCode() => PitchValue.GetHashCode();

    public override string ToString() => $"Pitch Bend ({PitchValue})";

    public void ApplyToState(ref MidiState midiState)
    {
        midiState.PitchBend = PitchValue;
    }
}

//public class ProgramChangeEvent : IMusicEvent
//{
//    public SBN Program;
//
//    public ProgramChangeEvent(SBN program)
//    {
//        Program = program;
//    }
//
//    public override string ToString() => $"Program Change ({Program})";
//}

public abstract class ControlEvent : InstrumentStateEvent
{
    public abstract SBN ControlNumber { get; }

    public abstract SBN? ControlNumberLsb { get; }

    public abstract void ApplyToState(ref MidiState midiState);
}

//public class BankSelectEvent : TwoParamControlEvent
//{
//    public FTBN Bank
//    {
//        get => Combined;
//        set => Combined = value;
//    }
//
//    public override SBN ControlNumber { get; } = (byte) ControlNumbers.BankSelect;
//
//    public override SBN? ControlNumberLSB { get; } = (byte) ControlNumbers.BankSelectLsb;
//
//    public FTBN ApplyOn(FTBN original) =>
//        (ushort) (((HeadSet ? Head : original >> 7) << 7) | (TailSet ? Tail : original & SBN.MaxValue));
//
//    public override string ToString() => $"Bank Select ({Bank})";
//}

public sealed class VolumeEventEvent : ControlEvent
{
    public VolumeEventEvent(SBN volume)
    {
        Volume = volume;
    }

    public SBN Volume { get; set; }

    public override SBN ControlNumber { get; } = (byte) ControlNumbers.Volume;

    public override SBN? ControlNumberLsb { get; } = null;

    private bool Equals(VolumeEventEvent other) => Volume.Equals(other.Volume);

    public override bool Equals(object obj) =>
        ReferenceEquals(this, obj) || obj is VolumeEventEvent other && Equals(other);

    public override int GetHashCode() => Volume.GetHashCode();

    public override void ApplyToState(ref MidiState midiState)
    {
        midiState.Volume = Volume;
    }

    public override string ToString() => $"Volume Change ({Volume})";
}

public sealed class ExpressionEventEvent : ControlEvent
{
    public ExpressionEventEvent(SBN expression)
    {
        Expression = expression;
    }

    public SBN Expression { get; set; }

    public override SBN ControlNumber { get; } = (byte) ControlNumbers.Expression;

    public override SBN? ControlNumberLsb { get; } = null;

    private bool Equals(ExpressionEventEvent other) => Expression.Equals(other.Expression);

    public override bool Equals(object obj) =>
        ReferenceEquals(this, obj) || obj is ExpressionEventEvent other && Equals(other);

    public override int GetHashCode() => Expression.GetHashCode();

    public override void ApplyToState(ref MidiState midiState)
    {
        midiState.Expression = Expression;
    }

    public override string ToString() => $"Expression Change ({Expression})";
}
}