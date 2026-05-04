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

        GD.Print($"[DeckEditor] Button lagt til: {cardData.CardName}, size={button.Size}, minSize={button.CustomMinimumSize}");

        if (isInDeck)
            button.Pressed += () => MoveToInventory(cardPath);
        else
            button.Pressed += () => MoveToDeck(cardPath);
    }

    private void MoveToDeck(string cardPath)
    {
        if (_currentDeck.Count >= 25) return;

        // Sjekk max copies
        var cardData = GD.Load<CardData>(cardPath);
        int maxCopies = cardData.CardRarity switch
        {
            CardData.Rarity.Common => 2,
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
            GD.Print($"[DeckEditor] Deck må ha 25 kort, har {_currentDeck.Count}");
            // Vis feilmelding til spilleren
            _deckCountLabel.AddThemeColorOverride("font_color", Colors.Red);
            return;
        }
        _deckCountLabel.RemoveThemeColorOverride("font_color");
        PlayerData.SaveDeck(_currentDeck);
        GD.Print($"[DeckEditor] Deck lagret med {_currentDeck.Count} kort");
    }

    private void OnClosePressed()
    {
        _isOpen = false; // ← reset
        Visible = false;
        GetParent<CanvasLayer>().Visible = true;
    }


}
