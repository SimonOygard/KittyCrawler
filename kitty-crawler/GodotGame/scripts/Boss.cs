using Godot;
using Interaction;
using System;


public partial class Boss : CharacterBody2D, IInteractable
{
    private AnimatedSprite2D _sprite;
    private bool _hasBeenInteractedWith = false;

    [Export] private LevelTransition _levelTransition;
    [Export] private KittyCrawler.TELT.BossData _bossData;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _levelTransition =  GetNode<LevelTransition>("LevelTransition");

        if (_sprite != null && _sprite.Name == "Skester")
        {
            _sprite.Play("skester_idle");
        }
        else
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

        TriggerCardbattle();
        GD.Print("Boss has been interacted with " + Name);

    }

    private void TriggerCardbattle()
    {
        if (_bossData != null)
        {
            TeltBattleConfig.Instance.CurrentBoss = _bossData;
            GD.Print("Telt battle triggered for boss: " + GetTree().CurrentScene.SceneFilePath);
            TeltBattleConfig.Instance.ReturnScenePath = GetTree().CurrentScene.SceneFilePath;
        }
        else
            GD.PrintErr("BossData ikke satt på Boss: " + Name);

        if (_levelTransition != null)
            _levelTransition.TriggerTransition();
        else
            GD.PrintErr("LevelTransition node is not assigned for Boss: " + Name);
    }
}
