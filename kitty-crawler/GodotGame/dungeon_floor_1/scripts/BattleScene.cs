using Godot;
using System;

public partial class BattleScene : Node2D
{
    public string EnemyName;
    public string ReturnScenePath;
    private Label _diceLabel;

    public override void _Ready()
    {
        GD.Print($"Entering battle with {EnemyName}!");
        GD.Print("Return scene: " + ReturnScenePath);

        int result = DiceRoll.Instance.RollDice(20);
        GD.Print($"Player rolled a {result} on a d20.");
        _diceLabel = GetNode<Label>("DiceResultLabel");
        UpdateLabelText($"Player rolled a {result} on a d20.");
    }

    public void UpdateLabelText(string text)
    {
        if (_diceLabel != null)
        {
            _diceLabel.Text = text;
        }
        else
        {
            GD.PrintErr("DiceResultLabel not found. Cannot update label text.");
        }
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
