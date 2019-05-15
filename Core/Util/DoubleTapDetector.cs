namespace MusicMachine
{
public class DoubleTapDetector
{
    private float _timeSinceLastPress;
    private readonly InputAction _inputAction;
    public DoubleTapDetector(InputAction inputAction) => _inputAction = inputAction;

    public bool DoubleTapped(float tapTime, float delta)
    {
        if (_inputAction.JustPressed())
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