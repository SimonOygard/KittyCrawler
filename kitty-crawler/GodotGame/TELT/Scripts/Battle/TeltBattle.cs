using Godot;
using System.Collections.Generic;
using System.Linq;

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
    [Export] private CanvasLayer _diceRollOverlay;
    [Export] private CanvasLayer _ui;
    [Export] private HBoxContainer _enemyHandDisplay;
    [Export] private Texture2D _cardBackTexture;
    [Export] private Panel _playerDeckDisplay;
    [Export] private Label _deckCountLabel;
    [Export] private Panel _discardPile;
    [Export] private TextureRect _discardTopCard;
    [Export] private CanvasLayer _discardSheet;
    [Export] private GridContainer _discardGrid;
    [Export] private Button _closeDiscardButton;
    [Export] private Label _diceResultLabel;
    [Export] private Label _diceTitle;


    // ── Kortscene ─────────────────────────────────────────────────────
    [Export] private PackedScene _cardVisualScene;

    // ── Bakgrunn ─────────────────────────────────────────────────────
    [Export] private TextureRect _background;

// Teksturer for hver bakgrunn
    [Export] private Texture2D _basicBackground;
    [Export] private Texture2D _croxBackground;
    [Export] private Texture2D _eveBackground;
    [Export] private Texture2D _hildaBackground;
    [Export] private Texture2D _mioBackground;
    [Export] private Texture2D _skesterBackground;

    public void SetBackground(Texture2D texture)
    {
        if (_background != null)
            _background.Texture = texture;
    }


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

        _deckCountLabel.Visible = false;
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
        _gameManager.BoardUpdated += () => // ← her
        {
            UpdateSlotVisuals();
            UpdateUI();
            UpdateEnemyHandDisplay();
            UpdateDeckDisplay();
            UpdateDiscardPile();
        };

        GD.Print("Signals koblet!");

        // Skjul vanlig UI til terningkast er gjort
        _diceRollOverlay.Visible = true;

        // Koble knapper
        _oddButton.Pressed += () =>
        {
            GD.Print("Odd trykket!");
            OnDiceChoice(true);
        };
        _evenButton.Pressed += () =>
        {
            GD.Print("Even trykket!");
            OnDiceChoice(false);
        };
        _combatButton.Pressed += OnCombatPressed;
        _nextMatchButton.Pressed += OnNextMatchPressed;
        _combatButton.Visible = false;
        _nextMatchButton.Visible = false;
        GD.Print("Knapper koblet!");

        _ui.Visible = false;

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

        _discardPile.GuiInput += (inputEvent) =>
        {
            if (inputEvent is InputEventMouseButton mouseEvent
                && mouseEvent.Pressed
                && mouseEvent.ButtonIndex == MouseButton.Left)
                ShowDiscardSheet();
        };
        _closeDiscardButton.Pressed += () => _discardSheet.Visible = false;
        _discardSheet.Visible = false;

        UpdateUI();
        UpdateEnemyHandDisplay();
        UpdateDeckDisplay();
        UpdateDiscardPile();
        SetBackground(_basicBackground);
        GD.Print("UI oppdatert!");

        _playerDeckDisplay.MouseEntered += () => _deckCountLabel.Visible = true;
        _playerDeckDisplay.MouseExited += () => _deckCountLabel.Visible = false;

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
        _oddButton.Visible = false;
        _evenButton.Visible = false;
        _diceTitle.Visible = false;

        _gameManager.RollDice(pickedOdd);

        // Vis resultat
        bool playerStarts = _gameManager.MatchStarter == GameManager.TurnOwner.Player;
        _diceResultLabel.Text = playerStarts ? "You go first!" : "Opponent starts.";
        _diceResultLabel.Visible = true;

        // Vent 1 sekund så start spillet
        GetTree().CreateTimer(1.3f).Timeout += () =>
        {
            _diceRollOverlay.Visible = false;
            _ui.Visible = true;
            SetBackground(_basicBackground);
            _oddButton.Disabled = true;
            _evenButton.Disabled = true;
        };
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
        GD.Print(
            $"OnCardClicked: waitingForTarget={_waitingForTarget}, waitingForHand={_waitingForHandTarget}, currentTurn={_gameManager.CurrentTurn}");

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
            UpdateEnemyHandDisplay();
            UpdateDeckDisplay();
            UpdateDiscardPile();

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
        var positions = new[]
        {
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
            UpdateEnemyHandDisplay();
            UpdateDeckDisplay();
            UpdateDiscardPile();

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
                UpdateEnemyHandDisplay();
                UpdateDeckDisplay();
                UpdateDiscardPile();

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
                        UpdateEnemyHandDisplay();
                        UpdateDeckDisplay();
                        UpdateDiscardPile();
                        _gameManager.SwitchTurnPublic();
                    }
                    else
                    {
                        _waitingForTarget = true;
                        _waitingForHandTarget = true;
                        _pendingAbilityCard = _selectedCard.CardData;
                        _pendingCardPosition = position;
                        _phaseLabel.Text = "Discard a card";
                        _combatButton.Visible = false; // ← legg til denne
                        RemoveCardFromHand(_selectedCard);
                        _selectedCard = null;
                        UpdateSlotVisuals();
                        HighlightHandCards();
                    }
                }
                else
                {
                    _waitingForTarget = true;
                    _waitingForHandTarget = false;
                    _pendingAbilityCard = _selectedCard.CardData;
                    _pendingCardPosition = position;
                    _phaseLabel.Text = "Select target";
                    _combatButton.Visible = false; // ← legg til denne
                    RemoveCardFromHand(_selectedCard);
                    _selectedCard = null;
                    UpdateSlotVisuals();
                    HighlightLegalTargets(_pendingAbilityCard);
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
                _phaseLabel.Text = "Choose a card to swap";
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
        ClearAllHighlights();

        UpdateSlotVisuals();
        UpdateUI();
        UpdateEnemyHandDisplay();
        UpdateDeckDisplay();
        UpdateDiscardPile();

        if (_gameManager.CheckWarPhase() && !_waitingForTarget && !_waitingForHandTarget)
            _combatButton.Visible = true;

        _gameManager.SwitchTurnPublic();
    }

    // ── Hånd-visuals ──────────────────────────────────────────────────
    private void RefreshHandVisuals()
    {
        foreach (var visual in _handVisuals)
            visual.QueueFree();
        _handVisuals.Clear();

        var hand = _player.GetHand();
        var newCards = _player.NewlyDrawnCards;
        int animIndex = 0;

        for (int i = 0; i < hand.Count; i++)
        {
            var card = hand[i];
            var visual = _cardVisualScene.Instantiate<CardVisual>();
            _handContainer.AddChild(visual);
            visual.Setup(card);
            visual.CardClicked += OnCardClicked;
            _handVisuals.Add(visual);

            if (newCards.Contains(card))
            {
                visual.Modulate = new Color(1, 1, 1, 0);
                visual.Position += new Vector2(0, 50);

                var tween = visual.CreateTween();
                tween.SetParallel(true);
                tween.TweenProperty(visual, "modulate:a", 1.0f, 0.3f)
                    .SetDelay(animIndex * 0.5f);
                tween.TweenProperty(visual, "position:y", visual.Position.Y - 50, 0.3f)
                    .SetDelay(animIndex * 0.5f)
                    .SetTrans(Tween.TransitionType.Quad)
                    .SetEase(Tween.EaseType.Out);
                animIndex++;
            }
        }

        _player.ClearLastDrawnCard();
    }

    private void AddCardToHandAnimated(CardData card)
    {
        var visual = _cardVisualScene.Instantiate<CardVisual>();
        _handContainer.AddChild(visual);
        visual.Setup(card);
        visual.CardClicked += OnCardClicked;
        _handVisuals.Add(visual);

        // Animer inn
        visual.Modulate = new Color(1, 1, 1, 0);
        visual.Position += new Vector2(0, 50);

        var tween = visual.CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(visual, "modulate:a", 1.0f, 0.3f);
        tween.TweenProperty(visual, "position:y", visual.Position.Y - 50, 0.3f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
    }

    private void RemoveCardFromHand(CardVisual visual)
    {
        _handVisuals.Remove(visual);
        visual.QueueFree();
    }

    // ── Slot-visuals ──────────────────────────────────────────────────
    private void UpdateSlotVisuals()
    {
        var positions = new[]
        {
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
        RefreshHighlights();
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
            visual.SetAsSlotCard(); // ← bytt fra MouseFilter = Pass
        }
    }

    // ── UI-oppdatering ────────────────────────────────────────────────
    private void UpdateUI()
    {
        _playerDamageLabel.Text = $"{_player.TotalDamageReceived}";
        _enemyDamageLabel.Text = $"{_enemy.TotalDamageReceived}";
        _phaseLabel.Text = $"Match {_gameManager.CurrentMatch} —\n{_gameManager.CurrentPhase}";
    }

    private Color GetDamageColor(int damage)
    {
        if (damage == 0) return Colors.White;
        if (damage <= 5) return Colors.Yellow;
        return Colors.Red;
    }

    // ── Signal-handlers ───────────────────────────────────────────────
    private void OnPhaseChanged(GameManager.GamePhase phase)
    {
        UpdateUI();
        UpdateEnemyHandDisplay();
        UpdateDeckDisplay();
        UpdateDiscardPile();

        if (phase == GameManager.GamePhase.PlayPhase)
        {
            if (_gameManager.CurrentMatch == 1)
            {
                // Vent litt før vi viser kortene i match 1
                GetTree().CreateTimer(1.8f).Timeout += () =>
                {
                    RefreshHandVisuals();
                };
            }
            else
            {
                RefreshHandVisuals();
            }
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
        UpdateEnemyHandDisplay();
        UpdateSlotVisuals();

        if (_battleMap.PlayerEmptySlotCount > 0 && _player.HasCardsInHand)
            _combatButton.Visible = false;

        if (turn == GameManager.TurnOwner.Enemy)
        {
            bool isVeryFirstTurn = _gameManager.CurrentMatch == 1
                                   && _battleMap.PlayerEmptySlotCount == 3
                                   && _battleMap.EnemyEmptySlotCount == 3;

            float delay = isVeryFirstTurn ? 5.0f : 0.6f;

            Callable.From(() =>
            {
                var timer = GetTree().CreateTimer(delay);
                timer.Timeout += () =>
                {
                    _enemyAI.TakeTurn();
                    UpdateSlotVisuals();
                    if (_battleMap.PlayerEmptySlotCount > 0 && _player.HasCardsInHand)
                        _combatButton.Visible = false;
                };
            }).CallDeferred();
        }
    }

    private void OnMatchEnded(int match, int playerDamage, int enemyDamage)
    {
        _resultLabel.Text = $"Match {match}:\nYou take {playerDamage} damage — Enemy take {enemyDamage}";
        UpdateSlotVisuals();

        FlashDamageLabel(_playerDamageLabel, playerDamage);
        FlashDamageLabel(_enemyDamageLabel, enemyDamage);
    }

    private void OnGameOver(GameManager.TurnOwner winner, int playerDamage, int enemyDamage)
    {
        // Fjern visuelle kort fra hånd
        foreach (var visual in _handVisuals)
            visual.QueueFree();
        _handVisuals.Clear();

        string winnerText = winner == GameManager.TurnOwner.Player ? "YOU WON!\n" : "YOU LOSE\n";
        _resultLabel.Text = $"{winnerText} | You: {playerDamage} — Opponent: {enemyDamage}";
        _phaseLabel.Text = "Game Over";
        PlayerData.DefeatNpc("npc_goblin_king", _enemy.TotalDamageReceived);
    }

    private async void FlashDamageLabel(Label label, int damageTaken)
    {
        if (damageTaken <= 0) return;

        label.AddThemeColorOverride("font_color", Colors.Red);
        await ToSignal(GetTree().CreateTimer(.35f), "timeout");
        label.AddThemeColorOverride("font_color", Colors.Black);
    }

    private void UpdateEnemyHandDisplay()
    {
        // Fjern gamle
        foreach (Node child in _enemyHandDisplay.GetChildren())
            child.QueueFree();

        // Lag en kortbakside per kort på motstanderens hånd
        for (int i = 0; i < _enemy.HandCount; i++)
        {
            var cardBack = new TextureRect();
            cardBack.Texture = _cardBackTexture;
            cardBack.CustomMinimumSize = new Vector2(128, 192);
            cardBack.StretchMode = TextureRect.StretchModeEnum.Scale;
            _enemyHandDisplay.AddChild(cardBack);
        }
    }

    private void UpdateDeckDisplay()
    {
        if (_deckCountLabel != null)
            _deckCountLabel.Text = $"{_player.DeckCount} cards remaining";
    }

    private void UpdateDiscardPile()
    {
        var discard = _player.GetDiscardPile();
        if (discard.Count > 0)
            _discardTopCard.Texture = discard[^1].Texture;
        else
            _discardTopCard.Texture = null;
    }

    private void ShowDiscardSheet()
    {
        // Fjern gamle kort
        foreach (Node child in _discardGrid.GetChildren())
            child.QueueFree();

        // Vis alle kort i discard
        foreach (var card in _player.GetDiscardPile())
        {
            var visual = _cardVisualScene.Instantiate<CardVisual>();
            _discardGrid.AddChild(visual);
            visual.Setup(card);
            visual.SetStatic();
            visual.CustomMinimumSize = new Vector2(64, 96); // ← mindre størrelse
        }

        _discardSheet.Visible = true;
    }

    //---Highlight kort-----
    private void HighlightLegalTargets(CardData card)
    {
        var positions = new[]
        {
            Slot.SlotPosition.Left,
            Slot.SlotPosition.Middle,
            Slot.SlotPosition.Right
        };

        for (int i = 0; i < 3; i++)
        {
            var playerPanel = _playerSlotsContainer.GetChild<Panel>(i);
            var enemyPanel = _enemySlotsContainer.GetChild<Panel>(i);
            var playerSlot = _battleMap.GetPlayerSlot(positions[i]);
            var enemySlot = _battleMap.GetEnemySlot(positions[i]);

            bool highlightPlayer = false;
            bool highlightEnemy = false;

            switch (card.Ability)
            {
                case CardData.AbilityType.GiveMinusStat: // Skeleton
                    highlightEnemy = enemySlot.IsOccupied;
                    highlightPlayer = playerSlot.IsOccupied && positions[i] != _pendingCardPosition;
                    break;
                case CardData.AbilityType.GivePlusStat: // Drake
                    highlightPlayer = playerSlot.IsOccupied;
                    highlightEnemy = enemySlot.IsOccupied;
                    break;
                case CardData.AbilityType.RemoveUnit: // Druid
                    highlightPlayer = playerSlot.IsOccupied;
                    highlightEnemy = enemySlot.IsOccupied;
                    break;
                case CardData.AbilityType.RemoveGainStats: // Mio
                    highlightPlayer = playerSlot.IsOccupied;
                    highlightEnemy = enemySlot.IsOccupied;
                    break;
                case CardData.AbilityType.SwitchSlots: // Hilda
                    highlightPlayer = playerSlot.IsOccupied;
                    highlightEnemy = enemySlot.IsOccupied;
                    break;
            }

            SetSlotHighlight(playerPanel, highlightPlayer);
            SetSlotHighlight(enemyPanel, highlightEnemy);
        }
    }

    private void SetSlotHighlight(Panel panel, bool highlight)
    {
        panel.Modulate = highlight
            ? new Color(1.10f, 1.10f, 1.10f, 1f)
            : Colors.White;
    }

    private void HighlightHandCards()
    {
        foreach (var visual in _handVisuals)
            visual.SetHighlighted(true);
    }

    private void RefreshHighlights()
    {
        if (_waitingForTarget && _pendingAbilityCard != null)
            HighlightLegalTargets(_pendingAbilityCard);
        else if (_waitingForHandTarget)
            HighlightHandCards();
    }

    private void ClearAllHighlights()
    {
        for (int i = 0; i < 3; i++)
        {
            var playerPanel = _playerSlotsContainer.GetChild<Panel>(i);
            var enemyPanel = _enemySlotsContainer.GetChild<Panel>(i);
            playerPanel.Modulate = Colors.White; // ← panel ikke CardVisual
            enemyPanel.Modulate = Colors.White;
        }

        foreach (var visual in _handVisuals)
            visual.SetHighlighted(false);
    }

    //--Animation


}
