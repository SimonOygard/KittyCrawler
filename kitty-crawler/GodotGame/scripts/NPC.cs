using Godot;
using Interaction;
using DialogueManagerRuntime;
using KittyCrawler.TELT;

public partial class NPC : CharacterBody2D, IInteractable
{
    private AnimatedSprite2D _sprite;
    private bool _hasBeenInteractedWith = false;
    private bool _wantsBattle = false;

    [Export] private LevelTransition _levelTransition;
    [Export] private NPCData _npcData;
    [Export] private Resource _dialogueResource;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _levelTransition = GetNode<LevelTransition>("LevelTransition");

        if (_sprite != null)
            _sprite.Play("skester_idle");

        DialogueManager.DialogueEnded += OnDialogueEnded;

        // Start PostBattle dialog automatisk hvis NPC er beseiret og kort er mottatt
        if (_npcData != null
            && PlayerData.HasDefeatedNpc(_npcData.NpcId)
            && PlayerData.HasReceivedCard(_npcData.NpcId))
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
        bool hasDefeated = PlayerData.HasDefeatedNpc(_npcData.NpcId);
        bool hasCard = PlayerData.HasReceivedCard(_npcData.NpcId);

        GD.Print($"[NPC] Interact: hasDefeated={hasDefeated}, hasCard={hasCard}");

        if (hasDefeated && hasCard)
        {
            // Spill PostBattle dialog
            var states = new Godot.Collections.Array<Variant>();
            states.Add(Variant.From(this));
            DialogueManager.ShowDialogueBalloon(_dialogueResource, "PostBattle", states);
            return;
        }

        if (!hasDefeated && _dialogueResource != null)
        {
            var states = new Godot.Collections.Array<Variant>();
            states.Add(Variant.From(this));
            DialogueManager.ShowDialogueBalloon(_dialogueResource, "start", states);
        }
        else if (!hasDefeated)
        {
            TriggerCardbattle();
        }
    }

    // Kalles av do start_battle() i dialogen
    public void StartBattle()
    {
        GD.Print("[NPC] StartBattle kalt!");
        _wantsBattle = true;
    }

    private void OnDialogueEnded(Resource resource)
    {
        GD.Print($"[NPC] DialogueEnded: resource={resource?.ResourcePath}, dialogueResource={_dialogueResource?.ResourcePath}, wantsBattle={_wantsBattle}");
        WorldStateManager.Instance.SetMode(WorldStateManager.GameMode.Gameplay);
        if (resource != _dialogueResource) return;

        if (_wantsBattle)
        {
            _hasBeenInteractedWith = true;
            TriggerCardbattle();
        }
    }

    private void TriggerCardbattle()
    {
        if (_npcData != null)
        {
            TeltBattleConfig.Instance.CurrentNPC = _npcData;
            TeltBattleConfig.Instance.ReturnScenePath = GetTree().CurrentScene.SceneFilePath;
        }
        else
            GD.PrintErr("NPCData ikke satt på NPC: " + Name);

        if (_levelTransition != null)
            _levelTransition.TriggerTransition();
        else
            GD.PrintErr("LevelTransition ikke assignet for NPC: " + Name);
    }
}
