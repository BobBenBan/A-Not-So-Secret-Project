using System;
using System.Collections.Generic;
using Godot;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Util;

namespace MusicMachine.Tracks.Mappers
{
public interface IMapper : IAwaitable<bool>
{
    void AnalyzeTrack(IEnumerable<Pair<long, MusicEvent>> track, MappingInfo info);

    void Prepare();
    IEnumerable<Pair<long, Action>> MapTrack(IEnumerable<Pair<long, MusicEvent>> track, MappingInfo info);
}

public struct MappingInfo
{
    public readonly bool IsValid;
    public readonly FTBN Bank;
    public readonly SBN Program;

    //possibly more!?!?!?
    public readonly TempoMap TempoMap;

    public MappingInfo(TempoMap tempoMap, FTBN bank, SBN program)
    {
        TempoMap = tempoMap ?? throw new ArgumentNullException(nameof(tempoMap));
        Bank = bank;
        Program = program;
        IsValid = true;
    }
}
}