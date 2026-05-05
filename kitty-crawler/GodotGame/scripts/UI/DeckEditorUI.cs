using Godot;
using System.Collections.Generic;
using System.Linq;
using KittyCrawler.TELT;

namespace KittyCrawler;

public partial class DeckEditorUI : CanvasLayer
{
    [Export] private GridContainer _deckGrid;
    [Export] private GridContainer _inventoryGrid;
    [Export] private Label _deckCountLabel;
    [Export] private Button _saveDeckButton;
    [Export] private Button _closeButton;
    [Export] private PackedScene _cardButtonVisualScene;
    [Export] private Control _previewContainer;
    [Export] private PackedScene _cardVisualScene;
    [Export] private Label _feedbackLabel;

    private CardVisual _previewCard = null;

    private List<string> _currentDeck = new();
    private List<string> _inventory = new();

    public override void _Ready()
    {
        _saveDeckButton.Pressed += OnSaveDeckPressed;
        _closeButton.Pressed += OnClosePressed;
        Visible = false;
    }

    private bool _isOpen = false;

    public void Open()
    {
        if (_isOpen) return; // ← blokkér dobbelt åpning
        _isOpen = true;

        _currentDeck = new List<string>(PlayerData.SavedDeck);
        var ownedCards = PlayerData.OwnedCards;
        _inventory = ownedCards.Where(c => !_currentDeck.Contains(c)).ToList();

        RefreshUI();
        Visible = true;
    }

    private void RefreshUI()
    {
        GD.Print($"[DeckEditor] RefreshUI starter, deck={_currentDeck.Count}, inventory={_inventory.Count}");

        foreach (Node child in _deckGrid.GetChildren())
            child.QueueFree();
        foreach (Node child in _inventoryGrid.GetChildren())
            child.QueueFree();

        GD.Print($"[DeckEditor] DeckGrid er null: {_deckGrid == null}");
        GD.Print($"[DeckEditor] InventoryGrid er null: {_inventoryGrid == null}");

        foreach (var cardPath in _currentDeck)
            AddCardEntry(_deckGrid, cardPath, true);

        foreach (var cardPath in _inventory)
            AddCardEntry(_inventoryGrid, cardPath, false);

        _deckCountLabel.Text = $"{_currentDeck.Count}/25";
    }

    private void AddCardEntry(GridContainer grid, string cardPath, bool isInDeck)
    {
        var cardData = GD.Load<CardData>(cardPath);
        if (cardData == null) return;

        var button = _cardButtonVisualScene.Instantiate<CardButtonVisual>();
        button.CustomMinimumSize = new Vector2(128, 40);
        grid.AddChild(button);
        button.Setup(cardData);

        // Hover preview
        button.MouseEntered += () => ShowPreview(cardData);
        button.MouseExited += () => HidePreview();

        if (isInDeck)
            button.Pressed += () => MoveToInventory(cardPath);
        else
            button.Pressed += () => MoveToDeck(cardPath);
    }

    private void ShowPreview(CardData cardData)
    {
        HidePreview();
        _previewCard = _cardVisualScene.Instantiate<CardVisual>();
        _previewContainer.AddChild(_previewCard);
        _previewCard.Setup(cardData);
        _previewCard.SetStatic();
    }

    private void HidePreview()
    {
        if (_previewCard != null)
        {
            _previewCard.QueueFree();
            _previewCard = null;
        }
    }

    private void MoveToDeck(string cardPath)
    {
        if (_currentDeck.Count >= 25) return;

        // Sjekk max copies
        var cardData = GD.Load<CardData>(cardPath);
        int maxCopies = cardData.CardRarity switch
        {
            CardData.Rarity.Common => 3,
            CardData.Rarity.Uncommon => 2,
            CardData.Rarity.Rare => 1,
            _ => 1
        };

        int copiesInDeck = _currentDeck.Count(c => c == cardPath);
        if (copiesInDeck >= maxCopies) return;

        _inventory.Remove(cardPath);
        _currentDeck.Add(cardPath);
        RefreshUI();
    }

    private void MoveToInventory(string cardPath)
    {
        int index = _currentDeck.IndexOf(cardPath);
        if (index >= 0)
            _currentDeck.RemoveAt(index); // ← fjern kun én instans
        _inventory.Add(cardPath);
        RefreshUI();
    }


    private void OnSaveDeckPressed()
    {
        if (_currentDeck.Count < 25)
        {
            ShowFeedback("Unable to save deck", Colors.Red);
            return;
        }
        PlayerData.SaveDeck(_currentDeck);
        ShowFeedback("Deck saved!", Colors.Green);
    }

    private async void ShowFeedback(string message, Color color)
    {
        _feedbackLabel.Text = message;
        _feedbackLabel.AddThemeColorOverride("font_color", color);
        _feedbackLabel.Visible = true;
        await ToSignal(GetTree().CreateTimer(2f), "timeout");
        _feedbackLabel.Visible = false;
    }

    private void OnClosePressed()
    {
        _isOpen = false; // ← reset
        Visible = false;
        GetParent<CanvasLayer>().Visible = true;
    }




}
