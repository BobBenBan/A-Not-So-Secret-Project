using MusicMachine.Util.Maths;

namespace MusicMachine.Mechanisms.Glowing
{
public interface IGlowingArray
{
    IContainsGlowing GetGlowingForNote(SBN note);
}
}