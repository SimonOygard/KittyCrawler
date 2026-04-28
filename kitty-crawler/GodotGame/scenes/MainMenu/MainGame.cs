using Godot;
using System;

public partial class MainGame : Node2D
{
    private MainMenu mainMenu;

    private LevelTransition _levelTransition;

    public override void _Ready()
    {
        mainMenu = GetNodeOrNull<MainMenu>("UI/MainMenu");
        _levelTransition = GetNode<LevelTransition>("UI/LevelTransition");

        mainMenu.StartGameRequested += StartGame;
    }

    public async void StartGame(bool newGame)
    {
        mainMenu?.Hide();

        if (newGame)
        {
            if (!string.IsNullOrEmpty(_levelTransition.ScenePath))
            {
                _levelTransition.TriggerTransition();
                GD.Print("Starting a new game...");
                return;
            }
        }
        else
        {
            // Load an existing game
            GD.Print("Loading an existing game...");
        }

    }
}
