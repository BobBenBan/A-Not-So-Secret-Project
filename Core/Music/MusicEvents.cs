namespace MusicMachine.Music
{
public interface IMusicEvent
{
}

public abstract class NoteEvent : IMusicEvent
{
//    protected NoteEvent Corresponding
//    {
//        get => _corresponding;
//        set
//        {
//            _corresponding.ClearCorresponding();              //clear self link
//            _corresponding = value;                           //set
//            if (value != null && value.Corresponding != this) //set other corresponding.
//                value.Corresponding = this;                   //set and clear
//        }
//    }
//
//    internal void ClearCorresponding()
//    {
//        if (_corresponding != null)               //other
//            _corresponding._corresponding = null; //clear link to self
//        _corresponding = null;                    //clear link
//    }
    public SBN NoteNumber;

    public SBN Velocity;
//    private NoteEvent _corresponding = null;
    protected NoteEvent()
    {
    }
    protected NoteEvent(SBN noteNumber, SBN velocity)
    {
        NoteNumber = noteNumber;
        Velocity   = velocity;
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

    public NoteOnEvent(Melanchall.DryWetMidi.Smf.NoteOnEvent noteOnEvent)
        : this(noteOnEvent.NoteNumber, noteOnEvent.Velocity)
    {
    }
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
    public NoteOffEvent()
    {
    }
    public NoteOffEvent(SBN noteNumber, SBN velocity)
        : base(noteNumber, velocity)
    {
    }
    public NoteOffEvent(Melanchall.DryWetMidi.Smf.NoteOffEvent noteOffEvent)
        : this(noteOffEvent.NoteNumber, noteOffEvent.Velocity)
    {
    }

    public override string ToString() => $"Note Off {{Num: {NoteNumber}, Vel: {Velocity})";

    public static implicit operator NoteOffEvent(Melanchall.DryWetMidi.Smf.NoteOffEvent noteOffEvent)
    {
        return new NoteOffEvent(noteOffEvent.NoteNumber, noteOffEvent.Velocity);
    }
}

public class PitchBendEvent : IMusicEvent
{
    public FTBN PitchValue;
    public PitchBendEvent()
    {
    }
    public PitchBendEvent(FTBN pitchValue)
    {
        PitchValue = pitchValue;
    }

    public float AsRange => PitchValue / 8192 - 1;

    public override string ToString() => $"Pitch Bend ({PitchValue})";

    public PitchBendEvent(Melanchall.DryWetMidi.Smf.PitchBendEvent pitchBendEvent)
        : this(pitchBendEvent.PitchValue)
    {
    }
}

public class ProgramChangeEvent : IMusicEvent
{
    public SBN Program;
    public ProgramChangeEvent(SBN program)
    {
        Program = program;
    }
    public ProgramChangeEvent()
    {
    }

    public override string ToString() => $"Program Change ({Program})";

    public ProgramChangeEvent(Melanchall.DryWetMidi.Smf.ProgramChangeEvent programChangeEvent)
        : this(programChangeEvent.ProgramNumber)
    {
    }
}

public abstract class ControlEvent : IMusicEvent
{
    public abstract SBN ControlNumber { get; }

    public abstract SBN? ControlNumberLSB { get; }
}

public abstract class TwoParamControlEvent : ControlEvent
{
    public bool HeadSet => _head != null;

    private SBN? _head;

    public SBN Head
    {
        get => _head ?? default;
        set => _head = value;
    }

    private SBN? _tail;

    public bool TailSet => _tail != null;

    public SBN Tail
    {
        get => _tail ?? default;
        set => _tail = value;
    }

    protected FTBN Combined
    {
        get => new FTBN(Head, Tail);
        set
        {
            Head = value.Head;
            Tail = value.Tail;
        }
    }
}

public class BankSelectEvent : TwoParamControlEvent
{
    public FTBN Bank
    {
        get => Combined;
        set => Combined = value;
    }

    public FTBN ApplyOn(FTBN original)
    {
        return (ushort) (((HeadSet ? Head : original >> 7) << 7)
                       | (TailSet ? Tail : original & SBN.MaxValue));
    }

    public override SBN ControlNumber { get; } = (byte) ControlNumbers.BankSelect;

    public override SBN? ControlNumberLSB { get; } = (byte) ControlNumbers.BankSelectLsb;

    public override string ToString() => $"Bank Select ({Bank})";
}

public class VolumeChangeEvent : ControlEvent
{
    public SBN Volume { get; set; }

    public VolumeChangeEvent(SBN volume)
    {
        Volume = volume;
    }

    public override SBN ControlNumber { get; } = (byte) ControlNumbers.Volume;

    public override SBN? ControlNumberLSB { get; } = null;

    public override string ToString() => $"Volume Change ({Volume})";
}

public class ExpressionChangeEvent : ControlEvent
{
    public SBN Expression { get; set; }

    public ExpressionChangeEvent(SBN expression)
    {
        Expression = expression;
    }

    public override SBN ControlNumber { get; } = (byte) ControlNumbers.Expression;

    public override SBN? ControlNumberLSB { get; } = null;

    public override string ToString() => $"Expression Change ({Expression})";
}
}