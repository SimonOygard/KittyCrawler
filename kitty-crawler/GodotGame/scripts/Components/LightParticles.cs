using Godot;

public partial class LightParticles : GpuParticles2D
{
    public override void _Ready()
    {
        Amount = 16;
        AmountRatio = 1.0f;
        Emitting = true;
    }
}
