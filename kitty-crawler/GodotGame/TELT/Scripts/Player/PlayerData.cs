using Godot;
using System.Collections.Generic;

namespace KittyCrawler.TELT;

public partial class PlayerData : Node
{
    public string PlayerName { get; set; } = "Player";
    public int TotalDamageReceived { get; set; } = 0;

    private List<CardData> _deck = new();
    private List<CardData> _hand = new();
    private List<CardData> _discardPile = new();

    // ── Deck ──────────────────────────────────────────────────────────
    public void SetDeck(List<CardData> deck)
    {
        _deck = new List<CardData>(deck);
    }

    public void ShuffleDeck()
    {
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        for (int i = _deck.Count - 1; i > 0; i--)
        {
            int j = (int)(rng.Randi() % (uint)(i + 1));
            (_deck[i], _deck[j]) = (_deck[j], _deck[i]);
        }
    }

    // ── Trekking ──────────────────────────────────────────────────────
    public CardData LastDrawnCard { get; private set; } = null;

    private List<CardData> _newlyDrawnCards = new();

    public List<CardData> NewlyDrawnCards => new(_newlyDrawnCards);

    public bool TryDrawCard()
    {
        if (_deck.Count == 0) return false;

        var card = _deck[0];
        _deck.RemoveAt(0);
        _hand.Add(card);
        LastDrawnCard = card;
        _newlyDrawnCards.Add(card); // ← legg til
        return true;
    }

    public void ClearLastDrawnCard()
    {
        LastDrawnCard = null;
        _newlyDrawnCards.Clear(); // ← tøm listen
    }

    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
            TryDrawCard();
    }

    // ── Hånd ──────────────────────────────────────────────────────────
    public List<CardData> GetHand() => new(_hand);
    public int HandCount => _hand.Count;
    public bool HasCardsInHand => _hand.Count > 0;

    public bool TryPlayCard(CardData card)
    {
        return _hand.Remove(card);
    }

    // ── Discard ───────────────────────────────────────────────────────
    public void DiscardCard(CardData card)
    {
        _hand.Remove(card);
        _discardPile.Add(card);
    }

    public void DiscardHand()
    {
        _discardPile.AddRange(_hand);
        _hand.Clear();
    }

    // ── Opprydding mellom matcher ─────────────────────────────────────
    public void CollectBattlemapCards(List<CardData> cardsFromBattlemap)
    {
        foreach (var card in cardsFromBattlemap)
        {
            card.ResetCurrentDamage();
            card.IsPoisoned = false; // ← legg til
            card.IsEnraged = false; // ← legg til
        }

        _discardPile.AddRange(cardsFromBattlemap);
    }

    // ── Damage ────────────────────────────────────────────────────────
    public void ReceiveDamage(int amount)
    {
        TotalDamageReceived += amount;
    }

    // ── Debug ─────────────────────────────────────────────────────────
    public void PrintState()
    {
        GD.Print(
            $"[{PlayerName}] Deck: {_deck.Count} | Hånd: {_hand.Count} | Discard: {_discardPile.Count} | Damage: {TotalDamageReceived}");
    }

    public int DeckCount => _deck.Count;
    public List<CardData> GetDiscardPile() => new(_discardPile);

    // ── Score ──────────────────────────────────────────────────────────
    private static int _totalDamageDealt = 0;
    private const string SavePath = "user://telt_score.json";

    public static int TotalDamageDealt
    {
        get => _totalDamageDealt;
        private set => _totalDamageDealt = value;
    }

    private static HashSet<string> _defeatedNpcs = new();

    public static bool HasDefeatedNpc(string npcId)
    {
        return _defeatedNpcs.Contains(npcId);
    }

    public static void DefeatNpc(string npcId, int damageDealt)
    {
        if (_defeatedNpcs.Contains(npcId)) return; // Allerede beseiret

        _defeatedNpcs.Add(npcId);
        AddDamageDealt(damageDealt);
        SaveScore();
    }

    public static void AddDamageDealt(int amount)
    {
        _totalDamageDealt += amount;
        SaveScore();
    }

    private static HashSet<string> _receivedCards = new();

    public static bool HasReceivedCard(string npcId)
    {
        return _receivedCards.Contains(npcId);
    }

    public static void GiveRewardCard(string npcId)
    {
        if (_receivedCards.Contains(npcId)) return;
        _receivedCards.Add(npcId);
        SaveScore();
    }

    public static void SaveScore()
    {
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        var data = new Godot.Collections.Dictionary
        {
            ["damageDealt"] = _totalDamageDealt,
            ["defeatedNpcs"] = string.Join(",", _defeatedNpcs),
            ["receivedCards"] = string.Join(",", _receivedCards)
        };
        file.StoreString(Json.Stringify(data));
    }

    public static void LoadScore()
    {
        if (!FileAccess.FileExists(SavePath)) return;
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
        var json = file.GetAsText();
        var data = Json.ParseString(json).AsGodotDictionary();
        _totalDamageDealt = data["damageDealt"].AsInt32();

        if (data.ContainsKey("defeatedNpcs") && data["defeatedNpcs"].AsString() != "")
        {
            foreach (var npc in data["defeatedNpcs"].AsString().Split(","))
                _defeatedNpcs.Add(npc);
        }

        if (data.ContainsKey("receivedCards") && data["receivedCards"].AsString() != "")  // ← legg til
            foreach (var card in data["receivedCards"].AsString().Split(","))
                _receivedCards.Add(card);
    }

}
