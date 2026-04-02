//using Godot;
//using System;

//namespace PlayerBody;
//public partial class Player : CharacterBody2D
//{
//    [Export] public float Speed = 300f;
//    [Export] public float Acceleration = 10f;
//    [Export] public float Friction = 8f;

//    public Vector2 velocity = Vector2.Zero;

//    private AnimatedSprite2D _sprite;

//    public override void _Ready()
//    {
//        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
//    }

//    public override void _PhysicsProcess(double delta)
//    {
//        Vector2 direction = Input.GetVector("left", "right", "up", "down");
//        velocity = direction * Speed;
//        Velocity = velocity;
//        MoveAndSlide();

//        UpdateAnimation(direction);
//    }

//    private void UpdateAnimation(Vector2 direction)
//    {
//        if (direction == Vector2.Zero)
//        {
//            _sprite.Play("Idle");
//            return;
//        }

//        if (Math.Abs(direction.X) > Math.Abs(direction.Y))
//        {
//            if (direction.X > 0)
//            {
//                _sprite.Play("RunRight");
//            }
//            else
//            {
//                _sprite.Play("RunLeft");
//            }
//        }
//        else
//        {
//            if (direction.Y > 0)
//            {
//                _sprite.Play("RunDown");
//            }
//            else
//            {
//                _sprite.Play("RunUp");
//            }
    
//        }
//    }
//}

