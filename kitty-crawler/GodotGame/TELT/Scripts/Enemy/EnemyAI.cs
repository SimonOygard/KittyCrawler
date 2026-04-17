using Godot;
using System.Collections.Generic;
using System.Linq;

namespace KittyCrawler.TELT;

public partial class EnemyAI : Node
{
    private PlayerData _enemyData;
    private PlayerData _playerData;
    private BattleMap _battleMap;
    private GameManager _gameManager;
    private AbilityResolver _abilityResolver;

    // ── Init ──────────────────────────────────────────────────────────
    public void Initialize(PlayerData enemyData, PlayerData playerData, BattleMap battleMap, GameManager gameManager, AbilityResolver abilityResolver)
    {
        _enemyData = enemyData;
        _playerData = playerData;
        _battleMap = battleMap;
        _gameManager = gameManager;
        _abilityResolver = abilityResolver;
    }

    // ── Hovedmetode: AI velger kort og slot ───────────────────────────
    public void TakeTurn()
    {
        GD.Print($"[AI] TakeTurn: Kort på hånd={_enemyData.HandCount}, CurrentTurn={_gameManager.CurrentTurn}");
        if (_gameManager.CurrentTurn != GameManager.TurnOwner.Enemy) return;
        if (!_enemyData.HasCardsInHand)
        {
            GD.Print("[AI] Ingen kort på hånd!");
            return;
        }

        var hand = _enemyData.GetHand();
        var availableSlots = GetAvailableSlots();
        GD.Print($"[AI] Ledige slots: {availableSlots.Count}");
        if (availableSlots.Count == 0)
        {
            GD.Print("[AI] Ingen ledige slots!");
            _gameManager.SwitchTurnPublic(); // ← kun her, siden TryPlayCard aldri ble kalt
            return;
        }

        CardData chosenCard = ChooseCard(hand, availableSlots);
        Slot.SlotPosition chosenSlot = ChooseSlot(chosenCard, availableSlots);

        _gameManager.TryPlayCard(chosenCard, chosenSlot, GameManager.TurnOwner.Enemy);

        if (!_abilityResolver.NeedsTarget(chosenCard))
            _abilityResolver.ResolveNoTarget(chosenCard, chosenSlot, false);
        else
        {
            var targetType = _abilityResolver.GetTargetType(chosenCard);

            if (targetType == AbilityResolver.TargetType.HandCard)
            {
                // AI kaster svakeste kort fra hånd
                var currentHand = _enemyData.GetHand();
                if (currentHand.Count > 0)
                {
                    var weakest = currentHand.OrderBy(c => c.GetCurrentDamage()).First();
                    _abilityResolver.ResolveWithHandTarget(chosenCard, weakest, false);
                }
            }
            else
            {
                Slot targetSlot = ChooseAbilityTarget(chosenCard, chosenSlot);
                if (targetSlot != null)
                    _abilityResolver.ResolveWithSlotTarget(chosenCard, targetSlot, false);
            }
        }

        if (_gameManager.CheckWarPhase())
        {
            GD.Print("[AI] Krigsfase klar etter ability!");
            return;
        }

        GD.Print($"[AI] Spilte {chosenCard.CardName} i {chosenSlot}");
    }

    // ── Velg kort ─────────────────────────────────────────────────────
    private CardData ChooseCard(List<CardData> hand, List<Slot.SlotPosition> availableSlots)
    {
        // Finn hvilke lanes spilleren allerede har fylt
        var playerCards = availableSlots
            .Where(s => !_battleMap.IsEnemySlotEmpty(s))
            .ToList();

        // Prioriter å slå spillerens sterkeste kort
        CardData bestCounter = TryCounterPlayerSlot(hand, availableSlots);
        if (bestCounter != null) return bestCounter;

        // Fallback: spill sterkeste kort på hånd
        return hand.OrderByDescending(c => c.Damage).First();
    }

    // ── Prøv å kontre spillerens kort ─────────────────────────────────
    private CardData TryCounterPlayerSlot(List<CardData> hand, List<Slot.SlotPosition> availableSlots)
    {
        int bestAdvantage = int.MinValue;
        CardData bestCard = null;

        foreach (var slot in availableSlots)
        {
            // Sjekk om spilleren har et kort i denne lanen
            var playerSlot = _battleMap.GetPlayerSlot(slot);
            if (playerSlot.IsEmpty) continue;

            int playerPower = playerSlot.GetDamage();

            // Finn et kort på hånd som vinner denne lanen
            foreach (var card in hand)
            {
                int advantage = card.Damage - playerPower;
                if (advantage > bestAdvantage)
                {
                    bestAdvantage = advantage;
                    bestCard = card;
                }
            }
        }

        return bestCard;
    }

    // ── Velg slot ─────────────────────────────────────────────────────
    private Slot.SlotPosition ChooseSlot(CardData card, List<Slot.SlotPosition> availableSlots)
    {
        // Prioriter lane der kortet vinner
        foreach (var slot in availableSlots)
        {
            var playerSlot = _battleMap.GetPlayerSlot(slot);
            if (playerSlot.IsEmpty) continue;

            if (card.Damage > playerSlot.GetDamage())
                return slot;
        }

        // Prioriter lane der spilleren er sterkest (minimer tap)
        var mostDangerousLane = availableSlots
            .Where(s => !_battleMap.GetPlayerSlot(s).IsEmpty)
            .OrderByDescending(s => _battleMap.GetPlayerSlot(s).GetDamage())
            .FirstOrDefault();

        if (mostDangerousLane != default)
            return mostDangerousLane;

        // Fallback: første ledige slot
        return availableSlots.First();
    }

    // ── Hjelpemetoder ─────────────────────────────────────────────────
    private List<Slot.SlotPosition> GetAvailableSlots()
    {
        var available = new List<Slot.SlotPosition>();
        foreach (Slot.SlotPosition pos in System.Enum.GetValues(typeof(Slot.SlotPosition)))
        {
            if (_battleMap.IsEnemySlotEmpty(pos))
                available.Add(pos);
        }
        return available;
    }

    private Slot ChooseAbilityTarget(CardData card, Slot.SlotPosition ownPosition)
    {
        switch (card.Ability)
        {
            case CardData.AbilityType.GiveMinusStat:
                // Skeleton: velg spillerens sterkeste kort (ikke seg selv)
                Slot bestPlayerSlot = null;
                int highestDamage = -1;
                for (int i = 0; i < 3; i++)
                {
                    var slot = _battleMap.GetPlayerSlot((Slot.SlotPosition)i);
                    if (slot.IsOccupied && slot.GetDamage() > highestDamage)
                    {
                        highestDamage = slot.GetDamage();
                        bestPlayerSlot = slot;
                    }
                }
                return bestPlayerSlot;

            case CardData.AbilityType.GivePlusStat:
                // Drake: velg egne sterkeste kort
                Slot bestEnemySlot = null;
                int highest = -1;
                for (int i = 0; i < 3; i++)
                {
                    var slot = _battleMap.GetEnemySlot((Slot.SlotPosition)i);
                    if (slot.IsOccupied && slot.GetDamage() > highest)
                    {
                        highest = slot.GetDamage();
                        bestEnemySlot = slot;
                    }
                }
                return bestEnemySlot;

            case CardData.AbilityType.RemoveUnit:
                // Druid: fjern spillerens sterkeste kort
                Slot strongestPlayer = null;
                int strongestDamage = -1;
                for (int i = 0; i < 3; i++)
                {
                    var slot = _battleMap.GetPlayerSlot((Slot.SlotPosition)i);
                    if (slot.IsOccupied && slot.GetDamage() > strongestDamage)
                    {
                        strongestDamage = slot.GetDamage();
                        strongestPlayer = slot;
                    }
                }
                return strongestPlayer;

            case CardData.AbilityType.RemoveGainStats:
                // Mio: fjern spillerens sterkeste kort
                Slot mioTarget = null;
                int mioBest = -1;
                for (int i = 0; i < 3; i++)
                {
                    var slot = _battleMap.GetPlayerSlot((Slot.SlotPosition)i);
                    if (slot.IsOccupied && slot.GetDamage() > mioBest)
                    {
                        mioBest = slot.GetDamage();
                        mioTarget = slot;
                    }
                }
                return mioTarget;

            default:
                return null;
        }
    }
}
