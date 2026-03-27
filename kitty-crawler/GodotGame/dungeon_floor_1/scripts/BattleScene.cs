using Godot;
using System;

public partial class BattleScene : Node2D
{
    public string EnemyName;
    public string ReturnScenePath;

    public override void _Ready()
    {
        GD.Print($"Entering battle with {EnemyName}!");
        GD.Print("Return scene: " + ReturnScenePath);
    }

    private async void EndBattle()
    {
        if (!string.IsNullOrEmpty(ReturnScenePath))
        {
            GD.Print("Battle ended. Returning to: " + ReturnScenePath);
            await FadeTransition.Instance.FadeToBlack();
            GetTree().ChangeSceneToFile(ReturnScenePath);
            await FadeTransition.Instance.FadeFromBlack();
            return;
        }
        else
        {
            GD.PrintErr("ReturnScenePath is empty. Cannot return to previous scene.");
        }
    }

    public override void _Process(double delta) // kan endres senere
    {
        if (Input.IsActionJustPressed("down"))
        {
            GD.Print("Down pressed in battle");
            EndBattle();
        }
    }
}
