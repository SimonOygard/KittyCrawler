using Godot;
using System;

public partial class GameOver : CanvasLayer
{
    [Signal]
    public delegate void NameInputSubmittedEventHandler(string playerName);
    [Signal]
    public delegate void LeaderboardSubmitRequestedEventHandler();
    [Signal]
    public delegate void LeaderboardRequestedEventHandler();


    private LineEdit NameInput;
    private WorldStateManager _worldState;

    public override void _Ready()
    {
        NameInput = GetNode<LineEdit>("GameOver/MarginContainer/VBoxContainer/NameInput");
    }

    public void OnLeaderBoardPressed()
    {
        EmitSignal(SignalName.LeaderboardRequested);
        GD.Print("Leaderboard button pressed");
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
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
