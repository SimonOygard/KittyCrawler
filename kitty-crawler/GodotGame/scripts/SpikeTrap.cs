using Godot;
using PlayerBody;
using System;

public partial class SpikeTrap : Node
{
    private AnimatedSprite2D _sprite;
    private Area2D _area;
    private StaticBody2D _staticBody;
    private CollisionShape2D _blockerShape;

    public bool IsArmed
    {
        get => _isArmed;
        set
        {
            _isArmed = value;
            UpdateTrapState();
        }
    }

    private bool _isArmed = false;

    
    private BossTrap _bossTrap = BossTrap.Spade;
    [Export]
    public BossTrap BossTrapType;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _area = GetNode<Area2D>("Area2D");

        _staticBody = GetNode<StaticBody2D>("StaticBody2D");
        _blockerShape = _staticBody.GetNode<CollisionShape2D>("BlockerShape");

        if (_area != null)
            _area.BodyEntered += OnAreaBodyEntered;

        UpdateTrapState();
    }

    // on body entered -> if body is player -> if trap is armed -> damage player
    private void OnAreaBodyEntered(Node body)
    {
        if (body is Player player && IsArmed)
        {
            GD.Print("Player hit the spike trap and took damage!");
        }
    }

    private void UpdateTrapState()
    {
        if (_sprite == null || _area == null || _blockerShape == null)
            return;

        if (_isArmed)
        {
            _sprite.Play("default");
            _area.Monitoring = true;
            _blockerShape.Disabled = false;
        }
        else
        {
            _sprite.Play("disarmed");
            _area.Monitoring = false;
            _blockerShape.Disabled = true;
        }
    }

}

public enum BossTrap
{
    Spade,
    Heart,
    Club,
    Diamond
}
