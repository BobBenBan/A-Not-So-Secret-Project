using System;
using Godot.Collections;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Music;
using MusicMachine.Util;

namespace MusicMachine.Tracks
{
public delegate Pair<long, Action>? Mapper(long time, MusicEvent @event, TempoMap tempoMap);

public static class Mappers
{
    //todo: revise sloppy implementation
    public static SortofVirtualSynth PrepareSynth(this Program program, string soundFontFile)
    {
        var usedProgNums = new Array<int>();
        foreach (var track in program.MusicTracks)
            usedProgNums.Add(track.CombinedPresetNum);
        var synth = new SortofVirtualSynth(program.MusicTracks.Count)
        {
            SoundFontFile = soundFontFile, UsedPresetNumbers = usedProgNums
        };
        for (var index = 0; index < program.MusicTracks.Count; index++)
        {
            var track = program.MusicTracks[index];
            synth.PlayingStates[index].State.Program = track.Program;
            synth.PlayingStates[index].State.Bank    = track.Bank; //set channel right
            var index2 = index;
            Mapper mapper = (time, @event, map) =>
            {
                var    index1 = index2;
                Action action = () => { synth.SendEvent(index1, @event); };
                return new Pair<long, Action>(time, action);
            };
            track.Mappers.Add(mapper);
        }
        return synth;
    }
}
}