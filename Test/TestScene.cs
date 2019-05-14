using Godot;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Music;
using MusicMachine.Scenes;
using MusicMachine.Scenes.Music;
using Note = MusicMachine.Music.Note;

namespace MusicMachine.Test
{
public class TestScene : Area
{
    private Player _player;

    private Node _objects;

    private RayCast _launchPoint;
    private TrackPlayer<Note> _trackPlayer;

    private readonly PackedScene _obj =
        ResourceLoader.Load<PackedScene>("res://Scenes/Objects/Teapot.tscn");

    public override void _Ready()
    {
        _player = GetNode<Player>("Player");
        _player.Primary = OnAction;
        _player.Secondary = OnSecondary;
        _objects = GetNode("Objects");
        _launchPoint = GetNode<RayCast>("LaunchPoint");
//        MidiFileTest.MakeMidiFile();
//        var midiFile = MidiFileTest.GetMidiFile();
        var midiFile = MidiFile.Read(ProjectSettings.GlobalizePath("res://Resources/midi.mid"));
        var tempoMap = midiFile.GetTempoMap();
        var track = new Track<int,Note>();
        var ct = 0;
        foreach (var chunk in midiFile.GetTrackChunks())
        {
            var thisNoteTrack = chunk.GetNotes().MakeNoteTrack(tempoMap);
          //  if (ct == 1) //transpose;
                foreach (var list in thisNoteTrack.IterateElements())
                {
                    for (var index = 0; index < list.Events.Count; index++)
                    {
                        var note = list.Events[index];
                        note.ActingNoteNumber -= 4;
                        list.Events[index] = note;
                    }
                }

            track.AddRange(thisNoteTrack);
            //ct++;
        }

        _trackPlayer =
            new TrackPlayer<Note>(track);
        AddChild(_trackPlayer);
        var animation = new Animation();
        animation.AddTrack(Animation.TrackType.Animation);
        _trackPlayer.Action = LaunchAMusicalPot;
    }

    private void LaunchAMusicalPot(int tick, Note note)
    {
        var obj = (Teapot) _obj.Instance();
        var transform = _launchPoint.Transform;
        transform.origin += new Vector3(0, 0, note.Pitch / 70);
        obj.SimpleLaunchFrom(transform, 1, 10);
        _objects.AddChild(obj, true);
        obj.Pitch = note.Pitch / 440 * 1.2f;
        obj.ConstantVol = -20;
    }

    public void OnAction(float delta)
    {
        var obj = (WorldObject) _obj.Instance();
        obj.SimpleLaunchFrom(_player.CameraLocation, 1, 10);
        _objects.AddChild(obj, true);
    }

    private void OnSecondary(float delta)
    {
        if (!_trackPlayer.Playing) _trackPlayer.Play();
        else _trackPlayer.Pause();
    }

    private void OnBodyExited(Node body)
    {
        if (!body.TryCall("OnWorldExit"))
            body.QueueFree();
    }
}
}