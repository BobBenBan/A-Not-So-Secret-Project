using System.Collections.Generic;
using Godot;
using MusicMachine.Music;

namespace MusicMachine.Test
{
public class MidiPlayerDisplayer : Spatial
{
    private static readonly Spatial[][] Pointers = new Spatial[16][];
    private static readonly PackedScene Pointer = GD.Load<PackedScene>("res://Test/Pointer.tscn");
    private readonly Queue<Spatial> _queuedPointers = new Queue<Spatial>();
    private readonly bool[] _cacheArr = new bool[128];
    private readonly Spatial _displayPoint;
    private readonly MidiSongPlayer _player;
    public MidiPlayerDisplayer(Spatial displayPoint, MidiSong midiSong, string soundFontFile)
    {
        _displayPoint = displayPoint;
        _player = new MidiSongPlayer(midiSong) {SoundFontFile = soundFontFile};
        for (var index = 0; index < Pointers.Length; index++)
            Pointers[index] = new Spatial[128];
        _player.OnStop += delegate { CallDeferred(nameof(Stop)); };
        AddChild(_player);
    }
    public override void _Ready()
    {
        SetProcess(false);
    }
    public void Play()
    {
        _player.Play();
        SetProcess(true);
    }
    public void Stop()
    {
        foreach (var child in GetChildren())
        {
            if (child is Spatial spatial)
                spatial.QueueFree();
        }
        _queuedPointers.Clear();
        _player.Stop();
    }
    public override void _Process(float _)
    {
        var channelHasPt = _cacheArr;
        for (var channelNum = 0; channelNum < MidiSongPlayer.NumChannels; channelNum++)
        {
            for (var index = 0; index < _cacheArr.Length; index++)
                channelHasPt[index] = false;
            var channel    = _player.Channels[channelNum];
            var pitchBend  = channel.PitchBend;
            var expression = channel.Expression;
            var volume     = channel.Volume;
            var notesOn    = channel.NotesOn;
            foreach (var player in notesOn)
            {
                Spatial pointer;
                if ((pointer = Pointers[channelNum][player.Key]) == null)
                {
                    pointer = GetPointer();
                    Pointers[channelNum][player.Key] = pointer;
                }
                pointer.SetTranslation(
                    _displayPoint.Translation
                  + Vector3.Forward * channelNum / 3
                  + Vector3.Right * player.Key * (1 + pitchBend / 2) / 10);
                pointer.SetScale(new Vector3(1, 1, 1) * 2 * expression * volume * player.Value.Velocity / 150f);
                channelHasPt[player.Key] = true;
            }
            for (var index = 0; index < _cacheArr.Length; index++)
            {
                Spatial pointer;
                if (!channelHasPt[index] && (pointer = Pointers[channelNum][index]) != null)
                {
                    pointer.SetVisible(false);
                    _queuedPointers.Enqueue(pointer);
                    Pointers[channelNum][index] = null;
                }
            }
        }
    }
    private Spatial GetPointer()
    {
        if (_queuedPointers.Count != 0)
        {
            var qPointer = _queuedPointers.Dequeue();
            qPointer.SetVisible(true);
            return qPointer;
        }
        var pointer = (Spatial) Pointer.Instance();
        AddChild(pointer);
        return pointer;
    }
}
}