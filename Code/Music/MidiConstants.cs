namespace MusicMachine.Music
{
public static class MidiConstants
{
    public const byte DrumChannel = 0x09;
    public const int DrumBank = 128;
}

public enum ControlNumbers : byte
{
    BankSelect = 0x00,
    BankSelectLsb = 0x20,

    //1-5
    Volume = 0x07,
    Balance = 0x08,
    Pan = 0x0A,
    Expression = 0x0B
}
}