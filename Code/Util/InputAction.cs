using System.Diagnostics.Contracts;
using Godot;

namespace MusicMachine.Util
{
public struct InputAction
{
    public readonly string Name;

    public InputAction(string name)
    {
        Name = name;
    }

    [Pure] public bool Pressed() => Input.IsActionPressed(Name);

    [Pure] public bool JustPressed() => Input.IsActionJustPressed(Name);

    [Pure] public bool JustReleased() => Input.IsActionJustPressed(Name);

    [Pure] public float ActionStrength() => Input.GetActionStrength(Name);

    [Pure] public static explicit operator string(InputAction inputAction) => inputAction.Name;

    [Pure] public static implicit operator InputAction(string name) => new InputAction(name);
}
}