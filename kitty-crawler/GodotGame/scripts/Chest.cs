using Godot;
using Interaction;
using System;

namespace ChestInteractions;

public partial class Chest : CharacterBody2D, IInteractable
{
    private AnimatedSprite2D _sprite;
    private bool _hasBeenInteractedWith = false;
    public bool IsMimic { get; set; } = false;

    [Export]
    private LevelTransition _levelTransition;
    
    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _levelTransition = GetNode<LevelTransition>("LevelTransition");


        if (_sprite != null)
        {
            _sprite.Play("idle");
        }

    }

    public void Interact()
    {
        if (_hasBeenInteractedWith)
        {
            return;
        }

        _hasBeenInteractedWith = true;

        if (IsMimic)
        {
            TriggerMimic();
            GD.Print("Mimic has been interacted with " + Name);
        }
        else
        {
            OpenChest();
            GD.Print("Chest has been interacted with " + Name);
        }
    }


    private void TriggerMimic()
    {
        GD.Print("Mimic triggered! " + Name);

        if (_sprite != null && _sprite.SpriteFrames.HasAnimation("mimic"))
        {
            _sprite.Play("mimic");
            GD.Print("Mimic animation played for " + Name);
        }

    }

    private void OpenChest()
    {
        if (_sprite != null && _sprite.SpriteFrames.HasAnimation("open"))
        {
            _sprite.Play("open");
            GD.Print("Chest opened animation played for " + Name);
        }
    }

}
