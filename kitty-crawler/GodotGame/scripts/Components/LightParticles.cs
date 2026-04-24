using Godot;

public partial class LightParticles : GpuParticles2D
{
    private float _time = 0.0f;

    public override void _Ready()
    {
        Amount = 16;
    }

    public override void _Process(double delta)
    {
        _time += (float)delta;

        // Varierer antall gnister litt
        AmountRatio = Mathf.Lerp(0.5f, 1.0f,
            (Mathf.Sin(_time * 3.0f) + 1.0f) / 2.0f);
    }
}
