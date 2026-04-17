using Godot;
using Interaction;
using System;

public partial class Chest : CharacterBody2D, IInteractable
{
    private AnimatedSprite2D _sprite;
    private bool _hasBeenInteractedWith = false;
    private bool _isMimic = false;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        if (_sprite != null)
        {
            _sprite.Play("idle");
        }

        _isMimic = true; // new Random().Next(1, 11) == 10; // 10% chance to be a mimic
        GD.Print("Chest " + Name + " is a mimic: " + _isMimic);

    }

    public void Interact()
    {
        if (_hasBeenInteractedWith)
        {
            return;
        }

        _hasBeenInteractedWith = true;

        if (_isMimic)
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
