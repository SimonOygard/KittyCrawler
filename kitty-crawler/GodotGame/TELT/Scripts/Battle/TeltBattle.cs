using Godot;
using System.Collections.Generic;

namespace KittyCrawler.TELT;

public partial class TeltBattle : Node2D
{
    // ── Node-referanser ───────────────────────────────────────────────
    [Export] private GameManager _gameManager;
    [Export] private BattleMap _battleMap;
    [Export] private AbilityResolver _abilityResolver;
    [Export] private PlayerData _player;
    [Export] private PlayerData _enemy;
    [Export] private EnemyAI _enemyAI;

    // ── UI ────────────────────────────────────────────────────────────
    [Export] private HBoxContainer _playerSlotsContainer;
    [Export] private HBoxContainer _enemySlotsContainer;
    [Export] private HBoxContainer _handContainer;
    [Export] private Label _phaseLabel;
    [Export] private Label _playerDamageLabel;
    [Export] private Label _enemyDamageLabel;
    [Export] private Button _oddButton;
    [Export] private Button _evenButton;
    [Export] private Label _resultLabel;
    [Export] private Button _combatButton;
    [Export] private Button _nextMatchButton;

    // ── Kortscene ─────────────────────────────────────────────────────
    [Export] private PackedScene _cardVisualScene;

    // ── Tilstand ──────────────────────────────────────────────────────
    private CardVisual _selectedCard = null;
    private List<CardVisual> _handVisuals = new();

    //---Target------
    private bool _waitingForTarget = false;
    private CardData _pendingAbilityCard = null;
    private Slot.SlotPosition _pendingCardPosition;
    private bool _waitingForHandTarget = false;
    private Slot _hildaFirstTarget = null;

    // ── Ready ─────────────────────────────────────────────────────────
    public override void _Ready()
    {
        GD.Print("TeltBattle _Ready kjører!");
        GD.Print($"OddButton er null: {_oddButton == null}");
        GD.Print($"EvenButton er null: {_evenButton == null}");
        GD.Print($"GameManager er null: {_gameManager == null}");
        // Koble GameManager signals
        _gameManager.PhaseChanged += OnPhaseChanged;
        _gameManager.TurnChanged += OnTurnChanged;
        _gameManager.MatchEnded += OnMatchEnded;
        _gameManager.GameOver += OnGameOver;
        _gameManager.ReadyForCombat += () =>
        {
            if (!_waitingForTarget && !_waitingForHandTarget)
                _combatButton.Visible = true;
        };
        _gameManager.BoardUpdated += () =>  // ← her
        {
            UpdateSlotVisuals();
            UpdateUI();
        };

        GD.Print("Signals koblet!");

        // Koble knapper
        _oddButton.Pressed += () => { GD.Print("Odd trykket!"); OnDiceChoice(true); };
        _evenButton.Pressed += () => { GD.Print("Even trykket!"); OnDiceChoice(false); };
        _combatButton.Pressed += OnCombatPressed;
        _nextMatchButton.Pressed += OnNextMatchPressed;
        _combatButton.Visible = false;
        _nextMatchButton.Visible = false;
        GD.Print("Knapper koblet!");

        // Koble slots
        ConnectSlotInputs();
        GD.Print("Slots koblet!");

        // Sett opp decks
        SetupDecks();
        GD.Print("Decks satt opp!");

        // Init GameManager
        _gameManager.Initialize(_player, _enemy, _battleMap);
        _enemyAI.Initialize(_enemy, _player, _battleMap, _gameManager, _abilityResolver);
        _abilityResolver.Initialize(_battleMap, _player, _enemy, _gameManager);

        GD.Print("GameManager initialisert!");

        UpdateUI();
        GD.Print("UI oppdatert!");
    }

    // ── Deck-oppsett ──────────────────────────────────────────────────
    private void SetupDecks()
    {
        var startDeck = new List<CardData>
        {
            // Commons (2x hver)
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Yeti.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Yeti.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Watcher.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Watcher.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Goblin.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Goblin.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Imp.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Imp.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Snake.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Snake.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Skeleton.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Skeleton.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Tortoise.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Tortoise.tres")).Duplicate() as CardData,
            // 1x Skester 🍓
            (GD.Load<CardData>("res://TELT/Resources/Cards/Rare_Skester.tres")).Duplicate() as CardData,
        };

        _player.SetDeck(startDeck);
        _player.ShuffleDeck();

        var enemyDeck = startDeck.ConvertAll(c => c.Duplicate() as CardData);
        _enemy.SetDeck(enemyDeck);
        _enemy.ShuffleDeck();
    }

    // ── Terningkast ───────────────────────────────────────────────────
    private void OnDiceChoice(bool pickedOdd)
    {
        _oddButton.Disabled = true;
        _evenButton.Disabled = true;
        _gameManager.RollDice(pickedOdd);
    }

    // --"videre" knapper
    private void OnCombatPressed()
    {
        _combatButton.Visible = false;
        _gameManager.TriggerWarPhase();
    }

    private void OnNextMatchPressed()
    {
        _nextMatchButton.Visible = false;
        _gameManager.TriggerNextMatch();
    }

    // ── Kortvalg fra hånd ─────────────────────────────────────────────
    private void OnCardClicked(CardVisual card)
    {
        GD.Print($"OnCardClicked: waitingForTarget={_waitingForTarget}, waitingForHand={_waitingForHandTarget}, currentTurn={_gameManager.CurrentTurn}");

        if (_waitingForHandTarget)
        {
            _abilityResolver.ResolveWithHandTarget(_pendingAbilityCard, card.CardData, true);
            RemoveCardFromHand(card);
            _waitingForTarget = false;
            _waitingForHandTarget = false;
            _pendingAbilityCard = null;
            _gameManager.HoldTurn = false;
            RefreshHandVisuals();
            UpdateSlotVisuals();
            UpdateUI();

            // Sjekk krigsfase FØR vi bytter tur
            if (_gameManager.CheckWarPhase())
            {
                _combatButton.Visible = true;
                return;
            }

            _gameManager.SwitchTurnPublic();
            return;
        }

        if (_waitingForTarget) return;
        if (_gameManager.CurrentTurn != GameManager.TurnOwner.Player) return;

        // Deselekter alltid forrige kort
        if (_selectedCard != null)
            _selectedCard.SetSelected(false);

        // Hvis man klikker samme kort, deselekter
        if (_selectedCard == card)
        {
            _selectedCard = null;
            return;
        }

        // Velg nytt kort
        _selectedCard = card;
        _selectedCard.SetSelected(true);
    }

    // ── Slot-klikk ────────────────────────────────────────────────────
    private void ConnectSlotInputs()
    {
        var positions = new[] {
            Slot.SlotPosition.Left,
            Slot.SlotPosition.Middle,
            Slot.SlotPosition.Right
        };

        for (int i = 0; i < _playerSlotsContainer.GetChildCount(); i++)
        {
            int index = i;
            var slot = _playerSlotsContainer.GetChild<Panel>(i);
            slot.GuiInput += (inputEvent) =>
            {
                if (_waitingForTarget)
                    OnTargetSlotClicked(inputEvent, positions[index], true);
                else
                    OnSlotClicked(inputEvent, positions[index]);
            };
        }

        for (int i = 0; i < _enemySlotsContainer.GetChildCount(); i++)
        {
            int index = i;
            var slot = _enemySlotsContainer.GetChild<Panel>(i);
            slot.GuiInput += (inputEvent) =>
            {
                if (_waitingForTarget)
                    OnTargetSlotClicked(inputEvent, positions[index], false);
                else if (_selectedCard?.CardData.Ability == CardData.AbilityType.AnySlot)
                    OnSkesterClicked(inputEvent, positions[index]);
            };
        }
    }
    private void OnSkesterClicked(InputEvent inputEvent, Slot.SlotPosition position)
    {
        if (_selectedCard == null) return;
        if (inputEvent is not InputEventMouseButton mouseEvent) return;
        if (!mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left) return;

        bool success = _gameManager.TryPlaceSkester(_selectedCard.CardData, position);

        if (success)
        {
            _player.TryPlayCard(_selectedCard.CardData);
            RemoveCardFromHand(_selectedCard);
            _selectedCard = null;
            UpdateSlotVisuals();
            UpdateUI();

            if (_gameManager.CheckWarPhase())
            {
                _combatButton.Visible = true;
                return;
            }

            // Regel 3.3: Hvis motstanders slots er fylt men spilleren har ledige slots,
            // fortsetter spillerens tur
            if (_battleMap.AllEnemySlotsFilled && _battleMap.PlayerEmptySlotCount > 0)
            {
                GD.Print("[Skester] Motstander fylt — spiller får ekstra tur");
                return; // Ikke bytt tur
            }

            _gameManager.SwitchTurnPublic();
        }
    }

    private bool HasLegalTargets(CardData card)
    {
        // Sjekk om det finnes noen opptatte slots
        for (int i = 0; i < 3; i++)
        {
            if (_battleMap.GetPlayerSlot((Slot.SlotPosition)i).IsOccupied) return true;
            if (_battleMap.GetEnemySlot((Slot.SlotPosition)i).IsOccupied) return true;
        }
        return false;
    }

    private void OnSlotClicked(InputEvent inputEvent, Slot.SlotPosition position)
{
    if (_selectedCard == null) return;
    if (inputEvent is not InputEventMouseButton mouseEvent) return;
    if (!mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left) return;

    // Sett HoldTurn FØR TryPlayCard hvis kortet trenger mål
    if (_abilityResolver.NeedsTarget(_selectedCard.CardData))
        _gameManager.HoldTurn = true;

    bool success = _gameManager.TryPlayCard(_selectedCard.CardData, position, GameManager.TurnOwner.Player);

    if (success)
    {
        if (!_abilityResolver.NeedsTarget(_selectedCard.CardData))
        {
            _abilityResolver.ResolveNoTarget(_selectedCard.CardData, position, true);
            RemoveCardFromHand(_selectedCard);
            _selectedCard = null;
            RefreshHandVisuals();
            UpdateSlotVisuals();
            UpdateUI();

            if (_gameManager.CheckWarPhase())
            {
                _combatButton.Visible = true;
                return;
            }
        }
        else
        {
            var targetType = _abilityResolver.GetTargetType(_selectedCard.CardData);

            if (targetType == AbilityResolver.TargetType.HandCard)
            {
                // Sjekk om det er kort på hånd å kaste
                if (!_player.HasCardsInHand)
                {
                    GD.Print($"[Ability] {_selectedCard.CardData.CardName} fizzlet — ingen kort på hånd");
                    _gameManager.HoldTurn = false;
                    RemoveCardFromHand(_selectedCard);
                    _selectedCard = null;
                    UpdateSlotVisuals();
                    UpdateUI();
                    _gameManager.SwitchTurnPublic();
                }
                else
                {
                    _waitingForTarget = true;
                    _waitingForHandTarget = true;
                    _pendingAbilityCard = _selectedCard.CardData;
                    _pendingCardPosition = position;
                    _phaseLabel.Text = "Velg et kort fra hånden!";
                    _combatButton.Visible = false; // ← legg til denne
                    RemoveCardFromHand(_selectedCard);
                    _selectedCard = null;
                    UpdateSlotVisuals();
                }
            }
            else
            {
                _waitingForTarget = true;
                _waitingForHandTarget = false;
                _pendingAbilityCard = _selectedCard.CardData;
                _pendingCardPosition = position;
                _phaseLabel.Text = "Velg et mål!";
                _combatButton.Visible = false; // ← legg til denne
                RemoveCardFromHand(_selectedCard);
                _selectedCard = null;
                UpdateSlotVisuals();
            }
        }
    }
    else
    {
        _gameManager.HoldTurn = false;
    }
}

    private void OnTargetSlotClicked(InputEvent inputEvent, Slot.SlotPosition position, bool isPlayerSlot)
    {
        if (inputEvent is not InputEventMouseButton mouseEvent) return;
        if (!mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left) return;

        var targetSlot = isPlayerSlot
            ? _battleMap.GetPlayerSlot(position)
            : _battleMap.GetEnemySlot(position);

        if (targetSlot.IsEmpty) return;

        if (_pendingAbilityCard.Ability == CardData.AbilityType.GiveMinusStat
            && isPlayerSlot
            && position == _pendingCardPosition)
        {
            GD.Print("[Ability] Skeleton kan ikke targete seg selv!");
            return;
        }

        // Hilda trenger to targets
        if (_pendingAbilityCard.Ability == CardData.AbilityType.SwitchSlots)
        {
            if (_hildaFirstTarget == null)
            {
                _hildaFirstTarget = targetSlot;
                _phaseLabel.Text = "Velg det andre kortet å bytte med!";
                return;
            }
            else
            {
                _abilityResolver.ResolveSwitchSlots(_hildaFirstTarget, targetSlot);
                _hildaFirstTarget = null;
            }
        }
        else
        {
            _abilityResolver.ResolveWithSlotTarget(_pendingAbilityCard, targetSlot, true);
        }

        _waitingForTarget = false;
        _pendingAbilityCard = null;
        _gameManager.HoldTurn = false;

        UpdateSlotVisuals();
        UpdateUI();

        if (_gameManager.CheckWarPhase() && !_waitingForTarget && !_waitingForHandTarget)
            _combatButton.Visible = true;

        _gameManager.SwitchTurnPublic();
    }

    // ── Hånd-visuals ──────────────────────────────────────────────────
    private void RefreshHandVisuals()
    {
        // Fjern gamle
        foreach (var visual in _handVisuals)
            visual.QueueFree();
        _handVisuals.Clear();

        // Lag nye
        foreach (var card in _player.GetHand())
        {
            var visual = _cardVisualScene.Instantiate<CardVisual>();
            _handContainer.AddChild(visual);
            visual.Setup(card);
            visual.CardClicked += OnCardClicked;
            _handVisuals.Add(visual);
        }
    }

    private void RemoveCardFromHand(CardVisual visual)
    {
        _handVisuals.Remove(visual);
        visual.QueueFree();
    }

    // ── Slot-visuals ──────────────────────────────────────────────────
    private void UpdateSlotVisuals()
    {
        var positions = new[] {
            Slot.SlotPosition.Left,
            Slot.SlotPosition.Middle,
            Slot.SlotPosition.Right
        };

        for (int i = 0; i < 3; i++)
        {
            var playerSlot = _battleMap.GetPlayerSlot(positions[i]);
            var playerPanel = _playerSlotsContainer.GetChild<Panel>(i);
            UpdateSlotPanel(playerPanel, playerSlot);

            var enemySlot = _battleMap.GetEnemySlot(positions[i]);
            var enemyPanel = _enemySlotsContainer.GetChild<Panel>(i);
            UpdateSlotPanel(enemyPanel, enemySlot);
        }
    }

    private void UpdateSlotPanel(Panel panel, Slot slot)
    {
        foreach (Node child in panel.GetChildren())
            child.QueueFree();

        if (slot.IsOccupied)
        {
            var visual = _cardVisualScene.Instantiate<CardVisual>();
            panel.AddChild(visual);
            visual.Setup(slot.Card);
            visual.SetPlayable(false);
            visual.SetAsSlotCard(); // ← bytt fra MouseFilter = Pass
        }
    }

    // ── UI-oppdatering ────────────────────────────────────────────────
    private void UpdateUI()
    {
        _playerDamageLabel.Text = $"Damage: {_player.TotalDamageReceived}";
        _enemyDamageLabel.Text = $"Damage: {_enemy.TotalDamageReceived}";
        _phaseLabel.Text = $"Match {_gameManager.CurrentMatch} — {_gameManager.CurrentPhase}";
    }

    // ── Signal-handlers ───────────────────────────────────────────────
    private void OnPhaseChanged(GameManager.GamePhase phase)
    {
        UpdateUI();

        if (phase == GameManager.GamePhase.PlayPhase)
        {
            RefreshHandVisuals();
            _combatButton.Visible = false;
            _nextMatchButton.Visible = false;
        }

        if (phase == GameManager.GamePhase.WarPhase)
        {
            _combatButton.Visible = false;
            _nextMatchButton.Visible = true;
        }

        UpdateSlotVisuals();
    }

    private void OnTurnChanged(GameManager.TurnOwner turn)
    {
        GD.Print($"OnTurnChanged: {turn}");
        UpdateUI();
        UpdateSlotVisuals();

        if (_battleMap.PlayerEmptySlotCount > 0 && _player.HasCardsInHand)
            _combatButton.Visible = false;

        if (turn == GameManager.TurnOwner.Enemy)
        {
            // CallDeferred unngår re-entrant signal-problemer
            Callable.From(() =>
            {
                _enemyAI.TakeTurn();
                UpdateSlotVisuals();
                if (_battleMap.PlayerEmptySlotCount > 0 && _player.HasCardsInHand)
                    _combatButton.Visible = false;
            }).CallDeferred();
        }
    }

    private void OnMatchEnded(int match, int playerDamage, int enemyDamage)
    {
        _resultLabel.Text = $"Match {match}: Du tok {playerDamage} — Fiende tok {enemyDamage}";
        UpdateSlotVisuals();
    }

    private void OnGameOver(GameManager.TurnOwner winner, int playerDamage, int enemyDamage)
    {
        // Fjern visuelle kort fra hånd
        foreach (var visual in _handVisuals)
            visual.QueueFree();
        _handVisuals.Clear();

        string winnerText = winner == GameManager.TurnOwner.Player ? "DU VANT! 🎉" : "Du tapte 😢";
        _resultLabel.Text = $"{winnerText} | Du: {playerDamage} — Fiende: {enemyDamage}";
        _phaseLabel.Text = "SPILLET ER FERDIG";
    }


}
