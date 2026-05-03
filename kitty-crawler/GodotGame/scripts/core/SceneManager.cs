using Godot;
using KittyCrawler.TELT;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Threading.Tasks;

public partial class SceneManager : Node
{
    [Signal]
    public delegate void LevelLoadedEventHandler(string scenePath);

    [Signal]
    public delegate void GameOverRequestedEventHandler();

    public static SceneManager Instance { get; private set; }
    public string CurrentLevelPath { get; private set; } = "";
    public string PreviousLevelPath { get; private set; } = "";
    public string CurrentBossName { get; private set; } = "";

    private bool _isChangingScene = false;

    public override void _Ready()
	{
        Instance = this;
	}
    public void GameOver()
    {
        EmitSignal(SignalName.GameOverRequested);
    }

    public async Task ChangeSceneAsync(string scenePath, TransitionType type = TransitionType.Generic, string objectName = "")
    {
        if (_isChangingScene)
        {
            GD.PrintErr("Scene change already in progress. Ignoring request to change to: " + scenePath);
            return;
        }

        _isChangingScene = true;

        try
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                GD.PrintErr("Scene path is null or empty. Cannot change scene.");
                return;
            }

            var newScene = GD.Load<PackedScene>(scenePath);

            if (newScene == null)
            {
                GD.PrintErr("Failed to load scene at path: " + scenePath);
                return;
            }

            var instance = newScene.Instantiate();
            instance.Name = "CurrentLevel";
            GD.Print("Scene instantiated: " + scenePath);

            SetupScene(instance, scenePath, type, objectName);
            await FadeTransition.Instance.FadeToBlack();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            ReplaceCurrentScene(instance, scenePath);

            await FadeTransition.Instance.FadeFromBlack();
            EmitSignal(SignalName.LevelLoaded, scenePath);
            GD.Print("Scene transition finished: " + scenePath);
        }
        finally
        {
            _isChangingScene = false;
        }
    }
   
    public void UnloadCurrentLevel()
    {
        var old = GetTree().CurrentScene;
        if (old != null)
        {
            old.QueueFree();
        }
        CurrentLevelPath = "";
    }

    public async Task LoadPreviousLevel()
    {
        if (string.IsNullOrEmpty(PreviousLevelPath))
        {
            GD.PrintErr("No previous level to load.");
            return;
        }
        PreviousLevelPath = CurrentLevelPath;
        await ChangeSceneAsync(PreviousLevelPath);
    }

    private void ReplaceCurrentScene(Node newScene, string scenePath)
    {
        var currentScene = GetTree().CurrentScene;
        
        if (currentScene != null && currentScene.Name == "CurrentLevel")
        {
            PreviousLevelPath = currentScene.SceneFilePath;
            currentScene.QueueFree();
            GD.Print("Current scene freed");
        }

        GetTree().Root.AddChild(newScene);
        GetTree().CurrentScene = newScene;

        CurrentLevelPath = scenePath;

        GD.Print("New scene set as current");
    }

    private void SetupScene(Node instance, string scenePath, TransitionType type, string objectName)
    {
        switch (type)
        {
            case TransitionType.Battle:
                if (instance is BattleScene battle)
                {
                    battle.EnemyName = objectName;
                    battle.ReturnScenePath = GetTree().CurrentScene?.SceneFilePath ?? "";
                    GD.Print("Enemy name assigned");
                }
                else
                {
                    GD.PrintErr("Transition type is Battle, but scene is not a BattleScene.");
                }
                break;

            case TransitionType.Puzzle:
                break;

            case TransitionType.Trap:
                break;

            case TransitionType.Dialogue:
                break;

            case TransitionType.Teleport:
                break;

            case TransitionType.Generic:
                break;
        }
    }
}
