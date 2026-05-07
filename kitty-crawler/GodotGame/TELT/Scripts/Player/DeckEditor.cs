using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KittyCrawler.TELT;

public partial class DeckEditor : Node
{
    private const int RequiredDeckSize = 25;
    private const int MaxCopiesCommon = 3;
    private const int MaxCopiesUncommon = 2;
    private const int MaxCopiesRare = 1;

    private List<string> _currentDeckPaths = new();
    private List<CardData> _currentDeck = new();

    public bool IsValid => _currentDeck.Count == RequiredDeckSize;
    public int CardsRemaining => RequiredDeckSize - _currentDeck.Count;

    public bool TryAddCard(CardData card)
    {
        if (_currentDeck.Count >= RequiredDeckSize) return false;

        int currentCopies = _currentDeck.Count(c => c.CardName == card.CardName);
        int maxCopies = GetMaxCopies(card);
        if (currentCopies >= maxCopies) return false;

        _currentDeck.Add(card);
        _currentDeckPaths.Add(card.ResourcePath); // ← ta vare på path direkte
        return true;
    }

    public bool TryRemoveCard(CardData card)
    {
        int idx = _currentDeck.FindIndex(c => c.CardName == card.CardName);
        if (idx < 0) return false;
        _currentDeck.RemoveAt(idx);
        _currentDeckPaths.RemoveAt(idx);
        return true;
    }

    public int GetCopiesInDeck(CardData card)
    {
        return _currentDeck.Count(c => c.CardName == card.CardName);
    }

    public int GetMaxCopies(CardData card)
    {
        return card.CardRarity switch
        {
            CardData.Rarity.Common   => MaxCopiesCommon,
            CardData.Rarity.Uncommon => MaxCopiesUncommon,
            CardData.Rarity.Rare     => MaxCopiesRare,
            _ => 0
        };
    }

    public void Save()
    {
        if (!IsValid) { GD.PrintErr($"[DeckEditor] {_currentDeck.Count}/{RequiredDeckSize} kort."); return; }
        PlayerData.SaveDeck(_currentDeckPaths);
        GD.Print("[DeckEditor] Deck lagret.");
    }

    public void LoadFromSaved()
    {
        _currentDeck.Clear();
        _currentDeckPaths.Clear();

        foreach (var path in PlayerData.SavedDeck)
        {
            var card = GD.Load<CardData>(path);
            if (card == null) { GD.PrintErr($"[DeckEditor] Ugyldig path: {path}"); continue; }
            _currentDeck.Add(card);
            _currentDeckPaths.Add(path);
        }
    }

    public List<CardData> GetDeck()
    {
        if (!IsValid)
            throw new InvalidOperationException(
                $"Deck må ha nøyaktig {RequiredDeckSize} kort. Har {_currentDeck.Count}.");
        return new List<CardData>(_currentDeck);
    }
}
