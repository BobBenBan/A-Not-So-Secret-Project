using Godot;

namespace MusicMachine.Resources
{
public class Ref : Object
{
    public Ref(object item)
    {
        Item = item;
    }
    public Ref()
    {
    }
    public object Item { get; }
    public override string ToString() => Item.ToString();
}
}