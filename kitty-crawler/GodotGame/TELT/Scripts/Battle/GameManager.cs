using Godot;
using System;
using KittyCrawler.scripts;
using System.Collections.Generic;

namespace KittyCrawler.TELT;

public partial class GameManager : Node
{
    // ── Fase ──────────────────────────────────────────────────────────
    public enum GamePhase
    {
        DiceRoll,
        DrawPhase,
        PlayPhase,
        WarPhase,
        CleanupPhase,
        GameOver
    }

    public enum TurnOwner { Player, Enemy }

    // ── Tilstand ──────────────────────────────────────────────────────
    public GamePhase CurrentPhase { get; private set; } = GamePhase.DiceRoll;
    public TurnOwner CurrentTurn { get; private set; } = TurnOwner.Player;
    public TurnOwner MatchStarter { get; private set; } = TurnOwner.Player;
    public int CurrentMatch { get; private set; } = 1;
    private const int TotalMatches = 4;

    // ── Referanser ────────────────────────────────────────────────────
    private PlayerData _player;
    private PlayerData _enemy;
    private BattleMap _battleMap;

    // ── Signals ───────────────────────────────────────────────────────
    [Signal] public delegate void PhaseChangedEventHandler(GamePhase newPhase);
    [Signal] public delegate void TurnChangedEventHandler(TurnOwner turn);
    [Signal] public delegate void MatchEndedEventHandler(int matchNumber, int playerDamage, int enemyDamage);
    [Signal] public delegate void GameOverEventHandler(TurnOwner winner, int playerDamage, int enemyDamage);
    [Signal] public delegate void ReadyForCombatEventHandler();
    [Signal] public delegate void BoardUpdatedEventHandler();
    [Signal] public delegate void StatTickedEventHandler(int slotIndex, bool isPlayerSlot, bool isPositive);

    // ── Init ──────────────────────────────────────────────────────────
    public void Initialize(PlayerData player, PlayerData enemy, BattleMap battleMap)
    {
        _player = player;
        _enemy = enemy;
        _battleMap = battleMap;
    }

    // ── Terningkast ───────────────────────────────────────────────────
    public void RollDice(bool playerPickedOdd)
    {
        if (CurrentPhase != GamePhase.DiceRoll) return;

        int roll = DiceRoll.Instance.RollDice(6);
        bool isOdd = roll % 2 != 0;
        bool playerWins = playerPickedOdd == isOdd;

        GD.Print($"Terning: {roll} ({(isOdd ? "odd" : "even")}) → {(playerWins ? "Spiller" : "Fiende")} starter");

        MatchStarter = playerWins ? TurnOwner.Player : TurnOwner.Enemy;
        CurrentTurn = MatchStarter;

        SetPhase(GamePhase.DrawPhase);
        ExecuteDrawPhase();
    }

    // ── Trekk-fase ────────────────────────────────────────────────────
    private void ExecuteDrawPhase()
    {
        int cardsToDraw = CurrentMatch == 1 ? 5 : 4;

        _player.DrawCards(cardsToDraw);
        _enemy.DrawCards(cardsToDraw);

        GD.Print($"Match {CurrentMatch}: Begge trekker {cardsToDraw} kort");

        SetPhase(GamePhase.PlayPhase);
        EmitSignal(SignalName.TurnChanged, (int)CurrentTurn); // ← legg til denne

    }

    // ── Spill-fase ────────────────────────────────────────────────────
    public bool TryPlayCard(CardData card, Slot.SlotPosition position, TurnOwner owner)
    {
        if (CurrentPhase != GamePhase.PlayPhase) return false;
        if (CurrentTurn != owner) return false;

        bool placed = owner == TurnOwner.Player
            ? _battleMap.TryPlacePlayerCard(card, position)
            : _battleMap.TryPlaceEnemyCard(card, position);

        if (!placed) return false;

        // Fjern kortet fra hånd
        if (owner == TurnOwner.Player)
            _player.TryPlayCard(card);
        else
            _enemy.TryPlayCard(card);

        GD.Print($"{owner} spilte {card.CardName} i {position}-slot");
        _battleMap.PrintState();
        EmitSignal(SignalName.BoardUpdated);


        // Sjekk om krigsfase skal starte
        if (_battleMap.ShouldStartWarPhase(_player.HasCardsInHand, _enemy.HasCardsInHand))
        {
            EmitSignal(SignalName.ReadyForCombat);
            return true;
        }

        // Regel 3.3: Hvis motstander har fylt sine slots, får aktiv spiller ny tur
        bool opponentFull = owner == TurnOwner.Player
            ? _battleMap.AllEnemySlotsFilled
            : _battleMap.AllPlayerSlotsFilled;

        if (!opponentFull)
            SwitchTurn();

        return true;
    }

    public bool TryPlaceSkester(CardData card, Slot.SlotPosition position)
    {
        if (CurrentPhase != GamePhase.PlayPhase) return false;
        return _battleMap.TryPlaceEnemyCard(card, position);
    }

    //---bytt fase
    public void TriggerWarPhase()
    {
        SetPhase(GamePhase.WarPhase);
        ExecuteWarPhase();
    }

    public void TriggerNextMatch()
    {
        SetPhase(GamePhase.CleanupPhase);
        ExecuteCleanup();
    }

    // ── Krig-fase ─────────────────────────────────────────────────────
    public bool CheckWarPhase()
    {
        return _battleMap.ShouldStartWarPhase(_player.HasCardsInHand, _enemy.HasCardsInHand);
    }
    private void ExecuteWarPhase()
    {
        GD.Print($"=== KRIGSFASE - Match {CurrentMatch} ===");
        _battleMap.PrintState();

        var (playerDamage, enemyDamage) = _battleMap.ResolveWar();

        _player.ReceiveDamage(playerDamage);
        _enemy.ReceiveDamage(enemyDamage);

        GD.Print($"Spiller tar {playerDamage} damage (totalt: {_player.TotalDamageReceived})");
        GD.Print($"Fiende tar {enemyDamage} damage (totalt: {_enemy.TotalDamageReceived})");

        EmitSignal(SignalName.MatchEnded, CurrentMatch, playerDamage, enemyDamage);
    }

    // ── Opprydding ────────────────────────────────────────────────────
    private void ExecuteCleanup()
    {
        GD.Print("ExecuteCleanup starter");
        var playerCards = _battleMap.CollectPlayerCards();
        var enemyCards = _battleMap.CollectEnemyCards();
        GD.Print("Kort samlet");

        _player.CollectBattlemapCards(playerCards);
        _enemy.CollectBattlemapCards(enemyCards);
        GD.Print($"Opprydding fullført etter match {CurrentMatch}");

        if (CurrentMatch >= TotalMatches)
        {
            GD.Print("GameOver trigges");
            _player.DiscardHand();
            _enemy.DiscardHand();
            SetPhase(GamePhase.GameOver);
            ExecuteGameOver();
            return;
        }

        CurrentMatch++;
        MatchStarter = MatchStarter == TurnOwner.Player ? TurnOwner.Enemy : TurnOwner.Player;
        CurrentTurn = MatchStarter;

        GD.Print($"=== MATCH {CurrentMatch} starter ===");
        GD.Print("Setter DrawPhase...");
        SetPhase(GamePhase.DrawPhase);
        GD.Print("DrawPhase satt, starter ExecuteDrawPhase");
        ExecuteDrawPhase();
        GD.Print("ExecuteDrawPhase ferdig");
    }
    //---bytt tur--
    public void SwitchTurnPublic()
    {
        SwitchTurn();
    }

    // ── Game Over ─────────────────────────────────────────────────────
    private void ExecuteGameOver()
    {
        bool isDraw = _player.TotalDamageReceived == _enemy.TotalDamageReceived;

        TurnOwner winner;
        if (isDraw)
            winner = TurnOwner.Enemy;
        else
            winner = _player.TotalDamageReceived < _enemy.TotalDamageReceived
                ? TurnOwner.Player
                : TurnOwner.Enemy;

        if (winner == TurnOwner.Player)
            PlayerData.AddDamageDealt(_enemy.TotalDamageReceived);

        GD.Print($"=== SPILLET ER FERDIG ===");
        GD.Print($"Spiller: {_player.TotalDamageReceived} damage");
        GD.Print($"Fiende:  {_enemy.TotalDamageReceived} damage");
        GD.Print(isDraw ? "Uavgjort — spiller vinner!" : $"Vinner: {winner}");
        EmitSignal(SignalName.GameOver, (int)winner, _player.TotalDamageReceived, _enemy.TotalDamageReceived);
    }

    // ── Hjelpemetoder ─────────────────────────────────────────────────
    public bool HoldTurn { get; set; } = false;


    private void SwitchTurn()
    {
        if (HoldTurn) return;

        CurrentTurn = CurrentTurn == TurnOwner.Player
            ? TurnOwner.Enemy
            : TurnOwner.Player;

        EmitSignal(SignalName.TurnChanged, (int)CurrentTurn); // ← først, trigger UpdateSlotVisuals
        TickPoisonAndRage(); // ← så tick, nye noder er nå klare
        GD.Print($"Tur: {CurrentTurn}");
    }

    private void SetPhase(GamePhase phase)
    {
        CurrentPhase = phase;
        EmitSignal(SignalName.PhaseChanged, (int)phase);
        GD.Print($"Fase: {phase}");
    }

    // Tick buff/debuff
    public void TickPoisonAndRage()
    {
        GD.Print($"Emitter StatTicked: slot {"i"}, isPlayer={true}, positive={false}");
        // Tick kun på aktiv spillers egne kort
        for (int i = 0; i < 4; i++)
        {
            if (CurrentTurn == TurnOwner.Player)
            {
                var playerSlot = _battleMap.GetPlayerSlot((Slot.SlotPosition)i);
                if (playerSlot.IsOccupied && playerSlot.Card.IsPoisoned)
                {
                    playerSlot.Card.CurrentDamage = Mathf.Max(0, playerSlot.Card.GetCurrentDamage() - 1);
                    EmitSignal(SignalName.StatTicked, i, true, false);
                    GD.Print($"[Poison] {playerSlot.Card.CardName} tikket ned til {playerSlot.Card.CurrentDamage}");
                }
                if (playerSlot.IsOccupied && playerSlot.Card.IsEnraged)
                {
                    playerSlot.Card.CurrentDamage = Mathf.Min(9, playerSlot.Card.GetCurrentDamage() + 1);
                    EmitSignal(SignalName.StatTicked, i, true, true);
                    GD.Print($"[Rage] {playerSlot.Card.CardName} tikket opp til {playerSlot.Card.CurrentDamage}");
                }
            }
            else
            {
                var enemySlot = _battleMap.GetEnemySlot((Slot.SlotPosition)i);
                if (enemySlot.IsOccupied && enemySlot.Card.IsPoisoned)
                {
                    enemySlot.Card.CurrentDamage = Mathf.Max(0, enemySlot.Card.GetCurrentDamage() - 1);
                    EmitSignal(SignalName.StatTicked, i, false, false);
                    GD.Print($"[Poison] {enemySlot.Card.CardName} tikket ned til {enemySlot.Card.CurrentDamage}");
                }
                if (enemySlot.IsOccupied && enemySlot.Card.IsEnraged)
                {
                    enemySlot.Card.CurrentDamage = Mathf.Min(9, enemySlot.Card.GetCurrentDamage() + 1);
                    EmitSignal(SignalName.StatTicked, i, false, true);
                    GD.Print($"[Rage] {enemySlot.Card.CardName} tikket opp til {enemySlot.Card.CurrentDamage}");
                }
            }
        }
    }

    public void EmitReadyForCombat()
    {
        EmitSignal(SignalName.ReadyForCombat);
    }
}
