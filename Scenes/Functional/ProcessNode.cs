using System;
using System.Collections.Concurrent;
using Godot;
using MusicMachine.Scenes.Global;
using MusicMachine.Util.Maths;

namespace MusicMachine.Scenes.Functional
{
//TODO TO DO TO DO TO DO TO DO TO DO TO DO TO DO TO DO CONCURRENCY ISSUES!!!!!!!!!!
public abstract class ProcessNode : Node
{
    public enum Mode
    {
        Process,
        Physics,
        Audio,
        Manual
    }

    private readonly ConcurrentQueue<Action> _afterDeferredActions = new ConcurrentQueue<Action>();
    private readonly ConcurrentQueue<Action> _beforeDeferredActions = new ConcurrentQueue<Action>();
    private readonly Action<long> _doStepTicks;
    private bool _enabled = true;
    private Mode _mode = Mode.Manual;

    protected ProcessNode()
    {
        _doStepTicks = DoStepTicks;
    }

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
//            if (_enabled == value) return;
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
                AudioProcess.OnProcess += _doStepTicks;
            else
                AudioProcess.OnProcess -= _doStepTicks;
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
        ProcessMode = _mode; //reset;
        Ready();
    }

    protected virtual void Ready()
    {
    }

    public sealed override void _Process(float delta)
    {
        DoStepTicks((delta * 10_000_000.0).RoundToLong());
    }

    public sealed override void _PhysicsProcess(float delta)
    {
        DoStepTicks((delta * 10_000_000.0).RoundToLong());
    }

    protected abstract void StepTicks(long ticks);

    protected void DoStepTicks(long ticks)
    {
        while (_beforeDeferredActions.TryDequeue(out var action)) action();
        StepTicks(ticks);
        while (_afterDeferredActions.TryDequeue(out var action)) action();
    }

    public void ManualStepTicks(long ticks)
    {
        if (_mode == Mode.Manual) DoStepTicks(ticks);
    }

    public void QueueCallBefore(Action action)
    {
        _beforeDeferredActions.Enqueue(action);
    }

    public void QueueCallAfter(Action action)
    {
        _afterDeferredActions.Enqueue(action);
    }
}
}