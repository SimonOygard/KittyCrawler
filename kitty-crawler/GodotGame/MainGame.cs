using Godot;
using System;

public partial class MainGame : Node2D
{
    private MainMenu mainMenu;

    public override void _Ready()
    {
        mainMenu = GetNodeOrNull<MainMenu>("MainMenu");

        mainMenu.StartGameRequested += StartGame;
    }

    public async void StartGame(bool newGame)
    {
        mainMenu?.Hide();

        if (newGame)
        {
            // Start a new game
            GD.Print("Starting a new game...");
        }
        else
        {
            // Load an existing game
            GD.Print("Loading an existing game...");
        }

    }
}
