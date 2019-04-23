using MusicMachine.Config;

namespace MusicMachine.Util
{
public class DoubleTapDetector
{
    private float _timeSinceLastPress = 0;
    private readonly Action _action;
    public DoubleTapDetector(Action action) => _action = action;

    public bool DoubleTapped(float tapTime, float delta)
    {
        if (_action.JustPressed())
        {
            var tapped = _timeSinceLastPress < tapTime;
            _timeSinceLastPress = 0;
            return tapped;
        }

        _timeSinceLastPress += delta;

        return false;
    }
}
}