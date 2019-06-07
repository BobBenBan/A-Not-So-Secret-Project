using Godot;
using MusicMachine.Scenes.Functional;

namespace MusicMachine.Tracks
{
public class ProgramPlayer : Node
{
    private readonly ActionPlayer _actionPlayer;
    private Program _program;

    public Program Program
    {
        get => _program;
        set
        {
            Stop();
            _program = value;
        }
    }

    public ProgramPlayer(Program program = null, ProcessNode.Mode mode = ProcessNode.Mode.Physics)
    {
        _program = program; //bypass Stop()
        _actionPlayer = new ActionPlayer(null, mode);
        AddChild(_actionPlayer);
    }

    public void Play()
    {
        //todo: queue instead
        _actionPlayer.Track = Program.GetMappedTrack();
        Stop();
    }

    public void Stop()
    {
        _actionPlayer.Stop();
    }
}
}