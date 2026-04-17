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
    public bool TryDrawCard()
    {
        if (_deck.Count == 0) return false;

        var card = _deck[0];
        _deck.RemoveAt(0);
        _hand.Add(card);
        return true;
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
            card.ResetCurrentDamage(); // ← nullstill stats
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
        GD.Print($"[{PlayerName}] Deck: {_deck.Count} | Hånd: {_hand.Count} | Discard: {_discardPile.Count} | Damage: {TotalDamageReceived}");
    }
}
