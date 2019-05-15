namespace MusicMachine.BetterAnimation
{
public abstract class Key
{
    private readonly Track _owner;

    private protected Key(Track owner)
    {
        _owner = owner;
    }
}
}