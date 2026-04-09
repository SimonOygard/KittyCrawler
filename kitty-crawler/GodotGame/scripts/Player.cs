using Godot;
using System;


namespace PlayerBody

#region Gammal Movement

/*
public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 300f;
    [Export] public float Acceleration = 10f;
    [Export] public float Friction = 8f;

    public Vector2 velocity = Vector2.Zero;

    private AnimatedSprite2D _sprite;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = Input.GetVector("left", "right", "up", "down");
        velocity = direction * Speed;
        Velocity = velocity;
        MoveAndSlide();

        UpdateAnimation(direction);
    }

    private void UpdateAnimation(Vector2 direction)
    {
        if (direction == Vector2.Zero)
        {
            _sprite.Play("Idle");
            return;
        }

        if (Math.Abs(direction.X) > Math.Abs(direction.Y))
        {
            if (direction.X > 0)
            {
                _sprite.Play("RunRight");
            }
            else
            {
                _sprite.Play("RunLeft");
            }
        }
        else
        {
            if (direction.Y > 0)
            {
                _sprite.Play("RunDown");
            }
            else
            {
                _sprite.Play("RunUp");
            }

        }
    }
}

*/
/*
using Godot;
using Game.Core;
using Logger = Game.Core.Logger;

namespace Game.Gameplay
*/


#endregion
{
    public partial class Player : CharacterBody2D
    {
        [Export] public int GridSize = 16;
        [Export] public float TurnThreshold = 0.1f;

        private Vector2 _targetPosition;
        private bool _isMoving = false;
        private Vector2 _direction = Vector2.Down;

        private AnimatedSprite2D _sprite;
        private RayCast2D _ray;
        private float _inputHoldTime = 0f;
        //private const float TurnThreshold = 0.1f;


        public override void _Ready()
        {
            Position = new Vector2(
                Mathf.Round(Position.X / GridSize) * GridSize,
                Mathf.Round(Position.Y / GridSize) * GridSize
            );
            _targetPosition = Position;

            _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            _ray = GetNode<RayCast2D>("RayCast2D");
            _ray.Enabled = true;
        }


        public override void _Process(double delta)
        {
            if (_isMoving)
            {
                MoveTowardTarget(delta);
            }
            else
            {
                HandleInput((float)delta);
            }
        }

        private void HandleInput(float delta)
        {
            Vector2 input = Vector2.Zero;

            if (Input.IsActionPressed("up")) input = Vector2.Up;
            else if (Input.IsActionPressed("down")) input = Vector2.Down;
            else if (Input.IsActionPressed("left")) input = Vector2.Left;
            else if (Input.IsActionPressed("right")) input = Vector2.Right;

            if (input != Vector2.Zero)
            {
                if (input != _direction)
                {
                    _direction = input;
                    _inputHoldTime = 0f;
                    PlayTurnAnimation();
                }
                else
                {
                    _inputHoldTime += delta;

                    if (_inputHoldTime >= TurnThreshold)
                    {
                        _ray.TargetPosition = input * GridSize;
                        _ray.ForceRaycastUpdate();

                        if (!_ray.IsColliding())
                        {
                            _targetPosition = Position + input * GridSize;
                            _isMoving = true;
                        }
                        UpdateAnimation();
                    }
                }
            }
            else
            {
                _inputHoldTime = 0f;
                UpdateIdleAnimation();
            }
        }


        private void MoveTowardTarget(double delta)
        {
            Position = Position.MoveToward(_targetPosition, (float)delta * GridSize * 6);

            if (Position.DistanceTo(_targetPosition) < 0.1f)
            {
                Position = _targetPosition;
                _isMoving = false;
            }
        }

        private void PlayTurnAnimation()
        {
            string turnAnim = "";
            if (_direction == Vector2.Up) turnAnim = "turn_up";
            else if (_direction == Vector2.Down) turnAnim = "turn_down";
            else if (_direction == Vector2.Left) turnAnim = "turn_left";
            else if (_direction == Vector2.Right) turnAnim = "turn_right";

            _sprite.Play(turnAnim);
        }




        private void UpdateIdleAnimation()
        {
            string idleAnim = "";
            if (_direction == Vector2.Up) idleAnim = "idle_up";
            else if (_direction == Vector2.Down) idleAnim = "idle_down";
            else if (_direction == Vector2.Left) idleAnim = "idle_left";
            else if (_direction == Vector2.Right) idleAnim = "idle_right";

            if (_sprite.Animation != idleAnim)
                _sprite.Play(idleAnim);
        }

        private void UpdateAnimation()
        {
            if (_isMoving)
            {
                string walkAnim = "";
                if (_direction == Vector2.Up) walkAnim = "walk_up";
                else if (_direction == Vector2.Down) walkAnim = "walk_down";
                else if (_direction == Vector2.Left) walkAnim = "walk_left";
                else if (_direction == Vector2.Right) walkAnim = "walk_right";

                if (_sprite.Animation != walkAnim)
                    _sprite.Play(walkAnim);
            }
        }
    }
}
