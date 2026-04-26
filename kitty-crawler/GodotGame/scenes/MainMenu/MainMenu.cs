using Godot;
using System;

public partial class MainMenu : Control
{
    [Signal]
    public delegate void StartGameRequestedEventHandler(bool newGame);

    private Button[] _buttons;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        var margin = GetNode<MarginContainer>("MarginContainer");
        margin.AddThemeConstantOverride("margin_left", 100);
        margin.AddThemeConstantOverride("margin_top", 260);

        var menu = GetNode<VBoxContainer>("MarginContainer/VBoxContainer");
        menu.AddThemeConstantOverride("separation", 14);


        _buttons = new[]
        {
            GetNode<Button>("MarginContainer/VBoxContainer/Continue"),
            GetNode<Button>("MarginContainer/VBoxContainer/NewGame"),
            GetNode<Button>("MarginContainer/VBoxContainer/LoadGame"),
            GetNode<Button>("MarginContainer/VBoxContainer/Leaderboard"),
            GetNode<Button>("MarginContainer/VBoxContainer/Quit")
        };

        foreach (var button in _buttons)
        {
            SetupButton(button);
        }


    }

	public void OnNewGamePressed()
    {
        EmitSignal(SignalName.StartGameRequested, true);
        GD.Print("New Game button pressed");
    }

    public void OnLoadPressed()
    {
        EmitSignal(SignalName.StartGameRequested, true);
        GD.Print("Load game button pressed");
    }

    public void OnContinuePressed()
    {
        EmitSignal(SignalName.StartGameRequested, true);
        GD.Print("Continue game button pressed");
    }

    public void OnLeaderboardPressed()
    {
        GD.Print("Leaderboard button pressed");
    }

    public void OnQuitPressed()
    {
        GetTree().Quit();
    }

    private void SetupButton(Button button)
    {
        button.Flat = true; // Set the button to flat style for better aesthetics
        button.FocusMode = FocusModeEnum.All; // Allow the button to receive focus
        button.Alignment = HorizontalAlignment.Left;

        button.AddThemeFontSizeOverride("font_size", 36);

        button.AddThemeColorOverride("font_color", Colors.White);
        button.AddThemeColorOverride("font_color_hover", Colors.WhiteSmoke);
        button.AddThemeColorOverride("font_focus_color", Colors.LightGray);
        button.AddThemeColorOverride("font_pressed_color", Colors.White);

        var transparent = new StyleBoxEmpty();
        button.AddThemeStyleboxOverride("normal", transparent);
        button.AddThemeStyleboxOverride("hover", transparent);
        button.AddThemeStyleboxOverride("focus", transparent);
        button.AddThemeStyleboxOverride("pressed", transparent);
        button.AddThemeStyleboxOverride("disabled", transparent);

        button.AddThemeConstantOverride("outline_size", 0); // Remove default outline

        button.MouseEntered += () => SetTextGlow(button, true);
        button.MouseExited += () =>
        {
            if (!button.HasFocus())
                SetTextGlow(button, false);
        };

        button.FocusEntered += () => SetTextGlow(button, true);
        button.FocusExited += () => SetTextGlow(button, false);
    }

    private void SetTextGlow(Button button, bool enabled)
    {
        button.AddThemeConstantOverride("outline_size", enabled ? 8 : 0);
        button.AddThemeColorOverride("font_outline_color",
            enabled ? new Color(1, 1, 1, 0.85f) : Colors.Transparent);
    }
}

