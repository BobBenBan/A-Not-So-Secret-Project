using System;
using Godot;
using MusicMachine.Programs;
using MusicMachine.Programs.Mappers;

namespace MusicMachine.Scenes.Functional.Tracks
{
//TODO: move most implementation to Program, not Program Player.
public class ProgramPlayer : Node
{
    private readonly ActionPlayer _actionPlayer;
    private readonly Track<Action> _actionTrack = new Track<Action>();
    public readonly Program Program;

    public ProgramPlayer(Program program, ProcessNode.Mode mode = ProcessNode.Mode.Physics)
    {
        Program       = program ?? throw new ArgumentNullException(nameof(program)); //bypass Stop()
        _actionPlayer = new ActionPlayer(null, mode);
        AddChild(_actionPlayer);
    }

    public bool Playing => _actionPlayer.Playing;

    public void AnalyzeTracks()
    {
        var info = new MappingInfo(Program.TempoMap);
        foreach (var track in Program.Tracks)
            track.AnalyzeThis(info);
    }

    public void CreateTrack()
    {
        //Todo: In the future, this will be async.
        _actionTrack.Clear();
        var info = new MappingInfo(Program.TempoMap);
        foreach (var track in Program.Tracks)
            _actionTrack.AddRange(track.MapThis(info));
        _actionPlayer.Track = _actionTrack;
    }

    public void Play()
    {
        _actionPlayer.Play();
    }

    public void Stop()
    {
        _actionPlayer.Stop();
    }
}
}