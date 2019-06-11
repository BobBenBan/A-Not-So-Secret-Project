using MusicMachine.Util.Maths;

namespace MusicMachine.Programs
{
public struct PlayingState
{
    public static PlayingState Default => new PlayingState {Expression = 127, PitchBend = 8192, Volume = 127};

    public FTBN PitchBend;
    public SBN Volume;
    public SBN Expression;
    public FTBN Bank;
    public SBN Program;
    public bool IsDrumTrack;

    public float PitchBendRange => PitchBend / 8192f - 1;

    public float VolumeRange => Volume / 127f;

    public float ExpressionRange => Expression / 127f;
}
}