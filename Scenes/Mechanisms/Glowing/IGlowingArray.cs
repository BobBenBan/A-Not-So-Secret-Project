using MusicMachine.Util.Maths;

namespace MusicMachine.Scenes.Mechanisms.Glowing
{
public interface IGlowingArray
{
    IContainsGlowing GetGlowingForNote(SBN note);
}
}