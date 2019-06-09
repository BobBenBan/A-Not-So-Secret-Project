using System;
using Godot;

namespace MusicMachine.Scenes.Global
{
public sealed class GlobalNode : Node
{
    private static GlobalNode _instance;

    public static GlobalNode Instance
    {
        get => _instance ?? throw new InvalidOperationException($"No {nameof(AudioProcess)} node instantiated");
        private set
        {
            if (value != null && _instance != null)
                throw new InvalidOperationException($"Two {nameof(AudioProcess)} nodes instantiated!");
            _instance = value;
        }
    }

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        Instance = null;
    }
}
}