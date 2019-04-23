using MusicMachine.Util;

namespace MusicMachine.Config
{
    public static class Inputs
    {
        public static readonly Action PlayerMoveForward = "player_move_forward";
        public static readonly Action PlayerMoveBackward = "player_move_backward";
        public static readonly Action PlayerMoveLeft = "player_move_left";
        public static readonly Action PlayerMoveRight = "player_move_right";
        public static readonly Action PlayerJump = "player_jump";
        public static readonly Action PlayerSlow = "player_slow";
        public static readonly Action PlayerRun = "player_run";
        public static readonly Action PlayerCancelCursor = "player_cancel_cursor";

        public static readonly DoubleTapDetector PlayerDoubleJump = new DoubleTapDetector(PlayerJump);

        public static readonly Action PlayerPrimaryActin = "action_main";
        public static readonly Action PlayerSecondaryAction = "action_secondary";

    }
}