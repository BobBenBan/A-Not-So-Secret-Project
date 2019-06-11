using System.Collections.Generic;
using Godot;
using MusicMachine.Programs;
using MusicMachine.Util;

namespace MusicMachine.Scenes.Mechanisms.MovingObject
{
[Tool]
public class MovingObject : Spatial
{
    private readonly Queue<Spatial> _cachedObjects = new Queue<Spatial>();
    [Export] private readonly Vector3 _direction = Vector3.Right;
    public readonly Spatial[] CurSpatials = new Spatial[128];
    private Spatial _objects;
    private bool valid = false;

    [Export] private float Scalar { get; set; } = 1;

    [Export] private PackedScene ObjectScene { get; set; }

    public override string _GetConfigurationWarning() => ObjectScene == null ? "No object scene set" : "";

    public override void _Ready()
    {
        base._Ready();
        if (Engine.EditorHint) return;
        AddChild(_objects = new Spatial {Name = "Objects"});
        if (ObjectScene == null) return;
        var instance = ObjectScene.Instance();
        if (!(instance is Spatial spatial))
        {
            GD.PushWarning("Provided scene is not a spatial!!!");
            return;
        }
        spatial.Visible = false;
        _objects.AddChild(spatial);
        _cachedObjects.Enqueue(spatial);
        valid = true;
    }

    public void AddObject(NoteOnEvent note)
    {
        if (!valid) return;
        var spatial = GetOrCreate();
        var tf      = spatial.GlobalTransform;
        tf.origin               = this.GetGlobalTranslation();
        spatial.GlobalTransform = tf;
        spatial.Scale           = Vector3.One * (Scalar * note.Velocity / 127f);
        spatial.GlobalTranslate(_direction * note.NoteNumber);
        CurSpatials[note.NoteNumber] = spatial;
    }

    public void RemoveObject(NoteOffEvent note)
    {
        if (!valid) return;
        var spatial = CurSpatials[note.NoteNumber];
        if (spatial == null) return;
        CurSpatials[note.NoteNumber] = null;
        RemoveSpatial(spatial);
    }

    private void RemoveSpatial(Spatial spatial)
    {
        spatial.Visible = false;
        _cachedObjects.Enqueue(spatial);
    }

    private Spatial GetOrCreate()
    {
        Spatial spatial;
        if (_cachedObjects.Count != 0)
        {
            spatial = _cachedObjects.Dequeue();
            spatial.SetVisible(true);
        }
        else
        {
            spatial = (Spatial) ObjectScene.Instance();
            _objects.AddChild(spatial);
        }
        return spatial;
    }
}
}