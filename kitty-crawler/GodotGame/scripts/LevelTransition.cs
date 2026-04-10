using Godot;
using System;

public partial class LevelTransition : Area2D
{
    [Export] public string ScenePath { get; set; } = string.Empty;

    [Export] public TransitionType Type { get; set; } = TransitionType.Battle;

    // Battlespecific transition
    [Export] public string EnemyName { get; set; } = "Skeleton";

    private bool _triggered = false;

    private async void _on_body_entered(Node body)
    {
        GD.Print("Collision detected with: " + body.Name);

        if (_triggered) return;

        if (body.IsInGroup("player"))
        {
            GD.Print("Player detected!");
            _triggered = true;

            var newScene = GD.Load<PackedScene>(ScenePath);
            GD.Print("Scene loaded");

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
    }

    private void SetScene(Node instance)
    {
        GD.Print("About to load scene: " + ScenePath);
        switch (Type)
        {
            case TransitionType.Battle:
                if (instance is BattleScene battle)
                {
                    battle.EnemyName = EnemyName;
                    battle.ReturnScenePath = GetTree().CurrentScene.SceneFilePath;
                    GD.Print("Enemy name assigned");
                }
                else
                {
                    GD.Print("Transition type is Battle but scene is not a BattleScene");
                }
                break;

            case TransitionType.Teleport:
                // Teleport-specific logic can be added here
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

public enum TransitionType
{
    Battle,
    Teleport,
    Puzzle,
    Trap,
    Dialogue,
    Door,
    Generic
}
