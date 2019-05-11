using Godot;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace MusicMachine.Scenes.Music
{
public class MultipleAudioStreamPlayer : Node
{
    [Export] public AudioStream Stream { get; private set; }
    [Export] public uint MaxInstances { get; private set; } = 5;
    private int _curStream;

    public override void _EnterTree()
    {
        for (var i = 0; i < MaxInstances; i++)
        {
            var player = new AudioStreamPlayer {Stream = Stream};
            AddChild(player);
        }
    }

    public void PlayInstance(float db = 0, float pitchScale = 1)
    {
        if (MaxInstances == 0) return;
        if (_curStream == MaxInstances) _curStream = 0;
        var player = GetChild<AudioStreamPlayer>(_curStream++);
        player.Stop();
        player.VolumeDb = db;
        player.PitchScale = pitchScale;
        player.Play();
    }
}
}