using System;
using System.Collections.Generic;
using Godot;
using MusicMachine.Programs;
using MusicMachine.Resources;
using Object = Godot.Object;

namespace MusicMachine.Test
{
public class TestOnly : Node
{
//    [Signal]
//    public delegate void ASig(object r);

    public override void _EnterTree()
    {
        var a = new SignalEmitter();
        AddChild(a);
        a.Connect(nameof(SignalEmitter.ASignal), this, nameof(ReceiveSignal));
        a.Emit(new Ref());
//        Connect(nameof(ASig), this, nameof(ReceiveSignal));
//        EmitSignal(nameof(ASig),"hi");
        GetTree().Quit();
    }

    private static void ReceiveSignal(Object o)
    {
        if (o != null)
            GD.Print(o);
        else GD.Print("null");
    }
}
}