using Godot;
using System;

public partial class LevelTransition : Node
{
    [Export] public string ScenePath { get; set; } = string.Empty;
    [Export] public TransitionType Type { get; set; } = TransitionType.Battle;
    [Export] public string ObjectName { get; set; } = "Skeleton";

    private bool _triggered = false;

     public async void TriggerTransition()
    {
        if (_triggered) return;

        _triggered = true;

        if (string.IsNullOrEmpty(ScenePath))
        {
            GD.PrintErr("ScenePath is not set for LevelTransition: " + Name);
            return;
        }


        var newScene = GD.Load<PackedScene>(ScenePath);
        if (newScene == null)
        {
            GD.PrintErr("Failed to load scene at path: " + ScenePath);
            return;
        }

        var instance = newScene.Instantiate();
        GD.Print("Scene instantiated");

        SetScene(instance);

        await FadeTransition.Instance.FadeToBlack();

        GetTree().Root.AddChild(instance);
        GetTree().CurrentScene.QueueFree();
        GetTree().CurrentScene = instance;

        await FadeTransition.Instance.FadeFromBlack();
        GD.Print("Scene transition finished");
    }
    

    // Method for scenechange based on type, allowing for specific logic to be executed based on the transition type
    private void SetScene(Node instance)
    {
        GD.Print("About to load scene: " + ScenePath);
        switch (Type)
        {
            case TransitionType.Battle:
                if (instance is BattleScene battle)
                {
                    battle.EnemyName = ObjectName;
                    battle.ReturnScenePath = GetTree().CurrentScene.SceneFilePath;
                    GD.Print("Enemy name assigned");
                }
                else
                {
                    GD.Print("Transition type is Battle but scene is not a BattleScene");
                }
                break;

            case TransitionType.Puzzle:
                // Puzzle-specific logic can be added here
                break;
            case TransitionType.Trap:
                // Trap-specific logic can be added here
                break;
            case TransitionType.Dialogue:
                // Dialogue-specific logic can be added here
                break;
            case TransitionType.Door:
                // Door-specific logic can be added here
                break;
            case TransitionType.Generic:
                // Generic transition logic can be added here
                break;
        }
    }
}

// Enum to define different types of transitions, allowing for specific logic based on the transition type
public enum TransitionType
{
    Battle,
    Puzzle,
    Trap,
    Dialogue,
    Door,
    Generic
}
