using System.Collections.Concurrent;
using System.Collections.Generic;
using MusicMachine.ThirdParty.Midi;
using MusicMachine.Util;

namespace MusicMachine.Music
{
public partial class SortofVirtualSynth
{
    public class PlayingState
    {
        //pan
        private readonly List<short> _cachedList = new List<short>();
        public readonly ConcurrentDictionary<short, AdsrPlayer> NotesOn = new ConcurrentDictionary<short, AdsrPlayer>();
        public MidiState State = MidiState.Default;

        public float PitchBend => State.PitchBend / 8192f - 1;

        public float Volume => State.Volume / 127f;

        public float Expression => State.Expression / 127f;

        public SBN Program => State.Program;

        public FTBN Bank => State.Bank;

        public bool IsDrumTrack => State.IsDrumTrack;

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

        public void Reset()
        {
            State = MidiState.Default;
        }

        public void StopAllNotes()
        {
            foreach (var player in NotesOn.Values) player.Stop();
            NotesOn.Clear();
        }
    }
}
}