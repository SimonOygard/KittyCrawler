using DialogueManagerRuntime;
using Godot;
using System;

public partial class MainGame : Node2D
{
    private MainMenu mainMenu;
    private LevelTransition _levelTransition;
    private GameTimerManager _gameTimer;
    private PauseMenu pauseMenu;
    private GameOver endMenu;
    private const string LeaderboardScenePath = "res://scenes/MainMenu/Leaderboard/Leaderboard.tscn";
    private const string GameScenePath = "res://Dungeon_floor_2/scenes/Levels/skesters_clubs.tscn";

    public override void _Ready()
    {
        AddToGroup("MainGame");
        ProcessMode = ProcessModeEnum.Always;

        mainMenu = GetNodeOrNull<MainMenu>("UI/MainMenu");
        endMenu = GetNodeOrNull<GameOver>("UI/GameOver");
        _levelTransition = GetNode<LevelTransition>("UI/LevelTransition");
        pauseMenu = GetNodeOrNull<PauseMenu>("UI/PauseMenu");

        _gameTimer = GetNode<GameTimerManager>("/root/GameTimerManager");

        mainMenu.StartGameRequested += StartGame;
        mainMenu.LeaderboardRequested += Leaderboard;
        mainMenu.Show();

        pauseMenu.Hide();
        pauseMenu.ResumeGameRequested += OnPausedContinueGame;

        endMenu.Hide();
        endMenu.LeaderboardRequested += Leaderboard;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("escape"))
        {
            OnPausePressed();
        }
    }

    // --- Start and Load game -------
    public void StartGame(bool newGame)
    {
        GD.Print($"[MainGame] StartGame called | newGame = {newGame}");

        mainMenu?.Hide();

        if (newGame)
        {
            _levelTransition.ScenePath = GameScenePath;
            _levelTransition.TriggerTransition();

            GD.Print("Starting a new game...");
            _gameTimer.StartTimer();
        }
        else
        {
            // Load an existing game
            GD.Print("Loading an existing game...");
            _gameTimer.ContinueTimer();
        }

    }
    // --- Leaderboard -------
    #region leaderboard
    public void Leaderboard()
    {
        GD.Print("Leaderboard button pressed");
        mainMenu?.Hide();

        _levelTransition.ScenePath = LeaderboardScenePath;
        _levelTransition.TriggerTransition();
    }

    public void OnMainMenuReturnPressed()
    {
        GD.Print("Main Menu button pressed");

        GetTree().Paused = false;

        var ui = GetNode("UI");
        var old = ui.GetNodeOrNull("CurrentLevel");

        if (old != null)
        {
            old.QueueFree();
        }
        _levelTransition.ScenePath = null;
        mainMenu?.Show();

    }
    #endregion 

    // --- PauseMenu -------
    #region pause menu
    public void OnPausedContinueGame()
    {
        GD.Print("Continue button pressed");

        GetTree().Paused = false;
        pauseMenu?.Hide();
        _gameTimer.ContinueTimer();
    }
    private void OnPausePressed()
    {
        if (mainMenu?.Visible ?? false)
        {
            GD.Print("Pause pressed, but main menu is visible. Ignoring pause action.");
            return;
        }

        GetTree().Paused = !GetTree().Paused;

        if (pauseMenu is not null)
        {
            GD.Print($"Pause state changed: {GetTree().Paused}. Updating pause menu visibility.");
            pauseMenu.Visible = GetTree().Paused;
        }

        if (GetTree().Paused)
        {
            GD.Print("Game paused. Pausing timer.");
            _gameTimer.PauseTimer();
        }
        else
        {
            GD.Print("Game resumed. Continuing timer.");
            _gameTimer.ContinueTimer();
        }
    }
    #endregion

    // --- Game Over -------
    
    
}
