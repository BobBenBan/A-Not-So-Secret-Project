using MusicMachine.Util;

namespace MusicMachine.Music
{
public struct MidiState
{
    public FTBN PitchBend;
    public SBN Volume;
    public SBN Expression;
    public FTBN Bank;
    public SBN Program;
    public bool IsDrumTrack;

    public static MidiState Default => new MidiState {PitchBend = 8192, Volume = 127, Expression = 127};
}
}