using System;
using Melanchall.DryWetMidi.Common;

namespace MusicMachine.Music
{
public struct FBN
{
    public const byte MaxValue = 15;
    private byte _value;

    public FBN(byte value)
        : this()
    {
        Value = value;
    }

    public byte Value
    {
        get => _value;
        set
        {
            if (value > MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value));
            _value = value;
        }
    }

    public static implicit operator FBN(byte value) => new FBN(value);

    public static implicit operator byte(FBN value) => value.Value;

    public static implicit operator FourBitNumber(FBN value) => new FourBitNumber(value);

    public static implicit operator FBN(FourBitNumber value) => new FBN(value);
}

/// <summary>
///     Seven Bit Number with implicit conversion.
/// </summary>
public struct SBN
{
    public const byte MaxValue = 127;
    private byte _value;

    public SBN(byte value)
        : this()
    {
        Value = value;
    }

    public byte Value
    {
        get => _value;
        set
        {
            if (value > MaxValue)
                throw new ArgumentOutOfRangeException();
            _value = value;
        }
    }

    public static implicit operator SBN(byte value) => new SBN(value);

    public static implicit operator byte(SBN value) => value.Value;

    public static implicit operator SevenBitNumber(SBN value) => new SevenBitNumber(value);

    public static implicit operator SBN(SevenBitNumber value) => new SBN(value);
}

/// <summary>
///     Wrapped 14 bit number.
/// </summary>
public struct FTBN
{
    public const ushort MaxValue = 0x3FFF;

    public SBN Head { get; set; }

    public SBN Tail { get; set; }

    public ushort Value
    {
        get => (ushort) ((Head << 7) | Tail);
        set
        {
            if (value > MaxValue)
                throw new ArgumentOutOfRangeException();
            Head = (SBN) (value >> 7);
            Tail = (SBN) (value & SBN.MaxValue);
        }
    }

    public FTBN(ushort value)
        : this()
    {
        Value = value;
    }

    public FTBN(SBN head, SBN tail)
    {
        Head = head;
        Tail = tail;
    }

    public static implicit operator FTBN(ushort value) => new FTBN(value);

    public static implicit operator ushort(FTBN value) => value.Value;
}
}