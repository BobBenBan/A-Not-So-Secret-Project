using System.Collections.Generic;
using System.Threading;
using Godot;
using Thread = System.Threading.Thread;

namespace MusicMachine.Scenes
{
/// <summary>
/// This node will be the first thing that runs when the game starts, and handles low-level game managing.
/// This node should not interact with the scene directly.
/// This node manages threads.
/// </summary>
public class Global : Node
{

//    private Thread _QueueActionThread;
    public override void _Ready()
    {
        void CenterScreen()
        {
            OS.SetWindowPosition((OS.GetScreenSize(0) - OS.GetWindowSize()) / 2);
        }

        CenterScreen();

        Looper.Instance.Start();
    }

    public override void _Notification(int what)
    {
        if (what == MainLoop.NotificationWmQuitRequest)
        {
            OnQuit();
        }
    }

    private void OnQuit()
    {
        Looper.Instance.Stop();
    }
}
}