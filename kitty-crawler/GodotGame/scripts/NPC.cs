using Godot;
using Interaction;
using System;


public partial class NPC : CharacterBody2D, IInteractable
{
    private AnimatedSprite2D _sprite;
    private bool _hasBeenInteractedWith = false;

    [Export] private LevelTransition _levelTransition;
    [Export] private KittyCrawler.TELT.NPCData _npcData;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _levelTransition =  GetNode<LevelTransition>("LevelTransition");

        if (_sprite != null)
        {
            _sprite.Play("skester_idle");
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
        GD.Print("NPC has been interacted with " + Name);

    }

    private void TriggerCardbattle()
    {
        GD.Print($"TriggerCardbattle kalt, npcData={_npcData?.NPCName ?? "null"}");

        if (_npcData != null)
        {
            TeltBattleConfig.Instance.CurrentNPC = _npcData;
            GD.Print("Telt battle triggered for NPC: " + GetTree().CurrentScene.SceneFilePath);
            TeltBattleConfig.Instance.ReturnScenePath = GetTree().CurrentScene.SceneFilePath;
        }
        else
            GD.PrintErr("NPCData ikke satt på NPC: " + Name);

        GD.Print($"LevelTransition er null: {_levelTransition == null}");
        GD.Print($"ScenePath: {_levelTransition?.ScenePath}");

        if (_levelTransition != null)
            _levelTransition.TriggerTransition();
        else
            GD.PrintErr("LevelTransition node is not assigned for NPC: " + Name);
    }
}
