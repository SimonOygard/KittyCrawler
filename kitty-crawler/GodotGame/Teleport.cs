using Godot;
using Interaction;
using System;

public partial class Teleport : CharacterBody2D, IInteractable
{
    private AnimatedSprite2D _sprite;
    private bool _hasBeenInteractedWith = false;

    [Export] private LevelTransition _levelTransition;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _levelTransition = GetNode<LevelTransition>("LevelTransition");

        if (_sprite != null)
        {
            _sprite.Play("default");
        }

    }

    public void Interact()
    {
        if (_hasBeenInteractedWith)
        {
            return;
        }

        _hasBeenInteractedWith = true;

        TriggerTeleport();
        GD.Print("Teleport has been interacted with " + Name);

    }

    private void TriggerTeleport()
    {
        if (_sprite != null)
        {
            // change scene
            GD.Print("Teleport triggered. Changing position");
        }

        if (_levelTransition != null)
        {
            _levelTransition.TriggerTransition();
        }
        else
        {
            GD.PrintErr("LevelTransition node is not assigned for Teleport: " + Name);
        }
    }
}
