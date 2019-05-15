using System;
using Godot;
using Object = Godot.Object;

namespace MusicMachine.Resources
{
public class Ref : Object
{
    public object Item { get; }

    public Ref(object item)
    {
        Item = item;
    }

    public Ref() { }

    public override string ToString() => Item.ToString();
}
}