using Godot;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using MusicMachine.Programs;
using MusicMachine.Scenes;
using static MusicMachine.Scenes.SimpleTrackPlayer<MusicMachine.Music.Note>;
using Note = MusicMachine.Music.Note;

namespace MusicMachine.Test
{
public class TestScene : Area
{
    private readonly PackedScene _obj =
        ResourceLoader.Load<PackedScene>("res://Scenes/Objects/Teapot.tscn");

    private RayCast _launchPoint;
    private Node _objects;
    private Player _player;
    private SimpleTrackPlayer<Note> _simpleTrackPlayer;
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
        var track    = new Track<Note>();
        var ct       = 0;
        foreach (var chunk in midiFile.GetTrackChunks())
        {
            var thisNoteTrack = chunk.GetNotes().MakeNoteTrack(tempoMap);
//          //  if (ct == 1) //transpose;
//                foreach (var list in thisNoteTrack.IterateElements())
//                {
//                    for (var index = 0; index < list.Events.Count; index++)
//                    {
//                        var note = list.Events[index];
//                        note.ActingNoteNumber -= 4;
//                        list.Events[index] = note;
//                    }
//                }

            track.AddRange(thisNoteTrack);
            //ct++;
        }

        _simpleTrackPlayer = new SimpleTrackPlayer<Note>(track);
        AddChild(_simpleTrackPlayer);
        var animation = new Animation();
        animation.AddTrack(Animation.TrackType.Animation);
        _simpleTrackPlayer.Connect(nameof(EventAction), this, nameof(LaunchAMusicalPot));
    }
    private void LaunchAMusicalPot(object element)
    {
        GD.Print(element ?? "null");
//        foreach (var note in element.Events)
//        {
//            var obj = (Teapot) _obj.Instance();
//            var transform = _launchPoint.Transform;
//            transform.origin += new Vector3(0, 0, note.Pitch / 70);
//            obj.SimpleLaunchFrom(transform, 1, 10);
//            _objects.AddChild(obj, true);
//            obj.Pitch = note.Pitch / 440 * 1.2f;
//            obj.ConstantVol = -20;
//        }
    }
    public void OnAction(float delta)
    {
        var obj = (WorldObject) _obj.Instance();
        obj.SimpleLaunchFrom(_player.CameraLocation, 1, 10);
        _objects.AddChild(obj, true);
    }
    private void OnSecondary(float delta)
    {
        if (!_simpleTrackPlayer.IsPlaying)
            _simpleTrackPlayer.Play();
        else
            _simpleTrackPlayer.Pause();
    }
    private void OnBodyExited(Node body)
    {
        if (!body.TryCall("OnWorldExit"))
            body.QueueFree();
    }
}
}