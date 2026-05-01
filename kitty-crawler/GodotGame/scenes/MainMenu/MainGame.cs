using DialogueManagerRuntime;
using Godot;
using System;

public partial class MainGame : Node2D
{
    private MainMenu mainMenu;
    private LevelTransition _levelTransition;
    private GameTimerManager _gameTimer;
    private PauseMenu pauseMenu;


    public override void _Ready()
    {
        AddToGroup("MainGame");
        ProcessMode = ProcessModeEnum.Always;

        mainMenu = GetNodeOrNull<MainMenu>("UI/MainMenu");
        _levelTransition = GetNode<LevelTransition>("UI/LevelTransition");
        pauseMenu = GetNodeOrNull<PauseMenu>("UI/PauseMenu");

        _gameTimer = GetNode<GameTimerManager>("/root/GameTimerManager");

        mainMenu.StartGameRequested += StartGame;
        mainMenu.LeaderboardRequested += Leaderboard;
        mainMenu.Show();

        pauseMenu.Hide();
        pauseMenu.ResumeGameRequested += OnPausedContinueGame;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("escape"))
        {
            OnPausePressed();
        }
    }

    public async void StartGame(bool newGame)
    {
        GD.Print($"[MainGame] StartGame called | newGame = {newGame}");

        mainMenu?.Hide();

        if (newGame)
        {
            if (!string.IsNullOrEmpty(_levelTransition.ScenePath))
            {
                _levelTransition.TriggerTransition();
                GD.Print("Starting a new game...");
                _gameTimer.StartTimer();
                return;
            }
        }
        else
        {
            // Load an existing game
            GD.Print("Loading an existing game...");
            _gameTimer.ContinueTimer();
        }

    }

    public void OnPausedContinueGame()
    {
        GD.Print("Continue button pressed");

        GetTree().Paused = false;
        pauseMenu?.Hide();
        _gameTimer.ContinueTimer();
    }

    public async void Leaderboard()
    {
        GD.Print("Leaderboard button pressed");
        mainMenu?.Hide();

        _levelTransition.ScenePath = "res://scenes/MainMenu/Leaderboard/Leaderboard.tscn";
        _levelTransition.TriggerTransition();
    }

    public async void OnMainMenuReturnPressed()
    {
        GD.Print("Main Menu button pressed 2");
        var container = GetNode("UI");
        var old = container.GetNodeOrNull("CurrentLevel");
        if (old != null)
        {
            old.QueueFree();
        }
        mainMenu.Show();
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
}
