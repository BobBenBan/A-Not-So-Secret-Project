using Godot;
using System;

public class Hold : Spatial
{
    public bool HasHold => _hold != null;

    private Spatial _hold; //should always equal only child, or null of none.
    public override void _Ready()
    {
        if(GetChildCount() > 1) GD.PushError("Player Should not hold more than one Spatial");
        if (GetChildCount() == 1)
            _hold = GetChild<Spatial>(0);
    }

    public Spatial PeekHold() => _hold;

    public Spatial TakeHold()
    {
        var o = _hold;
        if(_hold!=null)
            RemoveChild(_hold);
        _hold = null;
        return o;
    }

    public void FreeHold()
    {
        if (_hold != null)
        {
            RemoveChild(_hold);
            _hold.QueueFree();
        }
        _hold = null;
    }

    public Spatial SetAndGetHold(Spatial hold)
    {
        var o = TakeHold();
        _hold = hold;
        AddChild(_hold);
        return o;
    }

    public void SetAndFreeHold(Spatial hold)
    {
        FreeHold();
        _hold = hold;
        AddChild(_hold);
    }
}
