using Godot;
using System.Collections.Generic;
using System.Linq;

namespace KittyCrawler.TELT;

public partial class BattleMap : Node
{
    // Alle 6 slots
    private Slot[] _playerSlots = new Slot[4];
    private Slot[] _enemySlots = new Slot[4];

    public override void _Ready()
    {
        // Opprett player slots
        for (int i = 0; i < 4; i++)
        {
            _playerSlots[i] = new Slot
            {
                Position = (Slot.SlotPosition)i,
                Owner = Slot.SlotOwner.Player
            };
        }

        // Opprett enemy slots
        for (int i = 0; i < 4; i++)
        {
            _enemySlots[i] = new Slot
            {
                Position = (Slot.SlotPosition)i,
                Owner = Slot.SlotOwner.Enemy
            };
        }
    }

    // ── Plassering ────────────────────────────────────────────────────
    public bool TryPlacePlayerCard(CardData card, Slot.SlotPosition position)
    {
        return _playerSlots[(int)position].TryPlaceCard(card);
    }

    public bool TryPlaceEnemyCard(CardData card, Slot.SlotPosition position)
    {
        return _enemySlots[(int)position].TryPlaceCard(card);
    }

    // ── Slot-tilstand ─────────────────────────────────────────────────
    public bool IsPlayerSlotEmpty(Slot.SlotPosition position) => _playerSlots[(int)position].IsEmpty;
    public bool IsEnemySlotEmpty(Slot.SlotPosition position) => _enemySlots[(int)position].IsEmpty;

    public int PlayerEmptySlotCount => _playerSlots.Count(s => s.IsEmpty);
    public int EnemyEmptySlotCount => _enemySlots.Count(s => s.IsEmpty);

    public bool AllPlayerSlotsFilled => _playerSlots.All(s => s.IsOccupied);
    public bool AllEnemySlotsFilled => _enemySlots.All(s => s.IsOccupied);

    // ── Krigsfase-trigger (regel 3, 3.3, 3.4) ────────────────────────
    public bool ShouldStartWarPhase(bool playerHasCards, bool enemyHasCards)
    {
        GD.Print($"ShouldStartWarPhase: AllPlayer={AllPlayerSlotsFilled}, AllEnemy={AllEnemySlotsFilled}, playerHasCards={playerHasCards}, enemyHasCards={enemyHasCards}");
        // Alle 6 slots fylt
        if (AllPlayerSlotsFilled && AllEnemySlotsFilled)
            return true;

        // Begge har ingen kort på hånd OG ingen ledige slots å fylle
        if (!playerHasCards && !enemyHasCards)
            return true;

        // Spiller har ingen kort på hånd OG enemy slots er fylt
        if (!playerHasCards && AllEnemySlotsFilled)
            return true;

        // Enemy har ingen kort på hånd OG player slots er fylt
        if (!enemyHasCards && AllPlayerSlotsFilled)
            return true;

        return false;
    }

    // ── Krigsfase-beregning ───────────────────────────────────────────
    public (int playerDamage, int enemyDamage) ResolveWar()
    {
        int playerDamage = 0;
        int enemyDamage = 0;

        for (int i = 0; i < 4; i++)
        {
            int playerPower = _playerSlots[i].GetDamage();
            int enemyPower = _enemySlots[i].GetDamage();

            // Tortoise: ingen damage overstiger dette kortets stats
            if (_playerSlots[i].IsOccupied && _playerSlots[i].Card.Ability == CardData.AbilityType.NoExceedTortoise)
                enemyPower = Mathf.Min(enemyPower, playerPower);
            if (_enemySlots[i].IsOccupied && _enemySlots[i].Card.Ability == CardData.AbilityType.NoExceedTortoise)
                playerPower = Mathf.Min(playerPower, enemyPower);

            // Drake: alltid går damage gjennom uansett
            if (_playerSlots[i].IsOccupied && _playerSlots[i].Card.Ability == CardData.AbilityType.AllDamageExceeds)
                enemyDamage += playerPower;
            else if (_enemySlots[i].IsOccupied && _enemySlots[i].Card.Ability == CardData.AbilityType.AllDamageExceeds)
                playerDamage += enemyPower;

            int diff = playerPower - enemyPower;

            if (diff > 0) enemyDamage += diff;
            else if (diff < 0) playerDamage += -diff;
        }

        return (playerDamage, enemyDamage);
    }

    // ── Opprydding ────────────────────────────────────────────────────
    public List<CardData> CollectPlayerCards()
    {
        var cards = new List<CardData>();
        foreach (var slot in _playerSlots)
            if (slot.IsOccupied)
                cards.Add(slot.RemoveCard());
        return cards;
    }

    public List<CardData> CollectEnemyCards()
    {
        var cards = new List<CardData>();
        foreach (var slot in _enemySlots)
            if (slot.IsOccupied)
                cards.Add(slot.RemoveCard());
        return cards;
    }

    // ── Hent slots ────────────────────────────────────────────────────
    public Slot GetPlayerSlot(Slot.SlotPosition position) => _playerSlots[(int)position];
    public Slot GetEnemySlot(Slot.SlotPosition position) => _enemySlots[(int)position];

    // ── Debug ─────────────────────────────────────────────────────────
    public void PrintState()
    {
        GD.Print("=== Battle Map ===");
        foreach (var slot in _enemySlots)  GD.Print($"  {slot}");
        GD.Print("  ──────────────");
        foreach (var slot in _playerSlots) GD.Print($"  {slot}");
    }
}
