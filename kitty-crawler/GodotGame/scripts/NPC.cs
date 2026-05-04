using Godot;
using Interaction;
using DialogueManagerRuntime;

public partial class NPC : CharacterBody2D, IInteractable
{
    private AnimatedSprite2D _sprite;
    private bool _hasBeenInteractedWith = false;
    private bool _wantsBattle = false;

    [Export] private LevelTransition _levelTransition;
    [Export] private KittyCrawler.TELT.NPCData _npcData;
    [Export] private Resource _dialogueResource;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _levelTransition = GetNode<LevelTransition>("LevelTransition");

        if (_sprite != null)
            _sprite.Play("skester_idle");

        DialogueManager.DialogueEnded += OnDialogueEnded;
    }

    public void Interact()
    {
        if (_hasBeenInteractedWith) return;

        if (_dialogueResource != null)
        {
            var states = new Godot.Collections.Array<Variant>();
            states.Add(Variant.From(this));
            DialogueManager.ShowDialogueBalloon(_dialogueResource, "start", states);
        }
        else
        {
            _hasBeenInteractedWith = true;
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
