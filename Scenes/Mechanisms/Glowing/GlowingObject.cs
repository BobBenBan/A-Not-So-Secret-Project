using Godot;
using MusicMachine.Mechanisms.Glowing;
using MusicMachine.Util.Maths;

namespace MusicMachine.Scenes.Mechanisms.Glowing
{
[Tool]
public class GlowingObject : MeshInstance, IContainsGlowing
{
    private float _curTime;
    private OmniLight _light;
    private SpatialMaterial _material;
    [Export(PropertyHint.ExpEasing)] public float Curve = 1f;
    [Export(PropertyHint.Range, "0,100")] public float Energy = 1;
    [Export] public bool IsGlowing = false;
    [Export(PropertyHint.Range, "0,100")] public float OffTime = 1;
    [Export(PropertyHint.Range, "0,100")] public float OnTime = 1;

    public GlowingObject Glowing => this;

    public override void _Ready()
    {
        _material = GetMaterialOverride() as SpatialMaterial;
        _light    = GetNode<OmniLight>("OmniLight");
        _curTime  = IsGlowing ? 1 : 0;
    }

    public override void _Process(float delta)
    {
        _curTime += IsGlowing ? delta / OnTime : -delta / OffTime;
        _curTime.Constrain(0, 1);
        if (_material == null) return;
        var energy = Energy * Mathf.Ease(_curTime, Curve);
//        GD.Print(energy.ToString());
        _material.EmissionEnergy = energy;
        _light.Visible           = _curTime != 0;
        _light.LightEnergy       = energy / 12 + 0.1f;
    }
}
}