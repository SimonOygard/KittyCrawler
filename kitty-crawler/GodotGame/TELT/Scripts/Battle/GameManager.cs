using Godot;
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
    private const int TotalMatches = 3;

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
        int cardsToDraw = CurrentMatch == 1 ? 5 : 3;

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

        // Sjekk om krigsfase skal starte
        if (_battleMap.ShouldStartWarPhase(_player.HasCardsInHand, _enemy.HasCardsInHand))
        {
            EmitSignal(SignalName.ReadyForCombat);
            return true; // Vent på spilleren
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

        SetPhase(GamePhase.CleanupPhase);
        ExecuteCleanup();
    }

    // ── Opprydding ────────────────────────────────────────────────────
    private void ExecuteCleanup()
    {
        // Alle kort fra battlemap går i discard
        var playerCards = _battleMap.CollectPlayerCards();
        var enemyCards = _battleMap.CollectEnemyCards();

        _player.CollectBattlemapCards(playerCards);
        _enemy.CollectBattlemapCards(enemyCards);

        GD.Print($"Opprydding fullført etter match {CurrentMatch}");

        if (CurrentMatch >= TotalMatches)
        {
            GD.Print($"Spiller hånd før discard: {_player.HandCount}");
            _player.DiscardHand();
            _enemy.DiscardHand();
            GD.Print($"Spiller hånd etter discard: {_player.HandCount}");
            SetPhase(GamePhase.GameOver);
            ExecuteGameOver();
            return;
        }

        // Neste match
        CurrentMatch++;

        // Bytt startspiller
        MatchStarter = MatchStarter == TurnOwner.Player
            ? TurnOwner.Enemy
            : TurnOwner.Player;
        CurrentTurn = MatchStarter;

        GD.Print($"=== MATCH {CurrentMatch} starter ===");
        SetPhase(GamePhase.DrawPhase);
        ExecuteDrawPhase();


    }
    //---bytt tur--
    public void SwitchTurnPublic()
    {
        SwitchTurn();
    }

    // ── Game Over ─────────────────────────────────────────────────────
    private void ExecuteGameOver()
    {
        TurnOwner winner = _player.TotalDamageReceived <= _enemy.TotalDamageReceived
            ? TurnOwner.Player
            : TurnOwner.Enemy;

        GD.Print($"=== SPILLET ER FERDIG ===");
        GD.Print($"Spiller: {_player.TotalDamageReceived} damage");
        GD.Print($"Fiende:  {_enemy.TotalDamageReceived} damage");
        GD.Print($"Vinner: {winner}");

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

        EmitSignal(SignalName.TurnChanged, (int)CurrentTurn);
        GD.Print($"Tur: {CurrentTurn}");
    }

    private void SetPhase(GamePhase phase)
    {
        CurrentPhase = phase;
        EmitSignal(SignalName.PhaseChanged, (int)phase);
        GD.Print($"Fase: {phase}");
    }
}
