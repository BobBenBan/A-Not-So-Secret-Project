using MusicMachine.Util;

namespace MusicMachine.Config
{
public static class Inputs
{
    public static readonly InputAction PlayerMoveForward = "player_move_forward";
    public static readonly InputAction PlayerMoveBackward = "player_move_backward";
    public static readonly InputAction PlayerMoveLeft = "player_move_left";
    public static readonly InputAction PlayerMoveRight = "player_move_right";
    public static readonly InputAction PlayerJump = "player_jump";
    public static readonly InputAction PlayerSlow = "player_slow";
    public static readonly InputAction PlayerRun = "player_run";
    public static readonly InputAction PlayerCancelCursor = "player_cancel_cursor";

    public static readonly DoubleTapDetector PlayerDoubleJump = new DoubleTapDetector(PlayerJump);

    public static readonly InputAction PlayerPrimaryAction = "action_main";
    public static readonly InputAction PlayerSecondaryInputAction = "action_secondary";
}
}