using Godot;
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
            CardData.AbilityType.DrawCard => false,
            CardData.AbilityType.AllEnemyMinusStat => false,
            CardData.AbilityType.SwitchSlots => true,
            CardData.AbilityType.AnySlot          => false,
            _                                     => true
        };
    }

    public TargetType GetTargetType(CardData card)
    {
        return card.Ability switch
        {
            CardData.AbilityType.GivePlusStat    => TargetType.AnySlot,
            CardData.AbilityType.GiveMinusStat   => TargetType.AnySlot,
            CardData.AbilityType.RemoveUnit      => TargetType.AnySlot,
            CardData.AbilityType.SwitchSlots     => TargetType.AnySlot,
            CardData.AbilityType.RemoveGainStats => TargetType.AnySlot,
            CardData.AbilityType.DiscardDraw     => TargetType.HandCard,
            CardData.AbilityType.DiscardGainStats => TargetType.HandCard,
            _                                    => TargetType.None
        };
    }

    // ── Resolve uten mål ─────────────────────────────────────────────
    public void ResolveNoTarget(CardData card, Slot.SlotPosition placedAt, bool isPlayer)
    {
        switch (card.Ability)
        {
            case CardData.AbilityType.DrawCard:
                ResolveDrawCard(isPlayer);
                break;
            case CardData.AbilityType.AllEnemyMinusStat:
                ResolveAllEnemyMinusStat(isPlayer);
                break;
            case CardData.AbilityType.None:
            case CardData.AbilityType.AnySlot:
                break;
        }

        EmitSignal(SignalName.AbilityResolved, card);
    }

    // ── Resolve med mål ───────────────────────────────────────────────
    public void ResolveWithSlotTarget(CardData card, Slot targetSlot, bool isPlayer)
    {
        switch (card.Ability)
        {
            case CardData.AbilityType.GivePlusStat:
                ResolveGivePlusStat(targetSlot);
                break;
            case CardData.AbilityType.GiveMinusStat:
                ResolveGiveMinusStat(targetSlot);
                break;
            case CardData.AbilityType.RemoveUnit:
                ResolveRemoveUnit(targetSlot);
                break;
            case CardData.AbilityType.RemoveGainStats:
                ResolveRemoveGainStats(targetSlot, isPlayer);
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

    // Tortoise: ingen damage kan overstige dette kortets stats i samme lane
    // Denne kjøres direkte i BattleMap.ResolveWar

    // Eve: motstander MÅ fylle alle sine åpne slots
    // Ny metode:
    private void ResolveAllEnemyMinusStat(bool isPlayer)
    {
        for (int i = 0; i < 3; i++)
        {
            var slot = isPlayer
                ? _battleMap.GetEnemySlot((Slot.SlotPosition)i)
                : _battleMap.GetPlayerSlot((Slot.SlotPosition)i);

            if (slot.IsOccupied)
            {
                slot.Card.CurrentDamage = Mathf.Max(0, slot.Card.GetCurrentDamage() - 2);
                GD.Print($"[Ability] Eve: {slot.Card.CardName} er nå {slot.Card.CurrentDamage}");
            }
        }
    }

    // Drake: gi en unit +2 stats (maks 9)
    private void ResolveGivePlusStat(Slot targetSlot)
    {
        if (targetSlot.IsEmpty) return;
        targetSlot.Card.CurrentDamage = Mathf.Min(9, targetSlot.Card.GetCurrentDamage() + 2);
        GD.Print($"[Ability] Drake: +2 stats → {targetSlot.Card.CardName} er nå {targetSlot.Card.CurrentDamage}");
    }

    private void ResolveDrawCard(bool isPlayer)
    {
        var data = isPlayer ? _player : _enemy;
        data.TryDrawCard();
        GD.Print($"[Ability] Golem: Trakk et kort");
    }

// Skeleton: gi en unit -1 stats (min 0)
    private void ResolveGiveMinusStat(Slot targetSlot)
    {
        if (targetSlot.IsEmpty) return;
        targetSlot.Card.CurrentDamage = Mathf.Max(0, targetSlot.Card.GetCurrentDamage() - 1);
        GD.Print($"[Ability] Skeleton: -1 stats → {targetSlot.Card.CardName} er nå {targetSlot.Card.CurrentDamage}");
    }

    // Druid: fjern en hvilken som helst unit
    private void ResolveRemoveUnit(Slot targetSlot)
    {
        if (targetSlot.IsEmpty) return;
        GD.Print($"[Ability] Druid: Fjernet {targetSlot.Card.CardName}");
        targetSlot.RemoveCard();
    }

    // Mio: fjern en unit, få dens stats (maks 9)
    private void ResolveRemoveGainStats(Slot targetSlot, bool isPlayer)
    {
        if (targetSlot.IsEmpty) return;

        int gained = targetSlot.Card.GetCurrentDamage();
        string cardName = targetSlot.Card.CardName;
        targetSlot.RemoveCard();

        for (int i = 0; i < 3; i++)
        {
            var slot = isPlayer
                ? _battleMap.GetPlayerSlot((Slot.SlotPosition)i)
                : _battleMap.GetEnemySlot((Slot.SlotPosition)i);

            if (slot.IsOccupied && slot.Card.Ability == CardData.AbilityType.RemoveGainStats)
            {
                slot.Card.CurrentDamage = Mathf.Min(9, slot.Card.GetCurrentDamage() + gained);
                GD.Print($"[Ability] Mio: Fjernet {cardName}, fikk +{gained} stats → nå {slot.Card.CurrentDamage}");
                break;
            }
        }
    }
    // Golem: kast et kort fra hånd, trekk et nytt
    private void ResolveDiscardDraw(CardData targetCard, bool isPlayer)
    {
        var data = isPlayer ? _player : _enemy;
        data.DiscardCard(targetCard);
        data.TryDrawCard();
        GD.Print($"[Ability] Watcher: Kastet {targetCard.CardName}, trakk nytt kort");
    }

    // Croxy: kast et kort, få dens stats (maks 9)
    private void ResolveDiscardGainStats(CardData croxy, CardData targetCard, bool isPlayer)
    {
        var data = isPlayer ? _player : _enemy;
        int gained = targetCard.GetCurrentDamage();
        data.DiscardCard(targetCard);
        croxy.CurrentDamage = Mathf.Min(9, croxy.GetCurrentDamage() + gained);
        GD.Print($"[Ability] Croxy: Kastet {targetCard.CardName}, fikk +{gained} stats → nå {croxy.CurrentDamage}");
    }
}
