using System;
using Godot;
using MusicMachine.Game;

namespace MusicMachine.Scenes
{
public class GlobalNode : Node
{
    private GlobalNode _instance;

    private GlobalNode()
    {
    }

    public GlobalNode Instance
    {
        get => _instance ?? throw new ShouldNotHappenException();
        private set => _instance = value;
    }

    public override void _Ready()
    {
        if (Instance != null) throw new InvalidOperationException("Two singletons created. Thats a no-no.");
        Instance = this;

        void CenterScreen()
        {
            OS.SetWindowPosition((OS.GetScreenSize(0) - OS.GetWindowSize()) / 2);
        }

        CenterScreen();

        Loops.Start();
    }

    public override void _Notification(int what)
    {
        if (what == MainLoop.NotificationWmQuitRequest)
            OnQuit();
    }

    private void OnQuit()
    {
        Loops.Stop();
    }
}
}