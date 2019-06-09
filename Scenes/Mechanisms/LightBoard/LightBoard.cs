using System.Collections;
using System.Collections.Generic;
using Godot;
using MusicMachine.Scenes.Objects;
using MusicMachine.Util.Maths;

namespace MusicMachine.Scenes.Mechanisms.LightBoard
{
public class LightBoard : WorldObject
{
    public readonly IReadOnlyList<Bulb> Bulbs;
    private Spatial _bulbs;
    private PackedScene _bulbScene;
    private int[] _onCount;
    private Transform _transform1;
    private Transform _transform2;

    public LightBoard()
    {
        Bulbs = new BulbList(this);
    }

    [Export(PropertyHint.Dir)] public string BulbScene { get; private set; } = Bulb.Dir;

    [Export(PropertyHint.Range, "0,127")] public byte StartNum { get; private set; } = 20;

    [Export(PropertyHint.Range, "0,127")] public byte EndNum { get; private set; } = 99;

    public byte NumBulbs => (byte) (EndNum - StartNum + 1);

    public override void _Ready()
    {
        base._Ready();
        if (EndNum < StartNum)
        {
            var t = EndNum;
            EndNum   = StartNum;
            StartNum = t;
        }
        _transform1 = GetNode<Spatial>("FirstBulb").Transform;
        _transform2 = GetNode<Spatial>("NextBulb").Transform;
        _bulbs      = new Spatial {Name = "Bulbs"};
        AddChild(_bulbs);

        _bulbScene = GD.Load(Bulb.Dir) as PackedScene;
        if (_bulbScene == null)
        {
            GD.PushWarning("Supplied bulb scene does not exist!");
            return;
        }
        for (var i = 0; i < NumBulbs; i++)
        {
            if (!(_bulbScene.Instance() is Bulb bulb))
            {
                GD.PushWarning("Supplied scene is not a bulb!");
                return;
            }
            bulb.Transform = _transform1.InterpolateWith(_transform2, i);
            _bulbs.AddChild(bulb);
        }
        _onCount = new int[NumBulbs];
    }

    public bool AddOnCount(int index)
    {
        index -= StartNum;
        if (!index.InRange(0, NumBulbs))
            return false;
        if (_onCount[index]++ == 0)
            _bulbs.GetChild<Bulb>(index).LightOn = true;
        return true;
    }

    public bool RemoveOnCount(int index)
    {
        index -= StartNum;
        if (!index.InRange(0, NumBulbs))
            return false;
        if (_onCount[index] == 0)
            return false;
        if (--_onCount[index] == 0)
            _bulbs.GetChild<Bulb>(index).LightOn = false;
        return true;
    }

    private void OnWorldExit()
    {
        Translation     = new Vector3(0, 10, 0);
        LinearVelocity  = Vector3.Zero;
        AngularVelocity = Vector3.Zero;
    }

    private sealed class BulbList : IReadOnlyList<Bulb>
    {
        private readonly LightBoard _board;

        internal BulbList(LightBoard board)
        {
            _board = board;
        }

        public IEnumerator<Bulb> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _board.NumBulbs;

        public Bulb this[int index]
        {
            get
            {
                index -= _board.StartNum;
                return index.InRange(0, Count) ? _board._bulbs.GetChild<Bulb>(index) : null;
            }
        }
    }
}
}