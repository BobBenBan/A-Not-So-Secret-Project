using System.Collections.Concurrent;
using System.Collections.Generic;
using MusicMachine.ThirdParty.Midi;

namespace MusicMachine.Music
{
public class PlayingState
{
    //pan
    private readonly List<short> _cachedList = new List<short>();
    public readonly ConcurrentDictionary<short, AdsrPlayer> NotesOn = new ConcurrentDictionary<short, AdsrPlayer>();
    public InstrumentTrackState State = InstrumentTrackState.Default;

    public float PitchBend => State.PitchBend / 8192f - 1;

    public float Volume => State.Volume / 127f;

    public float Expression => State.Expression / 127f;

    public void ClearNotPlaying()
    {
        var toRemove = _cachedList;
        toRemove.Clear();
        foreach (var pair in NotesOn)
            if (!pair.Value.Playing)
                toRemove.Add(pair.Key);
        foreach (var i in toRemove)
            NotesOn.TryRemove(i, out _);
    }

    public void UpdateVolume(float ampDb, float volumeDb)
    {
//        GD.Print("UPDATE VOLUME:");
        foreach (var player in NotesOn.Values)
            player.UpdateChannelVolume(ampDb, volumeDb, this);
    }

    public void UpdatePitchBend()
    {
        foreach (var player in NotesOn.Values)
            player.PitchBend = PitchBend;
    }
}
}