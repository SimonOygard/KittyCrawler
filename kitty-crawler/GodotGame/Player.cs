using Godot;
using System;

namespace PlayerBody;
public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 300f;

    public Vector2 velocity = Vector2.Zero;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = Input.GetVector("left", "right", "up", "down");
        velocity = direction * Speed;
        Velocity = velocity;
        MoveAndSlide();
    }
}

