using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KittyCrawler.TELT;

public partial class DeckEditor : Node
{
    private const int RequiredDeckSize = 15;
    private const int MaxCopiesCommon = 3;
    private const int MaxCopiesUncommon = 2;
    private const int MaxCopiesRare = 1;

    private List<CardData> _currentDeck = new();

    public bool IsValid => _currentDeck.Count == RequiredDeckSize;
    public int CardsRemaining => RequiredDeckSize - _currentDeck.Count;

    public bool TryAddCard(CardData card)
    {
        if (_currentDeck.Count >= RequiredDeckSize)
            return false;

        int currentCopies = _currentDeck.Count(c => c.CardName == card.CardName);
        int maxCopies = card.CardRarity switch
        {
            CardData.Rarity.Common   => MaxCopiesCommon,
            CardData.Rarity.Uncommon => MaxCopiesUncommon,
            CardData.Rarity.Rare     => MaxCopiesRare,
            _ => 0
        };

        if (currentCopies >= maxCopies)
            return false;

        _currentDeck.Add(card);
        return true;
    }

    public bool TryRemoveCard(CardData card)
    {
        return _currentDeck.Remove(card);
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

    public List<CardData> GetDeck()
    {
        if (!IsValid)
            throw new InvalidOperationException(
                $"Deck må ha nøyaktig {RequiredDeckSize} kort. Har {_currentDeck.Count}.");
        return new List<CardData>(_currentDeck);
    }
}
