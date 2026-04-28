using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    [Export] private Panel _druidChoicePanel;
    [Export] private Button _plusThreeButton;
    [Export] private Button _minusThreeButton;
    [Export] private Panel _enemyDeckDisplay;
    [Export] private Label _enemyDeckCountLabel;
    [Export] private Panel _enemyDiscardPile;
    [Export] private CanvasLayer _enemyDiscardSheet;
    [Export] private GridContainer _enemyDiscardGrid;
    [Export] private Button _enemyCloseDiscardButton;
    [Export] private CanvasLayer _gameOverScreen;
    [Export] private Label _gameOverResultLabel;
    [Export] private Label _gameOverScoreLabel;
    [Export] private Button _rematchButton;
    [Export] private Button _continueButton;


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

    // Boss
    //var boss = TeltBattleConfig.Instance.CurrentBoss;

    public void SetBackground(Texture2D texture)
    {
        if (_background != null)
            _background.Texture = texture;
    }


    // ── Tilstand ──────────────────────────────────────────────────────
    private CardVisual _selectedCard = null;
    private List<CardVisual> _handVisuals = new();
    private Slot _pendingDruidTarget = null;

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
            UpdateEnemyDeckDisplay();
            UpdateEnemyDiscardPile();
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

        _continueButton.Pressed += () =>
        {
            var returnPath = TeltBattleConfig.Instance.ReturnScenePath;
            if (!string.IsNullOrEmpty(returnPath))
                GetTree().ChangeSceneToFile(returnPath);
            else
                GD.PrintErr("[TELT] Ingen ReturnScenePath satt!");
        };

        _gameOverScreen.Visible = false;
        _rematchButton.Pressed += () =>
        {
            PlayerData.ResetSessionDamage();
            GetTree().ReloadCurrentScene();
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
        SetBackground(_basicBackground);

        _abilityResolver.StatReset += (slotIndex, isPlayerSlot) =>
        {
            var panel = isPlayerSlot
                ? _playerSlotsContainer.GetChild<Panel>(slotIndex)
                : _enemySlotsContainer.GetChild<Panel>(slotIndex);

            foreach (Node child in panel.GetChildren())
                if (child is CardVisual visual)
                    visual.FlashStatColor(new Color(0.3f, 0.5f, 1f, 1f)); // blå
        };

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

        _enemyDiscardPile.GuiInput += (inputEvent) =>
        {
            if (inputEvent is InputEventMouseButton mouseEvent
                && mouseEvent.Pressed
                && mouseEvent.ButtonIndex == MouseButton.Left)
                ShowEnemyDiscardSheet();
        };
        _enemyCloseDiscardButton.Pressed += () => _enemyDiscardSheet.Visible = false;
        _enemyDiscardSheet.Visible = false;

        _druidChoicePanel.Visible = false;
        _plusThreeButton.Pressed += () => OnDruidChoice(3);
        _minusThreeButton.Pressed += () => OnDruidChoice(-3);
        _gameManager.StatTicked += (slotIndex, isPlayerSlot, isPositive) =>
        {
            float delay = isPositive ? 0.65f : 0.1f;
            GetTree().CreateTimer(delay).Timeout += () =>
            {
                var panel = isPlayerSlot
                    ? _playerSlotsContainer.GetChild<Panel>(slotIndex)
                    : _enemySlotsContainer.GetChild<Panel>(slotIndex);

                foreach (Node child in panel.GetChildren())
                    if (child is CardVisual visual && !visual.IsQueuedForDeletion())
                        visual.FlashStatColor(isPositive
                            ? new Color(0.2f, 1f, 0.2f, 1f)
                            : new Color(1f, 0.2f, 0.2f, 1f));
            };

            UpdateUI();
            UpdateEnemyHandDisplay();
            UpdateDeckDisplay();
            UpdateDiscardPile();
            UpdateEnemyDeckDisplay();
            UpdateEnemyDiscardPile();
            GD.Print("UI oppdatert!");
        };

        _abilityResolver.OnTargetHighlight = (slotIndex, isPlayerSlot) =>
        {
            _lastAITargetSlot = slotIndex;
            _lastAITargetIsPlayer = isPlayerSlot;
        };

        _playerDeckDisplay.MouseEntered += () => _deckCountLabel.Visible = true;
        _playerDeckDisplay.MouseExited += () => _deckCountLabel.Visible = false;
        _enemyDeckDisplay.MouseEntered += () => _enemyDeckCountLabel.Visible = true;
        _enemyDeckDisplay.MouseExited += () => _enemyDeckCountLabel.Visible = false;
        _enemyDeckCountLabel.Visible = false;
    }


// ── Deck-oppsett ──────────────────────────────────────────────────
    private void SetupDecks()
    {
        var boss = TeltBattleConfig.Instance?.CurrentBoss;

        var startDeck = new List<CardData>
        {
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Yeti.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Watcher.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Goblin.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Imp.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Snake.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Skeleton.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Tortoise.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Wraith.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Spider.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Bat.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Minotaur.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Elemental.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Uncommon_Golem.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Uncommon_Druid.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Uncommon_Drake.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Uncommon_Dryad.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Uncommon_Cat.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Uncommon_Sludge.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Uncommon_Horror.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Rare_Skester.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Rare_Eve.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Rare_Hilda.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Rare_Croxy.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Rare_Mio.tres")).Duplicate() as CardData,
            (GD.Load<CardData>("res://TELT/Resources/Cards/Common_Minotaur.tres")).Duplicate() as CardData,
        };

        _player.SetDeck(startDeck);
        _player.ShuffleDeck();

        if (boss != null && boss.Deck.Count > 0)
        {
            var enemyDeck = new List<CardData>();
            foreach (var card in boss.Deck)
                enemyDeck.Add(card.Duplicate() as CardData);
            _enemy.SetDeck(enemyDeck);
        }
        else
        {
            var enemyDeck = startDeck.ConvertAll(c => c.Duplicate() as CardData);
            _enemy.SetDeck(enemyDeck);
        }

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
    private async void OnCombatPressed()
    {
        _combatButton.Visible = false;

        foreach (var label in _combatDamageLabels)
            label.QueueFree();
        _combatDamageLabels.Clear();

        await PlayCombatAnimation();
        _gameManager.TriggerWarPhase();
    }

    private async void OnNextMatchPressed()
    {
        _nextMatchButton.Visible = false;

        foreach (var label in _combatDamageLabels)
            label.QueueFree();
        _combatDamageLabels.Clear();

        await PlayCleanupAnimation();
        _gameManager.TriggerNextMatch();
    }

    // ── Kortvalg fra hånd ─────────────────────────────────────────────
    private bool _isProcessingAction = false;

    private void OnCardClicked(CardVisual card)
    {
        GD.Print($"OnCardClicked: waitingForTarget={_waitingForTarget}, waitingForHand={_waitingForHandTarget}, currentTurn={_gameManager.CurrentTurn}");

        if (_gameManager.CurrentTurn != GameManager.TurnOwner.Player) return;

        if (_waitingForHandTarget)
        {
            if (_isProcessingAction) return; // ← blokkér dobbelt-klikk
            _isProcessingAction = true;

            card.SetSelected(false);
            var abilityCard = _pendingAbilityCard;
            _abilityResolver.ResolveWithHandTarget(_pendingAbilityCard, card.CardData, true);
            RemoveCardFromHand(card);
            _waitingForTarget = false;
            _waitingForHandTarget = false;
            _pendingAbilityCard = null;
            _gameManager.HoldTurn = false;
            RefreshHandVisuals();
            UpdateSlotVisuals();

            if (abilityCard.Ability == CardData.AbilityType.DiscardGainStats)
            {
                GetTree().CreateTimer(0.05f).Timeout += () =>
                    FlashSlotForCard(abilityCard, true, new Color(0.2f, 1f, 0.2f, 1f));
            }

            UpdateUI();
            UpdateEnemyHandDisplay();
            UpdateDeckDisplay();
            UpdateDiscardPile();
            UpdateEnemyDeckDisplay();
            UpdateEnemyDiscardPile();

            _isProcessingAction = false; // ← frigjør

            if (_gameManager.CheckWarPhase())
            {
                _combatButton.Visible = true;
                return;
            }

            _gameManager.SwitchTurnPublic();
            return;
        }

        if (_waitingForTarget) return;

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
            Slot.SlotPosition.MidLeft,
            Slot.SlotPosition.MidRight,
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
            UpdateEnemyDeckDisplay();
            UpdateEnemyDiscardPile();

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
        for (int i = 0; i < 4; i++)
        {
            if (_battleMap.GetPlayerSlot((Slot.SlotPosition)i).IsOccupied) return true;
            if (_battleMap.GetEnemySlot((Slot.SlotPosition)i).IsOccupied) return true;
        }

        return false;
    }

    private void OnSlotClicked(InputEvent inputEvent, Slot.SlotPosition position)
{
    if (_selectedCard == null || _selectedCard.IsQueuedForDeletion() || _selectedCard.CardData == null) return;
    if (inputEvent is not InputEventMouseButton mouseEvent) return;
    if (!mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left) return;
    if (_isProcessingAction) return;
    _isProcessingAction = true;

    var cardToPlay = _selectedCard;
    var cardData   = _selectedCard.CardData;

    if (_abilityResolver.NeedsTarget(cardData))
        _gameManager.HoldTurn = true;

    bool success = _gameManager.TryPlayCard(cardData, position, GameManager.TurnOwner.Player);

    if (success)
    {
        if (!_abilityResolver.NeedsTarget(cardData))
        {
            var ability = cardData.Ability;
            _abilityResolver.ResolveNoTarget(cardData, position, true);
            RemoveCardFromHand(cardToPlay);
            _selectedCard = null;

            // Bug 3-fix: DrawCard/DrawTwoCards - legg til nye kort enkeltvis
            // i stedet for full RefreshHandVisuals (forhindrer hopping)
            if (ability == CardData.AbilityType.DrawCard ||
                ability == CardData.AbilityType.DrawTwoCards)
            {
                foreach (var newCard in _player.NewlyDrawnCards)
                    AddCardToHandAnimated(newCard);
                _player.ClearLastDrawnCard();
            }
            else
            {
                RefreshHandVisuals();
            }

            UpdateSlotVisuals();

            GetTree().CreateTimer(0.05f).Timeout += () =>
            {
                switch (ability)
                {
                    case CardData.AbilityType.AllEnemyMinusStat:
                        FlashAllOccupiedSlots(false, new Color(1f, 0.2f, 0.2f, 1f));
                        break;
                    case CardData.AbilityType.AllAllyPlusStat:
                        FlashAllOccupiedSlots(true, new Color(0.2f, 1f, 0.2f, 1f));
                        break;
                }
            };

            UpdateUI();
            UpdateEnemyHandDisplay();
            UpdateDeckDisplay();
            UpdateDiscardPile();
            UpdateEnemyDeckDisplay();
            UpdateEnemyDiscardPile();

            if (_gameManager.CheckWarPhase())
            {
                _combatButton.Visible = true;
                _isProcessingAction = false;
                return;
            }
        }
        else
        {
            var targetType = _abilityResolver.GetTargetType(cardData);

            if (targetType == AbilityResolver.TargetType.HandCard)
            {
                if (!_player.HasCardsInHand)
                {
                    GD.Print($"[Ability] {cardData.CardName} fizzlet — ingen kort på hånd");
                    _gameManager.HoldTurn = false;
                    RemoveCardFromHand(cardToPlay);
                    _selectedCard = null;
                    UpdateSlotVisuals();
                    UpdateUI();
                    UpdateEnemyHandDisplay();
                    UpdateDeckDisplay();
                    UpdateDiscardPile();
                    UpdateEnemyDeckDisplay();
                    UpdateEnemyDiscardPile();
                    _isProcessingAction = false;
                    _gameManager.SwitchTurnPublic();
                    return;
                }
                else
                {
                    _waitingForTarget     = true;
                    _waitingForHandTarget = true;
                    _pendingAbilityCard   = cardData;
                    _pendingCardPosition  = position;
                    _phaseLabel.Text      = "Discard a card";
                    _combatButton.Visible = false;
                    RemoveCardFromHand(cardToPlay);
                    _selectedCard = null;
                    UpdateSlotVisuals();
                    HighlightHandCards();
                }
            }
            else
            {
                _waitingForTarget     = true;
                _waitingForHandTarget = false;
                _pendingAbilityCard   = cardData;
                _pendingCardPosition  = position;
                _phaseLabel.Text      = "Select target";
                _combatButton.Visible = false;
                RemoveCardFromHand(cardToPlay);
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

    _isProcessingAction = false;
}

    private void OnTargetSlotClicked(InputEvent inputEvent, Slot.SlotPosition position, bool isPlayerSlot)
    {
        if (_waitingForHandTarget) return;

        if (inputEvent is not InputEventMouseButton mouseEvent) return;
        if (!mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left) return;

        var targetSlot = isPlayerSlot
            ? _battleMap.GetPlayerSlot(position)
            : _battleMap.GetEnemySlot(position);

        if (targetSlot.IsEmpty) return;

        if (_pendingAbilityCard.Ability == CardData.AbilityType.GiveMinusOneStat
            && isPlayerSlot
            && position == _pendingCardPosition)
        {
            GD.Print("[Ability] - 1 kan ikke targete seg selv!");
            return;
        }

        if ((_pendingAbilityCard.Ability == CardData.AbilityType.ApplyPoison ||
             _pendingAbilityCard.Ability == CardData.AbilityType.ApplyRage)
            && targetSlot.Card.HasStatus)
        {
            GD.Print("[Ability] Target har allerede en status!");
            return;
        }

        if (_pendingAbilityCard.Ability == CardData.AbilityType.GivePlusMinusThree)
        {
            _pendingDruidTarget = targetSlot;

            // Finn panelet som tilhører denne sloten
            var panel = isPlayerSlot
                ? _playerSlotsContainer.GetChild<Panel>((int)position)
                : _enemySlotsContainer.GetChild<Panel>((int)position);

            // Plasser DruidChoicePanel over sloten
            var globalPos = panel.GlobalPosition;
            _druidChoicePanel.GlobalPosition = new Vector2(
                globalPos.X + panel.Size.X / 2 - _druidChoicePanel.Size.X / 2,
                globalPos.Y - _druidChoicePanel.Size.Y - 10
            );

            _druidChoicePanel.Visible = true;
            return;
        }

        // Hilda trenger to targets
        if (_pendingAbilityCard.Ability == CardData.AbilityType.SwitchSlots)
        {
            if (_hildaFirstTarget == null)
            {
                _hildaFirstTarget = targetSlot;
                _phaseLabel.Text = "Choose a card\nto swap";
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

        var resolvedAbility = _pendingAbilityCard?.Ability;
        var resolvedTarget = targetSlot;

        _waitingForTarget = false;
        _pendingAbilityCard = null;
        _gameManager.HoldTurn = false;
        ClearAllHighlights();


        UpdateSlotVisuals(); // ← først
        GetTree().CreateTimer(0.05f).Timeout += () =>
        {
            switch (resolvedAbility)
            {
                case CardData.AbilityType.GivePlusOneStat:
                case CardData.AbilityType.GivePlusTwoStats:
                    FlashStatOnSlot(resolvedTarget, new Color(0.2f, 1f, 0.2f, 1f));
                    break;
                case CardData.AbilityType.GiveMinusOneStat:
                case CardData.AbilityType.GiveMinusTwoStats:
                    FlashStatOnSlot(resolvedTarget, new Color(1f, 0.2f, 0.2f, 1f));
                    break;
                case CardData.AbilityType.ResetStat:
                    FlashStatOnSlot(resolvedTarget, new Color(0.3f, 0.5f, 1f, 1f));
                    break;
            }
        };

        UpdateUI();
        UpdateEnemyHandDisplay();
        UpdateDeckDisplay();
        UpdateDiscardPile();
        UpdateEnemyDeckDisplay();
        UpdateEnemyDiscardPile();


        if (_gameManager.CheckWarPhase() && !_waitingForTarget && !_waitingForHandTarget)
            _combatButton.Visible = true;

        _gameManager.SwitchTurnPublic();
    }

    // ── Hånd-visuals ──────────────────────────────────────────────────
    private void RefreshHandVisuals()
    {
        _selectedCard = null;

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
                    .SetDelay(animIndex * 0.3f);
                tween.TweenProperty(visual, "position:y", visual.Position.Y - 50, 0.3f)
                    .SetDelay(animIndex * 0.3f)
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
            Slot.SlotPosition.MidLeft,
            Slot.SlotPosition.MidRight,
            Slot.SlotPosition.Right
        };

        for (int i = 0; i < 4; i++)
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
            visual.ApplyStatusVisual();
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
        UpdateEnemyDeckDisplay();
        UpdateEnemyDiscardPile();

        if (phase == GameManager.GamePhase.PlayPhase)
        {
            foreach (var label in _combatDamageLabels)
                label.QueueFree();
            _combatDamageLabels.Clear();

            if (_gameManager.CurrentMatch == 1)
                GetTree().CreateTimer(1.8f).Timeout += () => { RefreshHandVisuals(); };
            else
                RefreshHandVisuals();

            _combatButton.Visible = false;
            _nextMatchButton.Visible = false;
        }

        if (phase == GameManager.GamePhase.WarPhase)
        {
            _combatButton.Visible = false;
            _nextMatchButton.Text = _gameManager.CurrentMatch >= 4 ? "Finish" : "Next Match";
            _nextMatchButton.Visible = true;
        }

        if (phase != GameManager.GamePhase.WarPhase && phase != GameManager.GamePhase.CleanupPhase)
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

        if (turn == GameManager.TurnOwner.Player)
        {
            bool hasSkester = _player.GetHand().Any(c => c.Ability == CardData.AbilityType.AnySlot);
            bool noLegalMoves = !_player.HasCardsInHand
                                || (_battleMap.PlayerEmptySlotCount == 0 && (!hasSkester || _battleMap.EnemyEmptySlotCount == 0));

            if (noLegalMoves)
            {
                if (_gameManager.CheckWarPhase())
                    _combatButton.Visible = true;
                else
                    _gameManager.SwitchTurnPublic();
                return;
            }
        }

        if (turn == GameManager.TurnOwner.Enemy)
        {
            if (_selectedCard != null)
            {
                _selectedCard.SetSelected(false);
                _selectedCard = null;
            }

            bool isVeryFirstTurn = _gameManager.CurrentMatch == 1
                                   && _battleMap.PlayerEmptySlotCount == 4
                                   && _battleMap.EnemyEmptySlotCount == 4;

            float delay = isVeryFirstTurn ? 3.7f : 0.5f;

            Callable.From(() =>
            {
                var timer = GetTree().CreateTimer(delay);
                timer.Timeout += () =>
                {
                    _enemyAI.TakeTurn();
                    UpdateSlotVisuals();

                    // Flash AI target highlight
                    if (_lastAITargetSlot >= 0)
                    {
                        int slot = _lastAITargetSlot;
                        bool isPlayer = _lastAITargetIsPlayer;
                        _lastAITargetSlot = -1;
                        GetTree().CreateTimer(0.1f).Timeout += () =>
                        {
                            var panel = isPlayer
                                ? _playerSlotsContainer.GetChild<Panel>(slot)
                                : _enemySlotsContainer.GetChild<Panel>(slot);
                            panel.Modulate = new Color(1.3f, 1.3f, 1.3f, 1f);
                            GetTree().CreateTimer(0.5f).Timeout += () => panel.Modulate = Colors.White;
                        };
                    }

                    // Flash stat tick farger
                    GetTree().CreateTimer(0.1f).Timeout += () =>
                    {
                        UpdateSlotVisuals();
                        // StatTicked signalet håndterer farger via callback
                    };

                    if (_battleMap.PlayerEmptySlotCount > 0 && _player.HasCardsInHand)
                        _combatButton.Visible = false;
                };
            }).CallDeferred();
        }
    }





    private void OnMatchEnded(int match, int playerDamage, int enemyDamage)
    {
        _resultLabel.Text = $"Match {match}:\nYou take {playerDamage} damage\n— Enemy take {enemyDamage}";
        UpdateUI();
        FlashDamageLabel(_playerDamageLabel, playerDamage);
        FlashDamageLabel(_enemyDamageLabel, enemyDamage);
    }

    private void OnGameOver(GameManager.TurnOwner winner, int playerDamage, int enemyDamage)
    {
        foreach (var visual in _handVisuals)
            visual.QueueFree();
        _handVisuals.Clear();

        UpdateSlotVisuals();

        var boss = TeltBattleConfig.Instance?.CurrentBoss;
        bool playerWon = winner == GameManager.TurnOwner.Player;

        if (boss != null)
        {
            PlayerData.DefeatNpc(boss.NpcId, _enemy.TotalDamageReceived);

            if (playerWon && !PlayerData.HasReceivedCard(boss.NpcId))
                PlayerData.GiveRewardCard(boss.NpcId);
        }
        else
        {
            // Fallback for testing uten config
            PlayerData.DefeatNpc("npc_goblin_king", _enemy.TotalDamageReceived);
        }

        ShowGameOverScreen(winner, playerDamage, enemyDamage);
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

    // Deck display

    private void UpdateDeckDisplay()
    {
        if (_deckCountLabel != null)
            _deckCountLabel.Text = $"{_player.DeckCount} cards remaining";
    }

    private void UpdateDiscardPile()
    {
        // Fjern eksisterende innhold
        foreach (Node child in _discardPile.GetChildren())
            child.QueueFree();

        var discard = _player.GetDiscardPile();
        if (discard.Count == 0) return;

        var visual = _cardVisualScene.Instantiate<CardVisual>();
        _discardPile.AddChild(visual);
        visual.Setup(discard[^1]);
        visual.SetStatic();
        visual.CustomMinimumSize = new Vector2(111, 173);
        visual.Size = new Vector2(111, 173);
        visual.Scale = new Vector2(111f / 128f, 173f / 192f);
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

    private void UpdateEnemyDeckDisplay()
    {
        if (_enemyDeckCountLabel != null)
            _enemyDeckCountLabel.Text = $"{_enemy.DeckCount} cards remaining";
    }

    private void UpdateEnemyDiscardPile()
    {
        foreach (Node child in _enemyDiscardPile.GetChildren())
            child.QueueFree();

        var discard = _enemy.GetDiscardPile();
        if (discard.Count == 0) return;

        var visual = _cardVisualScene.Instantiate<CardVisual>();
        _enemyDiscardPile.AddChild(visual);
        visual.Setup(discard[^1]);
        visual.SetStatic();
        visual.CustomMinimumSize = new Vector2(111, 173);
        visual.Size = new Vector2(111, 173);
        visual.Scale = new Vector2(111f / 128f, 173f / 192f);
    }

    private void ShowEnemyDiscardSheet()
    {
        foreach (Node child in _enemyDiscardGrid.GetChildren())
            child.QueueFree();

        foreach (var card in _enemy.GetDiscardPile())
        {
            var visual = _cardVisualScene.Instantiate<CardVisual>();
            _enemyDiscardGrid.AddChild(visual);
            visual.Setup(card);
            visual.SetStatic();
            visual.CustomMinimumSize = new Vector2(64, 96);
        }

        _enemyDiscardSheet.Visible = true;
    }

    //---Highlight kort-----

    private int _lastAITargetSlot = -1;
    private bool _lastAITargetIsPlayer = false;


    private void HighlightLegalTargets(CardData card)
    {
        var positions = new[]
        {
            Slot.SlotPosition.Left,
            Slot.SlotPosition.MidLeft,
            Slot.SlotPosition.MidRight,
            Slot.SlotPosition.Right
        };

        for (int i = 0; i < 4; i++)
        {
            var playerPanel = _playerSlotsContainer.GetChild<Panel>(i);
            var enemyPanel = _enemySlotsContainer.GetChild<Panel>(i);
            var playerSlot = _battleMap.GetPlayerSlot(positions[i]);
            var enemySlot = _battleMap.GetEnemySlot(positions[i]);

            bool highlightPlayer = false;
            bool highlightEnemy = false;

            switch (card.Ability)
            {
                case CardData.AbilityType.GiveMinusOneStat:
                case CardData.AbilityType.GiveMinusTwoStats:
                    highlightEnemy = enemySlot.IsOccupied;
                    highlightPlayer = playerSlot.IsOccupied && positions[i] != _pendingCardPosition;
                    break;
                case CardData.AbilityType.GivePlusOneStat:
                case CardData.AbilityType.GivePlusTwoStats: // Drake
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
                case CardData.AbilityType.ResetStat:
                    highlightPlayer = playerSlot.IsOccupied;
                    highlightEnemy = enemySlot.IsOccupied;
                    break;
                case CardData.AbilityType.CopyStat:
                    highlightPlayer = playerSlot.IsOccupied;
                    highlightEnemy = enemySlot.IsOccupied;
                    break;
                case CardData.AbilityType.GivePlusMinusThree: // Druid
                    highlightPlayer = playerSlot.IsOccupied;
                    highlightEnemy = enemySlot.IsOccupied;
                    break;
                case CardData.AbilityType.ApplyPoison:
                case CardData.AbilityType.ApplyRage:
                    highlightPlayer = playerSlot.IsOccupied && !playerSlot.Card.HasStatus;
                    highlightEnemy = enemySlot.IsOccupied && !enemySlot.Card.HasStatus;
                    break;
            }

            SetSlotHighlight(playerPanel, highlightPlayer);
            SetSlotHighlight(enemyPanel, highlightEnemy);
        }
    }

    // Druids choice
    private void OnDruidChoice(int amount)
    {
        var druidTarget = _pendingDruidTarget;
        _pendingDruidTarget = null;

        _druidChoicePanel.Visible = false;
        if (druidTarget == null) return; // ← druidTarget, ikke _pendingDruidTarget

        int newStat = Mathf.Clamp(druidTarget.Card.GetCurrentDamage() + amount, 0, 9);
        druidTarget.Card.CurrentDamage = newStat;
        GD.Print($"[Ability] Druid: {druidTarget.Card.CardName} er nå {newStat}");

        _waitingForTarget = false;
        _pendingAbilityCard = null;
        _gameManager.HoldTurn = false;
        ClearAllHighlights();

        UpdateSlotVisuals();
        GetTree().CreateTimer(0.05f).Timeout += () =>
            FlashStatOnSlot(druidTarget, amount > 0
                ? new Color(0.2f, 1f, 0.2f, 1f)
                : new Color(1f, 0.2f, 0.2f, 1f));

        UpdateUI();
        UpdateEnemyHandDisplay();
        UpdateDeckDisplay();
        UpdateDiscardPile();
        UpdateEnemyDeckDisplay();
        UpdateEnemyDiscardPile();

        if (_gameManager.CheckWarPhase())
        {
            _combatButton.Visible = true;
            return;
        }

        _gameManager.SwitchTurnPublic();
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

    private async void FlashSlotHighlight(Slot.SlotPosition position, bool isPlayerSlot)
    {
        var panel = isPlayerSlot
            ? _playerSlotsContainer.GetChild<Panel>((int)position)
            : _enemySlotsContainer.GetChild<Panel>((int)position);

        panel.Modulate = new Color(1.15f, 1.15f, 1.15f, 1f);
        await ToSignal(GetTree().CreateTimer(0.4f), "timeout");
        panel.Modulate = Colors.White;
    }

    private void ClearAllHighlights()
    {
        for (int i = 0; i < 4; i++)
        {
            var playerPanel = _playerSlotsContainer.GetChild<Panel>(i);
            var enemyPanel = _enemySlotsContainer.GetChild<Panel>(i);
            playerPanel.Modulate = Colors.White; // ← panel ikke CardVisual
            enemyPanel.Modulate = Colors.White;
        }

        foreach (var visual in _handVisuals)
            visual.SetHighlighted(false);
    }

    // Damage flash
    private void FlashStatOnSlot(Slot targetSlot, Color color)
    {
        var positions = new[]
            { Slot.SlotPosition.Left, Slot.SlotPosition.MidLeft, Slot.SlotPosition.MidRight, Slot.SlotPosition.Right };
        for (int i = 0; i < 4; i++)
        {
            if (_battleMap.GetPlayerSlot(positions[i]) == targetSlot)
            {
                foreach (Node child in _playerSlotsContainer.GetChild<Panel>(i).GetChildren())
                    if (child is CardVisual visual && !visual.IsQueuedForDeletion())
                        visual.FlashStatColor(color);
                return;
            }

            if (_battleMap.GetEnemySlot(positions[i]) == targetSlot)
            {
                foreach (Node child in _enemySlotsContainer.GetChild<Panel>(i).GetChildren())
                    if (child is CardVisual visual && !visual.IsQueuedForDeletion())
                        visual.FlashStatColor(color);
                return;
            }
        }
    }

    private async void FlashSlotBlue(Slot.SlotPosition position, bool isPlayer)
    {

        var panel = isPlayer
            ? _playerSlotsContainer.GetChild<Panel>((int)position)
            : _enemySlotsContainer.GetChild<Panel>((int)position);

        panel.Modulate = new Color(0.3f, 0.5f, 1f, 1f); // blå
        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
        panel.Modulate = Colors.White;
    }

    private void FlashAllOccupiedSlots(bool isPlayerSlots, Color color)
    {
        for (int i = 0; i < 4; i++)
        {
            var positions = new[]
            {
                Slot.SlotPosition.Left, Slot.SlotPosition.MidLeft, Slot.SlotPosition.MidRight, Slot.SlotPosition.Right
            };
            var slot = isPlayerSlots ? _battleMap.GetPlayerSlot(positions[i]) : _battleMap.GetEnemySlot(positions[i]);
            if (!slot.IsOccupied) continue;
            var panel = isPlayerSlots
                ? _playerSlotsContainer.GetChild<Panel>(i)
                : _enemySlotsContainer.GetChild<Panel>(i);
            foreach (Node child in panel.GetChildren())
                if (child is CardVisual visual && !visual.IsQueuedForDeletion())
                    visual.FlashStatColor(color);
        }
    }

    private void FlashSlotForCard(CardData card, bool isPlayer, Color color)
    {
        var positions = new[]
            { Slot.SlotPosition.Left, Slot.SlotPosition.MidLeft, Slot.SlotPosition.MidRight, Slot.SlotPosition.Right };
        for (int i = 0; i < 4; i++)
        {
            var slot = isPlayer ? _battleMap.GetPlayerSlot(positions[i]) : _battleMap.GetEnemySlot(positions[i]);
            if (slot.IsOccupied && slot.Card == card)
            {
                var panel = isPlayer
                    ? _playerSlotsContainer.GetChild<Panel>(i)
                    : _enemySlotsContainer.GetChild<Panel>(i);
                foreach (Node child in panel.GetChildren())
                    if (child is CardVisual visual && !visual.IsQueuedForDeletion())
                        visual.FlashStatColor(color);
                return;
            }
        }
    }

    // Game over screens
    private void ShowGameOverScreen(GameManager.TurnOwner winner, int playerDamage, int enemyDamage)
    {
        bool isDraw = playerDamage == enemyDamage;
        bool playerWon = winner == GameManager.TurnOwner.Player;

        _gameOverResultLabel.Text = isDraw ? "DRAW" :
            playerWon ? "VICTORY!" : "DEFEAT";
        _gameOverScoreLabel.Text =
            $"You: {playerDamage} — Opponent: {enemyDamage}";

        _rematchButton.Visible = !playerWon;
        _continueButton.Visible = playerWon;

        foreach (Node child in _playerSlotsContainer.GetChildren())
            if (child is Panel panel)
                foreach (Node grandchild in panel.GetChildren())
                    grandchild.QueueFree();

        foreach (Node child in _enemySlotsContainer.GetChildren())
            if (child is Panel panel)
                foreach (Node grandchild in panel.GetChildren())
                    grandchild.QueueFree();

// Rydd hånd
        foreach (var visual in _handVisuals)
            visual.QueueFree();
        _handVisuals.Clear();

// Rydd enemy hand display
        foreach (Node child in _enemyHandDisplay.GetChildren())
            child.QueueFree();

// Rydd discard piles
        foreach (Node child in _discardPile.GetChildren())
            child.QueueFree();
        foreach (Node child in _enemyDiscardPile.GetChildren())
            child.QueueFree();

// Skjul knapper og labels
        _ui.Visible = false;


        _gameOverScreen.Visible = true;
    }



    //--Animation

    private List<Label> _combatDamageLabels = new();

    private async Task PlayCombatAnimation()
    {
        var laneResults = _battleMap.GetLaneResults();
        var positions = new[] { Slot.SlotPosition.Left, Slot.SlotPosition.MidLeft, Slot.SlotPosition.MidRight, Slot.SlotPosition.Right };

        for (int i = 0; i < 4; i++)
        {
            var playerPanel = _playerSlotsContainer.GetChild<Panel>(i);
            var enemyPanel = _enemySlotsContainer.GetChild<Panel>(i);

            bool hasPlayer = _battleMap.GetPlayerSlot(positions[i]).IsOccupied;
            bool hasEnemy = _battleMap.GetEnemySlot(positions[i]).IsOccupied;
            if (!hasPlayer && !hasEnemy) continue;

            Vector2 playerOrigin = playerPanel.Position;
            Vector2 enemyOrigin = enemyPanel.Position;

            // Pull back
            var pull = CreateTween().SetParallel(true);
            if (hasPlayer) pull.TweenProperty(playerPanel, "position:y", playerOrigin.Y + 15f, 0.15f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            if (hasEnemy) pull.TweenProperty(enemyPanel, "position:y", enemyOrigin.Y - 15f, 0.15f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            await ToSignal(pull, "finished");

            // Rush
            var rush = CreateTween().SetParallel(true);
            if (hasPlayer) rush.TweenProperty(playerPanel, "position:y", playerOrigin.Y - 30f, 0.1f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
            if (hasEnemy) rush.TweenProperty(enemyPanel, "position:y", enemyOrigin.Y + 30f, 0.1f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
            await ToSignal(rush, "finished");

            // ← Alltid vis damage label her, selv ved 0
            ShowLaneDamageLabel(i, laneResults[i].playerDamage, laneResults[i].enemyDamage);

            // Tilbake
            var ret = CreateTween().SetParallel(true);
            if (hasPlayer) ret.TweenProperty(playerPanel, "position:y", playerOrigin.Y, 0.2f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            if (hasEnemy) ret.TweenProperty(enemyPanel, "position:y", enemyOrigin.Y, 0.2f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            await ToSignal(ret, "finished");

            await ToSignal(GetTree().CreateTimer(0.15f), "timeout");
        }

        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
    }

    private void ShowLaneDamageLabel(int laneIndex, int playerDamage, int enemyDamage)
    {
        if (playerDamage == 0 && enemyDamage == 0)
        {
            SpawnDamageLabel(laneIndex, 0, null); // ← 0 i oransje
            return;
        }

        if (playerDamage > 0)
            SpawnDamageLabel(laneIndex, playerDamage, true);

        if (enemyDamage > 0)
            SpawnDamageLabel(laneIndex, enemyDamage, false);
    }

    private void SpawnDamageLabel(int laneIndex, int amount, bool? isPlayerDamage)
    {
        var label = new Label();
        label.Text = isPlayerDamage == null ? "0" :
            isPlayerDamage.Value ? $"-{amount}" : $"+{amount}";

        Color color = isPlayerDamage == null
            ? new Color(1f, 0.6f, 0.1f, 1f)
            : isPlayerDamage.Value
                ? new Color(1f, 0.2f, 0.2f, 1f)
                : new Color(0.2f, 1f, 0.2f, 1f);

        label.AddThemeColorOverride("font_color", color);
        label.AddThemeFontSizeOverride("font_size", 28);

        var playerPanel = _playerSlotsContainer.GetChild<Panel>(laneIndex);
        var enemyPanel = _enemySlotsContainer.GetChild<Panel>(laneIndex);

        // Midtpunkt mellom enemy-bunn og player-topp
        float midY = (enemyPanel.GlobalPosition.Y + enemyPanel.Size.Y + playerPanel.GlobalPosition.Y) / 2.12f;
        float midX = playerPanel.GlobalPosition.X + playerPanel.Size.X / 2f - 12f;

        _ui.AddChild(label);
        label.GlobalPosition = new Vector2(midX, midY);
        label.Modulate = new Color(1, 1, 1, 0);

        _combatDamageLabels.Add(label);

        var tween = label.CreateTween();
        tween.TweenProperty(label, "modulate:a", 1f, 0.15f);
    }

    private async Task PlayCleanupAnimation()
    {
        for (int i = 0; i < 4; i++)
        {
            var playerPanel = _playerSlotsContainer.GetChild<Panel>(i);
            var enemyPanel = _enemySlotsContainer.GetChild<Panel>(i);

            if (playerPanel.GetChildCount() > 0 && playerPanel.GetChild(0) is CardVisual playerVisual)
            {
                // Offset fra panel til discard i lokal space
                Vector2 targetOffset = _discardPile.GlobalPosition - playerPanel.GlobalPosition;
                var tween = playerVisual.CreateTween().SetParallel(true);
                tween.TweenProperty(playerVisual, "position", targetOffset, 0.4f)
                    .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
                tween.TweenProperty(playerVisual, "modulate:a", 0f, 0.4f);
            }

            if (enemyPanel.GetChildCount() > 0 && enemyPanel.GetChild(0) is CardVisual enemyVisual)
            {
                Vector2 targetOffset = _enemyDiscardPile.GlobalPosition - enemyPanel.GlobalPosition;
                var tween = enemyVisual.CreateTween().SetParallel(true);
                tween.TweenProperty(enemyVisual, "position", targetOffset, 0.4f)
                    .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
                tween.TweenProperty(enemyVisual, "modulate:a", 0f, 0.4f);
            }
        }

        await ToSignal(GetTree().CreateTimer(0.45f), "timeout");
        GD.Print("PlayCleanupAnimation ferdig");
    }

}
