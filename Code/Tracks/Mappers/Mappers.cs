using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MusicMachine.Interaction;
using MusicMachine.Util;

namespace MusicMachine.Tracks.Mappers
{
public static class Mappers
{
    public static readonly IMapper EmptyMapper = DaEmptyMapper.Instance;

    private sealed class DaEmptyMapper : IMapper
    {
        public static readonly DaEmptyMapper Instance = new DaEmptyMapper();

        private DaEmptyMapper()
        {
        }

        public void AnalyzeTrack(IEnumerable<Pair<long, MusicEvent>> track, MappingInfo info)
        {
        }

        public void Prepare()
        {
        }

        public IEnumerable<Pair<long, Action>> MapTrack(
            IEnumerable<Pair<long, MusicEvent>> track,
            MappingInfo info)
        {
            return Enumerable.Empty<Pair<long, Action>>();
        }

        public IAwaiter<bool> GetAwaiter() => new TrueAwaitable();
    }

    public static void MatchToTrack(this PlayingState state, MusicTrack track)
    {
        state.Program = track.Program;
        state.Bank = track.Bank;
        state.IsDrumTrack = track.IsDrumTrack;
    }
}
}