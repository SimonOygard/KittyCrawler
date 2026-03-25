using Godot;
using System;

public partial class LevelTransition : Area2D
{
    [Export] public string ScenePath { get; set; } = string.Empty;
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

            GD.Print("About to load scene: " + ScenePath);

            var battleScene = GD.Load<PackedScene>(ScenePath);
            GD.Print("Scene loaded");

            var instance = battleScene.Instantiate();
            GD.Print("Scene instantiated");

            if (instance is BattleScene battle)
            {
                battle.EnemyName = EnemyName;
                GD.Print("Enemy name assigned");
            }
            GD.Print("About to switch scenes");

            await FadeTransition.Instance.FadeToBlack();

            GetTree().Root.AddChild(instance);
            GetTree().CurrentScene.QueueFree();
            GetTree().CurrentScene = instance;

            await FadeTransition.Instance.FadeFromBlack();

            GD.Print("Scene transition finished");
        }
    }
}
