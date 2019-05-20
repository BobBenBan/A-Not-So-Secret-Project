namespace MusicMachine.Music
{
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