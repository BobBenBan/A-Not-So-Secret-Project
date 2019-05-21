using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;

namespace MusicMachine.Music
{
public abstract class ControlEvent : ChannelEvent
{
    public abstract SevenBitNumber ControlNumber { get; }

    public abstract SevenBitNumber? ControlNumberLSB { get; }

    protected ControlEvent(int parametersCount, FourBitNumber channel)
        : base(parametersCount)
    {
        Channel = channel;
    }
}

public abstract class TwoParamControlEvent : ControlEvent
{
    public TwoParamControlEvent(FourBitNumber channel)
        : base(2, channel)
    {
    }

    public bool MSBset { get; private set; } = false;

    public SevenBitNumber MSB
    {
        get => this[0];
        set
        {
            LSBset = true;
            this[0] = value;
        }
    }

    public bool LSBset { get; private set; } = false;

    public SevenBitNumber LSB
    {
        get => this[1];
        set
        {
            LSBset = true;
            this[1] = value;
        }
    }

    protected ushort Combined
    {
        get
        {
            if (!LSBset && !MSBset)
                throw new InvalidOperationException();
            return DataTypesUtilities.Combine(this[0], this[1]);
        }
        set
        {
            MSB = value.GetHead();
            LSB = value.GetTail();
        }
    }
}

public class BankSelectEvent : TwoParamControlEvent
{
    public ushort Bank
    {
        get => Combined;
        set => Combined = value;
    }

    public BankSelectEvent(FourBitNumber channel, SevenBitNumber intialMsb)
        : base(channel)
    {
        MSB = intialMsb;
    }
    public BankSelectEvent(FourBitNumber channel, SevenBitNumber msb, SevenBitNumber lsb)
        : base(channel)
    {
        MSB = msb;
        LSB = lsb;
    }

    public override SevenBitNumber ControlNumber { get; } = (SevenBitNumber) (byte) ControlNumbers.BankSelect;

    public override SevenBitNumber? ControlNumberLSB { get; } = (SevenBitNumber) (byte) ControlNumbers.BankSelectLsb;

    public override string ToString() => $"Bank Select [{Channel}] ({Bank})";
}

public class VolumeChangeEvent : ControlEvent
{
    public SevenBitNumber Volume
    {
        get => this[0];
        set => this[0] = value;
    }

    public VolumeChangeEvent(FourBitNumber channel, SevenBitNumber volume)
        : base(1, channel)
    {
        Volume = volume;
    }

    public override SevenBitNumber ControlNumber { get; } = (SevenBitNumber) (byte) ControlNumbers.Volume;

    public override SevenBitNumber? ControlNumberLSB { get; } = null;

    public override string ToString() => $"Volume Change [{Channel}] ({Volume})";
}

public class ExpressionChangeEvent : ControlEvent
{
    public SevenBitNumber Expression
    {
        get => this[0];
        set => this[0] = value;
    }

    public ExpressionChangeEvent(FourBitNumber channel, SevenBitNumber expression)
        : base(1, channel)
    {
        Expression = expression;
    }

    public override SevenBitNumber ControlNumber { get; } = (SevenBitNumber) (byte) ControlNumbers.Expression;

    public override SevenBitNumber? ControlNumberLSB { get; } = null;

    public override string ToString() => $"Expression Change [{Channel}] ({Expression})";
}
}