using Godot;

public partial class FlameLight : PointLight2D
{
    [Export] public float FlickerSpeed = 10.0f;
    [Export] public float EnergyMin = 0.8f;
    [Export] public float EnergyMax = 1.3f;
    [Export] public float RadiusVariation = 0.1f;

    private float _baseEnergy;
    private float _baseTextureScale;
    private float _time = 0.0f;

    public override void _Ready()
    {
        _baseEnergy = Energy;
        _baseTextureScale = TextureScale;
    }

    public override void _Process(double delta)
    {
        _time += (float)delta * FlickerSpeed;

        float noise = Mathf.Sin(_time * 1.7f)
                      * Mathf.Sin(_time * 2.3f)
                      * Mathf.Sin(_time * 0.9f);

        Energy = Mathf.Lerp(EnergyMin, EnergyMax, (noise + 1.0f) / 2.0f);
        TextureScale = _baseTextureScale + GD.Randf() * RadiusVariation * 2 - RadiusVariation;
    }
}
