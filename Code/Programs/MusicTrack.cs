using System.Collections.Generic;
using System.Linq;
using MusicMachine.Util;
using MusicMachine.Util.Maths;

namespace MusicMachine.Programs
{
/// <summary>
///     A label for a track that stores its time in units of MidiTicks.
///     This is also the class that
/// </summary>
public partial class MusicTrack
{
    private readonly FTBN _bank;
    public readonly bool IsDrumTrack;
    public readonly SBN Program;
    public readonly Track<MusicEvent> Track = new Track<MusicEvent>();

    public MusicTrack(FTBN bank, SBN program, bool isDrumTrack = false)
    {
        _bank       = bank;
        Program     = program;
        IsDrumTrack = isDrumTrack;
    }

    public FTBN Bank => IsDrumTrack ? (FTBN) MidiConstants.DrumBank : _bank;

    public int CombinedPresetNum => (Bank << 7) | Program;

    protected override IEnumerable<Pair<long, BaseEvent>> EventPairs =>
        Track.EventPairs.Select(x => new Pair<long, BaseEvent>(x.First, x.Second));
}
}