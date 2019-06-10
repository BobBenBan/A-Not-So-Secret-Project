using System.Collections.Generic;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Util.Maths;

namespace MusicMachine.Programs
{
public class Program
{
    public const float MaxSemitonesPitchBend = 12;
    private readonly TempoMapManager _tempoMapManager = new TempoMapManager();
    public readonly List<ProgramTrack> Tracks = new List<ProgramTrack>();

    public TempoMap TempoMap => _tempoMapManager.TempoMap;

    public void ReplaceTempoMap(TempoMap tempoMap)
    {
        _tempoMapManager.ReplaceTempoMap(tempoMap);
    }

    public MusicTrack GetMusicTrack(SBN program, FTBN bank)
    {
        foreach (var track in Tracks)
            if (track is MusicTrack musicTrack && musicTrack.Program == program && musicTrack.Bank == bank)
                return musicTrack;
        return null;
    }

    public ProgramTrack GetMusicTrack(SBN program) => GetMusicTrack(program, 0);

    public MusicTrack GetDrumTrack()
    {
        foreach (var track in Tracks)
        {
            if (track is MusicTrack musicTrack && musicTrack.IsDrumTrack) return musicTrack;
        }
        return null;
    }
}
}