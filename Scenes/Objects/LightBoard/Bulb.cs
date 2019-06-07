using Godot;
using MusicMachine.Util.Maths;

namespace MusicMachine.Scenes.Objects.LightBoard
{
[Tool]
public class Bulb : MeshInstance
{
    public const string Dir = "res://Scenes/Objects/LightBoard/Bulb.tscn";
    private float _curTime;
    private SpatialMaterial _material;
    private OmniLight _light;
    [Export(PropertyHint.ExpEasing)] public float Curve = 1f;
    [Export(PropertyHint.Range, "0,100")] public float Energy = 1;
    [Export] public bool LightOn = false;
    [Export(PropertyHint.Range, "0,100")] public float OffTime = 1;
    [Export(PropertyHint.Range, "0,100")] public float OnTime = 1;

    public override void _Ready()
    {
        _material = GetMaterialOverride() as SpatialMaterial;
        _light = GetNode<OmniLight>("OmniLight");
        _curTime = LightOn ? 1 : 0;
    }

    public override void _Process(float delta)
    {
        _curTime += LightOn ? delta / OnTime : -delta / OffTime;
        _curTime.Constrain(0, 1);
        if (_material == null) return;
        var energy = Energy * Mathf.Ease(_curTime, Curve);
//        GD.Print(energy.ToString());
        _material.EmissionEnergy = energy;
        _light.LightEnergy = energy;
        _light.LightIndirectEnergy = energy;
    }
}
}