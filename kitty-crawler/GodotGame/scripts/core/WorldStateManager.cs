using Godot;
using Godot.Collections;
using KittyCrawler.TELT;
using Microsoft.VisualBasic;
using System;
using System.Text.Json;

public partial class WorldStateManager : Node
{
    public WorldStateManager? Instance { get; private set; }

    public Vector2 PlayerPosition { get; set; }

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
    }

    public void LoadGame()
    {
        var savePath = "user://savegame.json";
        if (!FileAccess.FileExists(savePath)) return;
        using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
        var json = file.GetAsText();

        // JsonSerializer.Deserialize<WorldStateManager>(json);
    }


    // EmitSignal("NpcDefeated", npcId); // ← legg til signalemettering for NPC-beseirelse
    public void OnNpcDefeated(string npcId)
    {
        if (!BossesWon.Contains(npcId))
        {
            BossesWon.Add(npcId);
            SaveGame(); // Lagre spilltilstanden etter å ha beseiret en NPC
        }
    }

    public void OnCardAdded(string cardId)
    {
        if (!CardsOwned.Contains(cardId))
        {
            CardsOwned.Add(cardId);
            SaveGame(); // Lagre spilltilstanden etter å ha fått et nytt kort
        }
    }

   public void OnScoreUpdated(int newScore)
    {
        Score = newScore;
        SaveGame(); // Lagre spilltilstanden etter at poengsummen har blitt oppdatert
    }

    public void OnHealthUpdated(int newHealth)
    {
        Health = newHealth;
        SaveGame(); // Lagre spilltilstanden etter at helsen har blitt oppdatert
    }

    public void RegisterSaveEvents(TeltBattle battle)
    {
        battle.CardReceived += OnCardAdded;
        battle.NpcDefeated += OnNpcDefeated;
        battle.ScoreUpdated += OnScoreUpdated;
        // battle.HealthUpdated += OnHealthUpdated;
    }

    public void OnTimeUpdated(float newTimeSeconds)
    {
        TimeSeconds = newTimeSeconds;
        SaveGame();
    }

    public override void _ExitTree()
    {
        Instance = null;
        base._ExitTree();
    }
}
