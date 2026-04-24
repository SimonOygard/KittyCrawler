using Godot;
using System;
using System.Runtime.InteropServices.JavaScript;
using static Godot.TextServer;

[Tool]
public partial class WallTorch : Node2D
{
    private AnimatedSprite2D _sprite;
    private GpuParticles2D _particles ;

    private TorchDirection _direction = TorchDirection.Default;


    [Export]
    public TorchDirection Direction
    {
        get => _direction;
        set
        { if (_direction == value)
            {
                return;
            }
            _direction = value;
            UpdateTorchVisual();
        }
    }


    public override void _Ready()
    {
        UpdateTorchVisual();

    }

    private void UpdateTorchVisual()
    {
        _sprite = GetNode<AnimatedSprite2D>("WallTorchSprite");
        _particles = _particles ?? GetNode<GpuParticles2D>("GPUParticles2D");

        var spriteInitialized = false;
        

        if (_particles == null)
        {
            GD.Print("Particles not found!");
        }

        if (_sprite == null || spriteInitialized)
        {
            return;
        }

        switch (Direction)
        {
            case TorchDirection.Left:
                _sprite.FlipH = false;
                _sprite.Play("TorchSide");
                _particles.Position = new Vector2(-6, 0);
                spriteInitialized = true;
                break;
            case TorchDirection.Right:
                _sprite.FlipH = true;
                _sprite.Play("TorchSide");
                _particles.Position = new Vector2(6, 0);
                spriteInitialized = true;
                break;
            case TorchDirection.Default:
                _sprite.FlipH = false;
                _sprite.Play("Default");
                _particles.Position = new Vector2(0, -8);
                spriteInitialized = true;
                break;
        }
    }

    public enum TorchDirection
    {
        Default,
        Left,
        Right,
    }
}
