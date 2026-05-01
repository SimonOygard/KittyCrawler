using Godot;
using Godot.Collections;
using System.Collections.Generic;
using Array = Godot.Collections.Array;

public partial class LeaderboardPanel : PanelContainer
{
    [Export] public PackedScene LeaderboardRowScene;
    [Export] public bool ShowSubmitButton = false;

    [Signal]
    public delegate void MainMenuReturnRequestedEventHandler();

    private VBoxContainer _rowsContainer;
    private Button _submitButton;
    private LeaderboardApi _api;

    public override void _Ready()
    {
        _rowsContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/RowsContainer");
        _submitButton = GetNode<Button>("MarginContainer/VBoxContainer/SubmitButton");

        _api = GetNode<LeaderboardApi>("LeaderboardApi");
        
        // legg til if previous scene main menu IKKE vis submit button, ellers vis den
        _submitButton.Visible = ShowSubmitButton;

        _api.ScoresFetchedRequest += Populate;
        
    }

    private void OnMainMenuPressed()
    {
        GD.Print("Main Menu button pressed");
        GetTree().CallGroup("MainGame", "OnMainMenuReturnPressed");
    }

    private void Populate(Array entries)
    {
        GD.Print("Populating leaderboard with entries...");

        foreach (Node child in _rowsContainer.GetChildren())
            child.QueueFree();

        AddRow("Rank", "Name", "Score", "Time", true);

        int count = Mathf.Min(10, entries.Count);

        for (int i = 0; i < count; i++)
        {
            var dict = entries[i].AsGodotDictionary();

            AddRow(
                (i + 1).ToString(),
                dict["username"].ToString(),
                dict["score"].ToString(),
                FormatTime(dict["timeSeconds"])
            );
        }
    }

    private void AddRow(string rank, string name, string score, string time, bool isHeader = false)
    {
        var row = LeaderboardRowScene.Instantiate<LeaderboardRow>();
        _rowsContainer.AddChild(row);
        row.Setup(rank, name, score, time, isHeader);
    }

    private string FormatTime(Variant value)
    {
        int totalSeconds = value.AsInt32();
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return $"{minutes:00}:{seconds:00}";
    }
}


internal class LeaderboardEntry
{
    public int Rank { get; set; }
    public string PlayerName { get; set; }
    public int Score { get; set; }
    public int TimeSeconds { get; set; }

    public LeaderboardEntry(Dictionary dict, int rank)
    {
        Rank = rank;
        PlayerName = dict.GetValueOrDefault("username", "").AsString();
        Score = dict.GetValueOrDefault("score", 0).AsInt32();
        TimeSeconds = dict.GetValueOrDefault("timeSeconds", 0).AsInt32();
    }
}
