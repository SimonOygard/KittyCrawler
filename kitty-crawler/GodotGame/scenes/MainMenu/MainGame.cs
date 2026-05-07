using DialogueManagerRuntime;
using Godot;
using KittyCrawler.TELT;
using System;
using Game.Core;

public partial class MainGame : Node2D
{
    private MainMenu mainMenu;
    private LevelTransition _levelTransition;
    private GameTimerManager _gameTimer;
    private PauseMenu pauseMenu;
    private GameOver endMenu;
    private const string LeaderboardScenePath = "res://scenes/MainMenu/Leaderboard/Leaderboard.tscn";
    private const string GameScenePath = "res://Dungeon_floor_2/scenes/Levels/skesters_clubs.tscn";
    WorldStateManager _worldStateManager;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        mainMenu = GetNodeOrNull<MainMenu>("UI/MainMenu");
        endMenu = GetNodeOrNull<GameOver>("UI/GameOver");
        _levelTransition = GetNode<LevelTransition>("UI/LevelTransition");
        pauseMenu = GetNodeOrNull<PauseMenu>("UI/PauseMenu");

        _gameTimer = GetNode<GameTimerManager>("/root/GameTimerManager");
        _worldStateManager = GetNode<WorldStateManager>("/root/WorldStateManager");

        mainMenu.StartGameRequested += StartGame;
        mainMenu.LeaderboardRequested += Leaderboard;
        mainMenu.Show();

        pauseMenu.Hide();
        pauseMenu.ResumeGameRequested += OnPausedContinueGame;

        endMenu.Hide();
        endMenu.LeaderboardRequested += Leaderboard;

        SceneManager.Instance.GameOverRequested += GameOver;
        SceneManager.Instance.MainMenuLoaded += OnMainMenuReturnPressed;
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
            _worldStateManager.GameEnded = false;

            _worldStateManager.WorldStateReset();

            Globals.InitializeStartingCards();

            _levelTransition.ScenePath = GameScenePath;
            _levelTransition.TriggerTransition();
            // PlayerData.SaveDeck([]);

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
    // --- Game Over -------
    public void GameOver()
    {
        GD.Print("GameOver called. Unloading current level and showing end menu.");

        AudioManager.Instance.PlayEndGameTheme();
        SceneManager.Instance.UnloadCurrentLevel();

        endMenu?.Show();
        endMenu.UpdateScoreLabel();
        endMenu.UpdateTimerLabel();
        GD.Print("End menu shown.");
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
        GD.Print("Reloading main menu");
        endMenu?.Hide();
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

}
