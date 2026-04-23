using Godot;
using Interaction;
using PlayerBody;

public partial class Teleport : Area2D
{
    private AnimatedSprite2D _sprite;

    [Export]
    private Marker2D _destination;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        if (_sprite != null)
        {
            _sprite.Play("default");
        }

        BodyEntered += OnBodyEntered;
        GD.Print("Portal ready");
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Player player)
        {
            
            if (_destination == null)
            {
                GD.PrintErr("TeleportTarget not found!");
                return;
            }

            player.TeleportTo(_destination.GlobalPosition + new Vector2(0, 16));
            GD.Print("Teleported!");
        }
    }
}
