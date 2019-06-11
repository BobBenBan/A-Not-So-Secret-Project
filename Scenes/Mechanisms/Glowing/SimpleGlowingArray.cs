using Godot;
using MusicMachine.Mechanisms.Glowing;
using MusicMachine.Util.Maths;

namespace MusicMachine.Scenes.Mechanisms.Glowing
{
[Tool]
public class SimpleGlowingArray : Spatial, IGlowingArray
{
    private Spatial _objects;

    [Export(PropertyHint.Range, "0,127")] protected byte EndNum { get; private set; } = 99;

    [Export(PropertyHint.Range, "0,127")] protected byte StartNum { get; private set; } = 20;

    protected IContainsGlowing[] TheGlows;
    private Transform _transform1;
    private Transform _transform2;

    [Export] private PackedScene ObjScene { get; set; }

    public byte NumObjects => (byte) (EndNum - StartNum + 1);

    public virtual IContainsGlowing GetGlowingForNote(SBN note)
    {
        var noteNum = note - StartNum;
        return !noteNum.InRange(0, NumObjects) ? null : TheGlows[noteNum];
    }

    public override string _GetConfigurationWarning() => ObjScene == null ? "No Object Scene Supplied" : "";

    public override void _Ready()
    {
        base._Ready();
        _transform1 = GetNode<Spatial>("FirstObj").Transform;
        _transform2 = GetNode<Spatial>("NextObj").Transform;
        MakeObjects();
    }

    private void MakeObjects()
    {
        if (EndNum < StartNum)
        {
            var t = EndNum;
            EndNum   = StartNum;
            StartNum = t;
        }
        _objects = new Spatial {Name = "Objects"};
        AddChild(_objects);
        TheGlows = new IContainsGlowing[NumObjects];

        if (ObjScene == null) return;
        var instance = ObjScene.Instance();
        if (!(instance is IContainsGlowing) || !(instance is Spatial cur))
        {
            GD.PushWarning($"Provided scene at GlowingArray ({Name}) is not a glowing object.");
            return;
        }

        for (var i = 0; i < NumObjects; i++)
        {
            cur           = (Spatial) ObjScene.Instance();
            cur.Transform = _transform1.InterpolateWith(_transform2, i);
            _objects.AddChild(cur);
            if (!Engine.EditorHint)
                TheGlows[i] = (IContainsGlowing) cur;
        }
    }
}
}