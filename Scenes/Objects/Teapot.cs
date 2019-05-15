using Godot;

namespace MusicMachine.Scenes
{
public class Teapot : WorldObject
{
    private MultipleAudioStreamPlayer _streamPlayer;
    public float Pitch = 1;
    public float ConstantVol = float.NaN;

    public override void _Ready()
    {
        base._Ready();
        var material =
            (SpatialMaterial) MeshInstance.GetSurfaceMaterial(0).Duplicate();
        var color = new Color((float) GD.Randf(), (float) GD.Randf(), (float) GD.Randf());
        material.AlbedoColor = color;
        material.Emission = color;
        MeshInstance.SetSurfaceMaterial(0, material);

        _streamPlayer =
            GetNode<MultipleAudioStreamPlayer>("MultipleAudioStreamPlayer");
        Pitch = (float) GD.RandRange(0.5, 2);
        Connect("body_entered", this, nameof(OnBodyEntered));
//        Connect("body_shape_exited", this, nameof(OnBodyExited));
    }


//    private void OnBodyExited(int bodyId, Node node, int bodyShape,
//        int localShape)
//    {
//        Print("EXITED: BodyID: ", bodyId, ", Node: ", node, ", bodyShape: ", bodyShape,
//            ", localShape: ", localShape);
//    }

    private void OnBodyEntered(Node node)
    {
//        Print("ENTERED: BodyID: ", bodyId, ", Node: ", node, ", bodyShape: ", bodyShape,
//            ", localShape: ", localShape);
        var impactVel = node is RigidBody body
            ? LinearVelocity.DistanceSquaredTo(body.LinearVelocity)
            : LinearVelocity.LengthSquared();
        OnCollision(impactVel);
    }

    private void OnCollision(float approxForce)
    {
        if (float.IsNaN(ConstantVol))
        {
            approxForce = Mathf.Clamp(approxForce, 0, 22);
//        Print(_pitch);
            _streamPlayer.PlayInstance(-60 + approxForce, Pitch);
        }
        else
            _streamPlayer.PlayInstance(ConstantVol, Pitch);
    }
}
}