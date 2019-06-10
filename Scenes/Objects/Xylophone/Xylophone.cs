using System;
using Godot;
using MusicMachine.Programs;
using MusicMachine.Scenes.Mechanisms.Glowing;
using MusicMachine.Util;
using MusicMachine.Util.Maths;

namespace MusicMachine.Scenes.Objects.Xylophone
{
[Tool]
public class Xylophone : Spatial, IGlowingArray
{
    private static readonly byte[] ShiftPos = {0, 0, 1, 2, 2, 3, 3, 4, 5, 5, 6, 6};
    private static readonly byte[] RaisePos = {0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0, 1};
    private Spatial _bars;
    [Export] private byte _endNum = 79;
    private Spatial _firstBar;
    private Spatial _nextBar;
    private Spatial _raisedBar;
    [Export] private byte _startNum = 44;
    private IContainsGlowing[] _theGlows;

    [Export] private PackedScene BarScene { get; set; }

    public byte NumBars => (byte) (_endNum - _startNum + 1);

    public IContainsGlowing GetGlowingForNote(SBN note)
    {
        var noteNum = note - _startNum;
        return !noteNum.InRange(0, NumBars) ? null : _theGlows[noteNum];
    }

    public override void _Ready()
    {
        base._Ready();
//        if (Engine.EditorHint) return;
        _firstBar  = GetNode<Spatial>("FirstBar");
        _nextBar   = GetNode<Spatial>("NextBar");
        _raisedBar = _firstBar.GetNode<Spatial>("RaiseBar");
        MakeBars();
    }

    public override string _GetConfigurationWarning() => BarScene == null ? "No Bar Scene Supplied" : "";

    private void MakeBars()
    {
        if (_endNum < _startNum)
        {
            var t = _endNum;
            _endNum   = _startNum;
            _startNum = t;
        }
        _bars = new Spatial {Name = "Bars"};
        AddChild(_bars);
        _theGlows = new IContainsGlowing[NumBars];

        if (BarScene == null) return;
        var instance = BarScene.Instance();
        if (!(instance is IContainsGlowing) || !(instance is Spatial cur))
        {
            GD.PushWarning("Provided scene is not a glowing object.");
            return;
        }

        var firstShift = _startNum / 12 * 7 + ShiftPos[_startNum % 12];
        var firstTf    = _firstBar.Transform;
        var nextTf     = _nextBar.Transform;
        var raiseTf    = _raisedBar.Transform.origin;
        for (var i = _startNum; i <= _endNum; i++)
        {
            var raise     = RaisePos[i % 12] == 1;
            var numShift  = i / 12 * 7 + ShiftPos[i % 12] - firstShift;
            var transform = Transform.Identity;
            switch (numShift)
            {
            case 0:
                if (raise)
                {
                    _raisedBar.AddChild(cur);
                    raise = false;
                }
                else
                    _firstBar.AddChild(cur);
                break;
            case 1 when !raise:
                _nextBar.AddChild(cur);
                break;
            default:
                transform = firstTf.InterpolateWith(nextTf, numShift);
                _bars.AddChild(cur);
                break;
            }
            if (raise) transform = transform.Translated(raiseTf);
            cur.Transform            = transform;
            _theGlows[i - _startNum] = (IContainsGlowing) cur;
            //and the other thing
            if (i != _endNum)
                cur = (Spatial) BarScene.Instance();
        }
    }

    public Func<BaseEvent, Vector3?> GetNoteToBarMapper()
    {
        return @event =>
        {
            if (!(@event is NoteOnEvent noteOnEvent)) return null;
            var noteNumber = (int) noteOnEvent.NoteNumber;
            if (!noteNumber.InRange(_startNum, _endNum + 1)) return null;
            return ((Spatial) _theGlows[noteNumber - _startNum]).GetGlobalTranslation();
        };
    }
}
}