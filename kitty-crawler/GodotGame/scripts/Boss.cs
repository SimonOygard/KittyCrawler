using Godot;
using Interaction;
using KittyCrawler.TELT;
using System;
using System.Threading.Tasks;


public partial class Boss : CharacterBody2D, IInteractable
{
    private AnimatedSprite2D _sprite;

    [Signal]
    public delegate void BossInteractedEventHandler(string bossName);

    [Export] private LevelTransition _levelTransition;
    [Export] public BossData BossData;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _levelTransition =  GetNode<LevelTransition>("LevelTransition");

        if (_sprite != null)
        {
            _sprite.Play("idle");
        }

    }

    public void Interact()
    {
        if (WorldStateManager.Instance != null &&
            !WorldStateManager.Instance.PlayerCanAct)
        {
            return;
        }

        if (BossData == null)
        {
            GD.PushError($"{Name} has no BossData assigned.");
            return;
        }

        WorldDialogueManager.Instance?.OnBossInteracted(BossData);

        GD.Print("Boss has been interacted with " + Name);
    }

    private void TriggerCardbattle()
    {
        if (BossData != null)
        {
            TeltBattleConfig.Instance.CurrentBoss = BossData;
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
