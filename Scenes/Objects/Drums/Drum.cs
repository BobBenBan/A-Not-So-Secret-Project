using MusicMachine.Mechanisms.Glowing;
using MusicMachine.Scenes.Mechanisms.Glowing;

namespace MusicMachine.Scenes.Objects.Drums
{
public class Drum : WorldObject, IContainsGlowing
{
    public override void _Ready()
    {
        Glowing = GetNode<GlowingObject>("GlowingObject");
    }

    public GlowingObject Glowing { get; private set; }
}
}