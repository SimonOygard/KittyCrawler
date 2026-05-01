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
    private TeltBattle _teltBattle;

    // ── Init ──────────────────────────────────────────────────────────
    public void Initialize(PlayerData enemyData, PlayerData playerData, BattleMap battleMap, GameManager gameManager,
        AbilityResolver abilityResolver, TeltBattle teltBattle)
    {
        _enemyData = enemyData;
        _playerData = playerData;
        _battleMap = battleMap;
        _gameManager = gameManager;
        _abilityResolver = abilityResolver;
        _teltBattle = teltBattle;
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
        {
            GD.Print($"[AI] ResolveNoTarget for {chosenCard.Ability}");
            _abilityResolver.ResolveNoTarget(chosenCard, chosenSlot, false);
            _teltBattle.SetLastAIAbility(chosenCard.Ability);
            _gameManager.EmitBoardUpdated();
            GD.Print($"[AI] ResolveNoTarget ferdig, CurrentTurn={_gameManager.CurrentTurn}");

            if (chosenCard.Ability == CardData.AbilityType.OpponentDiscards)
                return; // signal håndterer turn-switch
        }
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
                    _gameManager.EmitBoardUpdated();
                }
            }
            else
            {
                Slot targetSlot = ChooseAbilityTarget(chosenCard, chosenSlot);
                GD.Print($"[AI] ChooseAbilityTarget for {chosenCard.Ability}: target={targetSlot?.Position.ToString() ?? "null"}");
                if (targetSlot != null)
                {
                    GD.Print($"[AI] Kaller resolve for {chosenCard.Ability}");
                    if (chosenCard.Ability == CardData.AbilityType.GivePlusMinusThree)
                    {
                        bool isEnemyTarget = false;
                        for (int i = 0; i < 4; i++)
                            if (_battleMap.GetPlayerSlot((Slot.SlotPosition)i) == targetSlot)
                                isEnemyTarget = true;
                        int amount = isEnemyTarget ? -3 : 3;
                        _abilityResolver.ResolveDruid(targetSlot, amount);
                    }
                    else if (chosenCard.Ability == CardData.AbilityType.SwitchSlots)
                    {
                        // Tell alle units unntatt Hilda selv
                        int otherUnits = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            var eSlot = _battleMap.GetEnemySlot((Slot.SlotPosition)i);
                            var pSlot = _battleMap.GetPlayerSlot((Slot.SlotPosition)i);
                            if (eSlot.IsOccupied && eSlot != targetSlot) otherUnits++;
                            if (pSlot.IsOccupied) otherUnits++;
                        }
                        if (otherUnits < 2)
                        {
                            GD.Print("[AI] Hilda fizzlet — for få andre targets");
                            return;
                        }

                        Slot slotA = targetSlot;
                        Slot slotB = null;
                        int highest = -1;
                        for (int i = 0; i < 4; i++)
                        {
                            var slot = _battleMap.GetPlayerSlot((Slot.SlotPosition)i);
                            if (slot.IsOccupied && slot.GetDamage() > highest)
                            {
                                highest = slot.GetDamage();
                                slotB = slot;
                            }
                        }
                        if (slotB != null)
                            _abilityResolver.ResolveSwitchSlots(slotA, slotB);
                    }
                    else
                    {
                        _abilityResolver.ResolveWithSlotTarget(chosenCard, targetSlot, false);
                    }
                }
            }
        }

        if (_gameManager.CheckWarPhase())
        {
            GD.Print("[AI] Krigsfase klar etter ability!");
            _gameManager.EmitReadyForCombat();
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
            bool hasPlayerUnits = false;
            for (int i = 0; i < 4; i++)
                if (_battleMap.GetPlayerSlot((Slot.SlotPosition)i).IsOccupied)
                { hasPlayerUnits = true; break; }

            var playableHand = hand.Where(c =>
            {
                if (c.Ability == CardData.AbilityType.RemoveUnit && !hasPlayerUnits)
                    return false;
                return true;
            }).ToList();

            if (playableHand.Count == 0)
                playableHand = hand;
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

                case CardData.AbilityType.GivePlusMinusThree:
                    // Finn motstanderens sterkeste
                    Slot druidEnemyTarget = null;
                    int druidHighest = -1;
                    for (int i = 0; i < 4; i++)
                    {
                        var slot = _battleMap.GetPlayerSlot((Slot.SlotPosition)i);
                        if (slot.IsOccupied && slot.GetDamage() > druidHighest)
                        {
                            druidHighest = slot.GetDamage();
                            druidEnemyTarget = slot;
                        }
                    }

                    // Hvis motstanderens sterkeste har 3+, gi -3
                    if (druidEnemyTarget != null && druidHighest >= 3)
                        return druidEnemyTarget;

                    // Ellers gi +3 til eget svakeste
                    Slot druidAllyTarget = null;
                    int druidLowest = int.MaxValue;
                    for (int i = 0; i < 4; i++)
                    {
                        var slot = _battleMap.GetEnemySlot((Slot.SlotPosition)i);
                        if (slot.IsOccupied && slot.GetDamage() < druidLowest)
                        {
                            druidLowest = slot.GetDamage();
                            druidAllyTarget = slot;
                        }
                    }
                    return druidAllyTarget;

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
                    Slot weakestEnemy = null;
                    int lowestEnemy = int.MaxValue;
                    for (int i = 0; i < 4; i++)
                    {
                        var slot = _battleMap.GetEnemySlot((Slot.SlotPosition)i);
                        if (slot.IsOccupied
                            && slot.Card.Ability != CardData.AbilityType.SwitchSlots // ← ikke Hilda selv
                            && slot.GetDamage() < lowestEnemy)
                        {
                            lowestEnemy = slot.GetDamage();
                            weakestEnemy = slot;
                        }
                    }
                    return weakestEnemy;
                default:
                    return null;
            }
        }
    }
