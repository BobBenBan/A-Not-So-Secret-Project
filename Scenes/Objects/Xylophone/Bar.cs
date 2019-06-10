using MusicMachine.Scenes.Mechanisms.Glowing;

namespace MusicMachine.Scenes.Objects.Xylophone
{
public class Bar : WorldObject, IContainsGlowing
{
    public GlowingObject Glowing { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Glowing = GetNode<GlowingObject>("Bar");
    }
}
}