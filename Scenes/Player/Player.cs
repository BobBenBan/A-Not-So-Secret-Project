using Godot;
using MusicMachine.Config;
using MusicMachine.Util;

namespace MusicMachine.Scenes
{
public class Player : KinematicBody
{
    private Camera _camera;
    private bool _enabled = true;
    private bool _flying;
    private Vector3 _gravity = new Vector3(0, -12f, 0);
    private bool _inFocus = true;
    private Vector3 _outputVel;
    private Spatial _pitch;
    private Spatial _roll;
    private bool _running;
    private Vector3 _up = new Vector3(0, 1, 0);
    [Export(PropertyHint.Range, "0,30")] public float Acceleration = 8f;
    [Export] public bool CanFly = true;
    [Export(PropertyHint.Range, "0,30")] public float Deceleration = 16f;
    [Export(PropertyHint.Range, "0,5")] public float DoubleTapTime = 0.3f;
    [Export(PropertyHint.Range, "0,30")] public float FlyVerticalSpeed = 6;
    [Export(PropertyHint.Range, "0,30")] public float JumpSpeed = 5;
    [Export(PropertyHint.Range, "0,30")] public float MaxGravityVelocity = 20;
    [Export(PropertyHint.Range, "0,90")] public float MaxSlopeDegrees = 45;
    [Export(PropertyHint.Range, "0,5")] public float MouseSensitivity = 0.15F;
    public Delegates.AsProcess Primary;
    [Export(PropertyHint.Range, "0,30")] public float RunSpeed = 10;
    public Delegates.AsProcess Secondary;
    [Export(PropertyHint.Range, "0,30")] public float SlowSpeed = 2;
    [Export] public Vector3 SpawnPoint = Vector3.Inf;
    [Export(PropertyHint.Range, "0,30")] public float WalkSpeed = 6;

    public Transform CameraLocation => _camera.GlobalTransform;

    [Export]
    public Vector3 Gravity
    {
        get => _gravity;
        set
        {
            _up = -value.Normalized();
            _gravity = value;
        }
    }

    private bool EffectiveEnabled
    {
        get => Input.GetMouseMode() == Input.MouseMode.Captured;
        set => Input.SetMouseMode(value ? Input.MouseMode.Captured : Input.MouseMode.Visible);
    }

    private bool InFocus
    {
        set
        {
            _inFocus = value;
            EffectiveEnabled = _inFocus && _enabled;
        }
    }

    private bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            EffectiveEnabled = _inFocus && _enabled;
        }
    }

    //TODO: BETTER GRAVITY
    private void ProcessMovement(float delta)
    {
        //Enable/disable
        if (Inputs.PlayerCancelCursor.JustPressed())
            Enabled = !Enabled;
        var targetVel = new Vector3();
        if (EffectiveEnabled)
        {
            //calculate input movement
            var inputMove = new Vector2();
            if (Inputs.PlayerMoveForward.Pressed())
                inputMove.y++;
            if (Inputs.PlayerMoveBackward.Pressed())
                inputMove.y--;
            if (Inputs.PlayerMoveLeft.Pressed())
                inputMove.x--;
            if (Inputs.PlayerMoveRight.Pressed())
                inputMove.x++;
            inputMove = inputMove.Normalized();

            //calculate movement component of target vel
            var camBasis = _roll.GetGlobalTransform().basis;
            targetVel += camBasis.z.Normalized() * inputMove.y;
            targetVel += -camBasis.x.Normalized() * inputMove.x;
            targetVel.y = 0;
            targetVel = targetVel.Normalized();

            var slowPressed    = Inputs.PlayerSlow.Pressed();
            var runningPressed = Inputs.PlayerRun.Pressed();
            var jumpPressed    = Inputs.PlayerJump.Pressed();
            var isOnFloor      = IsOnFloor();

            if (CanFly && Inputs.PlayerDoubleJump.DoubleTapped(DoubleTapTime, delta))
                _flying = !_flying;

            if (isOnFloor)
                _flying = false;
            //speed
            var slow = slowPressed && !_flying;
            _running = !slow && runningPressed && (_running || _flying || isOnFloor);
            var speed = slow ? SlowSpeed :
                _running     ? RunSpeed : WalkSpeed;

            //Flying, jumping

            if (_flying)
            {
                if (jumpPressed)
                    targetVel.y += FlyVerticalSpeed / WalkSpeed;
                else if (slowPressed)
                    targetVel.y -= FlyVerticalSpeed / WalkSpeed;
            } else if (jumpPressed && isOnFloor)
                _outputVel.y += JumpSpeed;

            targetVel *= speed;
        }

        var actualVel = _outputVel;
        if (!_flying)
        {
            _outputVel += Gravity * delta;
            _outputVel = _outputVel.ClampY(-MaxGravityVelocity, float.MaxValue);
            actualVel.y = 0;
        }

        var accel = (targetVel - actualVel).Dot(actualVel) > 0 ? Acceleration : Deceleration;
        actualVel = actualVel.LinearInterpolate(targetVel, accel * delta);
        if (!_flying)
            actualVel.y = _outputVel.y;

        _outputVel = MoveAndSlide(actualVel, _up, true, 2, Mathf.Deg2Rad(MaxSlopeDegrees));
    }

    public override void _PhysicsProcess(float delta)
    {
        ProcessMovement(delta);
        ProcessActions(delta);
    }

    private void ProcessActions(float delta)
    {
        if (Inputs.PlayerPrimaryAction.JustPressed())
        {
            InFocus = true;
            Primary?.Invoke(delta);
        }

        if (Inputs.PlayerSecondaryInputAction.JustPressed())
        {
            InFocus = true;
            Secondary?.Invoke(delta);
        }
    }

    public override void _Ready()
    {
        _pitch = GetNode<Spatial>("Pitch");
        _roll = _pitch.GetNode<Spatial>("Roll");
        _camera = _roll.GetNode<Camera>("Camera");
        _roll.GetNode<RayCast>("RayCast");
        if (SpawnPoint == Vector3.Inf)
            SpawnPoint = Translation;
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (!(inputEvent is InputEventMouseMotion move) || !EffectiveEnabled)
            return;
        _pitch.RotateX(Mathf.Deg2Rad(move.Relative.y * MouseSensitivity)); // ^ Y -> X
        RotateY(Mathf.Deg2Rad(move.Relative.x * -MouseSensitivity));
        //Rotate z?? future?
        _pitch.RotationDegrees = _pitch.RotationDegrees.ClampX(-80, 80);
        GetTree().SetInputAsHandled();
    }

    private void OnWorldExit()
    {
        Translation = SpawnPoint;
    }

    public override void _Notification(int what)
    {
        switch (what)
        {
        case MainLoop.NotificationWmFocusIn:
            InFocus = true;
            Enabled = true;
            break;
        case MainLoop.NotificationWmFocusOut:
            InFocus = false;
            break;
        }
    }
}
}