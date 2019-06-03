using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using MusicMachine.Util;

namespace MusicMachine.Scenes.Objects.LightBoard
{
[Tool]
public class LightBoard : Spatial
{
    private Spatial _bulbs;
    private PackedScene _bulbScene;
    private Transform _transform1;
    private Transform _transform2;
    public BulbList Bulbs;

    public LightBoard()
    {
        Bulbs = new BulbList(this);
    }

    [Export(PropertyHint.Dir)] public string BulbScene { get; private set; } = Bulb.Dir;

    [Export(PropertyHint.Range, "0,200")] public int NumBulbs { get; private set; } = 128;

    public override void _Ready()
    {
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
    }

    public struct BulbList : IReadOnlyList<Bulb>
    {
        private readonly LightBoard _board;

        internal BulbList(LightBoard board)
        {
            _board = board;
            Count  = _board.NumBulbs;
        }

        public IEnumerator<Bulb> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count { get; }

        public Bulb this[int index]
        {
            get
            {
                if (!index.InRange(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
                return _board._bulbs.GetChild<Bulb>(index);
            }
        }
    }
}
}