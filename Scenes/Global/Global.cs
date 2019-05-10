using Godot;

namespace MusicMachine.Scenes.Global
{
public class Global : Node
{
//    public static long ProcessTicks { get; private set; }
//    public static long ProcessNanos { get; private set; }
//    public static long PhysicsFrame { get; private set; }
//    public static long ProcessDeltaNanos { get; private set; }


    /**
     * ALWAYS RUNS.
     */
    public override void _Ready()
    {
        void CenterScreen()
        {
            OS.SetWindowPosition((OS.GetScreenSize(0) - OS.GetWindowSize()) / 2);
        }

        CenterScreen();
    }

//    public override void _Process(float delta)
//    {
//        ProcessTicks++;
//        ProcessDeltaNanos = (long) (delta * 1e9);
//        ProcessNanos += ProcessDeltaNanos;
//    }
//
//    public override void _PhysicsProcess(float delta)
//    {
//        PhysicsFrame++;
//    }
}
}