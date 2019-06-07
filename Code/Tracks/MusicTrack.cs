using System;
using System.Collections.Generic;
using MusicMachine.Tracks.Mappers;
using MusicMachine.Util;
using MusicMachine.Util.Maths;

namespace MusicMachine.Tracks
{
/// <summary>
///     A label for a track that stores its time in units of MidiTicks.
///     This is also the class that
/// </summary>
public partial class MusicTrack
{
    private readonly FTBN _bank;
    public readonly bool IsDrumTrack;
    public readonly List<IMapper> Mappers = new List<IMapper>();
    public readonly SBN Program;
    public readonly Track<MusicEvent> Track = new Track<MusicEvent>();
    public string Name;

    public MusicTrack(FTBN bank, SBN program, bool isDrumTrack = false)
    {
        _bank = bank;
        Program = program;
        IsDrumTrack = isDrumTrack;
    }

    public FTBN Bank => IsDrumTrack ? (FTBN) MidiConstants.DrumBank : _bank;

    public int CombinedPresetNum => (Bank << 7) | Program;

    public override string ToString() => Name;

    public IEnumerable<Pair<long, Action>> GetMappedEvents(MappingInfo mappingInfo)
    {
        if (!mappingInfo.IsValid)
            throw new ArgumentException(nameof(mappingInfo));
        foreach (var mapper in Mappers)
        foreach (var pair in mapper.MapTrack(Track.EventPairs, mappingInfo))
            yield return pair;
    }
}
}