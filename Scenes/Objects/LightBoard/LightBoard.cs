using Godot;
using MusicMachine.Scenes.Mechanisms.Glowing;
using MusicMachine.Util.Maths;

namespace MusicMachine.Scenes.Objects.LightBoard
{
[Tool]
public class LightBoard : WorldObject, IGlowingArray
{
    private Spatial _bulbs;
    [Export(PropertyHint.Range, "0,127")] private byte _endNum = 99;
    private int[] _onCount;
    [Export(PropertyHint.Range, "0,127")] private byte _startNum = 20;
    private IContainsGlowing[] _theGlows;
    private Transform _transform1;
    private Transform _transform2;

    [Export] private PackedScene BulbScene { get; set; }

    public byte NumBulbs => (byte) (_endNum - _startNum + 1);

    public IContainsGlowing GetGlowingForNote(SBN note)
    {
        var noteNum = note - _startNum;
        return !noteNum.InRange(0, NumBulbs) ? null : _theGlows[noteNum];
    }

    public override string _GetConfigurationWarning() => BulbScene == null ? "No Bar Scene Supplied" : "";

    public override void _Ready()
    {
        base._Ready();
        _transform1 = GetNode<Spatial>("FirstBulb").Transform;
        _transform2 = GetNode<Spatial>("NextBulb").Transform;
        MakeBulbs();
    }

    private void MakeBulbs()
    {
        if (_endNum < _startNum)
        {
            var t = _endNum;
            _endNum   = _startNum;
            _startNum = t;
        }
        _bulbs = new Spatial {Name = "Bulbs"};
        AddChild(_bulbs);
        _theGlows = new IContainsGlowing[NumBulbs];

        if (BulbScene == null) return;
        var instance = BulbScene.Instance();
        if (!(instance is IContainsGlowing) || !(instance is Spatial cur))
        {
            GD.PushWarning("Provided scene is not a glowing object.");
            return;
        }

        for (var i = 0; i < NumBulbs; i++)
        {
            cur           = (Spatial) BulbScene.Instance();
            cur.Transform = _transform1.InterpolateWith(_transform2, i);
            _bulbs.AddChild(cur);
            _theGlows[i] = (IContainsGlowing) cur;
        }
        _onCount = new int[NumBulbs];
    }

    private void OnWorldExit()
    {
        Translation     = new Vector3(0, 10, 0);
        LinearVelocity  = Vector3.Zero;
        AngularVelocity = Vector3.Zero;
    }
}
}