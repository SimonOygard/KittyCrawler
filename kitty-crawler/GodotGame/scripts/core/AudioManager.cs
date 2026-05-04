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


    // TELT

    // Demp musikk når rare kort spilles
    public async void DuckBackgroundMusic(float duration = 2.4f)
    {
        if (_backgroundMusicPlayer == null) return;

        float originalVolume = _backgroundMusicPlayer.VolumeDb;
        _backgroundMusicPlayer.VolumeDb = -20f; // ← dempet volum

        await ToSignal(GetTree().CreateTimer(duration), "timeout");

        _backgroundMusicPlayer.VolumeDb = originalVolume; // ← tilbake til original
    }

    //Bakgrunnsmusikk
    private static readonly AudioStream CombatDamage = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Fantasy UI SFX/Fantasy UI SFX/Skyward Hero/SkywardHero_UI (20).wav");
    private static readonly AudioStream Defeat = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Fantasy UI SFX/Fantasy UI SFX/Skyward Hero/SkywardHero_UI (1).wav");
    private static readonly AudioStream Victory = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Fantasy UI SFX/Fantasy UI SFX/Skyward Hero/SkywardHero_UI (38).wav");
    private static readonly AudioStream Draw = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Fantasy UI SFX/Fantasy UI SFX/Skyward Hero/SkywardHero_UI (42).wav");
    private static readonly AudioStream DiceRoll = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Card and Board/dice_roll_1.wav");
    private static readonly AudioStream DiceShake = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Card and Board/dice_shake_2.wav");
    private static readonly AudioStream BackgroundMusic = GD.Load<AudioStream>("res://assets/Audio/Music/wav/08 - Shop.wav");
    private static readonly AudioStream CleanUp = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Card and Board/card_fan.wav");


    public void PlayCleanUp()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = CleanUp;
        player.Play();
        player.Finished += player.QueueFree;
    }

    private AudioStreamPlayer _backgroundMusicPlayer;

    public void PlayBackgroundMusic()
    {
        if (_backgroundMusicPlayer != null) return; // allerede spiller

        _backgroundMusicPlayer = new AudioStreamPlayer();
        AddChild(_backgroundMusicPlayer);
        _backgroundMusicPlayer.Stream = BackgroundMusic; // ← din AudioStream
        _backgroundMusicPlayer.VolumeDb = -10f; // ← lavere volum, juster etter smak
        _backgroundMusicPlayer.Play();
        _backgroundMusicPlayer.Finished += () => _backgroundMusicPlayer.Play();
    }

    public void StopBackgroundMusic()
    {
        _backgroundMusicPlayer?.QueueFree();
        _backgroundMusicPlayer = null;
    }
    public void PlayDiceShake()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = DiceShake;
        player.Play();
        player.Finished += player.QueueFree;
    }
    public void PlayDiceRoll()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = DiceRoll;
        player.Play();
        player.Finished += player.QueueFree;
    }
    public void PlayDraw()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = Draw;
        player.Play();
        player.Finished += player.QueueFree;
    }
    public void PlayVictory()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = Victory;
        player.Play();
        player.Finished += player.QueueFree;
    }
    public void PlayDefeat()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = Defeat;
        player.Play();
        player.Finished += player.QueueFree;
    }
    public void PlayCombatDamage()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = CombatDamage;
        player.Play();
        player.Finished += player.QueueFree;
    }





    // Abilities
    private static readonly AudioStream CardDraw = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Card and Board/card_draw_2.wav");
    private static readonly AudioStream Discard = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Card and Board/card_draw_3.wav");
    private static readonly AudioStream MinusStat = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Fantasy UI SFX/Fantasy UI SFX/Fantasy/Fantasy_UI (12).wav");
    private static readonly AudioStream Eve = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Fantasy UI SFX/Fantasy UI SFX/Skyward Hero/SkywardHero_UI (3).wav");
    private static readonly AudioStream Croxy = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Fantasy UI SFX/Fantasy UI SFX/Skyward Hero/SkywardHero_UI (13).wav");
    private static readonly AudioStream RemoveUnit = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Card and Board/card_draw_1.wav");
    private static readonly AudioStream ResetStat = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Fantasy UI SFX/Fantasy UI SFX/Skyward Hero/SkywardHero_UI (12).wav");
    private static readonly AudioStream PlusStat = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Fantasy UI SFX/Fantasy UI SFX/Fantasy/Fantasy_UI (20).wav");
    private static readonly AudioStream Skester = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Fantasy UI SFX/Fantasy UI SFX/Fantasy/Fantasy_UI (27).wav");
    private static readonly AudioStream Hilda = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Fantasy UI SFX/Fantasy UI SFX/Skyward Hero/SkywardHero_UI (18).wav");
    private static readonly AudioStream Heal = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Fantasy UI SFX/Fantasy UI SFX/Fantasy/Fantasy_UI (10).wav");
    private static readonly AudioStream DealDamage = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Fantasy UI SFX/Fantasy UI SFX/Skyward Hero/SkywardHero_UI (16).wav");
    private static readonly AudioStream CopyStat = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Fantasy UI SFX/Fantasy UI SFX/Fantasy/Fantasy_UI (6).wav");
    private static readonly AudioStream Poison = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Fantasy UI SFX/Fantasy UI SFX/Skyward Hero/SkywardHero_UI (22).wav");
    private static readonly AudioStream Rage = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Fantasy UI SFX/Fantasy UI SFX/Skyward Hero/SkywardHero_UI (36).wav");

    public void PlayRage()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = Rage;
        player.Play();
        player.Finished += player.QueueFree;
    }
    public void PlayPoison()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = Poison;
        player.Play();
        player.Finished += player.QueueFree;
    }
    public void PlayCopyStat()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = CopyStat;
        player.Play();
        player.Finished += player.QueueFree;
    }
    public void PlayDealDamage()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = DealDamage;
        player.Play();
        player.Finished += player.QueueFree;
    }

    public void PlayHeal()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = Heal;
        player.Play();
        player.Finished += player.QueueFree;
    }
    public void PlayHilda()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = Hilda;
        player.Play();
        player.Finished += player.QueueFree;
    }
    public void PlayPlusStat()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = PlusStat;
        player.Play();
        player.Finished += player.QueueFree;
    }
    public void PlaySkester()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = Skester;
        player.Play();
        player.Finished += player.QueueFree;
    }
    public void PlayResetStat()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = ResetStat;
        player.Play();
        player.Finished += player.QueueFree;
    }
    public void PlayRemoveUnit()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = RemoveUnit;
        player.Play();
        player.Finished += player.QueueFree;
    }
    public void PlayCroxy()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = Croxy;
        player.Play();
        player.Finished += player.QueueFree;
    }

    public void PlayCardDraw()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = CardDraw;
        player.Play();
        player.Finished += player.QueueFree;
    }

    public void PlayDiscard()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = Discard;
        player.Play();
        player.Finished += player.QueueFree;
    }

    public void PlayMinusStat()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = MinusStat;
        player.Play();
        player.Finished += player.QueueFree;
    }

    public void PlayEve()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = Eve;
        player.Play();
        player.Finished += player.QueueFree;
    }


}
