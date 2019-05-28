namespace MusicMachine.Music
{
public struct InstrumentTrackState
{
    public FTBN PitchBend;
    public SBN Volume;
    public SBN Expression;

    public static InstrumentTrackState Default =>
        new InstrumentTrackState {PitchBend = 8192, Volume = 127, Expression = 127};
}
}