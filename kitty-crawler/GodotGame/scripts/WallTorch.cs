using Godot;
using System;
using System.Runtime.InteropServices.JavaScript;
using static Godot.TextServer;

[Tool]
public partial class WallTorch : Node2D
{
    private AnimatedSprite2D _sprite;

    [Export]
    public TorchDirection Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            UpdateTorchVisual();
        }
    }

    private TorchDirection _direction = TorchDirection.Default;


    public override void _Ready()
    {
        UpdateTorchVisual();

    }

    private void UpdateTorchVisual()
    {
        _sprite = GetNode<AnimatedSprite2D>("WallTorchSprite");

        if (_sprite == null)
        {
            return;
        }

        switch (Direction)
        {
            case TorchDirection.Left:
                _sprite.Play("TorchSide");
                GD.Print("Playing TorchSide animation");
                break;
            case TorchDirection.Right:
                _sprite.FlipH = true;
                _sprite.Play("TorchSide");
                GD.Print("Playing TorchSide animation");
                break;
            case TorchDirection.Default:
                _sprite.Play("Default");
                GD.Print("Playing Default animation");
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
