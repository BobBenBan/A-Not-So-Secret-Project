using MusicMachine.Util.Maths;

namespace MusicMachine.Programs
{
public abstract class MusicEvent : BaseEvent
{
    internal MusicEvent()
    {
    }
}

public abstract class MusicStateEvent : MusicEvent
{
    internal MusicStateEvent()
    {
    }

    public abstract void ApplyToState(ref PlayingState playingState);
}

public abstract class NoteEvent : MusicEvent
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

public sealed class PitchBendEvent : MusicStateEvent
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

    public override void ApplyToState(ref PlayingState playingState)
    {
        playingState.PitchBend = PitchValue;
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

public abstract class ControlEvent : MusicStateEvent
{
    public abstract SBN ControlNumber { get; }

    public abstract SBN? ControlNumberLsb { get; }
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

public sealed class VolumeChangeEvent : ControlEvent
{
    public VolumeChangeEvent(SBN volume)
    {
        Volume = volume;
    }

    public SBN Volume { get; set; }

    public override SBN ControlNumber { get; } = (byte) ControlNumbers.Volume;

    public override SBN? ControlNumberLsb { get; } = null;

    private bool Equals(VolumeChangeEvent other) => Volume.Equals(other.Volume);

    public override bool Equals(object obj) =>
        ReferenceEquals(this, obj) || obj is VolumeChangeEvent other && Equals(other);

    public override int GetHashCode() => Volume.GetHashCode();

    public override void ApplyToState(ref PlayingState playingState)
    {
        playingState.Volume = Volume;
    }

    public override string ToString() => $"Volume Change ({Volume})";
}

public sealed class ExpressionChangeEvent : ControlEvent
{
    public ExpressionChangeEvent(SBN expression)
    {
        Expression = expression;
    }

    public SBN Expression { get; set; }

    public override SBN ControlNumber { get; } = (byte) ControlNumbers.Expression;

    public override SBN? ControlNumberLsb { get; } = null;

    private bool Equals(ExpressionChangeEvent other) => Expression.Equals(other.Expression);

    public override bool Equals(object obj) =>
        ReferenceEquals(this, obj) || obj is ExpressionChangeEvent other && Equals(other);

    public override int GetHashCode() => Expression.GetHashCode();

    public override void ApplyToState(ref PlayingState playingState)
    {
        playingState.Expression = Expression;
    }

    public override string ToString() => $"Expression Change ({Expression})";
}
}