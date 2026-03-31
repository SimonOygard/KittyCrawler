using Godot;
using Game.Core;
using Logger = Game.Core.Logger;

namespace Game.Gameplay
{
    public partial class Player : CharacterBody2D
    {
        [Export] public int GridSize = 16;

        private Vector2 _targetPosition;
        private bool _isMoving = false;
        private Vector2 _direction = Vector2.Down;

        private AnimatedSprite2D _sprite;
        private RayCast2D _ray;
        private float _inputHoldTime = 0f;
        private const float TurnThreshold = 0.1f;


        public override void _Ready()
        {
            Position = new Vector2(
                Mathf.Round(Position.X / GridSize) * GridSize,
                Mathf.Round(Position.Y / GridSize) * GridSize
            );
            _targetPosition = Position;

            _sprite = GetNode<AnimatedSprite2D>("Animate");
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

            if (Input.IsActionPressed("ui_up")) input = Vector2.Up;
            else if (Input.IsActionPressed("ui_down")) input = Vector2.Down;
            else if (Input.IsActionPressed("ui_left")) input = Vector2.Left;
            else if (Input.IsActionPressed("ui_right")) input = Vector2.Right;

            if (input != Vector2.Zero)
            {
                if (input != _direction)
                {
                    // Ny retning — snu og reset timer
                    _direction = input;
                    _inputHoldTime = 0f;
                    PlayTurnAnimation();
                }
                else
                {
                    // Samme retning — tell opp
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
