using Godot;
using System;

public partial class PauseMenu : CanvasLayer
{
    [Signal]
    public delegate void PausePressedEventHandler();

    [Signal]
    public delegate void ResumeGameRequestedEventHandler();

    [Signal]
    public delegate void SaveGameRequestedEventHandler();

    public override void _Ready()
	{
	}

    public void OnPausedContinuePressed()
    {
        EmitSignal(SignalName.ResumeGameRequested);
        GD.Print("Continue game button pressed");
    }


    public void OnSavePressed()
    {
        EmitSignal(SignalName.SaveGameRequested);
        GD.Print("Save game button pressed");
    }

    public void OnQuitPressed()
    {
        GetTree().Quit();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("escape"))
        {
            GD.Print("Escape key pressed");
            EmitSignal(SignalName.PausePressed);
        }
    }
}
