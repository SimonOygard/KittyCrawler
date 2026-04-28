using Godot;
using Interaction;
using PlayerBody;

public partial class Teleport : Area2D
{
    private AnimatedSprite2D _sprite;

    [Export]
    private Marker2D _destination;

    [Export]
    public string ScenePath { get; set; } = string.Empty;

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
            if (!string.IsNullOrEmpty(ScenePath))
            {
                var transition = GetNode<LevelTransition>("LevelTransition");
                transition.ScenePath = ScenePath;

                transition.TriggerTransition();
                return;
            }

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
