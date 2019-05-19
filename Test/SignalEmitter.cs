using Godot;

namespace MusicMachine.Test
{
public class SignalEmitter : Node
{
    [Signal]
    public delegate void ASignal(Object anObj);

    public void Emit(Object anobj)
    {
        EmitSignal(nameof(ASignal), anobj);
    }
}
}