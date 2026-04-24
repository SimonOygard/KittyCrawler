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
    public void Initialize(PlayerData enemyData, PlayerData playerData, BattleMap battleMap, GameManager gameManager,
        AbilityResolver abilityResolver)
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
        if (_gameManager.CurrentTurn != GameManager.TurnOwner.Enemy) return;
        if (!_enemyData.HasCardsInHand)
        {
            if (_gameManager.CurrentTurn == GameManager.TurnOwner.Enemy)
                _gameManager.SwitchTurnPublic();
            return;
        }

        var hand = _enemyData.GetHand();

        // Skester-logikk
        var skester = hand.FirstOrDefault(c => c.Ability == CardData.AbilityType.AnySlot);
        if (skester != null)
        {
            var playerEmptySlots = GetPlayerEmptySlots();
            if (playerEmptySlots.Count > 0)
            {
                var targetSlot = playerEmptySlots.First();
                bool placed = _battleMap.TryPlacePlayerCard(skester, targetSlot);
                if (placed)
                {
                    _enemyData.TryPlayCard(skester);
                    GD.Print($"[AI] Skester plassert på spillerens {targetSlot}-slot!");
                    if (_gameManager.CheckWarPhase()) return;
                    _gameManager.SwitchTurnPublic();
                    return;
                }
            }
            else if (hand.Count == 1)
            {
                var availableSlots = GetAvailableSlots();
                if (availableSlots.Count > 0)
                {
                    _gameManager.TryPlayCard(skester, availableSlots.First(), GameManager.TurnOwner.Enemy);
                    return;
                }
            }
        }

        var availableSlots2 = GetAvailableSlots();
        if (availableSlots2.Count == 0)
        {
            _gameManager.SwitchTurnPublic();
            return;
        }

        CardData chosenCard = ChooseCard(hand, availableSlots2);
        Slot.SlotPosition chosenSlot = ChooseSlot(chosenCard, availableSlots2);

        _gameManager.TryPlayCard(chosenCard, chosenSlot, GameManager.TurnOwner.Enemy);

        if (!_abilityResolver.NeedsTarget(chosenCard))
            _abilityResolver.ResolveNoTarget(chosenCard, chosenSlot, false);
        else
        {
            var targetType = _abilityResolver.GetTargetType(chosenCard);
            if (targetType == AbilityResolver.TargetType.HandCard)
            {
                var currentHand = _enemyData.GetHand();
                if (currentHand.Count > 0)
                {
                    var weakest = currentHand
                        .Where(c => c.Ability != CardData.AbilityType.NoExceedTortoise
                                 && c.Ability != CardData.AbilityType.AnySlot)
                        .OrderBy(c => c.GetCurrentDamage())
                        .FirstOrDefault();
                    weakest ??= currentHand.OrderBy(c => c.GetCurrentDamage()).First();
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

        if (_gameManager.CurrentTurn == GameManager.TurnOwner.Enemy)
        {
            var timer = GetTree().CreateTimer(0.5f);
            timer.Timeout += TakeTurn;
        }


        GD.Print($"[AI] Spilte {chosenCard.CardName} i {chosenSlot}");
        }



// ── Hjelpemetode: spillerens ledige slots ─────────────────────────
        private List<Slot.SlotPosition> GetPlayerEmptySlots()
        {
            var available = new List<Slot.SlotPosition>();
            foreach (Slot.SlotPosition pos in System.Enum.GetValues(typeof(Slot.SlotPosition)))
            {
                if (_battleMap.IsPlayerSlotEmpty(pos))
                    available.Add(pos);
            }

            return available;
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
                case CardData.AbilityType.GiveMinusOneStat:
                case CardData.AbilityType.GiveMinusTwoStats:
                    Slot bestPlayerSlot = null;
                    int highestDamage = -1;
                    for (int i = 0; i < 4; i++)
                    {
                        var slot = _battleMap.GetPlayerSlot((Slot.SlotPosition)i);
                        if (slot.IsOccupied && slot.GetDamage() > highestDamage)
                        {
                            highestDamage = slot.GetDamage();
                            bestPlayerSlot = slot;
                        }
                    }
                    return bestPlayerSlot;

                case CardData.AbilityType.GivePlusOneStat:
                case CardData.AbilityType.GivePlusTwoStats:
                    // velg egne sterkeste kort
                    Slot bestEnemySlot = null;
                    int highest = -1;
                    for (int i = 0; i < 4; i++)
                    {
                        var slot = _battleMap.GetEnemySlot((Slot.SlotPosition)i);
                        if (slot.IsOccupied && slot.GetDamage() > highest)
                        {
                            highest = slot.GetDamage();
                            bestEnemySlot = slot;
                        }
                    }

                    return bestEnemySlot;

                case CardData.AbilityType.ResetStat:
                    // Wraith: reset spillerens sterkeste kort
                    Slot resetTarget = null;
                    int resetHighest = -1;
                    for (int i = 0; i < 4; i++)
                    {
                        var slot = _battleMap.GetPlayerSlot((Slot.SlotPosition)i);
                        if (slot.IsOccupied && slot.GetDamage() > resetHighest)
                        {
                            resetHighest = slot.GetDamage();
                            resetTarget = slot;
                        }
                    }
                    return resetTarget;

                case CardData.AbilityType.CopyStat:
                    // Cat: kopier fra spillerens sterkeste kort
                    Slot copyTarget = null;
                    int copyHighest = -1;
                    for (int i = 0; i < 4; i++)
                    {
                        var slot = _battleMap.GetPlayerSlot((Slot.SlotPosition)i);
                        if (slot.IsOccupied && slot.GetDamage() > copyHighest)
                        {
                            copyHighest = slot.GetDamage();
                            copyTarget = slot;
                        }
                    }
                    return copyTarget;

                case CardData.AbilityType.RemoveUnit:
                    // Druid: sjekk først om Skester er på eget brett
                    for (int i = 0; i < 4; i++)
                    {
                        var slot = _battleMap.GetEnemySlot((Slot.SlotPosition)i);
                        if (slot.IsOccupied && slot.Card.Ability == CardData.AbilityType.AnySlot)
                            return slot; // Fjern Skester fra eget brett først!
                    }
                    // Ellers fjern spillerens sterkeste kort
                    Slot strongestPlayer = null;
                    int strongestDamage = -1;
                    for (int i = 0; i < 4; i++)
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
                    for (int i = 0; i < 4; i++)
                    {
                        var slot = _battleMap.GetPlayerSlot((Slot.SlotPosition)i);
                        if (slot.IsOccupied && slot.GetDamage() > mioBest)
                        {
                            mioBest = slot.GetDamage();
                            mioTarget = slot;
                        }
                    }

                    return mioTarget;

                case CardData.AbilityType.ApplyPoison:
                    for (int i = 0; i < 4; i++)
                    {
                        var slot = _battleMap.GetPlayerSlot((Slot.SlotPosition)i);
                        if (slot.IsOccupied && !slot.Card.HasStatus)
                            return slot;
                    }
                    return null;

                case CardData.AbilityType.ApplyRage:
                    for (int i = 0; i < 4; i++)
                    {
                        var slot = _battleMap.GetEnemySlot((Slot.SlotPosition)i);
                        if (slot.IsOccupied && !slot.Card.HasStatus)
                            return slot;
                    }
                    return null;

                case CardData.AbilityType.SwitchSlots:
                    return null;

                default:
                    return null;
            }
        }
    }
