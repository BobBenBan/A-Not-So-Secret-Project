using System.Collections.Generic;
using MusicMachine.ThirdParty.Midi;
using MusicMachine.Tracks;
using MusicMachine.Util;

namespace MusicMachine.Scenes.Functional.Synth
{
internal sealed class ChannelState
{
    public PlayingState PlayingState = PlayingState.Default;

    //pan
    private readonly List<SBN> _cachedList = new List<SBN>();
    public readonly Dictionary<SBN, AdsrPlayer> NotesOn = new Dictionary<SBN, AdsrPlayer>();
    public readonly float AmpDb;
    public readonly float VolumeDb;

    public ChannelState(float ampDb, float volumeDb)
    {
        AmpDb = ampDb;
        VolumeDb = volumeDb;
    }

    public SBN Program
    {
        get => PlayingState.Program;
        set => PlayingState.Program = value;
    }

    public FTBN Bank
    {
        get => PlayingState.Bank;
        set => PlayingState.Bank = value;
    }

    public SBN Volume
    {
        set
        {
            PlayingState.Volume = value;
            UpdateVolume();
        }
    }

    public SBN Expression
    {
        set
        {
            PlayingState.Expression = value;
            UpdateVolume();
        }
    }

    public FTBN PitchBend
    {
        get => PlayingState.PitchBend;
        set
        {
            PlayingState.PitchBend = value;
            foreach (var player in NotesOn.Values)
                player.PitchBend = PlayingState.PitchBendRange;
        }
    }

    public bool IsDrumTrack
    {
        get => PlayingState.IsDrumTrack;
        set => PlayingState.IsDrumTrack = value;
    }

    public float VolumeRange => PlayingState.VolumeRange;

    public float ExpressionRange => PlayingState.ExpressionRange;

    public float PitchBendRange => PlayingState.PitchBendRange;

    private void UpdateVolume()
    {
        foreach (var player in NotesOn.Values)
            player.VolumeRange = PlayingState.VolumeRange * PlayingState.ExpressionRange;
    }

    public void ClearNotPlaying()
    {
        var toRemove = _cachedList;
        toRemove.Clear();
        foreach (var pair in NotesOn)
            if (!pair.Value.Playing)
                toRemove.Add(pair.Key);
        foreach (var i in toRemove)
            NotesOn.Remove(i);
    }

    public void ResetState()
    {
        PitchBend = 8192;
        Volume = 127;
        Expression = 127;
    }
}
}