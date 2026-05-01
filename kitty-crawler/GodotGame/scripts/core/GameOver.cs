using Godot;
using System;

public partial class GameOver : Control
{
    private LineEdit NameInput;
    private Button SubmitButton;

    private WorldStateManager _worldState;

    public override void _Ready()
    {
        NameInput = GetNode<LineEdit>("NameInput");
        SubmitButton = GetNode<Button>("SubmitScore");

        _worldState = GetNode<WorldStateManager>("/root/WorldStateManager");

        SubmitButton.Pressed += OnSubmitPressed;
    }

    private void OnSubmitPressed()
    {
        string playerName = NameInput.Text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            GD.Print("Player name cannot be empty.");
            return;
        }

        _worldState.UserName = playerName;
        _worldState.SaveGame();

        GD.Print($"Submitting score for player: {playerName} with time: {_worldState.TimeSeconds} seconds");

        GetTree().ChangeSceneToFile("res://scenes/MainMenu/MainMenu.tscn");
    }
    private void OnLeaderBoardPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/core/Leaderboard.tscn");
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
