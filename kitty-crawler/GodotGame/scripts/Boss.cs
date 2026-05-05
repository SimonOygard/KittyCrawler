using Godot;
using Interaction;
using DialogueManagerRuntime;
using KittyCrawler.TELT;

public partial class Boss : CharacterBody2D, IInteractable
{
    private AnimatedSprite2D _sprite;

    [Export] private LevelTransition _levelTransition;
    [Export] private BossData _bossData;
    [Export] private Resource _dialogueResource;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _levelTransition = GetNode<LevelTransition>("LevelTransition");

        if (_sprite != null && _sprite.Name == "Skester")
            _sprite.Play("skester_idle");
        else
            _sprite.Play("default");

        if (_bossData != null
            && PlayerData.HasDefeatedNpc(_bossData.NpcId)
            && TeltBattleConfig.Instance.JustReturnedFromBattle)
        {
            TeltBattleConfig.Instance.JustReturnedFromBattle = false;
            CallDeferred(nameof(StartPostBattleDialogue));
        }
    }

    private void StartPostBattleDialogue()
    {
        var states = new Godot.Collections.Array<Variant>();
        states.Add(Variant.From(this));
        DialogueManager.ShowDialogueBalloon(_dialogueResource, "PostBattle", states);
    }

    public void Interact()
    {
        if (_bossData == null) return;

        bool hasDefeated = PlayerData.HasDefeatedNpc(_bossData.NpcId);

        if (hasDefeated)
        {
            var states = new Godot.Collections.Array<Variant>();
            states.Add(Variant.From(this));
            DialogueManager.ShowDialogueBalloon(_dialogueResource, "PostBattle", states);
            return;
        }

        if (_dialogueResource != null)
        {
            var states = new Godot.Collections.Array<Variant>();
            states.Add(Variant.From(this));
            DialogueManager.ShowDialogueBalloon(_dialogueResource, "start", states);
        }
        else
        {
            TriggerBossCardbattle(); // ← rettet navn
        }
    }

    private void TriggerBossCardbattle()
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
            GD.PrintErr("LevelTransition ikke assignet for Boss: " + Name);
    }
}
