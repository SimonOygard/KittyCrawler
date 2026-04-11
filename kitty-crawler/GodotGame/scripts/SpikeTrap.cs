using Godot;
using System;

public partial class SpikeTrap : Node2D
{
    [Export] public float FallSpeed = 300;
    [Export] public float SpikeMoveDistance = 50;
    [Export] public float SpikeMoveSpeed = 200;

    private Sprite2D _spikeSprite;
    private Area2D _trigger;
    private Area2D _killzone;
    private Timer _spikeTimer;
    private Timer _retractTimer;
    private Area2D _killCollision;

    private bool _isTriggered = false;
    private bool _isMovingUp = false;
    private Vector2 _initialPosition;
    private Vector2 _targetPosition;


    public override void _Ready()
    {
        _killzone = GetNode<Area2D>("Killzone");
        _spikeTimer = GetNode<Timer>("SpikeTimer");
        _retractTimer = GetNode<Timer>("RetractTimer");
        _killCollision = GetNode<Area2D>("Killzone");
        _spikeSprite = GetNode<Sprite2D>("SpikeSprite");
        _trigger = GetNode<Area2D>("Trigger");

        GD.Print("SpikeTrap ready. Initial position: " + Position);

        _initialPosition = _spikeSprite.Position;
        _targetPosition = _initialPosition - new Vector2(0, SpikeMoveDistance);

        _spikeSprite.Position = _initialPosition;
        _killzone.Monitoring = false;
        _spikeTimer.OneShot = true;
        _retractTimer.OneShot = true;

    }

    public override void _Process(double delta)
    {
        if (_isMovingUp && _spikeSprite.Position != _targetPosition)
        {
            _spikeSprite.Position = _spikeSprite.Position.MoveToward(_targetPosition, SpikeMoveSpeed * (float)delta);
            _killCollision.Position = _spikeSprite.Position;

            if (_spikeSprite.Position == _targetPosition)
            {
                _isMovingUp = false;
                GD.Print("Spikes triggered");
            }
        }
        else if (!_isMovingUp && _spikeSprite.Position != _initialPosition && !_isTriggered)
        {
            _spikeSprite.Position = _spikeSprite.Position.MoveToward(_initialPosition, SpikeMoveSpeed * (float)delta);
            _killCollision.Position = _spikeSprite.Position;

            if (_spikeSprite.Position == _initialPosition)
            {
                GD.Print("Spikes retracted");
            }
        }
    }

    private void _on_trigger_body_entered(Node body)
    {
        if (_isTriggered)
        {
            return;
        }

        if (body.IsInGroup("player"))
        {
            GD.Print("Player entered spike trap trigger");
            _isTriggered = true;
            _trigger.Monitoring = false;
            _spikeTimer.Start();
        }
    }

    private void _on_spike_timer_timeout()
    {
        GD.Print("Spikes extending");
        _isMovingUp = true;
        _killzone.Monitoring = true;
        _retractTimer.Start();
    }

    private void _on_retract_timer_timeout()
    {
        GD.Print("Spikes retracting");
        _isTriggered = false;
        _trigger.Monitoring = true;
        _killzone.Monitoring = false;
    }

    private void _on_kill_zone_body_entered(Node body)
    {
        if (body.IsInGroup("player"))
        {
            GD.Print("Player hit by spikes");
            TakeDamage();
            // Implement player damage or death logic here
        }
    }

    private void TakeDamage()
    {
        GD.Print("You took damage from the spikes!");
    }
}
