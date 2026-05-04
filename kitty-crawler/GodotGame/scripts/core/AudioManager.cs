using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public partial class AudioManager : Node
{
    // Background music
    private AudioStreamPlayer _backgroundMusicPlayer;
    private static readonly AudioStream MainMenu = GD.Load<AudioStream>("res://assets/Audio/Music/wav/02-TitleTheme.wav");
    private static readonly AudioStream MainGame = GD.Load<AudioStream>("res://assets/Audio/Music/wav/12-FrozenAbyss.wav");
    private static readonly AudioStream EndGame = GD.Load<AudioStream>("res://assets/Audio/Music/wav/22-TheFinalofTheFantasy.wav");
    

    // Step sounds
    private TileMapLayer _currentFloorLayer;
    private static readonly AudioStream[] Footsteps =
    {
        GD.Load<AudioStream>("res://assets/Audio/SoundFX/FootSteps/FootstepsConcrete1.wav"),
        GD.Load<AudioStream>("res://assets/Audio/SoundFX/FootSteps/FootstepsConcrete2.wav"),
        GD.Load<AudioStream>("res://assets/Audio/SoundFX/FootSteps/FootstepsConcrete3.wav"),
        GD.Load<AudioStream>("res://assets/Audio/SoundFX/FootSteps/FootstepsConcrete4.wav"),
    };

    // Constants
    private const string Main_Menu= "res://scenes/MainMenu/MainScene.tscn";
    private const string Telt = "res://TELT/Scenes/TeltBattle.tscn";
    private const string End_Menu= "res://scenes/MainMenu/GameOver.tscn";

    private const float MusicVolumeDb = -10f;
    private const float SilentVolumeDb = -40f;
    private bool _isChangingMusic = false;



    public override void _Ready()
    {
        var sceneManager = GetNode<SceneManager>("/root/SceneManager");
        sceneManager.LevelLoaded += OnLevelLoaded;

        var currentScene = GetTree().CurrentScene;
        if (currentScene != null)
        {
            OnLevelLoaded(currentScene.SceneFilePath);
        }
    }

    private void OnLevelLoaded(string scenePath)
    {
        GD.Print("AudioManager received scenePath: " + scenePath);

        switch (scenePath)
        {
            case Main_Menu:
                PlayMainMenuTheme();
                break;

            case Telt:
                PlayTeltBackgroundMusic();
                break;

            case End_Menu:
                PlayEndGameTheme();
                break;

            default:
                PlayMainGameTheme();
                break;
        }
    }

    public void PlayMainMenuTheme()
    {
        PlayBackgroundMusic(MainMenu);
    }

    public void PlayMainGameTheme()
    {
        PlayBackgroundMusic(MainGame);
    }

    public void PlayEndGameTheme()
    {
        PlayBackgroundMusic(EndGame);
    }
    private void PlayBackgroundMusic(AudioStream stream)
    {
        ChangeBackgroundMusic(stream);
    }

    private async void ChangeBackgroundMusic(AudioStream stream)
    {
        if (_isChangingMusic)
            return;

        if (_backgroundMusicPlayer != null && _backgroundMusicPlayer.Stream == stream)
            return;

        _isChangingMusic = true;

        if (_backgroundMusicPlayer != null)
        {
            await FadeVolume(_backgroundMusicPlayer, _backgroundMusicPlayer.VolumeDb, SilentVolumeDb, 0.5f);
            _backgroundMusicPlayer.QueueFree();
            _backgroundMusicPlayer = null;
        }

        _backgroundMusicPlayer = new AudioStreamPlayer();
        AddChild(_backgroundMusicPlayer);

        _backgroundMusicPlayer.Stream = stream;
        _backgroundMusicPlayer.VolumeDb = SilentVolumeDb;
        _backgroundMusicPlayer.Play();
        _backgroundMusicPlayer.Finished += () => _backgroundMusicPlayer.Play();

        await FadeVolume(_backgroundMusicPlayer, SilentVolumeDb, MusicVolumeDb, 1.5f);

        _isChangingMusic = false;
    }

    private async Task FadeVolume(AudioStreamPlayer player, float fromDb, float toDb, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!IsInstanceValid(player))
                return;

            elapsed += (float)GetProcessDeltaTime();
            float t = Mathf.Clamp(elapsed / duration, 0f, 1f);

            player.VolumeDb = Mathf.Lerp(fromDb, toDb, t);

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        if (IsInstanceValid(player))
            player.VolumeDb = toDb;
    }


    public void StopBackgroundMusic()
    {
        _backgroundMusicPlayer?.QueueFree();
        _backgroundMusicPlayer = null;
    }

    // --- FootSteps---
    public void SetFloorLayer(TileMapLayer floorLayer)
    {
        _currentFloorLayer = floorLayer;
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


    // -- TELT -----------

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
    private static readonly AudioStream TeltBackgroundMusic = GD.Load<AudioStream>("res://assets/Audio/Music/wav/08 - Shop.wav");
    private static readonly AudioStream CleanUp = GD.Load<AudioStream>("res://assets/Audio/SoundFX/Card and Board/card_fan.wav");


    public void PlayCleanUp()
    {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = CleanUp;
        player.Play();
        player.Finished += player.QueueFree;
    }

    public void PlayTeltBackgroundMusic()
    {
        PlayBackgroundMusic(TeltBackgroundMusic);
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
