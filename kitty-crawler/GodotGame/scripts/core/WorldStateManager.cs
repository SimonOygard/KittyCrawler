using Godot;
using Godot.Collections;
using KittyCrawler.TELT;
using Microsoft.VisualBasic;
using System;
using System.Text.Json;

public partial class WorldStateManager : Node
{
    public static WorldStateManager? Instance { get; private set; }

    public GameMode Mode { get; private set; } = GameMode.Gameplay;

    public bool PlayerCanAct => Mode == GameMode.Gameplay;

    public bool GameEnded { get; set; } = false;

    public Vector2 PlayerPosition { get; set; }

    public Vector2? ReturnPlayerPosition { get; private set; }

    public string UserName { get; set; } = string.Empty;

    public Array<string> CardsOwned { get; set; } = new Array<string>();

    public Array<string> Deck { get; set; } = new Array<string>();

    public Array<string> BossesWon { get; set; } = new Array<string>();
    public int Score { get; set; }

    public int Health { get; set; }

    public float TimeSeconds { get; set; }

    public override void _Ready()
	{
        Instance = this;
    }
    public void SetMode(GameMode mode)
    {
        Mode = mode;
    }

    public void SaveGame()
    {
        var savePath = "user://savegame.json";
        using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);

        var data = new Dictionary
        {
            { "playerPosition", new Dictionary { { "x", PlayerPosition.X }, { "y", PlayerPosition.Y } } },
            { "cardsOwned", CardsOwned },
            { "deck", Deck },
            { "bossesWon", BossesWon },
            { "score", Score },
            { "health", Health },
            { "timeSeconds", TimeSeconds },
            { "userName", UserName }
        };

        file.StoreString(Json.Stringify(data));
        GD.Print($"Saving: score={Score}, time={TimeSeconds}");
    }

    public void LoadGame()
    {
        var savePath = "user://savegame.json";
        if (!FileAccess.FileExists(savePath)) return;

        using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
        var json = file.GetAsText();

        var data = Json.ParseString(json).AsGodotDictionary();

        Score = (int)data["score"];
        Health = (int)data["health"];
        TimeSeconds = (float)data["timeSeconds"];
        UserName = (string)data["userName"];
        GD.Print($"Loaded save: score={Score}, time={TimeSeconds}");

    }


    // EmitSignal("NpcDefeated", npcId); // ← legg til signalemettering for NPC-beseirelse
    public void OnNpcDefeated(string npcId)
    {
        if (!BossesWon.Contains(npcId))
        {
            BossesWon.Add(npcId);

            if (!GameEnded)
                SaveGame();
        }
    }


    public void OnCardAdded(string cardId)
    {
        if (!CardsOwned.Contains(cardId))
        {
            CardsOwned.Add(cardId);

            if (!GameEnded)
                SaveGame();
        }
    }

    public void OnScoreUpdated(int newScore)
    {
        if (GameEnded) return;

        Score += newScore;
        SaveGame();
    }

    public void OnTimeUpdated(float newTimeSeconds)
    {
        if (GameEnded) return;

        TimeSeconds = newTimeSeconds;
        SaveGame();
    }
    public void OnHealthUpdated(int newHealth)
    {
        if (GameEnded) return;

        Health = newHealth;
        SaveGame();
    }

    public void RegisterSaveEvents(TeltBattle battle)
    {
        battle.CardReceived += OnCardAdded;
        battle.NpcDefeated += OnNpcDefeated;
        battle.ScoreUpdated += OnScoreUpdated;
        // battle.HealthUpdated += OnHealthUpdated;
    }

    public override void _ExitTree()
    {
        Instance = null;
        base._ExitTree();
    }

    public void SetReturnPlayerPosition(Vector2 position)
    {
        ReturnPlayerPosition = position;
    }

    public bool TryConsumeReturnPlayerPosition(out Vector2 position)
    {
        if (ReturnPlayerPosition.HasValue)
        {
            position = ReturnPlayerPosition.Value;
            ReturnPlayerPosition = null; // prevents applying it on normal save/load
            return true;
        }

        position = Vector2.Zero;
        return false;
    }

    public void WorldStateReset()
    {
        Score = 0;
        Health = 0;
        TimeSeconds = 0;
        UserName = "";
    }

    public enum GameMode
    {
        Gameplay,
        Dialogue,
    }
}
