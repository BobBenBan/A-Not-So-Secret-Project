using MusicMachine.Util;

namespace MusicMachine.Tracks
{
public struct PlayingState
{
    public static PlayingState Default => new PlayingState {Expression = 127, PitchBend = 8192, Volume = 127};

    public FTBN PitchBend { get; set; }

    public SBN Volume { get; set; }

    public SBN Expression { get; set; }

    public FTBN Bank { get; set; }

    public SBN Program { get; set; }

    public bool IsDrumTrack { get; set; }

    public float PitchBendRange => PitchBend / 8192f - 1;

    public float VolumeRange => Volume / 127f;

    public float ExpressionRange => Expression / 127f;
}
}