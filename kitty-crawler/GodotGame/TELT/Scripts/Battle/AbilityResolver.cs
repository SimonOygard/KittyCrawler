using Godot;
using System;
using System.Collections.Generic;


namespace KittyCrawler.TELT;

public partial class AbilityResolver : Node
{
    private BattleMap _battleMap;
    private PlayerData _player;
    private PlayerData _enemy;
    private GameManager _gameManager;

    // ── Signals ───────────────────────────────────────────────────────
    [Signal] public delegate void AbilityRequiresTargetEventHandler(CardData card, TargetType targetType);
    [Signal] public delegate void AbilityResolvedEventHandler(CardData card);
    [Signal] public delegate void StatResetEventHandler(int slotIndex, bool isPlayerSlot);

    //--Highlight
    public Action<int, bool> OnTargetHighlight { get; set; }

    public enum TargetType
    {
        None,
        OwnSlot,
        EnemySlot,
        AnySlot,
        HandCard
    }

    // ── Init ──────────────────────────────────────────────────────────
    public void Initialize(BattleMap battleMap, PlayerData player, PlayerData enemy, GameManager gameManager)
    {
        _battleMap = battleMap;
        _player = player;
        _enemy = enemy;
        _gameManager = gameManager;
    }

    // ── Hovedmetode ───────────────────────────────────────────────────
    public bool NeedsTarget(CardData card)
    {
        return card.Ability switch
        {
            CardData.AbilityType.None             => false,
            CardData.AbilityType.NoExceedTortoise => false,
            CardData.AbilityType.GivePlusOneStat => true,
            CardData.AbilityType.GivePlusTwoStats => true,
            CardData.AbilityType.GiveMinusOneStat => true,
            CardData.AbilityType.GiveMinusTwoStats => true,
            CardData.AbilityType.CopyStat => true,
            CardData.AbilityType.DrawCard => false,
            CardData.AbilityType.DrawTwoCards => false,
            CardData.AbilityType.AllEnemyMinusStat => false,
            CardData.AbilityType.AllAllyPlusStat => false,
            CardData.AbilityType.SwitchSlots => true,
            CardData.AbilityType.ResetStat => true,
            CardData.AbilityType.ApplyPoison => true,
            CardData.AbilityType.ApplyRage => true,
            CardData.AbilityType.GivePlusMinusThree => true,
            CardData.AbilityType.AnySlot          => false,
            CardData.AbilityType.DealThreeDamage => false,
            CardData.AbilityType.HealThree => false,
            CardData.AbilityType.OpponentDiscards => false,
            _                                     => true
        };
    }

    public TargetType GetTargetType(CardData card)
    {
        return card.Ability switch
        {
            CardData.AbilityType.GivePlusOneStat   => TargetType.AnySlot,
            CardData.AbilityType.GivePlusTwoStats    => TargetType.AnySlot,
            CardData.AbilityType.GiveMinusOneStat   => TargetType.AnySlot,
            CardData.AbilityType.GiveMinusTwoStats => TargetType.AnySlot,
            CardData.AbilityType.RemoveUnit      => TargetType.AnySlot,
            CardData.AbilityType.SwitchSlots     => TargetType.AnySlot,
            CardData.AbilityType.CopyStat => TargetType.AnySlot,
            CardData.AbilityType.RemoveGainStats => TargetType.AnySlot,
            CardData.AbilityType.ApplyPoison => TargetType.AnySlot,
            CardData.AbilityType.ApplyRage   => TargetType.AnySlot,
            CardData.AbilityType.GivePlusMinusThree => TargetType.AnySlot,
            CardData.AbilityType.DiscardDraw     => TargetType.HandCard,
            CardData.AbilityType.DiscardGainStats => TargetType.HandCard,
            _                                    => TargetType.None
        };
    }

    // ── Resolve uten target ─────────────────────────────────────────────
    public void ResolveNoTarget(CardData card, Slot.SlotPosition placedAt, bool isPlayer)
    {
        switch (card.Ability)
        {
            case CardData.AbilityType.DrawCard:
                ResolveDrawCard(isPlayer);
                break;
            case CardData.AbilityType.DrawTwoCards:
                ResolveDrawTwoCards(isPlayer);
                break;
            case CardData.AbilityType.AllEnemyMinusStat:
                ResolveAllEnemyMinusStat(isPlayer);
                break;
            case CardData.AbilityType.AllAllyPlusStat:
                ResolveAllAllyPlusStat(isPlayer);
                break;
            case CardData.AbilityType.DealThreeDamage:
                ResolveDealThreeDamage(isPlayer);
                break;
            case CardData.AbilityType.HealThree:
                ResolveHealThree(isPlayer);
                break;
            case CardData.AbilityType.OpponentDiscards:
                ResolveOpponentDiscards(isPlayer);
                break;
            case CardData.AbilityType.AnySlot:
            case CardData.AbilityType.None:

                break;
        }

        EmitSignal(SignalName.AbilityResolved, card);
    }

    // ── Resolve med target ───────────────────────────────────────────────
    public void ResolveWithSlotTarget(CardData card, Slot targetSlot, bool isPlayer)
    {
        for (int i = 0; i < 4; i++)
        {
            if (_battleMap.GetPlayerSlot((Slot.SlotPosition)i) == targetSlot)
            {
                if (isPlayer) break; // ← ikke highlight når spilleren spiller
                OnTargetHighlight?.Invoke(i, true);
                break;
            }
            if (_battleMap.GetEnemySlot((Slot.SlotPosition)i) == targetSlot)
            {
                if (isPlayer) break; // ← ikke highlight når spilleren spiller
                OnTargetHighlight?.Invoke(i, false);
                break;
            }
        }

        switch (card.Ability)
        {
            case CardData.AbilityType.CopyStat:
                ResolveCopyStat(card, targetSlot, isPlayer);
                break;
            case CardData.AbilityType.GivePlusOneStat:
                ResolveGivePlusOneStat(targetSlot);
                break;
            case CardData.AbilityType.GivePlusTwoStats:
                ResolveGivePlusTwoStats(targetSlot);
                break;
            case CardData.AbilityType.GiveMinusOneStat:
                ResolveGiveMinusOneStat(targetSlot);
                break;
            case CardData.AbilityType.GiveMinusTwoStats:
                ResolveGiveMinusTwoStats(targetSlot);
                break;
            case CardData.AbilityType.RemoveUnit:
                ResolveRemoveUnit(targetSlot);
                break;
            case CardData.AbilityType.RemoveGainStats:
                ResolveRemoveGainStats(targetSlot, isPlayer);
                break;
            case CardData.AbilityType.ResetStat:
                ResolveResetStat(targetSlot);
                break;
            case CardData.AbilityType.ApplyPoison:
                ResolveApplyPoison(targetSlot);
                break;
            case CardData.AbilityType.ApplyRage:
                ResolveApplyRage(targetSlot);
                break;
            case CardData.AbilityType.SwitchSlots:
                // SwitchSlots krever to targets, håndteres separat
                break;
        }

        EmitSignal(SignalName.AbilityResolved, card);
    }

    public void ResolveWithHandTarget(CardData card, CardData targetCard, bool isPlayer)
    {
        switch (card.Ability)
        {
            case CardData.AbilityType.DiscardDraw:
                ResolveDiscardDraw(targetCard, isPlayer);
                break;
            case CardData.AbilityType.DiscardGainStats:
                ResolveDiscardGainStats(card, targetCard, isPlayer);
                break;
        }

        EmitSignal(SignalName.AbilityResolved, card);
    }

    public void ResolveSwitchSlots(Slot slotA, Slot slotB)
    {
        var cardA = slotA.RemoveCard();
        var cardB = slotB.RemoveCard();

        if (cardA != null) slotB.TryPlaceCard(cardA);
        if (cardB != null) slotA.TryPlaceCard(cardB);

        GD.Print($"[Ability] SwitchSlots: {slotA.Position} ↔ {slotB.Position}");
    }

    // ── Individuelle abilities ────────────────────────────────────────

    // Tortoise & Drake resolves in ResolveWar()

    // Eve: All enemies -2 stats
    private void ResolveAllEnemyMinusStat(bool isPlayer)
    {
        for (int i = 0; i < 4; i++)
        {
            var slot = isPlayer
                ? _battleMap.GetEnemySlot((Slot.SlotPosition)i)
                : _battleMap.GetPlayerSlot((Slot.SlotPosition)i);

            if (slot.IsOccupied)
            {
                slot.Card.CurrentDamage = Mathf.Max(0, slot.Card.GetCurrentDamage() - 2);
                GD.Print($"[Ability] Eve: {slot.Card.CardName} is now {slot.Card.CurrentDamage}");
            }
        }
    }

    // Give allies +2 stats
    private void ResolveAllAllyPlusStat(bool isPlayer)
    {
        for (int i = 0; i < 4; i++)
        {
            var slot = isPlayer
                ? _battleMap.GetPlayerSlot((Slot.SlotPosition)i)
                : _battleMap.GetEnemySlot((Slot.SlotPosition)i);

            if (slot.IsOccupied)
            {
                slot.Card.CurrentDamage = Mathf.Min(9, slot.Card.GetCurrentDamage() + 2);
                GD.Print($"[Ability] Croxy: {slot.Card.CardName} er nå {slot.Card.CurrentDamage}");
            }
        }
    }

    // Give +1 stat
    private void ResolveGivePlusOneStat(Slot targetSlot)
    {
        if (targetSlot.IsEmpty) return;
        targetSlot.Card.CurrentDamage = Mathf.Min(9, targetSlot.Card.GetCurrentDamage() + 1);
        GD.Print($"[Ability] Drake: +1 stats → {targetSlot.Card.CardName} er nå {targetSlot.Card.CurrentDamage}");
    }

    // Give a unit +2 stats
    private void ResolveGivePlusTwoStats(Slot targetSlot)
    {
        if (targetSlot.IsEmpty) return;
        targetSlot.Card.CurrentDamage = Mathf.Min(9, targetSlot.Card.GetCurrentDamage() + 2);
        GD.Print($"[Ability] +2 stats → {targetSlot.Card.CardName} is now {targetSlot.Card.CurrentDamage}");
    }

    // Give a unit -1 stat
    private void ResolveGiveMinusOneStat(Slot targetSlot)
    {
        if (targetSlot.IsEmpty) return;
        targetSlot.Card.CurrentDamage = Mathf.Max(0, targetSlot.Card.GetCurrentDamage() - 1);
        GD.Print($"[Ability] -1 stat → {targetSlot.Card.CardName} is now {targetSlot.Card.CurrentDamage}");
    }

    // Give -2 stats
    private void ResolveGiveMinusTwoStats(Slot targetSlot)
    {
        if (targetSlot.IsEmpty) return;
        targetSlot.Card.CurrentDamage = Mathf.Max(0, targetSlot.Card.GetCurrentDamage() - 2);
        GD.Print($"[Ability] -2 stats → {targetSlot.Card.CardName} is now {targetSlot.Card.CurrentDamage}");
    }

    // Draw a card
    private void ResolveDrawCard(bool isPlayer)
    {
        var data = isPlayer ? _player : _enemy;
        data.TryDrawCard();
        GD.Print($"[Ability] Drew a card");
    }

    // Draw two cards
    private void ResolveDrawTwoCards(bool isPlayer)
    {
        var data = isPlayer ? _player : _enemy;
        data.TryDrawCard();
        data.TryDrawCard();
        GD.Print($"[Ability] Drew two cards");
    }


    // Remove a unit
    private void ResolveRemoveUnit(Slot targetSlot)
    {
        if (targetSlot.IsEmpty) return;
        GD.Print($"[Ability] Removed unit: {targetSlot.Card.CardName}");
        targetSlot.RemoveCard();
    }

    // Remove unit, gain its stats (maks 9)
    private void ResolveRemoveGainStats(Slot targetSlot, bool isPlayer)
    {
        if (targetSlot.IsEmpty) return;

        int gained = targetSlot.Card.GetCurrentDamage();
        string cardName = targetSlot.Card.CardName;
        targetSlot.RemoveCard();

        for (int i = 0; i < 4; i++)
        {
            var slot = isPlayer
                ? _battleMap.GetPlayerSlot((Slot.SlotPosition)i)
                : _battleMap.GetEnemySlot((Slot.SlotPosition)i);

            if (slot.IsOccupied && slot.Card.Ability == CardData.AbilityType.RemoveGainStats)
            {
                slot.Card.CurrentDamage = Mathf.Min(9, slot.Card.GetCurrentDamage() + gained);
                GD.Print($"[Ability] Mio: removed {cardName}, got +{gained} stats → now {slot.Card.CurrentDamage}");
                break;
            }
        }
    }

    // + or - 3 stats
    public void ResolveDruid(Slot targetSlot, int amount)
    {
        if (targetSlot == null || targetSlot.IsEmpty) return;
        int newStat = Mathf.Clamp(targetSlot.Card.GetCurrentDamage() + amount, 0, 9);
        targetSlot.Card.CurrentDamage = newStat;
        GD.Print($"[Ability] Druid: {targetSlot.Card.CardName} er nå {newStat}");
        EmitSignal(SignalName.AbilityResolved, targetSlot.Card);
    }

    // Copy stat
    public bool HasLegalCopyStatTarget(bool isPlayer)
    {
        for (int i = 0; i < 4; i++)
        {
            var playerSlot = _battleMap.GetPlayerSlot((Slot.SlotPosition)i);
            var enemySlot = _battleMap.GetEnemySlot((Slot.SlotPosition)i);
            if (playerSlot.IsOccupied || enemySlot.IsOccupied) return true;
        }
        return false;
    }

    private void ResolveCopyStat(CardData card, Slot targetSlot, bool isPlayer)
    {
        if (targetSlot.IsEmpty) return;
        int copiedStat = targetSlot.Card.GetCurrentDamage();

        // Finn Cat på battlemap og sett stats
        for (int i = 0; i < 4; i++)
        {
            var slot = isPlayer
                ? _battleMap.GetPlayerSlot((Slot.SlotPosition)i)
                : _battleMap.GetEnemySlot((Slot.SlotPosition)i);

            if (slot.IsOccupied && slot.Card.Ability == CardData.AbilityType.CopyStat)
            {
                slot.Card.CurrentDamage = copiedStat;
                GD.Print($"[Ability] Cat: Kopierte {copiedStat} fra {targetSlot.Card.CardName}");
                break;
            }
        }
    }

    // Reset stats
    private void ResolveResetStat(Slot targetSlot)
    {
        if (targetSlot.IsEmpty) return;

        // Finn slot index og emit signal
        for (int i = 0; i < 4; i++)
        {
            if (_battleMap.GetPlayerSlot((Slot.SlotPosition)i) == targetSlot)
            {
                EmitSignal(SignalName.StatReset, i, true);
                break;
            }
            if (_battleMap.GetEnemySlot((Slot.SlotPosition)i) == targetSlot)
            {
                EmitSignal(SignalName.StatReset, i, false);
                break;
            }
        }

        targetSlot.Card.ResetCurrentDamage();
    }

    // Discard, draw
    private void ResolveDiscardDraw(CardData targetCard, bool isPlayer)
    {
        var data = isPlayer ? _player : _enemy;
        data.DiscardCard(targetCard);
        data.TryDrawCard();
        GD.Print($"[Ability] Watcher: discarded {targetCard.CardName}, drew a card");
    }

    // kast et kort, få dens stats (maks 9)
    private void ResolveDiscardGainStats(CardData Sludge, CardData targetCard, bool isPlayer)
    {
        var data = isPlayer ? _player : _enemy;
        int gained = targetCard.GetCurrentDamage();
        data.DiscardCard(targetCard);
        Sludge.CurrentDamage = Mathf.Min(9, Sludge.GetCurrentDamage() + gained);
        GD.Print($"[Ability] Sludge: discarded {targetCard.CardName}, gained +{gained} stats → now {Sludge.CurrentDamage}");
    }
    // Apply buff/debuff
    private void ResolveApplyPoison(Slot targetSlot)
    {
        if (targetSlot.IsEmpty) return;
        if (targetSlot.Card.HasStatus) return;
        targetSlot.Card.IsPoisoned = true;
        GD.Print($"[Ability] Poison: {targetSlot.Card.CardName} er nå poisoned");
    }

    private void ResolveApplyRage(Slot targetSlot)
    {
        if (targetSlot.IsEmpty) return;
        if (targetSlot.Card.HasStatus) return;
        targetSlot.Card.IsEnraged = true;
        GD.Print($"[Ability] Rage: {targetSlot.Card.CardName} er nå enraged");
    }

    // Deal 3 damage
    private void ResolveDealThreeDamage(bool isPlayer)
    {
        var target = isPlayer ? _enemy : _player;
        target.ReceiveDamage(3);
        GD.Print($"[Ability] DealThreeDamage: {target.PlayerName} tar 3 skade");
    }

    // Heal 3 damage

    private void ResolveHealThree(bool isPlayer)
    {
        var target = isPlayer ? _player : _enemy;
        target.TotalDamageReceived = Mathf.Max(0, target.TotalDamageReceived - 3);
        GD.Print($"[Ability] HealThree: {target.PlayerName} healer 3");
    }

    // Opponent discards

    private void ResolveOpponentDiscards(bool isPlayer)
    {
        GD.Print($"[Ability] ResolveOpponentDiscards: isPlayer={isPlayer}");
        var opponent = isPlayer ? _enemy : _player;
        GD.Print($"[Ability] Opponent har {opponent.HandCount} kort");
        if (!opponent.HasCardsInHand)
        {
            GD.Print("[Ability] OpponentDiscards fizzlet — ingen kort på hånd");
            return; // ← fizzle, turn-switch skjer normalt
        }
        _gameManager.TriggerOpponentDiscard(isPlayer);
        GD.Print($"[Ability] OpponentDiscards trigget");
    }
}
