using DialogueManagerRuntime;
using Godot;
using Godot.Collections;
using KittyCrawler.TELT;
using System;

public partial class WorldDialogueManager : Node
{
    [Signal]
    public delegate void DialogueTriggerCardBattleRequestedEventHandler(BossData bossData);

    [Signal]
    public delegate void DialogueEndedEventHandler();

    private WorldStateManager? _worldStateManager;

    public static WorldDialogueManager? Instance { get; private set; }

    private Dictionary<string, string> _bossDialoguePaths = new();

    private Array<string> _bossesInteractedWith= [];

    private BossData _currentBossData;

    public override void _Ready()
    {
        Instance = this;

        if (SceneManager.Instance != null)
        {
            DialogueTriggerCardBattleRequested += SceneManager.Instance.OnDialogueBattleRequested;
        }
        else
        {
            GD.PushWarning("SceneManager.Instance was null when WorldDialogueManager was ready.");
        }
    }

    public void OnBossInteracted(BossData bossData)
    {
        if (bossData == null)
        {
            GD.PushError("BossData was null.");
            return;
        }
        if (bossData.DialogueResource == null)
        {
            GD.PushError($"BossData for {bossData.BossName} has no DialogueResource assigned.");
            return;
        }
        _currentBossData = bossData;

        string dialogueTitle = "start";

        if (WorldStateManager.Instance != null && WorldStateManager.Instance.BossesWon.Contains(bossData.NpcId))
        {
            dialogueTitle = "PostBattle";
        }
        else if (_bossesInteractedWith.Contains(bossData.NpcId))
        {
            dialogueTitle = "BackForMore";
        }

        WorldStateManager.Instance?.SetMode(WorldStateManager.GameMode.Dialogue);

        DialogueManager.ShowDialogueBalloon (bossData.DialogueResource, dialogueTitle);
    }
    public void StartBossBattle()
    {
        if (_currentBossData == null)
        {
            GD.PushError("Tried to start boss battle, but no current BossData was set.");
            return;
        }

        //hMmmMMmm
        WorldStateManager.Instance?.SetMode(WorldStateManager.GameMode.Gameplay);

        EmitSignal(SignalName.DialogueTriggerCardBattleRequested, _currentBossData);
    }
    public void EndDialogue()
    {
        GD.Print("EndDialogue called");
        WorldStateManager.Instance?.SetMode(WorldStateManager.GameMode.Gameplay);
        EmitSignal(SignalName.DialogueEnded);
        GD.Print($"Mode is now: {WorldStateManager.Instance?.Mode}");
    }

    public void MarkBossInteracted()
    {
        if (_currentBossData == null) return;

        if (!_bossesInteractedWith.Contains(_currentBossData.NpcId))
        {
            _bossesInteractedWith.Add(_currentBossData.NpcId);
        }
    }

    public void ShowBossPostBattleDialogue(BossData bossData)
    {
        if (bossData == null || bossData.DialogueResource == null)
        {
            GD.PushError("Cannot show post battle dialogue. BossData or DialogueResource is null.");
            return;
        }

        _currentBossData = bossData;

        WorldStateManager.Instance?.SetMode(WorldStateManager.GameMode.Dialogue);

        DialogueManager.ShowDialogueBalloon(bossData.DialogueResource, "PostBattle");
    }

    // blir implementert ved senere anledning
    public void OnNPCInteracted()
    {
        // uses a dictionary nPCName (key) : ScriptName (value) to start correct dialogue
        // dialogue end stuff
    }




}
