using Godot;
using System;
using System.Diagnostics;

public partial class AudioManager : Node
{
    // Background music
    private static readonly AudioStream MainMenu = GD.Load<AudioStream>("res://assets/Audio/Music/wav/02-TitleTheme.wav");

    // Step sounds
    private TileMapLayer _currentFloorLayer;
    private static readonly AudioStream FootStep = GD.Load<AudioStream>("res://assets/Audio/SoundFX/08_Step_rock_02.wav");
    private static readonly AudioStream[] Footsteps =
    {
        GD.Load<AudioStream>("res://assets/Audio/SoundFX/FootSteps/FootstepsConcrete1.wav"),
        GD.Load<AudioStream>("res://assets/Audio/SoundFX/FootSteps/FootstepsConcrete2.wav"),
        GD.Load<AudioStream>("res://assets/Audio/SoundFX/FootSteps/FootstepsConcrete3.wav"),
        GD.Load<AudioStream>("res://assets/Audio/SoundFX/FootSteps/FootstepsConcrete4.wav"),
    };

    public void SetFloorLayer(TileMapLayer floorLayer)
    {
        _currentFloorLayer = floorLayer;
    }

    public void PlayMainTheme()
    {
        // if main menu play MainMenu
    }

    public void PlayFootsteps(Vector2 globalPosition)
    {
        if (_currentFloorLayer == null)
        {
            return;
        }

        Vector2 localPosition = _currentFloorLayer.ToLocal(globalPosition);
        Vector2I cellPosition = _currentFloorLayer.LocalToMap(localPosition);

        TileData data = _currentFloorLayer.GetCellTileData(cellPosition);
        if (data == null)
        {
            return;
        }

        string footStepType = data.GetCustomData("footstep_sounds").AsString();

        var player = new AudioStreamPlayer2D();
        AddChild(player);

        var rng = new Random();
        player.Stream = Footsteps[rng.Next(Footsteps.Length)];

        player.GlobalPosition = globalPosition;
        // player.Stream = FootStep;
        player.Play();
        player.Finished += player.QueueFree;
        
    }
}
