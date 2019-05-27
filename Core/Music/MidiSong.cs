using System.Collections.Generic;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace MusicMachine.Music
{
public class MidiSong
{
    public const float MaxSemitonesPitchBend = 7;
    public const byte DrumChannel = 0x09;
    private readonly TempoMapManager _tempoMapManager = new TempoMapManager();
    public readonly List<MidiTrack> Tracks = new List<MidiTrack>();

    public TempoMap TempoMap => _tempoMapManager.TempoMap;

    public void ReplaceTempoMap(TempoMap tempoMap)
    {
        _tempoMapManager.ReplaceTempoMap(tempoMap);
    }
}

/// <summary>
///     A label for a track that stores its time in units of MidiTicks.
/// </summary>
public class MidiTrack : Track<IMusicEvent>
{
    public FBN Channel;

    public MidiTrack(FBN channel)
        : base($"MidiTrack Channel {channel}")
    {
        Channel = channel;
    }

    public bool IsDrumTrack => Channel == MidiSong.DrumChannel;
}
}