using System;
using Godot;
using MusicMachine.Util;

namespace MusicMachine.Scenes.Functional
{
public abstract class ProcessOnlyNode : Node
{
    public enum Mode
    {
        Process,
        Physics,
        Audio,
        Manual
    }

    private Mode _mode = Mode.Manual;
    private bool _enabled;

    [Export(PropertyHint.Enum)]
    public Mode ProcessMode
    {
        get => _mode;
        set
        {
            SetProcess(_mode, false);
            _mode = value;
            if (_enabled)
                SetProcess(_mode, true);
        }
    }

    [Export]
    protected bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            SetProcess(_mode, _enabled);
        }
    }

    private void SetProcess(Mode mode, bool value)
    {
        switch (mode)
        {
        case Mode.Process:
            SetProcess(value);
            break;
        case Mode.Physics:
            SetPhysicsProcess(value);
            break;
        case Mode.Audio:
        {
            if (value)
                GlobalNode.AudioProcess += StepTicks;
            else
                GlobalNode.AudioProcess -= StepTicks;
            break;
        }
        case Mode.Manual: break;
        default:          throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    public sealed override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        Ready();
    }

    protected virtual void Ready()
    {
    }

    public sealed override void _Process(float delta)
    {
        StepTicks((delta * 10_000_000.0).RoundToLong());
    }

    public sealed override void _PhysicsProcess(float delta)
    {
        StepTicks((delta * 10_000_000.0).RoundToLong());
    }

    protected abstract void StepTicks(long ticks);

    public void ManualStepTicks(long ticks)
    {
        if (_mode == Mode.Manual) StepTicks(ticks);
    }
}
}