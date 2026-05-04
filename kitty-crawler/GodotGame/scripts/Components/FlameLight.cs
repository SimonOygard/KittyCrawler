using Godot;

public partial class FlameLight : PointLight2D
{
    [Export] public float FlickerSpeed = 2.0f;
    [Export] public float EnergyMin = 0.9f;
    [Export] public float EnergyMax = 1.1f;
    [Export] public float RadiusVariation = 0.05f;

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

        float t = (noise + 1.0f) / 2.0f; 

        Energy = Mathf.Lerp(EnergyMin, EnergyMax, t);
        TextureScale = _baseTextureScale + Mathf.Lerp(-RadiusVariation, RadiusVariation, t);
    }
}
