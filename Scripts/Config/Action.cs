using System.Diagnostics.Contracts;
using Godot;

namespace MusicMachine.Config
{
public struct Action
{
    public readonly string Name;

    public Action(string name)
    {
        Name = name;
    }

    [Pure]
    public bool Pressed() => Input.IsActionPressed(Name);

    [Pure]
    public bool JustPressed() => Input.IsActionJustPressed(Name);

    [Pure]
    public bool JustReleased() => Input.IsActionJustPressed(Name);

    [Pure]
    public float ActionStrength() => Input.GetActionStrength(Name);

    [Pure]
    public static explicit operator string(Action action)
    {
        return action.Name;
    }

    [Pure]
    public static implicit operator Action(string name)
    {
        return new Action(name);
    }
}
}