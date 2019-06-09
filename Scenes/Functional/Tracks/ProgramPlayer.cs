using System;
using System.Collections.Generic;
using System.Threading;
using Godot;
using MusicMachine.Interaction;
using MusicMachine.Programs;
using MusicMachine.Programs.Mappers;

namespace MusicMachine.Scenes.Functional.Tracks
{
//TODO: move most implementation to Program, not Program Player.
public class ProgramPlayer : Node
{
    [Signal] public delegate void CompletedPrep();

    private const int Unprepared = 0;
    private const int Preparing = 1;
    private const int Prepared = 2;
    private readonly ActionPlayer _actionPlayer;
    public readonly Program Program;
    private CombinedCompletionAwaiter _combinedCompletionAwaiter;
    private int _preparedState = Unprepared;

    public ProgramPlayer(Program program, ProcessNode.Mode mode = ProcessNode.Mode.Physics)
    {
        Program       = program ?? throw new ArgumentNullException(nameof(program)); //bypass Stop()
        _actionPlayer = new ActionPlayer(null, mode);
        AddChild(_actionPlayer);
    }

    //completion awaiters must always be concurrent.
    /// <summary>
    ///     Trys to begin preparing if the state of this ProgramPlayer is in the "Unprepared" state.
    ///     Will return null if not.
    /// </summary>
    /// <returns>If preparing was successfully stared.</returns>
    public bool TryBeginPrepare()
    {
        if (Interlocked.CompareExchange(ref _preparedState, Preparing, Unprepared) != Unprepared) return false;
        var list        = new List<ICompletionAwaiter>();
        var mappingInfo = new MappingInfo(Program.TempoMap);
        foreach (var track in Program.Tracks)
        foreach (var mapper in track.Mappers)
            list.Add(mapper.Prepare(track, mappingInfo));
        _combinedCompletionAwaiter = new CombinedCompletionAwaiter
        {
            Awaiters = list,
            CompletionCallback = () =>
            {
                if (Interlocked.CompareExchange(ref _preparedState, Prepared, Preparing) == Preparing)
                    EmitSignal(nameof(CompletedPrep));
            }
        };
        _combinedCompletionAwaiter.Begin();
        return true;
    }

    public void TryPlay()
    {
        if (_preparedState != Prepared) return;
        var mappingInfo = new MappingInfo(Program.TempoMap);
        var actionTrack = new Track<Action>();
        foreach (var track in Program.Tracks)
        foreach (var mapper in track.Mappers)
            actionTrack.AddRange(mapper.MapTrack(track, mappingInfo));
        _actionPlayer.Track = actionTrack;

        Stop();
    }

    public void Stop()
    {
        _actionPlayer.Stop();
    }
}
}